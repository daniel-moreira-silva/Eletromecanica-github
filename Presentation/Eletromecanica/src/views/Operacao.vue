<script setup>
import { ref, inject, onMounted, computed } from 'vue'
import ListaOrdemServico from '@/views/ListaOrdemServico.vue'
import OrdemServicoService from '@/services/ordem-servico/ordem-servico-service'
import LoadingOverlay from '@/components/base/LoadingOverlay.vue'

const endpoint = inject('endpoint')
const headerPadrao = inject('headerPadrao')
const chaveSeguranca = inject('chaveSeguranca')
const usuarioSeguranca = inject('usuarioSeguranca')
const statusOrdemServico = inject('statusOrdemServico')

const ordemServicoService = new OrdemServicoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
)

const activeTab = ref(0)
const loading = ref(false)

const tabs = ref([
  {
    label: 'Solicitadas',
    icon: 'list-alt',
    statusOrdemServicoId: statusOrdemServico.solicitada,
    count: 0,
  },
  {
    label: 'Distribuídas',
    icon: 'play-circle',
    statusOrdemServicoId: statusOrdemServico.iniciada,
    count: 0,
  },
  {
    label: 'Em execução',
    icon: 'person-digging',
    statusOrdemServicoId: statusOrdemServico.emAndamento,
    count: 0,
  },
  {
    label: 'Pendentes',
    icon: 'hourglass-half',
    statusOrdemServicoId: statusOrdemServico.pendente,
    count: 0,
  },
  {
    label: 'Finalizadas',
    icon: 'check-circle',
    statusOrdemServicoId: statusOrdemServico.finalizada,
    count: 0,
  },
  {
    label: 'Canceladas',
    icon: 'times-circle',
    statusOrdemServicoId: statusOrdemServico.cancelada,
    count: 0,
  },
])

const filtrosGlobais = ref({
  numero: null,
  statusId: null,
  todos: null,
  ativo: true,
  nome: null,
  numeroDocumento: null,
  endereco: null,
  bairro: null,
  pontoReferencia: null,
  // TODO: adicionar regiaoId quando o serviço de regiões for implementado no frontend
  // TODO: adicionar agendamentoId quando o serviço de agendamentos for implementado no frontend
  servicoSolicitadoId: null,
  servicoExecutadoId: null,
  motivoCancelamentoId: null,
  dataSolicitacaoInicio: null,
  dataSolicitacaoFim: null,
  dataAgendamentoInicio: null,
  dataAgendamentoFim: null,
  dataDespachoInicio: null,
  dataDespachoFim: null,
  dataFinalizacaoInicio: null,
  dataFinalizacaoFim: null,
  dataCancelamentoInicio: null,
  dataCancelamentoFim: null,
  dataDespachoProgramadoInicio: null,
  dataDespachoProgramadoFim: null,
  dataParalisacaoInicio: null,
  dataParalisacaoFim: null,
  ordenarPor: 'Numero',
  ordem: 'Asc',
  pagina: 1,
  itensPagina: 10,
})

const tabStateMap = ref({})

function createTabState() {
  return {
    loaded: false,
    loading: false,
    lista: [],
    pagina: 1,
    itensPagina: filtrosGlobais.value.itensPagina || 10,
    totalPaginas: 0,
    totalItens: 0,
  }
}

function ensureTabState(statusId) {
  if (!tabStateMap.value[statusId]) {
    tabStateMap.value[statusId] = createTabState()
  }
  return tabStateMap.value[statusId]
}

tabs.value.forEach(tab => ensureTabState(tab.statusOrdemServicoId))

const tabAtiva = computed(() => tabs.value[activeTab.value] || null)
const tabAtivaState = computed(() => {
  const statusId = tabAtiva.value?.statusOrdemServicoId
  return statusId ? ensureTabState(statusId) : null
})

async function carregarCounts() {
  loading.value = true
  try {
    const resp = await ordemServicoService.listarCount(filtrosGlobais.value)

    if (resp?.statusCode === 200) {
      const listaCounts = resp?.data?.data || []

      tabs.value = tabs.value.map(tab => {
        const item = listaCounts.find(
          c => String(c.statusId).toLowerCase() === String(tab.statusOrdemServicoId).toLowerCase()
        )
        return { ...tab, count: item?.totalOcorrencias ?? 0 }
      })
    }
  } finally {
    loading.value = false
  }
}

