<script setup>
import { ref, inject, watch, computed } from "vue";
import { useRoute, useRouter } from "vue-router";

import EquipamentoService from "@/services/configuracoes/equipamento-service.js";
import EstacaoService from "@/services/configuracoes/estacao-service.js";
import ServicoSolicitadoService from "@/services/configuracoes/servico-solicitado-service";
import Grid from '@/components/common/GridComponent.vue'

import Loading from "@/components/base/LoadingOverlay.vue";
import BaseButton from "@/components/base/BaseButton.vue";
import Snackbar from "@/components/base/Snackbar.vue";
import { usePermissoesTela } from "@/composables/usePermissoesTela";

import DocumentoTab from "@/views/configuracoes/Documento.vue";

const endpoint = inject("endpoint");
const headerPadrao = inject("headerPadrao");
const chaveSeguranca = inject("chaveSeguranca");
const usuarioSeguranca = inject("usuarioSeguranca");

const equipamentoService = new EquipamentoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);
const estacaoService = new EstacaoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);

const servicoSolicitadoService = new ServicoSolicitadoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);

const servicosOptions = ref([]);
const servicosSelecionadosRegra = ref([]);
const menuServicosRegra = ref(false);

const route = useRoute();
const router = useRouter();

const { hasPermission } = usePermissoesTela();

const loading = ref(false);
const retorno = ref(false);
const mensagemRetorno = ref(null);
const sucesso = ref(true);
const tabEquip = ref('equipamento');
const gridComponent = ref(null);
const gridRegraPreventiva = ref(null);

const carregandoEdicao = ref(false);

const formulario = ref(null);

const listaEstacoes = ref([]);
const listaTipoEquipamento = ref([]);
const listaEquipamentosPrincipais = ref([]);

const modalConfirmarVisualizacao = ref(false);
const componenteSelecionado = ref(null);

// enum (ajuste se seu enum for diferente)
const tiposValorCaracteristica = [
  { title: "Texto", value: 0 },
  { title: "Decimal", value: 1 },
  { title: "Inteiro", value: 2 },
  { title: "Booleano", value: 3 },
  { title: "Data", value: 4 },
];

const opcoesBooleano = [
  { title: "Sim", value: true },
  { title: "Não", value: false },
];

// Filds do gride de componentes
const componentFields = ref([
  {
    descricao: "Nome",
    valor: "nome",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
    desabilitarOrdenacao: true,
  },
  {
    descricao: "Tag",
    valor: "tag",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
    desabilitarOrdenacao: true,
  },
])

// Filds do gride de regras preventivas
const regraPreventivaFields = ref([
  {
    descricao: "Nome",
    valor: "nome",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
    desabilitarOrdenacao: true,
  },
  {
    descricao: "Periodicidade",
    valor: "intervaloDescricao",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
    desabilitarOrdenacao: true,
  },
  {
    descricao: "Início",
    valor: "dataInicioFormatada",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
    desabilitarOrdenacao: true,
  },
  {
    descricao: "Próximo Processamento",
    valor: "proximoProcessamentoFormatada",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
    desabilitarOrdenacao: true,
  },
  {
    descricao: "Status",
    valor: "ativo",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "switch",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: 'Ações', valor: 'ellipsis', tipo: 'menu', filtravel: false, ordenado: null, class: 'text-left',
    opcoesMenu: [
      { descricao: 'Editar', icone: 'pen-to-square', classe: 'text-left' },
      { descricao: 'Deletar', icone: 'trash', classe: 'text-left' },
    ]
  }
])

const customButtonsList = ref([
  {
    function: "inserir",
    customButtonIcon: "plus",
    customButtonDescription: "Nova regra",
    color: "primary",
  }
]);

async function customButtonClick(button) {
  if (button.function === "inserir") {
    novaRegraPreventiva()
  }
}

async function handleOptionClick({ item, opcao }) {
  if (opcao.descricao === 'Editar') {
    editarRegraPreventiva(item)
  } else if (opcao.descricao === 'Deletar') {
    await deletarRegraPreventiva(item)
  }
}

const regrasPreventivas = ref([]);
const modalRegraPreventiva = ref(false);
const carregandoRegrasPreventivas = ref(false);

const regraPreventivaEdicao = ref({
  id: null,
  equipamentoId: null,
  nome: "",
  descricao: "",
  intervalo: 1,
  unidadePeriodo: 1,
  dataInicio: null,
  prioridade: 1,
  ativo: true
});

const regrasPreventivasGrid = computed(() =>
  (regrasPreventivas.value || []).map(x => ({
    ...x,
    intervaloDescricao: `${x.intervalo} ${descricaoUnidadePeriodo(x.unidadePeriodo)}`,
    dataInicioFormatada: x.dataInicio ? new Date(x.dataInicio).toLocaleDateString("pt-BR") : "",
    proximoProcessamentoFormatada: x.proximoProcessamento ? new Date(x.proximoProcessamento).toLocaleDateString("pt-BR") : ""
  }))
);

function descricaoUnidadePeriodo(unidade) {
  switch (unidade) {
    case "Dia": return "dia(s)";
    case "Semana": return "semana(s)";
    case "Mes": return "mês(es)";
    case "Ano": return "ano(s)";
    default: return "";
  }
}

async function listarRegrasPreventivas() {
  if (!item.value?.id) {
    regrasPreventivas.value = [];
    return;
  }

  carregandoRegrasPreventivas.value = true;
  const result = await equipamentoService.listarRegrasPreventivas(item.value.id);
  carregandoRegrasPreventivas.value = false;

  if (result?.statusCode === 200) {
    regrasPreventivas.value = (result?.data?.data || []).map(x => ({
      ...x,
      intervaloDescricao: `${x.intervalo} ${descricaoUnidadePeriodo(x.unidadePeriodo)}`,
      dataInicioFormatada: x.dataInicio ? new Date(x.dataInicio).toLocaleDateString("pt-BR") : "",
      proximoProcessamentoFormatada: x.proximoProcessamento ? new Date(x.proximoProcessamento).toLocaleDateString("pt-BR") : ""
    }))
  } else {
    retorno.value = true;
    sucesso.value = false;
    mensagemRetorno.value = result?.data?.message || "Falha ao listar regras preventivas.";
  }
}

