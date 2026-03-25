// src/services/ocorrencia/wizard-pop.js
// Serviço MOCK do POP Wizard.
// Estrutura pensada para ser "backend-driven":
// - instruções dinâmicas por etapa (podem ser várias)
// - switches que aplicam filtros ao select da etapa
// - busca global (termo) que restringe opções nas 3 etapas
//
// Quando integrar com back-end, a ideia é manter os contratos:
// getEtapaCategoria / getEtapaServico / getEtapaDetalhe
// retornando { instrucoes, filtros, opcoes }.

const normalize = (s = '') =>
  (s || '')
    .toString()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9\s]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()

const includesAllTokens = (text, query) => {
  const t = normalize(text)
  const q = normalize(query)
  if (!q) return true
  const tokens = q.split(' ').filter(Boolean)
  return tokens.every(tok => t.includes(tok))
}

// Mock base (no futuro isso vem do back-end)
const CATEGORIAS = ['Água', 'Esgoto', 'Pavimentação', 'Eletromecânica']

const SERVICOS_BY_CATEGORIA = {
  Água: [
    'Vazamento de Água (S123)',
    'Classificar tipo de serviço',
    'Identificar nível de criticidade',
    'Associar localização do chamado',
    'Vincular atendimento a um consumidor (se aplicável)',
    'Gerar número de protocolo',
    'Encaminhar para análise e despacho',
    'Reclamação'
  ],
  Esgoto: [
    'Reclamação',
    'Fiscalização',
    'Registrar nova demanda',
    'Classificar tipo de serviço',
    'Identificar nível de criticidade',
    'Associar localização do chamado',
    'Vincular atendimento a um consumidor (se aplicável)',
    'Gerar número de protocolo',
    'Encaminhar para análise e despacho'
  ],
  Pavimentação: [
    'Reclamação',
    'Registrar nova demanda',
    'Classificar tipo de serviço',
    'Identificar nível de criticidade',
    'Associar localização do chamado',
    'Vincular atendimento a um consumidor (se aplicável)',
    'Gerar número de protocolo',
    'Encaminhar para análise e despacho'
  ],
  Eletromecânica: []
}

const DETALHES_BY_SERVICO = {
  'Vazamento de Água (S123)': ['Rede', 'Ramal', 'Cavalete', 'Muro', 'Passeio'],
  'Classificar tipo de serviço': [
    'Serviço corretivo',
    'Serviço preventivo',
    'Revisão técnica',
    'Consulta técnica',
    'Manutenção programada'
  ],
  'Identificar nível de criticidade': ['Baixa', 'Média', 'Alta', 'Urgente', 'Emergencial'],
  'Associar localização do chamado': ['Residencial', 'Comercial', 'Industrial', 'Público', 'Condomínio'],
  'Vincular atendimento a um consumidor (se aplicável)': [
    'Consumidor pessoa física',
    'Consumidor pessoa jurídica',
    'Usuário residencial',
    'Cliente institucional',
    'Consumidor especial'
  ],
  'Gerar número de protocolo': [
    'Protocolo automático',
    'Protocolo manual',
    'Protocolo prioritário',
    'Protocolo de urgência',
    'Protocolo padrão'
  ],
  'Encaminhar para análise e despacho': [
    'Despacho interno',
    'Despacho terceirizado',
    'Encaminhar à gerência',
    'Encaminhar ao supervisor',
    'Despacho automático'
  ],
  'Reclamação': [
    'Cobrança indevida',
    'Má qualidade de serviço',
    'Vazamento contínuo',
    'Falta de pressão',
    'Demora no atendimento'
  ],
  'Fiscalização': [
    'Fiscalização de rotina',
    'Fiscalização emergencial',
    'Fiscalização programada',
    'Fiscalização ambiental',
    'Fiscalização de segurança'
  ],
  'Registrar nova demanda': [
    'Registro padrão',
    'Registro prioritário',
    'Registro com retorno',
    'Registro com evidência'
  ]
}

const TAGS_DETALHE = {
  'Fiscalização programada': ['agenda', 'horario'],
  'Fiscalização de rotina': ['rotina'],
  'Fiscalização emergencial': ['emergencia', 'urgente'],
  'Rede': ['rede', 'rua', 'publica'],
  'Ramal': ['ramal', 'imovel'],
  'Passeio': ['calcada', 'publica']
}

