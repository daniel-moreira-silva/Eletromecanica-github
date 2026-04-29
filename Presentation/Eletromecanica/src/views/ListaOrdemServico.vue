<script setup>
/* global defineProps, defineEmits */
import { ref, computed, watch, inject, onMounted } from 'vue'
import Grid from '@/components/common/GridComponent.vue'
import Paginacao from '@/components/common/PaginacaoComponent.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useRouter } from 'vue-router'
import OrdemServicoService from '@/services/ordem-servico/ordem-servico-service'
import MotivoCancelamentoService from '@/services/configuracoes/motivo-cancelamento-service'
import FuncionarioService from '@/services/configuracoes/funcionario-service'
import Loading from '@/components/base/LoadingOverlay.vue'
import Snackbar from '@/components/base/Snackbar.vue'
import OrdemServicoDocumentos from '@/views/ordem-servico/OrdemServicoDocumentos.vue'

const endpoint = inject('endpoint')
const headerPadrao = inject('headerPadrao')
const chaveSeguranca = inject('chaveSeguranca')
const usuarioSeguranca = inject('usuarioSeguranca')

const ordemServicoService = new OrdemServicoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
)

const motivoCancelamentoService = new MotivoCancelamentoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
)

const funcionarioService = new FuncionarioService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
)

const props = defineProps({
  status: { type: String, required: false },
  statusOrdemServicoId: { type: String, required: false },
  count: { type: Number, required: false },
  filtrosGlobais: { type: Object, required: false, default: () => ({}) },
  tabState: { type: Object, required: false, default: null },
  readonly: { type: Boolean, required: false },
})

const emit = defineEmits([
  'carregarTab',
  'atualizarTabState',
  'atualizarPaginacaoTab',
  'aplicarFiltrosGlobais',
  'atualizarResumo',
  'recarregarTabs',
])

// ── local state ──────────────────────────────────────────────────────────────
const loading = ref(false)
const loadingAcao = ref(false)
const modalFilter = ref(false)
const mensagemRetorno = ref(null)
const sucesso = ref(true)
const retorno = ref(null)

// filter modal inputs
const filtrosLocal = ref({ numero: null, todos: null, ordenarPor: 'Codigo', ordem: 'Asc' })

// iniciar dialog
const iniciarDialog = ref(false)
const iniciarEmLote = ref(false)
const ocorrenciasSelecionadasLote = ref([])
const sendConfirmDialog = ref(false)
const confirmDialog = ref(false)
const servicosSolicitados = ref([])

// devolver dialog
const devolverDialog = ref(false)
const devolverSelecionados = ref([])
const devolucaoObservacao = ref('')

// cancelar dialog
const cancelDialog = ref(false)
const cancelamentoMotivoId = ref(null)
const cancelamentoObservacao = ref('')
const listaMotivosCancelamento = ref([])

// documentos dialog
const documentosDialog = ref(false)
const ordemServicoDocumentosAtual = ref(null)

// despacho programado dialog
const despachoDialog = ref(false)
const despachoEmLote = ref(false)
const despachoFuncionarioId = ref(null)
const despachoDataHora = ref(null)
const despachoSelecionados = ref([])
const searchDespachoFuncionario = ref('')

const currentItem = ref({ id: '', codigo: '', endereco: '', servico: '' })
const confirmationNumber = ref('')

// equipe / funcionários
const selectedFuncionarioId = ref(null)
const listaFuncionarios = ref([])
const searchFuncionario = ref('')

// ── derived from tabState ─────────────────────────────────────────────────────
const lista = computed(() => props.tabState?.lista || [])
const totalPaginas = computed(() => props.tabState?.totalPaginas || 0)
const totalItens = computed(() => props.tabState?.totalItens || 0)
const paginaAtual = computed(() => props.tabState?.pagina || 1)
const itensPagina = computed(() => props.tabState?.itensPagina || 10)

// ── grid setup ────────────────────────────────────────────────────────────────
const selectedCount = computed(() => lista.value.filter(i => i.selecionado).length)
const selectedBatchCount = computed(() => (ocorrenciasSelecionadasLote.value || []).length)


const canSend = computed(() => !!selectedFuncionarioId.value)