async function listarServicosSolicitados() {
  const result = await servicoSolicitadoService.buscarTodos(false);

  if (result?.statusCode === 200) {
    servicosOptions.value = result?.data?.data || [];
  } else {
    retorno.value = true;
    sucesso.value = false;
    mensagemRetorno.value = result?.data?.message || "Falha ao listar serviços solicitados.";
  }
}

listarServicosSolicitados();

function editarRegraPreventiva(regra) {
  regraPreventivaEdicao.value = {
    id: regra.id,
    equipamentoId: regra.equipamentoId,
    nome: regra.nome,
    descricao: regra.descricao,
    intervalo: regra.intervalo,
    unidadePeriodo: regra.unidadePeriodo,
    dataInicio: regra.dataInicio ? regra.dataInicio.slice(0, 10) : null,
    prioridade: regra.prioridade,
    ativo: regra.ativo
  };

  servicosSelecionadosRegra.value = (regra.servicosSolicitados || []).map(x => ({
    id: x.servicoSolicitadoId,
    codigo: x.codigo,
    descricao: x.descricao
  }));

  modalRegraPreventiva.value = true;
}

function novaRegraPreventiva() {
  if (!item.value?.id) {
    retorno.value = true;
    sucesso.value = false;
    mensagemRetorno.value = "Salve o equipamento antes de cadastrar regras preventivas.";
    return;
  }

  regraPreventivaEdicao.value = {
    id: null,
    equipamentoId: item.value.id,
    nome: "",
    descricao: "",
    intervalo: 1,
    unidadePeriodo: 1,
    dataInicio: new Date().toISOString().slice(0, 10),
    prioridade: 1,
    ativo: true
  };

  servicosSelecionadosRegra.value = [];
  modalRegraPreventiva.value = true;
}

async function salvarRegraPreventiva() {
  if (!validarRegraPreventiva()) return;

  const payload = {
    ...regraPreventivaEdicao.value,
    intervalo: Number(regraPreventivaEdicao.value.intervalo),
    dataInicio: regraPreventivaEdicao.value.dataInicio,
    servicosSolicitados: (servicosSelecionadosRegra.value || []).map(x => ({
      servicoSolicitadoId: x.id
    }))
  };

  loading.value = true;
  const result = payload.id
    ? await equipamentoService.atualizarRegraPreventiva(payload)
    : await equipamentoService.salvarRegraPreventiva(payload);

  retorno.value = true;

  if (result?.statusCode === 200) {
    sucesso.value = true;
    mensagemRetorno.value = result?.data?.message || "Regra preventiva salva com sucesso.";
    modalRegraPreventiva.value = false;
    servicosSelecionadosRegra.value = [];
  } else {
    sucesso.value = false;
    mensagemRetorno.value = result?.data?.message || "Falha ao salvar regra preventiva.";
    modalRegraPreventiva.value = false;
  }
  
  loading.value = false;
  await listarRegrasPreventivas();
}

function removerServicoRegra(id) {
  servicosSelecionadosRegra.value = servicosSelecionadosRegra.value.filter(x => x.id !== id);
}

function limparServicosSelecionadosRegra() {
  servicosSelecionadosRegra.value = [];
}

function validarRegraPreventiva() {
  if (!regraPreventivaEdicao.value.nome?.trim()) {
    mensagemRetorno.value = "Informe o nome da regra preventiva.";
    sucesso.value = false;
    retorno.value = true;
    return false;
  }

  if (!regraPreventivaEdicao.value.intervalo || Number(regraPreventivaEdicao.value.intervalo) <= 0) {
    mensagemRetorno.value = "Informe um intervalo válido.";
    sucesso.value = false;
    retorno.value = true;
    return false;
  }

  if (!regraPreventivaEdicao.value.dataInicio) {
    mensagemRetorno.value = "Informe a data de início.";
    sucesso.value = false;
    retorno.value = true;
    return false;
  }

  if (!servicosSelecionadosRegra.value.length) {
    mensagemRetorno.value = "Selecione ao menos um serviço solicitado.";
    sucesso.value = false;
    retorno.value = true;
    return false;
  }

  return true;
}

async function deletarRegraPreventiva(regra) {
  loading.value = true;
  const result = await equipamentoService.deletarRegraPreventiva(regra.id);

  retorno.value = true;

  if (result?.statusCode === 200) {
    sucesso.value = true;
    mensagemRetorno.value = result?.data?.message || "Regra preventiva removida com sucesso.";
    await listarRegrasPreventivas();
  } else {
    sucesso.value = false;
    mensagemRetorno.value = result?.data?.message || "Falha ao remover regra preventiva.";
  }

  loading.value = false;
}

function handleFocusOut(event) {
  if(gridComponent.value) gridComponent.value.handleFocusOut(event);
  if(gridRegraPreventiva.value) gridRegraPreventiva.value.handleFocusOut(event);
}

// Converte string atual para boolean (para exibir no select)
function valorParaBoolean(valor) {
  if (valor === true) return true;
  if (valor === false) return false;
  const v = (valor ?? "").toString().trim().toLowerCase();
  if (v === "true" || v === "1" || v === "sim") return true;
  if (v === "false" || v === "0" || v === "nao" || v === "não") return false;
  return null;
}