function invalidarTabs() {
  const novoMapa = {}
  tabs.value.forEach(tab => {
    novoMapa[tab.statusOrdemServicoId] = createTabState()
  })
  tabStateMap.value = novoMapa
}

async function garantirCargaTab(statusId) {
  const state = ensureTabState(statusId)
  if (state.loaded || state.loading) return
  state.loading = true
}

function atualizarTabState(statusId, payload) {
  const state = ensureTabState(statusId)
  state.lista = payload?.lista || []
  state.totalPaginas = payload?.totalPaginas || 0
  state.totalItens = payload?.totalItens || 0
  state.pagina = payload?.pagina || state.pagina
  state.itensPagina = payload?.itensPagina || state.itensPagina
  state.loading = false
  state.loaded = true
}

function atualizarPaginacaoTab(statusId, payload) {
  const state = ensureTabState(statusId)
  if (payload?.pagina !== undefined) state.pagina = payload.pagina
  if (payload?.itensPagina !== undefined) state.itensPagina = payload.itensPagina
  state.loaded = false
}

async function aoTrocarTab(novoIndice) {
  activeTab.value = novoIndice
  const statusId = tabs.value[novoIndice]?.statusOrdemServicoId
  if (statusId) {
    const state = ensureTabState(statusId)
    state.loaded = false
    state.loading = false
    await garantirCargaTab(statusId)
  }
}

async function aplicarFiltrosGlobais(novosFiltros) {
  filtrosGlobais.value = { ...filtrosGlobais.value, ...novosFiltros, pagina: 1 }
  invalidarTabs()
  await carregarCounts()
  const statusId = tabAtiva.value?.statusOrdemServicoId
  if (statusId) await garantirCargaTab(statusId)
}

async function atualizarResumo() {
  await carregarCounts()
}

async function recarregarTabs() {
  invalidarTabs()
  await carregarCounts()
  const statusId = tabAtiva.value?.statusOrdemServicoId
  if (statusId) await garantirCargaTab(statusId)
}

onMounted(async () => {
  await carregarCounts()
  const statusId = tabAtiva.value?.statusOrdemServicoId
  if (statusId) await garantirCargaTab(statusId)
})
</script>

<template>
  <v-container fluid class="operacoes-container pa-0">
    <LoadingOverlay :active="loading" />

    <v-tabs v-model="activeTab" class="operacoes-tabs" grow @update:modelValue="aoTrocarTab">
      <v-tab
        v-for="(tab, idx) in tabs"
        :key="tab.statusOrdemServicoId"
        :value="idx"
        class="operacoes-tab"
        :class="{ 'operacoes-tab--active': activeTab === idx }"
      >
        <font-awesome-icon :icon="tab.icon" class="me-2 text-info" />
        {{ tab.label }} ({{ tab.count }})
      </v-tab>
    </v-tabs>

    <div class="operacoes-tab-item">
      <ListaOrdemServico
        v-if="tabAtiva && tabAtivaState"
        :key="tabAtiva.statusOrdemServicoId"
        :status="tabAtiva.label"
        :statusOrdemServicoId="tabAtiva.statusOrdemServicoId"
        :count="tabAtiva.count"
        :filtrosGlobais="filtrosGlobais"
        :tabState="tabAtivaState"
        @aplicarFiltrosGlobais="aplicarFiltrosGlobais"
        @atualizarResumo="atualizarResumo"
        @recarregarTabs="recarregarTabs"
        @carregarTab="garantirCargaTab(tabAtiva.statusOrdemServicoId)"
        @atualizarTabState="payload => atualizarTabState(tabAtiva.statusOrdemServicoId, payload)"
        @atualizarPaginacaoTab="payload => atualizarPaginacaoTab(tabAtiva.statusOrdemServicoId, payload)"
      />
    </div>
  </v-container>
</template>

<style scoped>
.operacoes-container {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.operacoes-tabs {
  position: sticky;
  top: 0;
  z-index: 10;
}

.operacoes-tab {
  text-transform: none !important;
  font-weight: 500;
}

.operacoes-tab--active {
  background-color: var(--v-theme-surface) !important;
  color: var(--v-theme-primary) !important;
}

.operacoes-tab,
.operacoes-tab > .v-btn__content {
  padding-top: 12px !important;
  padding-bottom: 12px !important;
}

.operacoes-tab-item {
  flex-grow: 1;
  padding: 10px;
}

::v-deep(.grid-component) {
  height: calc(100vh - 185px) !important;
}
</style>