// ── custom buttons (toolbar) ──────────────────────────────────────────────────
const customButtonsList = computed(() => {
  const disabled = selectedCount.value === 0
  let opcoesMenu = []

  if (props.status === 'Solicitadas') {
    opcoesMenu = [
      { descricao: 'Enviar para funcionario', icone: 'paper-plane', type: 'send' },
      { descricao: 'Programar despacho', icone: 'calendar-alt', type: 'schedule' },
      { descricao: 'Imprimir selecionado', icone: 'print', type: 'print' },
    ]
  } else if (props.status === 'Distribuídas') {
    opcoesMenu = [
      { descricao: 'Devolver ordem de serviço', icone: 'reply', type: 'return' },
      { descricao: 'Cancelar ordem de serviço', icone: 'times-circle', type: 'cancel' },
      { descricao: 'Imprimir selecionado', icone: 'print', type: 'print' },
    ]
  } else if (props.status === 'Em execução') {
    opcoesMenu = [
      { descricao: 'Cancelar ordem de serviço', icone: 'times-circle', type: 'cancel' },
      { descricao: 'Devolver ordem de serviço', icone: 'reply', type: 'return' },
      { descricao: 'Imprimir selecionado', icone: 'print', type: 'print' },
    ]
  } else if (props.status === 'Pendentes') {
    opcoesMenu = [
      { descricao: 'Cancelar ordem de serviço', icone: 'times-circle', type: 'cancel' },
      { descricao: 'Imprimir selecionado', icone: 'print', type: 'print' },
    ]
  } else if (props.status === 'Finalizadas') {
    opcoesMenu = [
      { descricao: 'Cancelar ordem de serviço', icone: 'times-circle', type: 'cancel' },
      { descricao: 'Imprimir selecionado', icone: 'print', type: 'print' },
    ]
  } else {
    opcoesMenu = [
      { descricao: 'Imprimir selecionado', icone: 'print', type: 'print' },
    ]
  }

  return [
    {
      customButtonDescription: 'Ações para ordens de serviço',
      customButtonIcon: 'ellipsis-vertical',
      type: 'menu',
      enableIfHasSelectedItems: disabled,
      hasMultipleAction: true,
      opcoesMenu,
    },
  ]
})