// Quando usuário escolhe Sim/Não, guarda como TEXTO em c.valor
function booleanParaTexto(valorBool) {
  if (valorBool === true) return "true";
  if (valorBool === false) return "false";
  return "";
}

// Limpa/ajusta o valor quando muda o tipo (evita ficar "abc" em campo numérico)
function onTipoValorChanged(c) {
  // se trocar para boolean e valor atual não for booleano reconhecível, limpa
  if (c.tipoValor === 3) {
    const b = valorParaBoolean(c.valor);
    c.valor = b === null ? "" : booleanParaTexto(b);
    return;
  }

  // se trocar para data, limpa se não for uma data simples (YYYY-MM-DD)
  if (c.tipoValor === 4) {
    const v = (c.valor ?? "").toString().trim();
    // mantém se já estiver em formato ISO simples
    const isoOk = /^\d{4}-\d{2}-\d{2}$/.test(v);
    c.valor = isoOk ? v : "";
    return;
  }

  // se trocar para número, limpa se não for número
  if (c.tipoValor === 1 || c.tipoValor === 2) {
    const v = (c.valor ?? "").toString().trim().replace(",", ".");
    const num = Number(v);
    c.valor = Number.isFinite(num) ? (c.tipoValor === 2 ? String(parseInt(num, 10)) : String(num)) : "";
    return;
  }
}

function isEmpty(v) {
  return v === null || v === undefined || String(v).trim() === "";
}

function normalizeNumber(v) {
  // aceita "1,23" e "1.23"
  return String(v).trim().replace(",", ".");
}

function regraDecimal(v) {
  if (isEmpty(v)) return true; // se quiser obrigar preenchimento, troque para: return 'Valor é obrigatório'
  const n = Number(normalizeNumber(v));
  return Number.isFinite(n) || "Informe um número decimal válido.";
}

function regraInteiro(v) {
  if (isEmpty(v)) return true; // se quiser obrigar preenchimento, troque para: return 'Valor é obrigatório'
  const s = String(v).trim();
  return /^-?\d+$/.test(s) || "Informe um número inteiro válido.";
}

function regraData(v) {
  if (isEmpty(v)) return true; // se quiser obrigar preenchimento, troque para: return 'Data é obrigatória'
  const s = String(v).trim();
  // no type="date" normalmente vem YYYY-MM-DD
  return /^\d{4}-\d{2}-\d{2}$/.test(s) || "Informe uma data válida.";
}

function regraBooleanoTexto(v) {
  if (isEmpty(v)) return true; // se quiser obrigar: return 'Selecione Sim ou Não'
  const s = String(v).trim().toLowerCase();
  return (s === "true" || s === "false") || "Selecione Sim ou Não.";
}


const tiposExtras = [
  { title: "Motor", value: "Motor" },
  { title: "Sensor", value: "MedidorVazao" },
  { title: "CLP", value: "CLP" },
  { title: "Bomba", value: "Bomba" },
  { title: "Nobreak", value: "Nobreak" },
];

const idParam = computed(() => route.params.id);
const inserindo = computed(() => idParam.value === "novo" || !idParam.value);
const titulo = computed(() => (inserindo.value ? "Inserir Equipamento" : "Editar Equipamento"));

const item = ref({
  id: null,
  estacaoId: null,
  tipoEquipamentoId: null,
  equipamentoPrincipalId: null,
  nome: "",
  tag: "",
  fabricante: "",
  modelo: "",
  numeroSerie: "",
  observacoes: "",
  ativo: true,
  bomba: null,
  motor: null,
  clp: null,
  nobreak: null,
  medidorVazao: null,
  caracteristicas: [],
  componentes: []
});

const isPrincipal = ref(true);

function onOcultarRetorno() {
  retorno.value = false;
}

function normalizeItemBase() {
  if (!item.value.caracteristicas) item.value.caracteristicas = [];
  if (item.value.ativo === undefined || item.value.ativo === null) item.value.ativo = true;

  item.value.componentes = (item.value.componentes || []).map(x => ({
    ...x,
    selecionado: false
  }));
}

async function listarTiposEquipamento() {
  const result = await equipamentoService.listarTiposEquipamento();
  listaTipoEquipamento.value = result?.data?.data || [];
}

async function listarEstacoes() {
  const result = await estacaoService.buscarEstacoes();
  listaEstacoes.value = result?.data?.data || [];
}

async function carregarPrincipaisDaEstacao(estacaoId) {
  listaEquipamentosPrincipais.value = [];
  if (!estacaoId) return;

  loading.value = true;
  const resp = await equipamentoService.listarEquipamentosPorEstacao(estacaoId, true);
  loading.value = false;

  if (resp?.statusCode === 200) {
    listaEquipamentosPrincipais.value = resp?.data?.data || [];
  } else {
    retorno.value = true;
    sucesso.value = false;
    mensagemRetorno.value = resp?.data?.message || "Falha ao carregar equipamentos principais.";
  }
}

function detectarExtrasNoEdit() {
  if (item.value.motor) componenteSelecionado.value = "Motor";
  else if (item.value.bomba) componenteSelecionado.value = "Bomba";
  else if (item.value.clp) componenteSelecionado.value = "CLP";
  else if (item.value.nobreak) componenteSelecionado.value = "Nobreak";
  else if (item.value.medidorVazao) componenteSelecionado.value = "MedidorVazao";
  else componenteSelecionado.value = null;
}


function limparComponentes() {
  item.value.motor = null;
  item.value.bomba = null;
  item.value.clp = null;
  item.value.nobreak = null;
  item.value.medidorVazao = null;
}