const DETALHAMENTO_BY_DETALHE = {
  'Rede': 'Verifique se o vazamento/ocorrência está na rede pública.\nSinalize riscos para trânsito e pedestres.\nRegistrar referência visual (esquina, poste, etc.).',
  'Ramal': 'Confirmar se é ramal do imóvel.\nOrientar acesso ao local.\nChecar existência de hidrômetro/cavalete.',
  'Cavalete': 'Validar se o problema é no cavalete.\nOrientar fechar registro, se possível.\nSeparar equipe com peças adequadas.',
  'Muro': 'Verificar impacto em propriedade.\nRegistrar evidências.\nOrientar isolamento se houver risco.',
  'Passeio': 'Ocorrência em calçada/via pública.\nSinalizar e isolar área.\nAvaliar necessidade de apoio de trânsito.',
  'Fiscalização de rotina': 'Vistoria padrão conforme checklist.\nConfirmar acesso ao local.\nRegistrar achados com fotos.',
  'Fiscalização emergencial': 'Priorizar deslocamento.\nVerificar risco imediato.\nAcionar apoio se necessário.',
  'Fiscalização programada': 'Confirmar janela de atendimento.\nValidar presença do responsável.\nRegistrar protocolo e agenda.',
}

function applySwitchFiltersToServicos(servicos, filtros = {}) {
  let out = [...servicos]

  // Exemplo: filtros que reduzem opções
  // (mock) "somente fiscalizacao"
  if (filtros.apenasFiscalizacao) {
    out = out.filter(s => normalize(s).includes('fiscaliza'))
  }

  // (mock) "somente vazamento"
  if (filtros.apenasVazamento) {
    out = out.filter(s => normalize(s).includes('vazamento'))
  }

  return out
}

function applySwitchFiltersToDetalhes(detalhes, filtros = {}) {
  let out = [...detalhes]

  // (mock) filtro por agendamento/janela
  if (filtros.exigeAgendamento) {
    out = out.filter(d => {
      const tags = TAGS_DETALHE[d] || []
      return tags.includes('agenda') || normalize(d).includes('programada')
    })
  }

  // (mock) filtro por via pública
  if (filtros.viaPublica) {
    out = out.filter(d => {
      const tags = TAGS_DETALHE[d] || []
      return tags.includes('publica') || tags.includes('calcada') || normalize(d).includes('rede')
    })
  }

  return out
}

// Instruções dinâmicas (mock). No futuro: backend retorna array de blocos.
function instrucoesEtapaCategoria() {
  return [
    { type: 'text', icon: 'comment-dots', text: 'Identifique primeiro se o problema é de Água, Esgoto, Pavimentação ou Eletromecânica.' },
    { type: 'text', icon: 'comment-dots', text: 'Água: “vazamento”, “falta de pressão”, “hidrômetro/cavalete”, “rompimento”.' },
    { type: 'text', icon: 'comment-dots', text: 'Exemplo de POP para identificar serviço de esgoto e fiscalização. Selecione Esgoto!' },
    { type: 'switch', id: 'temOdorOuRetorno', label: 'O solicitante menciona odor/retorno/extravasamento?', default: false },
    { type: 'switch', id: 'temCondicaoEspecial', label: 'Existe condição especial (horário restrito / fiscal / acesso controlado)?', default: false },
    { type: 'text', icon: 'comment-dots', text: 'Esgoto: “entupimento”, “retorno”, “odor forte”, “extravasamento”, “boca de lobo”.' },
  ]
}

function instrucoesEtapaServico({ categoria }) {
  const base = [
    { type: 'text', icon: 'comment-dots', text: 'Agora selecione o Tipo do problema (Serviço). Use as perguntas abaixo para reduzir as opções.' },
    { type: 'text', icon: 'comment-dots', text: 'Se a solicitação envolve agenda/visita programada, normalmente há um serviço específico (ex.: fiscalização programada).' }
  ]

  if (categoria === 'Esgoto') {
    base.push({ type: 'text', icon: 'comment-dots', text: 'Esgoto: extravasamento em via pública, retorno e odor forte são os mais comuns.' })
    base.push({ type: 'switch', id: 'apenasFiscalizacao', label: 'Mostrar apenas serviços de Fiscalização', default: false })
    base.push({ type: 'switch', id: 'priorizarViaPublica', label: 'Priorizar serviços relacionados a via pública (calçada/rua)', default: false })
  }

  if (categoria === 'Água') {
    base.push({ type: 'text', icon: 'comment-dots', text: 'Água: vazamento em ramal/rede/cavalete normalmente é classificado como “Vazamento de Água (S123)”.' })
    base.push({ type: 'switch', id: 'apenasVazamento', label: 'Mostrar apenas serviços de Vazamento', default: false })
  }

  base.push({ type: 'switch', id: 'temHorarioRestrito', label: 'O cliente só recebe em horários específicos?', default: false })
  base.push({ type: 'switch', id: 'precisaFiscal', label: 'Precisa de fiscal/acompanhamento no local?', default: false })

  return base
}

