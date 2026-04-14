<script setup>
/* global defineProps, defineEmits */
import { ref, computed, watch, inject, onMounted } from 'vue'
import Grid from '@/components/common/GridComponent.vue'
import Paginacao from '@/components/common/PaginacaoComponent.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useRouter } from 'vue-router'
import OrdemServicoService from '@/services/ordem-servico/ordem-servico-service'
import Loading from '@/components/base/LoadingOverlay.vue'
import Snackbar from '@/components/base/Snackbar.vue'

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
const filtrosLocal = ref({ numero: null, todos: null })

// iniciar dialog
const iniciarDialog = ref(false)
const iniciarEmLote = ref(false)
const ocorrenciasSelecionadasLote = ref([])
const sendConfirmDialog = ref(false)
const confirmDialog = ref(false)

// devolver dialog
const devolverDialog = ref(false)
const devolverSelecionados = ref([])
const devolucaoObservacao = ref('')

// cancelar dialog
const cancelDialog = ref(false)
const cancelamentoObservacao = ref('')

// despacho programado dialog
const despachoDialog = ref(false)
const despachoEmLote = ref(false)
const despachoEquipe = ref(null)
const despachoDataHora = ref(null)
const despachoSelecionados = ref([])

const currentItem = ref({ id: '', codigo: '', endereco: '', servico: '' })
const confirmationNumber = ref('')

// equipe / funcionários
const selectedTeam = ref(null)
const selectedAdutora = ref(null)
const teamOptions = ref([])
const adutoraOptions = ref([])
const searchEmp = ref('')
const selectedEmployees = ref([])
const listaFuncionarios = ref([])

// ── derived from tabState ─────────────────────────────────────────────────────
const lista = computed(() => props.tabState?.lista || [])
const totalPaginas = computed(() => props.tabState?.totalPaginas || 0)
const totalItens = computed(() => props.tabState?.totalItens || 0)
const paginaAtual = computed(() => props.tabState?.pagina || 1)
const itensPagina = computed(() => props.tabState?.itensPagina || 10)

// ── grid setup ────────────────────────────────────────────────────────────────
const selectedCount = computed(() => lista.value.filter(i => i.selecionado).length)
const selectedBatchCount = computed(() => (ocorrenciasSelecionadasLote.value || []).length)

const allEmployees = computed(() => listaFuncionarios.value.filter(Boolean))
const funcionariosFiltrados = computed(() => {
  const termo = (searchEmp.value || '').toLowerCase().trim()
  if (!termo) return allEmployees.value
  return allEmployees.value.filter(e => e.toLowerCase().includes(termo))
})

const canSend = computed(() =>
  !!selectedTeam.value && !!selectedAdutora.value && selectedEmployees.value.length > 0
)