function syncComponenteComItem() {
  limparComponentes();

  const componente = componenteSelecionado.value;

  if (componente === "Motor") {
    item.value.motor = { potencia: null, tensao: null, rpm: null };
  } else if (componente === "Bomba") {
    item.value.bomba = { vazao: null, alturaManometrica: null, potencia: null };
  } else if (componente === "CLP") {
    item.value.clp = { marca: null, firmware: null };
  } else if (componente === "Nobreak") {
    item.value.nobreak = { potenciaVa: null, autonomiaMinutos: null };
  } else if (componente === "MedidorVazao") {
    item.value.medidorVazao = {
      fabricante: null,
      modeloConversor: null,
      modeloSensor: null,
      diametro: null,
      fatorK: null,
      escalaMaxima: null,
    };
  }
}

function addCaracteristica() {
  item.value.caracteristicas.push({
    id: null,
    equipamentoId: item.value.id,
    nome: "",
    valor: "",
    unidadeMedida: "",
    tipoValor: 0
  });
}

function removerCaracteristica(idx) {
  item.value.caracteristicas.splice(idx, 1);
}

async function voltar() {
  await router.push({ name: "Equipamentos" });
}

function onSelecionarComponente(itemSelecionado) {
  componenteSelecionado.value = itemSelecionado;
  modalConfirmarVisualizacao.value = true;
}

function fecharModalVisualizacao() {
  modalConfirmarVisualizacao.value = false;
  componenteSelecionado.value = null;
}

async function confirmarVisualizacao() {
  const id = componenteSelecionado.value?.id;
  if (!id) {
    modalConfirmarVisualizacao.value = false;
    return;
  }

  modalConfirmarVisualizacao.value = false;

  loading.value = true;

  await router.push({ name: "EquipamentoDetalhe", params: { id } })
  tabEquip.value = "equipamento"
  await init()
}

watch(tabEquip, () => {
  if (modalConfirmarVisualizacao.value) fecharModalVisualizacao();
});

watch(
  () => item.value.estacaoId,
  async (estacaoId) => {
    if (!estacaoId) {
      listaEquipamentosPrincipais.value = [];
      isPrincipal.value = true;
      item.value.equipamentoPrincipalId = null;
      return;
    }

    if (inserindo.value) {
      item.value.equipamentoPrincipalId = null;
    }

    await carregarPrincipaisDaEstacao(estacaoId);
  }
);

watch(isPrincipal, (val) => {
  if (val) {
    item.value.equipamentoPrincipalId = null;
    componenteSelecionado.value = null;
    limparComponentes();
  }
});


watch(componenteSelecionado, (novo) => {
  if (carregandoEdicao.value) return;

  if (!novo) {
    limparComponentes();
    return;
  }

  if (!inserindo.value) {
    if (novo === "Motor" && item.value.motor) return;
    if (novo === "Bomba" && item.value.bomba) return;
    if (novo === "CLP" && item.value.clp) return;
    if (novo === "Nobreak" && item.value.nobreak) return;
    if (novo === "MedidorVazao" && item.value.medidorVazao) return;
  }

  syncComponenteComItem();
});

watch(() => item.value.equipamentoPrincipalId, (val) => {
  if (!val && isPrincipal.value === false) {
    componenteSelecionado.value = null;
    limparComponentes();
  }
})

async function carregarEquipamentoParaEdicao(id) {
  carregandoEdicao.value = true;
  loading.value = true;

  const resp = await equipamentoService.selecionarCompleto(id);
  loading.value = false;

  if (resp?.statusCode === 200) {
    item.value = resp?.data?.data || resp?.data || item.value;
    normalizeItemBase();
    isPrincipal.value = item.value.equipamentoPrincipalId == null;
    await carregarPrincipaisDaEstacao(item.value.estacaoId);

    detectarExtrasNoEdit();
    await listarRegrasPreventivas();
  } else {
    retorno.value = true;
    sucesso.value = false;
    mensagemRetorno.value = resp?.data?.message || "Falha ao carregar equipamento.";
  }

  carregandoEdicao.value = false;
}

async function salvarOuAtualizar() {
  const validation = await formulario.value.validate();
  if (!validation.valid) return;

  const request = JSON.parse(JSON.stringify(item.value));

  // regra: principal -> EquipamentoPrincipalId null
  if (isPrincipal.value) request.equipamentoPrincipalId = null;

  loading.value = true;
  const resp = inserindo.value
    ? await equipamentoService.salvar(request)
    : await equipamentoService.atualizar(request);
  loading.value = false;

  retorno.value = true;

  if (resp?.statusCode === 200) {
    sucesso.value = true;
    mensagemRetorno.value = resp?.data?.message || "Salvo com sucesso.";
  } else {
    sucesso.value = false;
    mensagemRetorno.value = resp?.data?.message || "Falha ao salvar.";
  }
}

async function cancelar() {
  await router.push({ name: "Equipamentos" });
}

async function init() {
  await Promise.all([listarTiposEquipamento(), listarEstacoes()]);

  if (!inserindo.value && idParam.value) {
    await carregarEquipamentoParaEdicao(idParam.value);
  } else {
    // inserção
    normalizeItemBase();
    isPrincipal.value = true;
    componenteSelecionado.value = null;
  }
}

init();
</script>