function instrucoesEtapaDetalhe({ categoria, servico }) {
  const base = [
    { type: 'text', icon: 'comment-dots', text: 'Selecione o detalhe do problema. O detalhe ajuda a equipe a ir preparada.' },
    { type: 'text', icon: 'comment-dots', text: 'Se for “via pública/calçada”, prefira detalhes que indiquem rede/passeio/local externo.' }
  ]

  if (categoria === 'Esgoto' && normalize(servico).includes('fiscaliza')) {
    base.push({ type: 'text', icon: 'comment-dots', text: 'Fiscalização: confirme se é rotina, emergencial ou programada (agenda).' })
    base.push({ type: 'switch', id: 'exigeAgendamento', label: 'Somente detalhes com agendamento/horário', default: false })
  }

  base.push({ type: 'switch', id: 'viaPublica', label: 'Problema em via pública / calçada', default: false })
  base.push({ type: 'switch', id: 'temRisco', label: 'Existe risco (trânsito/queda/obstrução/segurança)?', default: false })

  return base
}

function optionsFromInstructions(instrucoes) {
  // extrai switches em um formato uniforme pro front
  const switches = (instrucoes || [])
    .filter(i => i.type === 'switch')
    .map(i => ({
      id: i.id,
      label: i.label,
      default: Boolean(i.default)
    }))

  return switches
}

export const WizardPopService = {
  getEtapaCategoria({ termo = '' } = {}) {
    const instrucoes = instrucoesEtapaCategoria(termo)
    let opcoes = [...CATEGORIAS]
    if (termo) opcoes = opcoes.filter(c => includesAllTokens(c, termo))

    return {
      etapa: 'categoria',
      instrucoes,
      filtros: optionsFromInstructions(instrucoes),
      opcoes
    }
  },

  getEtapaServico({ categoria = '', termo = '', filtros = {} } = {}) {
    const instrucoes = instrucoesEtapaServico({ categoria, termo })
    const all = SERVICOS_BY_CATEGORIA[categoria] || []
    const filteredBySwitch = applySwitchFiltersToServicos(all, filtros)
    const lista = termo
      ? filteredBySwitch.filter(d => includesAllTokens(`${d} ${(TAGS_DETALHE[d] || []).join(' ')}`, termo))
      : filteredBySwitch

    // Agora o "detalhamento" já vem junto em cada item (contrato back-end)
    const opcoes = lista.map(d => ({
      title: d,
      value: d,
      detalhamento: DETALHAMENTO_BY_DETALHE[d] || `Detalhamento padrão para: ${d}.`
    }))

    return {
      etapa: 'servico',
      instrucoes,
      filtros: optionsFromInstructions(instrucoes),
      opcoes
    }
  },

  getEtapaDetalhe({ categoria = '', servico = '', termo = '', filtros = {} } = {}) {
    const instrucoes = instrucoesEtapaDetalhe({ categoria, servico, termo })
    const all = DETALHES_BY_SERVICO[servico] || []
    const filteredBySwitch = applySwitchFiltersToDetalhes(all, filtros)

    // lista final (já considerando busca por termo)
    const lista = termo
      ? filteredBySwitch.filter(d =>
        includesAllTokens(`${d} ${(TAGS_DETALHE[d] || []).join(' ')}`, termo)
      )
      : filteredBySwitch

    // agora o "detalhamento" vem junto (contrato do back-end)
    const opcoes = lista.map(d => ({
      title: d,
      value: d,
      detalhamento: DETALHAMENTO_BY_DETALHE[d] || `Detalhamento padrão para: ${d}.`
    }))

    return {
      etapa: 'detalhe',
      instrucoes,
      filtros: optionsFromInstructions(instrucoes),
      opcoes
    }
  }

}