// ── custom buttons (toolbar) ──────────────────────────────────────────────────
const customButtonsList = computed(() => {
  const disabled = selectedCount.value === 0
  let opcoesMenu = []

  if (props.status === 'Solicitadas') {
    opcoesMenu = [
      { descricao: 'Enviar para equipe', icone: 'paper-plane', type: 'send' },
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
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
      { descricao: 'Finalizar ordem serviço', icone: 'flag-checkered', classe: 'text-left' },
    ]
  }

  if (props.status === 'Distribuídas') {
    return [
      { descricao: 'Devolver ordem de serviço', icone: 'reply', classe: 'text-left' },
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
      { descricao: 'Finalizar ordem serviço', icone: 'flag-checkered', classe: 'text-left' },
    ]
  }

  if (props.status === 'Em execução') {
    return [
      { descricao: 'Devolver ordem de serviço', icone: 'reply', classe: 'text-left' },
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
      { descricao: 'Finalizar ordem serviço', icone: 'flag-checkered', classe: 'text-left' },
    ]
  }

  if (props.status === 'Pendentes') {
    return [
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
      { descricao: 'Finalizar ordem serviço', icone: 'flag-checkered', classe: 'text-left' },
    ]
  }

  if (props.status === 'Finalizadas') {
    return [
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Cancelar ordem serviço', icone: 'times-circle', classe: 'text-left' },
    ]
  }

  if (props.status === 'Canceladas') {
    return [
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
    ]
  }

  return [
    { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
  ]
}

const fields = computed(() => [
  { descricao: 'Nº O.S.', valor: 'codigo', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Data', valor: 'dataFormatada', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Status', valor: 'status', tipo: 'texto', filtravel: false, ordenado: null },
  {
    descricao: 'Ações',
    valor: 'ellipsis',
    tipo: 'menu',
    filtravel: false,
    ordenado: null,
    class: 'text-left',
    opcoesMenu: getOpcoesAcoesRegistro(),
  },
])

// ── fetch ─────────────────────────────────────────────────────────────────────
const router = useRouter()

async function listarItens() {
  if (loading.value) return

  loading.value = true
  emit('carregarTab')

  const filtros = {
    ...props.filtrosGlobais,
    statusId: props.statusOrdemServicoId ? [props.statusOrdemServicoId] : [],
    pagina: props.tabState?.pagina || 1,
    itensPagina: props.tabState?.itensPagina || 10,
  }

  const result = await ordemServicoService.listar(filtros)
  loading.value = false

  if (result?.statusCode === 200) {
    const listaFormatada = (result?.data?.data?.lista || []).map(x => ({
      ...x,
      dataFormatada: new Date(x.dataSolicitacao).toLocaleDateString('pt-BR'),
    }))

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

onMounted(() => {
  if (props.tabState && !props.tabState.loaded) listarItens()
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
  })
  modalFilter.value = false
}

function limparFiltro() {
  filtrosLocal.value = { numero: null, todos: null }
}

// ── form resets ───────────────────────────────────────────────────────────────
function resetForm() {
  selectedTeam.value = null
  selectedAdutora.value = null
  searchEmp.value = ''
  selectedEmployees.value = []
}

function resetDevolucaoForm() {
  devolucaoObservacao.value = ''
}

function resetCancelamentoForm() {
  cancelamentoObservacao.value = ''
}

function resetDespachoForm() {
  despachoEquipe.value = null
  despachoDataHora.value = null
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

  if (!selectedTeam.value || !selectedAdutora.value || !selectedEmployees.value.length) {
    mensagemRetorno.value = 'Preencha equipe, estação e ao menos um funcionário.'
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
        id: ordem.id,
        equipeId: selectedTeam.value,
        estacaoId: selectedAdutora.value,
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
        id: ordem.id,
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
        id: ordem.id,
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
  if (!despachoEquipe.value || !despachoDataHora.value) {
    mensagemRetorno.value = 'Informe a equipe e a data/hora do despacho.'
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
        id: ordem.id,
        equipeId: despachoEquipe.value,
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

// ── employee chip removal ─────────────────────────────────────────────────────
watch(selectedTeam, () => {
  selectedEmployees.value = []
  searchEmp.value = ''
})

function removeEmployee(emp) {
  selectedEmployees.value = selectedEmployees.value.filter(e => e !== emp)
}

// ── grid event handlers ───────────────────────────────────────────────────────
async function handleOptionClick({ item, opcao }) {
  if (opcao.descricao === 'Iniciar ordem serviço') {
    iniciarEmLote.value = false
    ocorrenciasSelecionadasLote.value = []
    setCurrentItem(item)
    resetForm()
    iniciarDialog.value = true
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
      :gridResizable="false"
      :hasCheckbox="true"
      :customButtonsList="customButtonsList"
      @listarItens="listarItens"
      @botaoOpcaoClick="handleOptionClick"
      @customButtonClick="handleCustomButtonClick"
      @abrirFiltro="modalFilter = true"
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
    <v-dialog v-model="iniciarDialog" max-width="800" height="100%" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-4">
        <v-card-title>
          <font-awesome-icon icon="play" class="text-primary me-2" />
          {{ iniciarEmLote ? 'Enviar ordens de serviço para equipe' : 'Iniciar Ordem de Serviço' }}
          <font-awesome-icon icon="xmark" @click="closeModal" class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-0" />
        <v-card-text>
          <p v-if="iniciarEmLote" class="mb-3">
            <strong>Ordens selecionadas:</strong> {{ selectedBatchCount }}
          </p>
          <template v-else>
            <p class="mb-1"><strong>Nº O.S:</strong> {{ currentItem.codigo }}</p>
            <p class="mb-1"><strong>Serviço:</strong> {{ currentItem.servico }}</p>
            <p class="mb-1"><strong>Endereço:</strong> {{ currentItem.endereco }}</p>
          </template>
          <v-divider class="mx-2 my-2" />
          <v-autocomplete
            v-model="selectedTeam"
            :items="teamOptions"
            item-title="descricao"
            item-value="id"
            label="Equipe"
            variant="solo"
            clearable
            density="comfortable"
            single-line
            hide-details
            class="selecao-equipe mb-3"
          />
          <v-autocomplete
            v-model="selectedAdutora"
            :items="adutoraOptions"
            item-title="descricao"
            item-value="id"
            label="Estação"
            variant="solo"
            clearable
            density="comfortable"
            single-line
            hide-details
            class="selecao-equipe"
          />
          <v-row class="box-listas" v-if="selectedTeam">
            <v-col cols="12">
              <div class="mb-2"><strong>Funcionários</strong></div>
              <v-text-field
                v-model="searchEmp"
                clearable
                dense
                variant="underlined"
                hide-details
                placeholder="Filtrar funcionários"
              >
                <template #prepend-inner>
                  <font-awesome-icon icon="search" class="text-primary me-2" />
                </template>
              </v-text-field>
              <div class="d-flex flex-wrap gap-2 mb-3">
                <v-chip
                  v-for="emp in selectedEmployees"
                  :key="emp"
                  closable
                  @click:close="removeEmployee(emp)"
                  class="chips-autocomplete"
                >
                  {{ emp }}
                </v-chip>
              </div>
              <div style="max-height:180px; overflow-y:auto;">
                <v-checkbox
                  v-for="emp in funcionariosFiltrados"
                  :key="emp"
                  v-model="selectedEmployees"
                  :value="emp"
                  :label="emp"
                  dense
                  hide-details
                  class="mb-0"
                />
              </div>
            </v-col>
          </v-row>
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Cancelar" type="cancel" @click="closeModal" />
          <BaseButton label="Enviar O.S." type="save" :disabled="!canSend" @click="prepareConfirmSend" />
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
            Deseja realmente enviar <strong>{{ selectedBatchCount }}</strong> ordem(ns) de serviço para a equipe selecionada?
          </p>
          <p v-else>
            Deseja realmente enviar a O.S. <strong>{{ confirmationNumber }}</strong> para a equipe selecionada?
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
          <span class="text-h6">O.S. enviada!</span>
          <font-awesome-icon icon="xmark" @click="closeModal" class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-4" />
        <v-card-text class="pb-4">
          <p><strong>Nº O.S:</strong> {{ confirmationNumber }}</p>
          <p><strong>Endereço:</strong> {{ currentItem.endereco }}</p>
          <p><strong>Serviço:</strong> {{ currentItem.servico }}</p>
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
            v-model="despachoEquipe"
            :items="teamOptions"
            item-title="descricao"
            item-value="id"
            label="Equipe"
            variant="outlined"
            clearable
            density="comfortable"
            hide-details
            class="mb-4"
          />
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