<template>
  <v-container fluid class="equip-page" @click="handleFocusOut($event)">
    <Loading :active="loading" v-if="loading" />

    <!-- Header -->
    <div class="page-header d-flex align-center">
      <div>
        <v-tooltip location="bottom" text="Voltar">
          <template #activator="{ props: activatorProps }">
            <v-btn
              size="small"
              v-bind="activatorProps"
              icon="arrow-left"
              class="text-primary mr-2 mb-2"
              @click="voltar"
            />
          </template>
        </v-tooltip>
      </div>

      <font-awesome-icon :icon="inserindo ? 'plus' : 'pen-to-square'" class="text-primary mr-1 fa-lg" />
      <div class="page-title">{{ titulo }}</div>

      <v-spacer />
    </div>

    <v-divider class="mb-3" />

    <v-form ref="formulario">
      <v-card class="main-card" variant="outlined">
        <v-card-text class="card-body">
          <!-- Tabs -->
          <v-tabs v-model="tabEquip" color="primary" density="compact">
            <v-tab value="equipamento">Equipamento</v-tab>
            <v-tab value="componentes" v-if="isPrincipal && !inserindo.value" :disabled="isPrincipal === false">Componentes</v-tab>
            <v-tab value="regrasPreventivas" :disabled="inserindo.value">Regras Preventivas</v-tab>
            <v-tab value="arquivos" :disabled="inserindo.value">Arquivos</v-tab>
          </v-tabs>

          <v-divider class="my-3" />

          <v-tabs-window v-model="tabEquip">
            <!-- ===================== -->
            <!-- TAB: EQUIPAMENTO -->
            <!-- ===================== -->
            <v-tabs-window-item value="equipamento">
              <!-- ===== Estrutura ===== -->
              <div class="section-header">
                <span>Estrutura</span>
              </div>

              <v-row dense class="row-tight">
                <v-col cols="12" md="6" class="col-tight">
                  <v-select
                    :items="listaEstacoes"
                    v-model="item.estacaoId"
                    item-title="nome"
                    item-value="id"
                    label="Estação"
                    clearable
                    density="compact"
                    variant="outlined"
                    :rules="[(v) => !!v || 'Estação é obrigatório']"
                  />
                </v-col>

                <v-col cols="12" md="6" class="col-tight" v-if="item.estacaoId">
                  <v-select
                    :items="[
                      { title: 'Sim', value: true },
                      { title: 'Não', value: false }
                    ]"
                    v-model="isPrincipal"
                    item-title="title"
                    item-value="value"
                    label="É um equipamento principal?"
                    density="compact"
                    variant="outlined"
                  />
                </v-col>

                <v-col cols="12" class="col-tight" v-if="item.estacaoId && isPrincipal === false">
                  <v-autocomplete
                    :items="listaEquipamentosPrincipais"
                    v-model="item.equipamentoPrincipalId"
                    item-title="nome"
                    item-value="id"
                    label="Vínculo a Equipamento Principal"
                    density="compact"
                    variant="outlined"
                    clearable
                    :rules="[(v) => !!v || 'Selecione o equipamento principal']"
                  >
                    <template #item="{ props, item: opt }">
                      <v-list-item
                        v-bind="props"
                        :title="opt?.raw?.nome"
                        :subtitle="opt?.raw?.tag ? `Tag: ${opt.raw.tag}` : ''"
                      />
                    </template>
                  </v-autocomplete>
                </v-col>
              </v-row>
              
              <!-- ===== Especializadas ===== -->
              <div v-if="isPrincipal === false" class="section-block">
                <div class="section-header">
                  <span>Informações especializadas</span>
                </div>

                <v-row dense class="row-tight">
                  <v-col cols="12" class="col-tight">
                    <v-select
                      v-model="componenteSelecionado"
                      :items="tiposExtras"
                      item-title="title"
                      item-value="value"
                      label="Selecione a especificação"
                      single-line
                      density="compact"
                      variant="outlined"
                      clearable
                      :disabled="!item.equipamentoPrincipalId"
                      :hint="!item.equipamentoPrincipalId ? 'Selecione o equipamento principal para habilitar componentes.' : ''"
                      persistent-hint
                    />
                  </v-col>
                </v-row>

                <!-- Cards compactos -->
                <v-card variant="outlined" class="sub-card" v-if="item.motor">
                  <div class="sub-card-title">Motor</div>
                  <v-row dense class="row-tight">
                    <v-col cols="12" md="4" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.motor.potencia" label="Potência" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="4" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.motor.tensao" label="Tensão" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="4" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.motor.rpm" label="RPM" density="compact" variant="outlined" />
                    </v-col>
                  </v-row>
                </v-card>

                <v-card variant="outlined" class="sub-card" v-if="item.bomba">
                  <div class="sub-card-title">Bomba</div>
                  <v-row dense class="row-tight">
                    <v-col cols="12" md="4" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.bomba.vazao" label="Vazão" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="4" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.bomba.alturaManometrica" label="Altura Manométrica" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="4" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.bomba.potencia" label="Potência" density="compact" variant="outlined" />
                    </v-col>
                  </v-row>
                </v-card>

                <v-card variant="outlined" class="sub-card" v-if="item.clp">
                  <div class="sub-card-title">CLP</div>
                  <v-row dense class="row-tight">
                    <v-col cols="12" md="6" class="col-tight">
                      <v-text-field v-model="item.clp.marca" label="Marca" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="6" class="col-tight">
                      <v-text-field v-model="item.clp.firmware" label="Firmware" density="compact" variant="outlined" />
                    </v-col>
                  </v-row>
                </v-card>

                <v-card variant="outlined" class="sub-card" v-if="item.nobreak">
                  <div class="sub-card-title">Nobreak</div>
                  <v-row dense class="row-tight">
                    <v-col cols="12" md="6" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.nobreak.potenciaVa" label="Potência (VA)" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="6" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.nobreak.autonomiaMinutos" label="Autonomia (min)" density="compact" variant="outlined" />
                    </v-col>
                  </v-row>
                </v-card>

                <v-card variant="outlined" class="sub-card" v-if="item.medidorVazao">
                  <div class="sub-card-title">Sensor (Medidor de Vazão)</div>
                  <v-row dense class="row-tight">
                    <v-col cols="12" md="4" class="col-tight">
                      <v-text-field v-model="item.medidorVazao.modeloConversor" label="Modelo Conversor" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="4" class="col-tight">
                      <v-text-field v-model="item.medidorVazao.modeloSensor" label="Modelo Sensor" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="4" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.medidorVazao.diametro" label="Diâmetro" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="4" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.medidorVazao.fatorK" label="Fator K" density="compact" variant="outlined" />
                    </v-col>
                    <v-col cols="12" md="4" class="col-tight">
                      <v-number-input control-variant="hidden" v-model="item.medidorVazao.escalaMaxima" label="Escala Máxima" density="compact" variant="outlined" />
                    </v-col>
                  </v-row>
                </v-card>
              </div>

              <!-- ===== Identificação ===== -->
              <div class="section-block">
                <div class="section-header">
                  <span>Identificação</span>
                </div>

                <v-row dense class="row-tight">
                  <v-col cols="12" md="6" class="col-tight">
                    <v-text-field
                      v-model="item.nome"
                      label="Nome"
                      clearable
                      density="compact"
                      variant="outlined"
                      :rules="[(v) => !!v || 'Nome é obrigatório']"
                    />
                  </v-col>

                  <v-col cols="12" md="6" class="col-tight">
                    <v-text-field
                      v-model="item.tag"
                      label="Tag"
                      clearable
                      density="compact"
                      variant="outlined"
                      :rules="[(v) => !!v || 'Tag é obrigatório']"
                    />
                  </v-col>
                </v-row>
              </div>

              <!-- ===== Técnicos ===== -->
              <div class="section-block">
                <div class="section-header">
                  <span>Dados técnicos</span>
                </div>

                <v-row dense class="row-tight">
                  <v-col cols="12" md="4" class="col-tight">
                    <v-text-field v-model="item.fabricante" label="Fabricante" clearable density="compact" variant="outlined" />
                  </v-col>

                  <v-col cols="12" md="4" class="col-tight">
                    <v-text-field v-model="item.modelo" label="Modelo" clearable density="compact" variant="outlined" />
                  </v-col>

                  <v-col cols="12" md="4" class="col-tight">
                    <v-text-field v-model="item.numeroSerie" label="Número de Série" clearable density="compact" variant="outlined" />
                  </v-col>
                </v-row>
              </div>

              <!-- ===== Classificação ===== -->
              <div class="section-block">
                <div class="section-header">
                  <span>Classificação</span>
                </div>

                <v-row dense class="row-tight">
                  <v-col cols="12" md="6" class="col-tight">
                    <v-select
                      :items="listaTipoEquipamento"
                      v-model="item.tipoEquipamentoId"
                      item-title="nome"
                      item-value="id"
                      label="Tipo Equipamento"
                      single-line
                      clearable
                      density="compact"
                      variant="outlined"
                      :rules="[(v) => !!v || 'Tipo equipamento é obrigatório']"
                    />
                  </v-col>
                </v-row>
              </div>

              <!-- ===== Observações ===== -->
              <div class="section-block">
                <div class="section-header">
                  <span>Observações</span>
                </div>

                <v-row dense class="row-tight">
                  <v-col cols="12" class="col-tight">
                    <v-textarea
                      v-model="item.observacoes"
                      label="Observações"
                      rows="2"
                      auto-grow
                      density="compact"
                      variant="outlined"
                    />
                  </v-col>
                </v-row>
              </div>

              <!-- ===== Características ===== -->
              <div class="section-block">
                <div class="d-flex align-center mb-1">
                  <div class="section-header no-line">
                    <span>Características específicas</span>
                  </div>
                  <v-spacer />
                  <BaseButton label="Adicionar" type="save" iconPosition="left" @click="addCaracteristica()" />
                </div>

                <v-card variant="outlined" class="pa-2" v-if="item.caracteristicas?.length">
                  <div v-for="(c, idx) in item.caracteristicas" :key="idx" class="mb-2">
                    <v-row dense class="row-tight">
                      <v-col cols="12" md="4" class="col-tight">
                        <v-text-field v-model="c.nome" label="Nome" density="compact" variant="outlined" />
                      </v-col>

                      <v-col cols="12" md="3" class="col-tight">
                        <v-select
                          v-model="c.tipoValor"
                          :items="tiposValorCaracteristica"
                          item-title="title"
                          item-value="value"
                          label="Tipo"
                          density="compact"
                          variant="outlined"
                        />
                      </v-col>

                      <v-col cols="12" md="3" class="col-tight">
                        <v-text-field v-model="c.unidadeMedida" label="Unidade Medida" density="compact" variant="outlined" />
                      </v-col>

                      <v-col cols="12" md="2" class="col-tight d-flex justify-end align-top">
                        <BaseButton label="Remover" type="cancel" iconPosition="left" @click="removerCaracteristica(idx)" />
                      </v-col>

                      <v-col cols="12" class="col-tight">
                        <!-- Valor (dinâmico conforme Tipo) -->
                        <v-text-field v-if="c.tipoValor === 0" v-model="c.valor" label="Valor" density="compact" variant="outlined" />

                        <v-text-field
                          v-else-if="c.tipoValor === 1"
                          v-model="c.valor"
                          label="Valor (decimal)"
                          density="compact"
                          variant="outlined"
                          inputmode="decimal"
                          type="text"
                          :rules="[regraDecimal]"
                          @blur="() => onTipoValorChanged(c)"
                          @input="(e) => (c.valor = (e?.target?.value || '').replace(/[^\d.,-]/g, ''))"
                        />

                        <v-text-field
                          v-else-if="c.tipoValor === 2"
                          v-model="c.valor"
                          label="Valor (inteiro)"
                          density="compact"
                          variant="outlined"
                          inputmode="numeric"
                          type="text"
                          :rules="[regraInteiro]"
                          @blur="() => onTipoValorChanged(c)"
                          @input="(e) => (c.valor = (e?.target?.value || '').replace(/[^\d-]/g, ''))"
                        />

                        <v-select
                          v-else-if="c.tipoValor === 3"
                          :model-value="valorParaBoolean(c.valor)"
                          @update:modelValue="(v) => (c.valor = booleanParaTexto(v))"
                          :items="opcoesBooleano"
                          item-title="title"
                          item-value="value"
                          label="Valor"
                          density="compact"
                          variant="outlined"
                          clearable
                          :rules="[() => regraBooleanoTexto(c.valor)]"
                        />

                        <v-text-field
                          v-else-if="c.tipoValor === 4"
                          v-model="c.valor"
                          label="Data"
                          density="compact"
                          variant="outlined"
                          type="date"
                          :rules="[regraData]"
                          @blur="() => onTipoValorChanged(c)"
                        />

                        <v-text-field v-else v-model="c.valor" label="Valor" density="compact" variant="outlined" />
                      </v-col>
                    </v-row>

                    <v-divider class="my-2" />
                  </div>
                </v-card>

                <div v-else class="text-caption text-grey">
                  Nenhuma característica adicionada.
                </div>
              </div>

              <!-- Botões FORA das abas -->
              <v-card-actions class="card-actions">
                <v-spacer />
                <BaseButton label="Cancelar" type="cancel" iconPosition="left" @click="cancelar" />
                <BaseButton
                  v-if="(inserindo && hasPermission('Criar')) || (!inserindo && hasPermission('Editar'))"
                  label="Salvar"
                  type="save"
                  iconPosition="left"
                  @click="salvarOuAtualizar"
                />
              </v-card-actions>
            </v-tabs-window-item>

            <!-- ===================== -->
            <!-- TAB: COMPONENTES -->
            <!-- ===================== -->
            <v-tabs-window-item value="componentes" class="tab-body">
              <div class="app-container">
                <Grid class="grid-component" 
                ref="gridComponent" 
                :fields="componentFields" 
                :list="item.componentes"
                :fitParent="true"
                :filters="{}" 
                :hideFilters="true"
                filterType="popup" gridOverflow="horizontal"
                @listarItens="() => { }" @selecionarItem="onSelecionarComponente" @botaoClick="() => { }"
                @abrirFiltro="() => { }" :hasCheckbox="false" :customButtonsList="[]"
                @customButtonClick="() => { }" @alterarOrdenacao="() => { }"
                @alterarStatus="() => { }" />
              </div>
            </v-tabs-window-item>
            
            <!-- ===================== -->
            <!-- TAB: REGRAS PREVENTIVAS -->
            <!-- ===================== -->
            <v-tabs-window-item value="regrasPreventivas">
              <div class="app-container">
                <Grid class="grid-regra-preventiva" 
                  ref="gridRegraPreventiva" 
                  :fields="regraPreventivaFields" 
                  :list="regrasPreventivasGrid"
                  :fitParent="true"
                  :filters="{}" 
                  :hideFilters="true"
                  filterType="popup" gridOverflow="horizontal"
                  @listarItens="() => { }" @selecionarItem="editarRegraPreventiva" @botaoClick="() => { }"
                  @abrirFiltro="() => { }" :hasCheckbox="false" :customButtonsList="customButtonsList"
                  @customButtonClick="customButtonClick($event)" @alterarOrdenacao="() => { }" @botaoOpcaoClick="handleOptionClick"
                  @alterarStatus="() => { }" />
              </div>
            </v-tabs-window-item>

            <!-- ===================== -->
            <!-- TAB: ARQUIVOS -->
            <!-- ===================== -->
            <v-tabs-window-item value="arquivos">
              <div class="section-block">
                <div class="section-header">
                  <span>Arquivos</span>
                </div>

                <v-card variant="outlined" class="pa-3">
                  <DocumentoTab
                    v-model:loading="loading"
                    :entidadeId="item.id"
                    :disabled="carregandoEdicao"
                    tipoEntidade="EQUIPAMENTO"
                  />
                </v-card>
              </div>
            </v-tabs-window-item>
          </v-tabs-window>

          <v-dialog v-model="modalConfirmarVisualizacao" max-width="520">
            <v-card>
              <v-card-text class="pa-5">
                <div class="d-flex align-center mb-3">
                  <v-spacer />
                  <font-awesome-icon
                    icon="xmark"
                    class="text-grey cursor-pointer"
                    @click="fecharModalVisualizacao"
                  />
                </div>

                <div class="text-body-1">
                  Deseja visualizar o componente
                  <b>{{ componenteSelecionado?.nome }}</b> - <b>{{ componenteSelecionado?.tag }}</b>?
                </div>
              </v-card-text>

              <v-card-actions class="pa-4 pt-0">
                <v-spacer />
                <BaseButton label="Não" type="cancel" iconPosition="left" @click="fecharModalVisualizacao" />
                <BaseButton label="Sim" type="save" iconPosition="left" @click="confirmarVisualizacao" />
              </v-card-actions>
            </v-card>
          </v-dialog>

          <v-dialog v-model="modalRegraPreventiva" max-width="700">
            <v-card>
              <v-card-title>
                <font-awesome-icon :icon="regraPreventivaEdicao.id ? 'pen-to-square' : 'plus'" class="text-primary mr-1" />
                {{ regraPreventivaEdicao.id ? "Editar regra preventiva" : "Nova regra preventiva" }}
              </v-card-title>

              <v-card-text>
                <v-row>
                  <v-col cols="12" md="4">
                    <v-text-field v-model="regraPreventivaEdicao.nome" label="Nome" variant="outlined" />
                  </v-col>

                  <v-col cols="12" md="4">
                    <v-text-field
                      v-model="regraPreventivaEdicao.dataInicio"
                      type="date"
                      label="Data início"
                      variant="outlined"
                    />
                  </v-col>
                </v-row>

                <v-row>
                  <v-col cols="12" md="4">
                    <v-text-field
                      v-model="regraPreventivaEdicao.intervalo"
                      type="number"
                      min="1"
                      label="Intervalo"
                      variant="outlined"
                    />
                  </v-col>

                  <v-col cols="12" md="4">
                    <v-select
                      v-model="regraPreventivaEdicao.unidadePeriodo"
                      :items="[
                        { title: 'Dia(s)', value: 0 },
                        { title: 'Semana(s)', value: 1 },
                        { title: 'Mês(es)', value: 2 },
                        { title: 'Ano(s)', value: 3 }
                      ]"
                      item-title="title"
                      item-value="value"
                      label="Unidade do período"
                      variant="outlined"
                      density="comfortable"
                    />
                  </v-col>

                  <v-col cols="12" md="4">
                    <v-select
                      v-model="regraPreventivaEdicao.prioridade"
                      :items="[
                        { title: 'Baixa', value: 0 },
                        { title: 'Média', value: 1 },
                        { title: 'Alta', value: 2 },
                        { title: 'Crítica', value: 3 }
                      ]"
                      item-title="title"
                      item-value="value"
                      label="Prioridade"
                      variant="outlined"
                      density="comfortable"
                    />
                  </v-col>
                </v-row>

                <v-row>
                  <v-col cols="12">
                    <v-autocomplete
                      v-model="servicosSelecionadosRegra"
                      v-model:menu="menuServicosRegra"
                      :items="servicosOptions"
                      label="Serviços Solicitados *"
                      multiple
                      return-object
                      chips
                      closable-chips
                      clearable
                      hide-selected
                      variant="outlined"
                      item-title="descricao"
                      item-value="id"
                      no-data-text="Nenhum serviço encontrado"
                    >
                      <template #chip="{ props, item }">
                        <v-chip v-bind="props" size="small" class="me-1">
                          <font-awesome-icon icon="wrench" class="me-2" />
                          {{ item?.raw?.codigo }}
                        </v-chip>
                      </template>
                    </v-autocomplete>
                  </v-col>
                </v-row>

                <v-row v-if="servicosSelecionadosRegra?.length">
                  <v-col cols="12">
                    <v-card variant="elevated" elevation="0" border rounded="lg">
                      <v-card-title class="d-flex align-center">
                        <font-awesome-icon icon="list-check" class="me-2 text-primary" />
                        Serviços selecionados
                        <v-spacer />
                        <v-chip size="small" variant="flat">{{ servicosSelecionadosRegra.length }}</v-chip>
                      </v-card-title>

                      <v-divider />

                      <v-card-text class="pa-0">
                        <v-list density="compact">
                          <v-list-item v-for="s in servicosSelecionadosRegra" :key="s.id">
                            <v-list-item-title class="text-body-2">
                              {{ s.descricao }}
                            </v-list-item-title>

                            <template #append>
                              <v-tooltip text="Remover serviço" location="top">
                                <template #activator="{ props }">
                                  <v-btn
                                    v-bind="props"
                                    icon
                                    variant="text"
                                    size="small"
                                    @click="removerServicoRegra(s.id)"
                                  >
                                    <font-awesome-icon icon="trash" />
                                  </v-btn>
                                </template>
                              </v-tooltip>
                            </template>
                          </v-list-item>
                        </v-list>
                      </v-card-text>

                      <v-divider />
                      <v-card-actions class="justify-end">
                        <v-btn variant="text" @click="limparServicosSelecionadosRegra">Limpar seleção</v-btn>
                      </v-card-actions>
                    </v-card>
                  </v-col>
                </v-row>
              </v-card-text>

              <v-card-actions>
                <v-spacer />
                <BaseButton label="Cancelar" type="cancel" iconPosition="left" @click="modalRegraPreventiva = false" />
                <BaseButton label="Salvar" type="save" iconPosition="left" @click="salvarRegraPreventiva" />
              </v-card-actions>
            </v-card>
          </v-dialog>

        </v-card-text>
      </v-card>
    </v-form>

    <Snackbar
      :retorno="retorno"
      :timeout="3000"
      :tipo="sucesso ? 'sucesso' : 'erro'"
      :mensagemRetorno="mensagemRetorno"
      @ocultarRetorno="onOcultarRetorno()"
    />
  </v-container>