// ── per-row actions ───────────────────────────────────────────────────────────
function getOpcoesAcoesRegistro() {
  if (props.readonly) {
    return [
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
    ]
  }

  if (props.status === 'Solicitadas') {
    return [
      { descricao: 'Iniciar ordem serviço', icone: 'share-square', classe: 'text-left' },
      { descricao: 'Programar ordem serviço', icone: 'calendar-alt', classe: 'text-left' },
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Documentos da O.S.', icone: 'file-alt', classe: 'text-left' },
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
      { descricao: 'Finalizar ordem serviço', icone: 'flag-checkered', classe: 'text-left' },
    ]
  }

  if (props.status === 'Distribuídas') {
    return [
      { descricao: 'Devolver ordem de serviço', icone: 'reply', classe: 'text-left' },
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Documentos da O.S.', icone: 'file-alt', classe: 'text-left' },
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
      { descricao: 'Finalizar ordem serviço', icone: 'flag-checkered', classe: 'text-left' },
    ]
  }

  if (props.status === 'Em execução') {
    return [
      { descricao: 'Devolver ordem de serviço', icone: 'reply', classe: 'text-left' },
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Documentos da O.S.', icone: 'file-alt', classe: 'text-left' },
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
      { descricao: 'Finalizar ordem serviço', icone: 'flag-checkered', classe: 'text-left' },
    ]
  }

  if (props.status === 'Pendentes') {
    return [
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Documentos da O.S.', icone: 'file-alt', classe: 'text-left' },
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
      { descricao: 'Finalizar ordem serviço', icone: 'flag-checkered', classe: 'text-left' },
    ]
  }

  if (props.status === 'Finalizadas') {
    return [
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Documentos da O.S.', icone: 'file-alt', classe: 'text-left' },
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
    ]
  }

  if (props.status === 'Canceladas') {
    return [
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Documentos da O.S.', icone: 'file-alt', classe: 'text-left' },
    ]
  }

  return [
    { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
    { descricao: 'Documentos da O.S.', icone: 'file-alt', classe: 'text-left' },
  ]
}

function getEstadoOrdenacaoColuna(campoOrdenacao) {
  const campoAtual = props.filtrosGlobais?.ordenarPor || 'Codigo'

  if (campoAtual !== campoOrdenacao) return null

  return (props.filtrosGlobais?.ordem || 'Asc') !== 'Desc'
}

const fields = computed(() => {
  const cols = [
    { descricao: 'Nº O.S.', valor: 'codigo', tipo: 'texto', filtravel: false, ordenado: getEstadoOrdenacaoColuna('Codigo'), campoOrdenacao: 'Codigo' },
    { descricao: 'Ações', valor: 'ellipsis', tipo: 'menu', filtravel: false, ordenado: null, class: 'text-left', desabilitarOrdenacao: true, opcoesMenu: getOpcoesAcoesRegistro() },
    { descricao: 'Endereço', valor: 'endereco', tipo: 'texto', filtravel: false, ordenado: getEstadoOrdenacaoColuna('Endereco'), campoOrdenacao: 'Endereco' },
    { descricao: 'Prioridade', valor: 'prioridadeDescricao', tipo: 'texto', filtravel: false, ordenado: getEstadoOrdenacaoColuna('Prioridade'), campoOrdenacao: 'Prioridade' },
    { descricao: 'Data Solicitação', valor: 'dataSolicitacaoFormatada', tipo: 'texto', filtravel: false, ordenado: getEstadoOrdenacaoColuna('DataSolicitacao'), campoOrdenacao: 'DataSolicitacao' },
    { descricao: 'Status', valor: 'status', tipo: 'texto', filtravel: false, ordenado: getEstadoOrdenacaoColuna('Status'), campoOrdenacao: 'Status' },
  ]

  if (!props.readonly) {
    cols.unshift({ descricao: '', valor: 'selecionado', tipo: 'checkbox', filtravel: false, ordenado: null, selecionado: null, })
  }

  return cols
})


// ── fetch ─────────────────────────────────────────────────────────────────────
const router = useRouter()

async function listarItens() {
  if (loading.value) return

  loading.value = true
  emit('carregarTab')

  const filtros = {
    ...filtrosLocal.value,
    statusId: props.statusOrdemServicoId ? [props.statusOrdemServicoId] : [],
    pagina: props.tabState?.pagina || 1,
    itensPagina: props.tabState?.itensPagina || 10,
  }

  const result = await ordemServicoService.listar(filtros)
  loading.value = false

  if (result?.statusCode === 200) {
    const listaFormatada = result?.data?.data?.lista || []

    emit('atualizarTabState', {
      lista: listaFormatada,
      totalPaginas: result.data.data.paginas,
      totalItens: result.data.data.totalItens,
      pagina: filtros.pagina,
      itensPagina: filtros.itensPagina,
    })
  } else {
    mensagemRetorno.value = result?.data?.message || 'Erro ao carregar ordens de serviço.'
    sucesso.value = false
    retorno.value = true
    emit('atualizarTabState', { lista: [], totalPaginas: 0, totalItens: 0 })
  }
}

watch(
  () => props.tabState?.loaded,
  loaded => { if (loaded === false && !loading.value) listarItens() }
)

watch(
  () => props.tabState?.loading,
  isLoading => { if (isLoading && !props.tabState?.loaded && !loading.value) listarItens() }
)

onMounted(async () => {
  if (props.tabState && !props.tabState.loaded) listarItens()

  const [respMotivos, respFuncionarios] = await Promise.all([
    motivoCancelamentoService.buscarTodos(),
    funcionarioService.buscarTodos(),
  ])

  if (respMotivos?.statusCode === 200) listaMotivosCancelamento.value = respMotivos.data?.data || []
  if (respFuncionarios?.statusCode === 200) listaFuncionarios.value = respFuncionarios.data?.data || []
})

// ── pagination ────────────────────────────────────────────────────────────────
function alterarPagina(val) {
  emit('atualizarPaginacaoTab', { pagina: val })
}

function alterarItensPorPagina(val) {
  emit('atualizarPaginacaoTab', { itensPagina: val, pagina: 1 })
}

// ── filter modal ──────────────────────────────────────────────────────────────
function filtrar() {
  emit('aplicarFiltrosGlobais', {
    numero: filtrosLocal.value.numero || null,
    todos: filtrosLocal.value.todos || null,
    ordenarPor: filtrosLocal.value.ordenarPor || null,
    ordem: filtrosLocal.value.ordem || null,
  })
  modalFilter.value = false
}

function limparFiltro() {
  filtrosLocal.value = { numero: null, todos: null, ordenarPor: null, ordem: null }
}

// ── form resets ───────────────────────────────────────────────────────────────
function resetForm() {
  selectedFuncionarioId.value = null
  searchFuncionario.value = ''
}

function resetDevolucaoForm() {
  devolucaoObservacao.value = ''
}

function resetCancelamentoForm() {
  cancelamentoMotivoId.value = null
  cancelamentoObservacao.value = ''
}

function resetDespachoForm() {
  despachoFuncionarioId.value = null
  despachoDataHora.value = null
  searchDespachoFuncionario.value = ''
}

// ── helper: set currentItem ───────────────────────────────────────────────────
function setCurrentItem(item) {
  currentItem.value = {
    id: item?.id ?? '',
    codigo: item?.codigo ?? '',
    endereco: item?.endereco ?? '',
    servico: item?.servico ?? '',
  }
}

// ── dialog helpers ────────────────────────────────────────────────────────────
function abrirDespachoProgramado({ emLote = false, selecionados = [] } = {}) {
  despachoEmLote.value = emLote
  despachoSelecionados.value = (selecionados || []).filter(item => !!item?.id)
  resetDespachoForm()
  despachoDialog.value = true
}

function fecharTodosModais() {
  iniciarDialog.value = false
  confirmDialog.value = false
  cancelDialog.value = false
  devolverDialog.value = false
  sendConfirmDialog.value = false
  despachoDialog.value = false
  iniciarEmLote.value = false
  ocorrenciasSelecionadasLote.value = []
  resetCancelamentoForm()
  resetDevolucaoForm()
}

function closeModal() {
  iniciarDialog.value = false
  confirmDialog.value = false
  iniciarEmLote.value = false
  ocorrenciasSelecionadasLote.value = []
  servicosSolicitados.value = []
  resetForm()
}

function submitStart() {
  confirmationNumber.value = currentItem.value.codigo
  confirmDialog.value = true
  iniciarDialog.value = false
  resetForm()
}

// ── actions: iniciar ──────────────────────────────────────────────────────────
function prepareConfirmSend() {
  confirmationNumber.value = currentItem.value.codigo
  sendConfirmDialog.value = true
}

async function confirmSend() {
  sendConfirmDialog.value = false

  const ordensParaEnviar = iniciarEmLote.value
    ? (ocorrenciasSelecionadasLote.value || []).filter(item => !!item?.id)
    : [currentItem.value].filter(item => !!item?.id)

  if (!ordensParaEnviar.length) {
    mensagemRetorno.value = 'Não foi possível identificar as ordens de serviço selecionadas.'
    sucesso.value = false
    retorno.value = true
    return
  }

  if (!selectedFuncionarioId.value) {
    mensagemRetorno.value = 'Selecione um funcionário para iniciar a ordem de serviço.'
    sucesso.value = false
    retorno.value = true
    return
  }

  let processadasComSucesso = 0
  let mensagemErro = null

  loadingAcao.value = true

  try {
    for (const ordem of ordensParaEnviar) {
      const respostaIniciar = await ordemServicoService.iniciarOrdemServico({
        ordemServicoId: ordem.id,
        funcionarioId: selectedFuncionarioId.value,
      })

      if (respostaIniciar?.statusCode !== 200) {
        mensagemErro = respostaIniciar?.data?.message || 'Falha ao iniciar ordem de serviço.'
        break
      }

      processadasComSucesso++
    }
  } finally {
    loadingAcao.value = false
  }

  if (mensagemErro) {
    mensagemRetorno.value = processadasComSucesso > 0
      ? `${mensagemErro} ${processadasComSucesso} ordem(ns) já havia(m) sido processada(s).`
      : mensagemErro
    sucesso.value = false
    retorno.value = true
    return
  }

  mensagemRetorno.value = iniciarEmLote.value
    ? `${processadasComSucesso} ordem(ns) enviada(s) com sucesso!`
    : 'Ordem de serviço enviada com sucesso!'
  sucesso.value = true
  retorno.value = true

  emit('recarregarTabs')
  submitStart()
}

// ── actions: devolver ─────────────────────────────────────────────────────────
async function confirmarDevolucaoOrdemServico() {
  const devolucaoEmLote = iniciarEmLote.value
  const ordensParaDevolver = iniciarEmLote.value
    ? (devolverSelecionados.value || []).filter(item => !!item?.id)
    : [currentItem.value].filter(item => !!item?.id)

  if (!ordensParaDevolver.length) {
    mensagemRetorno.value = 'Não foi possível identificar as ordens de serviço selecionadas.'
    sucesso.value = false
    retorno.value = true
    return
  }

  if (!devolucaoObservacao.value?.trim()) {
    mensagemRetorno.value = 'Informe a observação da devolução.'
    sucesso.value = false
    retorno.value = true
    return
  }

  let processadasComSucesso = 0
  let mensagemErro = null

  loadingAcao.value = true

  try {
    for (const ordem of ordensParaDevolver) {
      const respostaDevolucao = await ordemServicoService.devolverOrdemServico({
        ordemServicoId: ordem.id,
        observacaoDevolucao: devolucaoObservacao.value.trim(),
      })

      if (respostaDevolucao?.statusCode !== 200) {
        mensagemErro = respostaDevolucao?.data?.message || 'Falha ao devolver ordem de serviço.'
        break
      }

      processadasComSucesso++
    }

    if (mensagemErro) {
      mensagemRetorno.value = processadasComSucesso > 0
        ? `${mensagemErro} ${processadasComSucesso} ordem(ns) já havia(m) sido processada(s).`
        : mensagemErro
      sucesso.value = false
      retorno.value = true
      return
    }

    devolverDialog.value = false
    iniciarEmLote.value = false
    devolverSelecionados.value = []
    resetDevolucaoForm()

    mensagemRetorno.value = devolucaoEmLote
      ? `${processadasComSucesso} ordem(ns) devolvida(s) com sucesso.`
      : 'Ordem de serviço devolvida com sucesso.'
    sucesso.value = true
    retorno.value = true

    emit('recarregarTabs')
  } finally {
    loadingAcao.value = false
  }
}

// ── actions: cancelar ─────────────────────────────────────────────────────────
async function confirmarCancelamentoOrdemServico() {
  const cancelamentoEmLote = iniciarEmLote.value
  const ordensParaCancelar = iniciarEmLote.value
    ? (ocorrenciasSelecionadasLote.value || []).filter(item => !!item?.id)
    : [currentItem.value].filter(item => !!item?.id)

  if (!ordensParaCancelar.length) {
    mensagemRetorno.value = 'Não foi possível identificar as ordens de serviço selecionadas.'
    sucesso.value = false
    retorno.value = true
    return
  }

  if (!cancelamentoMotivoId.value) {
    mensagemRetorno.value = 'Informe o motivo do cancelamento.'
    sucesso.value = false
    retorno.value = true
    return
  }

  if (!cancelamentoObservacao.value?.trim()) {
    mensagemRetorno.value = 'Informe a observação do cancelamento.'
    sucesso.value = false
    retorno.value = true
    return
  }

  let processadasComSucesso = 0
  let mensagemErro = null

  loadingAcao.value = true

  try {
    for (const ordem of ordensParaCancelar) {
      const respostaCancelamento = await ordemServicoService.cancelarOrdemServico({
        ordemServicoId: ordem.id,
        motivoCancelamentoId: cancelamentoMotivoId.value,
        observacao: cancelamentoObservacao.value.trim(),
      })

      if (respostaCancelamento?.statusCode !== 200) {
        mensagemErro = respostaCancelamento?.data?.message || 'Falha ao cancelar ordem de serviço.'
        break
      }

      processadasComSucesso++
    }

    if (mensagemErro) {
      mensagemRetorno.value = processadasComSucesso > 0
        ? `${mensagemErro} ${processadasComSucesso} ordem(ns) já havia(m) sido processada(s).`
        : mensagemErro
      sucesso.value = false
      retorno.value = true
      return
    }

    cancelDialog.value = false
    iniciarEmLote.value = false
    ocorrenciasSelecionadasLote.value = []
    resetCancelamentoForm()

    mensagemRetorno.value = cancelamentoEmLote
      ? `${processadasComSucesso} ordem(ns) cancelada(s) com sucesso.`
      : 'Ordem de serviço cancelada com sucesso.'
    sucesso.value = true
    retorno.value = true

    emit('recarregarTabs')
  } finally {
    loadingAcao.value = false
  }
}

// ── actions: despacho programado ──────────────────────────────────────────────
async function programarDespacho() {
  if (!despachoFuncionarioId.value || !despachoDataHora.value) {
    mensagemRetorno.value = 'Informe o funcionário e a data/hora do despacho.'
    sucesso.value = false
    retorno.value = true
    return
  }

  const ordensParaDespacho = despachoSelecionados.value.filter(item => !!item?.id)

  if (!ordensParaDespacho.length) {
    mensagemRetorno.value = 'Não foi possível identificar a ordem de serviço para despacho.'
    sucesso.value = false
    retorno.value = true
    return
  }

  let processadasComSucesso = 0
  let mensagemErro = null

  loadingAcao.value = true

  try {
    for (const ordem of ordensParaDespacho) {
      const respostaDespacho = await ordemServicoService.despacharOrdemServico({
        ordemServicoId: ordem.id,
        funcionarioId: despachoFuncionarioId.value,
        dataDespachoProgramado: despachoDataHora.value,
      })

      if (respostaDespacho?.statusCode !== 200) {
        mensagemErro = respostaDespacho?.data?.message || 'Falha ao programar despacho da ordem de serviço.'
        break
      }

      processadasComSucesso++
    }

    if (mensagemErro) {
      mensagemRetorno.value = processadasComSucesso > 0
        ? `${mensagemErro} ${processadasComSucesso} ordem(ns) já havia(m) sido processada(s).`
        : mensagemErro
      sucesso.value = false
      retorno.value = true
      return
    }

    fecharTodosModais()

    mensagemRetorno.value = despachoEmLote.value
      ? `${processadasComSucesso} ordem(ns) programada(s) com sucesso.`
      : 'Despacho programado com sucesso.'
    sucesso.value = true
    retorno.value = true

    emit('recarregarTabs')
  } finally {
    loadingAcao.value = false
  }
}

// ── grid event handlers ───────────────────────────────────────────────────────
async function handleOptionClick({ item, opcao }) {
  if (opcao.descricao === 'Iniciar ordem serviço') {
    iniciarEmLote.value = false
    ocorrenciasSelecionadasLote.value = []
    setCurrentItem(item)
    resetForm()
    iniciarDialog.value = true
    loadingAcao.value = true
    const detalhes = await ordemServicoService.obterDetalhes(item.id)
    loadingAcao.value = false
    if (detalhes?.statusCode === 200) {
      servicosSolicitados.value = detalhes.data?.data?.servicosSolicitados || []
    }
  } else if (opcao.descricao === 'Programar ordem serviço') {
    iniciarEmLote.value = false
    ocorrenciasSelecionadasLote.value = []
    setCurrentItem(item)
    resetForm()
    abrirDespachoProgramado({ emLote: false, selecionados: [item] })
  } else if (opcao.descricao === 'Devolver ordem de serviço') {
    iniciarEmLote.value = false
    devolverSelecionados.value = [item].filter(r => !!r?.id)
    setCurrentItem(item)
    resetDevolucaoForm()
    devolverDialog.value = true
  } else if (opcao.descricao === 'Cancelar ordem serviço') {
    iniciarEmLote.value = false
    ocorrenciasSelecionadasLote.value = []
    setCurrentItem(item)
    resetCancelamentoForm()
    cancelDialog.value = true
  } else if (opcao.descricao === 'Finalizar ordem serviço') {
    mensagemRetorno.value = 'Funcionalidade de finalização em desenvolvimento.'
    sucesso.value = false
    retorno.value = true
  } else if (opcao.descricao === 'Detalhar ordem serviço') {
    await router.push({ name: 'DetalharOrdemServico', params: { id: item.id } })
  } else if (opcao.descricao === 'Documentos da O.S.') {
    ordemServicoDocumentosAtual.value = {
      id: item?.id ?? '',
      codigo: item?.codigo ?? '',
      endereco: item?.endereco ?? '',
      servico: item?.servico ?? '',
    }
    documentosDialog.value = true
  }
}

function handleCustomButtonClick(payload) {
  const opcao = payload?.opcao

  if (opcao?.type === 'print' || opcao?.descricao === 'Imprimir selecionado') {
    mensagemRetorno.value = 'Funcionalidade de impressão em desenvolvimento.'
    sucesso.value = false
    retorno.value = true
    return
  }

  if (opcao?.type === 'send' || opcao?.descricao === 'Enviar para equipe') {
    iniciarEmLote.value = true
    ocorrenciasSelecionadasLote.value = (payload?.selecionados || []).filter(item => !!item?.id)
    resetForm()
    iniciarDialog.value = true
    return
  }

  if (opcao?.type === 'schedule' || opcao?.descricao === 'Programar despacho') {
    abrirDespachoProgramado({ emLote: true, selecionados: (payload?.selecionados || []).filter(item => !!item?.id) })
    return
  }

  if (opcao?.type === 'return' || opcao?.descricao === 'Devolver ordem de serviço') {
    iniciarEmLote.value = true
    devolverSelecionados.value = (payload?.selecionados || []).filter(item => !!item?.id)
    resetDevolucaoForm()
    devolverDialog.value = true
    return
  }

  if (opcao?.type === 'cancel' || opcao?.descricao === 'Cancelar ordem de serviço') {
    iniciarEmLote.value = true
    ocorrenciasSelecionadasLote.value = (payload?.selecionados || []).filter(item => !!item?.id)
    resetCancelamentoForm()
    cancelDialog.value = true
    return
  }
}

function alterarOrdenacao(evento) {
  const idx = Number(evento?.ordenarPor ?? 0)
  let col = fields.value[idx]

  if (
    idx === 0 &&
    ['checkbox', 'menu', 'switch', 'botao'].includes(col?.tipo) &&
    props.filtrosGlobais?.ordenarPor
  ) {
    filtrosLocal.value.ordenarPor = 'Codigo';
    filtrosLocal.value.ordem = 'Asc';
    filtrar()
    listarItens();
    return
  }

  if (!col || col.desabilitarOrdenacao) return

  const fallback = (s) => (s ? s.charAt(0).toUpperCase() + s.slice(1) : 'Codigo')
  const campoApi = col.campoOrdenacao ?? fallback(col.valor)
  const ordemApi = Number(evento?.ordem) === 1 ? 'Desc' : 'Asc'

  filtrosLocal.value.ordenarPor = campoApi;
  filtrosLocal.value.ordem = ordemApi;
  filtrar()
  listarItens();
}
</script>

<template>
  <div>
    <Loading :active="loading || tabState?.loading || loadingAcao" />
    <Grid
      :fields="fields"
      :list="lista"
      :filters="{ pagina: paginaAtual, itensPagina }"
      gridType="responsive"
      filterType="popup"
      gridOverflow="horizontal"
      :gridResizable="true"
      :hasCheckbox="true"
      :customButtonsList="customButtonsList"
      @listarItens="listarItens"
      @botaoOpcaoClick="handleOptionClick"
      @customButtonClick="handleCustomButtonClick"
      @abrirFiltro="modalFilter = true"
      @alterarOrdenacao="alterarOrdenacao"
    />
    <Paginacao
      :totalPaginas="totalPaginas"
      :paginaAtual="paginaAtual"
      :totalItens="totalItens"
      @alterarItensPorPagina="alterarItensPorPagina"
      @alterarPagina="alterarPagina"
    />

    <!-- Filter Modal -->
    <v-dialog v-model="modalFilter" max-width="600" persistent scrim="rgba(0,0,0,0.7)">
      <v-card>
        <v-card-text class="pa-4">
          <div class="d-flex align-center mb-2">
            <font-awesome-icon icon="search" class="text-primary me-2" />
            <span class="text-h6">Filtrar ordens de serviço</span>
            <v-spacer />
            <font-awesome-icon icon="xmark" class="cursor-pointer" @click="modalFilter = false" />
          </div>
          <v-divider />
          <v-form>
            <v-container class="pa-3">
              <v-row>
                <v-col cols="12" class="pb-4">
                  <v-text-field
                    density="comfortable"
                    v-model="filtrosLocal.numero"
                    label="Nº O.S."
                    clearable
                    hide-details
                    variant="outlined"
                  />
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    density="comfortable"
                    v-model="filtrosLocal.todos"
                    label="Busca geral"
                    clearable
                    hide-details
                    variant="outlined"
                  />
                </v-col>
              </v-row>
            </v-container>
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <BaseButton label="Limpar filtro" type="clear" extraClass="me-2" @click="limparFiltro" />
          <BaseButton label="Cancelar" type="cancel" @click="modalFilter = false" />
          <BaseButton label="Filtrar" type="save" @click="filtrar" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal Iniciar O.S. -->
    <v-dialog v-model="iniciarDialog" max-width="480" persistent scrim="rgba(0,0,0,0.7)">
      <v-card>
        <v-card-title class="d-flex align-center pa-4 pb-2">
          <font-awesome-icon icon="play" class="text-primary me-2" />
          <span class="text-h6">{{ iniciarEmLote ? 'Iniciar ordens de serviço' : 'Iniciar Ordem de Serviço' }}</span>
          <v-spacer />
          <font-awesome-icon icon="xmark" @click="closeModal" class="text-close icon-clicavel" />
        </v-card-title>
        <v-divider />
        <v-card-text class="pa-4">
          <div v-if="iniciarEmLote" class="mb-4">
            <span class="text-body-2">Ordens selecionadas:</span>
            <strong class="ms-1">{{ selectedBatchCount }}</strong>
          </div>
          <div v-else class="mb-4">
            <div class="d-flex align-center mb-1">
              <span class="text-body-2 text-medium-emphasis" style="min-width:90px">Nº O.S.:</span>
              <strong>{{ currentItem.codigo }}</strong>
            </div>
            <div class="d-flex align-center mb-2">
              <span class="text-body-2 text-medium-emphasis" style="min-width:90px">Endereço:</span>
              <span class="text-body-2">{{ currentItem.endereco }}</span>
            </div>
            <div class="mt-1">
              <span class="text-body-2 text-medium-emphasis d-block mb-1">Serviços solicitados:</span>
              <div v-if="servicosSolicitados.length" class="d-flex flex-column">
                <div
                  v-for="(s, idx) in servicosSolicitados"
                  :key="idx"
                  class="d-flex align-center py-2 px-1"
                  :class="{ 'border-t': idx > 0 }"
                  style="border-color: rgba(0,0,0,0.08) !important"
                >
                  <span class="text-primary me-2" style="font-size:10px; flex-shrink:0">●</span>
                  <span class="text-body-2">{{ s.descricao }}</span>
                </div>
              </div>
              <span v-else class="text-body-2 text-medium-emphasis">Nenhum serviço solicitado.</span>
            </div>
          </div>

          <v-autocomplete
            v-model="selectedFuncionarioId"
            :items="listaFuncionarios"
            item-title="nome"
            item-value="id"
            label="Funcionário responsável"
            variant="outlined"
            clearable
            density="comfortable"
            hide-details
            v-model:search="searchFuncionario"
            no-data-text="Nenhum funcionário encontrado"
          >
            <template #prepend-inner>
              <font-awesome-icon icon="user" class="text-medium-emphasis me-1" />
            </template>
          </v-autocomplete>
        </v-card-text>
        <v-divider />
        <v-card-actions class="pa-4 justify-end">
          <BaseButton label="Cancelar" type="cancel" extraClass="me-2" @click="closeModal" />
          <BaseButton label="Iniciar O.S." type="save" :disabled="!canSend" @click="prepareConfirmSend" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal Confirmar Envio -->
    <v-dialog v-model="sendConfirmDialog" max-width="450" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-2">
        <v-card-title>
          <font-awesome-icon icon="paper-plane" class="text-primary me-4" />
          <span class="text-h6">Confirmar envio?</span>
          <font-awesome-icon icon="xmark" @click="sendConfirmDialog = false" class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-4" />
        <v-card-text class="pb-4">
          <p v-if="iniciarEmLote">
            Deseja realmente enviar <strong>{{ selectedBatchCount }}</strong> ordem(ns) de serviço para o funcionário selecionado?
          </p>
          <p v-else>
            Deseja realmente enviar a O.S. <strong>{{ confirmationNumber }}</strong> para o funcionário selecionado?
          </p>
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Não" type="cancel" extraClass="me-2" @click="sendConfirmDialog = false" />
          <BaseButton label="Sim, enviar" type="confirm" @click="confirmSend" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal O.S. Enviada -->
    <v-dialog v-model="confirmDialog" max-width="450" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-2">
        <v-card-title>
          <font-awesome-icon icon="clipboard-check" class="text-primary me-2" />
          <span class="text-h6">{{ iniciarEmLote ? 'Ordens enviadas!' : 'O.S. enviada!' }}</span>
          <font-awesome-icon icon="xmark" @click="closeModal" class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-4" />
        <v-card-text class="pb-4">
          <!-- Lote -->
          <template v-if="iniciarEmLote">
            <span class="text-body-2 text-medium-emphasis d-block mb-2">Ordens de serviço enviadas:</span>
            <div class="d-flex flex-column" style="max-height:260px; overflow-y:auto;">
              <div
                v-for="(os, idx) in ocorrenciasSelecionadasLote"
                :key="os.id"
                class="d-flex align-center py-2 px-1"
                :class="{ 'border-t': idx > 0 }"
                style="border-color: rgba(0,0,0,0.08) !important"
              >
                <span class="text-primary me-2" style="font-size:10px; flex-shrink:0">●</span>
                <span class="text-body-2"><strong>{{ os.codigo }}</strong></span>
                <span v-if="os.endereco" class="text-body-2 text-medium-emphasis ms-2">— {{ os.endereco }}</span>
              </div>
            </div>
          </template>

          <!-- Individual -->
          <template v-else>
            <div class="d-flex align-center mb-1">
              <span class="text-body-2 text-medium-emphasis" style="min-width:90px">Nº O.S.:</span>
              <strong>{{ confirmationNumber }}</strong>
            </div>
            <div class="d-flex align-center mb-2">
              <span class="text-body-2 text-medium-emphasis" style="min-width:90px">Endereço:</span>
              <span class="text-body-2">{{ currentItem.endereco }}</span>
            </div>
            <div class="mt-1">
              <span class="text-body-2 text-medium-emphasis d-block mb-1">Serviços solicitados:</span>
              <div v-if="servicosSolicitados.length" class="d-flex flex-column">
                <div
                  v-for="(s, idx) in servicosSolicitados"
                  :key="idx"
                  class="d-flex align-center py-2 px-1"
                  :class="{ 'border-t': idx > 0 }"
                  style="border-color: rgba(0,0,0,0.08) !important"
                >
                  <span class="text-primary me-2" style="font-size:10px; flex-shrink:0">●</span>
                  <span class="text-body-2">{{ s.descricao }}</span>
                </div>
              </div>
              <span v-else class="text-body-2 text-medium-emphasis">Nenhum serviço solicitado.</span>
            </div>
          </template>
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Fechar" type="confirm" @click="closeModal" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal Devolver O.S. -->
    <v-dialog v-model="devolverDialog" max-width="500" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-2">
        <v-card-title>
          <font-awesome-icon icon="reply" class="text-primary me-2" />
          <span class="text-h6">
            {{ iniciarEmLote ? 'Devolver ordens de serviço?' : 'Devolver ordem de serviço?' }}
          </span>
          <font-awesome-icon icon="xmark" @click="devolverDialog = false" class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-4" />
        <v-card-text class="pb-4">
          <p v-if="iniciarEmLote">
            Deseja realmente devolver <strong>{{ devolverSelecionados.length }}</strong> ordem(ns) de serviço?
          </p>
          <p v-else>
            Deseja realmente devolver a O.S. <strong>{{ currentItem.codigo }}</strong>?
          </p>
          <v-textarea
            v-model="devolucaoObservacao"
            label="Observação da devolução"
            variant="outlined"
            density="comfortable"
            rows="4"
            auto-grow
            hide-details
            class="mt-4"
          />
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Não" type="cancel" extraClass="me-2" @click="devolverDialog = false" />
          <BaseButton label="Sim, devolver" type="confirm" @click="confirmarDevolucaoOrdemServico" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal Cancelar O.S. -->
    <v-dialog v-model="cancelDialog" max-width="550" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-2">
        <v-card-title>
          <font-awesome-icon icon="times-circle" class="text-primary me-2" />
          <span class="text-h6">
            {{ iniciarEmLote ? 'Cancelar ordens de serviço?' : 'Cancelar ordem de serviço?' }}
          </span>
          <font-awesome-icon icon="xmark" @click="cancelDialog = false" class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-4" />
        <v-card-text class="pb-4">
          <p v-if="iniciarEmLote" class="mb-4">
            <strong>Ordens selecionadas:</strong> {{ selectedBatchCount }}
          </p>
          <p v-else class="mb-4">
            Cancelar a O.S. <strong>{{ currentItem.codigo }}</strong>?
          </p>
          <v-autocomplete v-model="cancelamentoMotivoId" :items="listaMotivosCancelamento" item-title="descricao"
            item-value="id" label="Motivo do cancelamento" variant="outlined" clearable density="comfortable"
            hide-details class="mb-4" />

          <v-textarea
            v-model="cancelamentoObservacao"
            label="Observação do cancelamento"
            variant="outlined"
            density="comfortable"
            rows="4"
            auto-grow
            hide-details
          />
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Não" type="cancel" extraClass="me-2" @click="cancelDialog = false" />
          <BaseButton label="Sim, cancelar" type="confirm" @click="confirmarCancelamentoOrdemServico" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal Despacho Programado -->
    <v-dialog v-model="despachoDialog" max-width="500" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-2">
        <v-card-title>
          <font-awesome-icon icon="calendar-alt" class="text-primary me-2" />
          <span class="text-h6">Programar despacho</span>
          <font-awesome-icon icon="xmark" @click="despachoDialog = false" class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-4" />
        <v-card-text class="pb-4">
          <p v-if="despachoEmLote" class="mb-4">
            <strong>Ordens selecionadas:</strong> {{ despachoSelecionados.length }}
          </p>
          <p v-else class="mb-4">
            <strong>Nº O.S:</strong> {{ currentItem.codigo }}
          </p>
          <v-autocomplete
            v-model="despachoFuncionarioId"
            :items="listaFuncionarios"
            item-title="nome"
            item-value="id"
            label="Funcionário responsável"
            variant="outlined"
            clearable
            density="comfortable"
            hide-details
            class="mb-4"
            v-model:search="searchDespachoFuncionario"
            no-data-text="Nenhum funcionário encontrado"
          >
            <template #prepend-inner>
              <font-awesome-icon icon="user" class="text-medium-emphasis me-1" />
            </template>
          </v-autocomplete>
          <v-text-field
            v-model="despachoDataHora"
            label="Data/hora do despacho"
            type="datetime-local"
            variant="outlined"
            density="comfortable"
            hide-details
          />
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Cancelar" type="cancel" extraClass="me-2" @click="despachoDialog = false" />
          <BaseButton label="Programar" type="save" @click="programarDespacho" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <OrdemServicoDocumentos
      v-model="documentosDialog"
      :ordemServico="ordemServicoDocumentosAtual"
      @update:modelValue="valor => { if (!valor) { documentosDialog = false; ordemServicoDocumentosAtual = null } }"
    />

    <Snackbar
      :retorno="retorno"
      :timeout="3000"
      :tipo="sucesso ? 'sucesso' : 'erro'"
      :mensagemRetorno="mensagemRetorno"
      @ocultarRetorno="() => { retorno = false }"
    />
  </div>
</template>

<style scoped>

:deep(.v-overlay__scrim) {
  background: rgba(0, 0, 0, 0.7) !important;
}

.box-listas {
  margin-top: 10px !important;
}

:deep(.v-icon--size-default) {
  font-size: calc(var(--v-icon-size-multiplier) * 1em);
}

.selecao-equipe :deep(.v-field__field) {
  height: 40px !important;
}

.chips-autocomplete {
  margin: 5px;
  font-size: 13px;
  height: 25px;
}
</style>