</template>

<style scoped>
.equip-page {
  padding-top: 8px;
}

.page-header {
  min-height: 36px;
  gap: 6px;
}

.page-title {
  font-size: 18px;
  font-weight: 600;
  line-height: 1.1;
}

.main-card {
  border-radius: 10px;
}

.card-body {
  padding: 12px 14px;
}

/* Seções mais compactas e alinhadas */
.section-block {
  margin-top: 10px;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 10px;
  font-weight: 600;
  color: rgba(0, 0, 0, 0.72);
  margin: 6px 0 8px 0;
}

.section-header::after {
  content: "";
  flex: 1;
  height: 1px;
  background: rgba(0, 0, 0, 0.08);
}

.section-header.no-line::after {
  display: none;
}

/* Rows/Cols mais apertadas */
.row-tight {
  margin-top: 0;
  margin-bottom: 0;
}

.col-tight {
  padding-top: 4px !important;
  padding-bottom: 4px !important;
}

/* Cards internos compactos */
.sub-card {
  padding: 10px;
  margin-top: 10px;
  margin-bottom: 10px;
  border-radius: 10px;
}

.sub-card-title {
  font-weight: 600;
  margin-bottom: 6px;
  color: rgba(0, 0, 0, 0.72);
}

.card-actions {
  padding: 10px 14px 12px;
}

.tab-body {
  height: 100%;
  min-height: 0;
}

.app-container {
  height: 100%;
  min-height: 0;
  display: flex;
  flex-direction: column;
}
</style>
