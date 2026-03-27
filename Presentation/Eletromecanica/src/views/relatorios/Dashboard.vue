<script setup>
import { ref, inject, onMounted, reactive, nextTick } from 'vue'
import { Chart, registerables } from 'chart.js'

import DashboardService from '@/services/relatorios/dashboard-service.js'
import EstacaoService   from '@/services/configuracoes/estacao-service.js'

// import Loading  from '@/components/base/LoadingOverlay.vue'
import Snackbar from '@/components/base/Snackbar.vue'

Chart.register(...registerables)

// ── Injeções ─────────────────────────────────────────────────────────────────
const endpoint         = inject('endpoint')
const headerPadrao     = inject('headerPadrao')
const chaveSeguranca   = inject('chaveSeguranca')
const usuarioSeguranca = inject('usuarioSeguranca')

// ── Services ─────────────────────────────────────────────────────────────────
const dashboardService = new DashboardService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
const estacaoService   = new EstacaoService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)

// ── Estado global ─────────────────────────────────────────────────────────────
const retorno         = ref(false)
const mensagemRetorno = ref(null)
const sucesso         = ref(true)
const estacaoId       = ref(null)
const listaEstacoes   = ref([])

// ── Estado granular por bloco ─────────────────────────────────────────────────
const blocos = reactive({
  statusOs:        { loading: false, dados: null, erro: false },
  indicadores:     { loading: false, dados: null, erro: false },
  disponibilidade: { loading: false, dados: null, erro: false },
  motivacao:       { loading: false, dados: null, erro: false },
  custos:          { loading: false, dados: null, erro: false },
  estoque:         { loading: false, dados: null, erro: false },
  osAtrasadas:     { loading: false, dados: null, erro: false },
})

// ── Refs dos canvas ───────────────────────────────────────────────────────────
const canvasMttr      = ref(null)
const canvasMtbf      = ref(null)
const canvasMotivacao = ref(null)

let chartMttr      = null
let chartMtbf      = null
let chartMotivacao = null

// ── Helpers visuais ───────────────────────────────────────────────────────────
function corDisponibilidade(valor) {
  if (valor >= 95) return 'text-success'
  if (valor >= 80) return 'text-warning'
  return 'text-error'
}

function corProgressBar(valor) {
  if (valor >= 95) return 'success'
  if (valor >= 80) return 'warning'
  return 'error'
}

function corChipAtraso(dias) {
  if (dias >= 5) return 'error'
  if (dias >= 2) return 'warning'
  return 'default'
}

function formatCurrency(valor) {
  return (valor ?? 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

// ── Carregamento granular ─────────────────────────────────────────────────────
async function carregarBloco(chave, fn) {
  blocos[chave].loading = true
  blocos[chave].erro    = false

  const result = await fn()

  blocos[chave].loading = false

  if (result?.statusCode === 200) {
    blocos[chave].dados = result.data.data

    if (chave === 'indicadores') await nextTick(() => renderizarIndicadores())
    if (chave === 'motivacao')   await nextTick(() => renderizarMotivacao())
  } else {
    blocos[chave].erro = true
  }
}

// ── Recargas individuais ──────────────────────────────────────────────────────
function recarregarStatusOs()        { carregarBloco('statusOs',        () => dashboardService.obterStatusOs(estacaoId.value)) }
function recarregarIndicadores()     { carregarBloco('indicadores',     () => dashboardService.obterIndicadores(estacaoId.value)) }
function recarregarDisponibilidade() { carregarBloco('disponibilidade', () => dashboardService.obterDisponibilidade(estacaoId.value)) }
function recarregarMotivacao()       { carregarBloco('motivacao',       () => dashboardService.obterMotivacao(estacaoId.value)) }
function recarregarCustos()          { carregarBloco('custos',          () => dashboardService.obterCustos(estacaoId.value)) }
function recarregarEstoque()         { carregarBloco('estoque',         () => dashboardService.obterEstoque()) }
function recarregarOsAtrasadas()     { carregarBloco('osAtrasadas',     () => dashboardService.obterOsAtrasadas(estacaoId.value)) }

// ── Carga inicial paralela ────────────────────────────────────────────────────
async function carregarTodos() {
  await Promise.all([
    carregarBloco('statusOs',        () => dashboardService.obterStatusOs(estacaoId.value)),
    carregarBloco('indicadores',     () => dashboardService.obterIndicadores(estacaoId.value)),
    carregarBloco('disponibilidade', () => dashboardService.obterDisponibilidade(estacaoId.value)),
    carregarBloco('motivacao',       () => dashboardService.obterMotivacao(estacaoId.value)),
    carregarBloco('custos',          () => dashboardService.obterCustos(estacaoId.value)),
    carregarBloco('estoque',         () => dashboardService.obterEstoque()),
    carregarBloco('osAtrasadas',     () => dashboardService.obterOsAtrasadas(estacaoId.value)),
  ])
}

// ── Gráficos ──────────────────────────────────────────────────────────────────
function renderizarIndicadores() {
  const dados = blocos.indicadores.dados
  if (!dados || !canvasMttr.value || !canvasMtbf.value) return

  chartMttr?.destroy()
  chartMtbf?.destroy()

  const opcoesBase = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: {
      x: { grid: { display: false }, ticks: { font: { size: 10 } } },
      y: { beginAtZero: true, ticks: { font: { size: 10 }, callback: v => v + 'h' } }
    }
  }

  chartMttr = new Chart(canvasMttr.value, {
    type: 'line',
    data: {
      labels: dados.serieMttr.map(s => s.mes),
      datasets: [{
        data:                 dados.serieMttr.map(s => s.valor),
        borderColor:          '#43a047',
        backgroundColor:      'rgba(67,160,71,0.08)',
        tension:              0.4,
        fill:                 true,
        pointRadius:          3,
        pointBackgroundColor: '#43a047',
        borderWidth:          2,
      }]
    },
    options: opcoesBase
  })

  chartMtbf = new Chart(canvasMtbf.value, {
    type: 'line',
    data: {
      labels: dados.serieMtbf.map(s => s.mes),
      datasets: [{
        data:                 dados.serieMtbf.map(s => s.valor),
        borderColor:          '#fb8c00',
        backgroundColor:      'rgba(251,140,0,0.08)',
        tension:              0.4,
        fill:                 true,
        pointRadius:          3,
        pointBackgroundColor: '#fb8c00',
        borderWidth:          2,
      }]
    },
    options: opcoesBase
  })
}

function renderizarMotivacao() {
  const dados = blocos.motivacao.dados
  if (!dados || !canvasMotivacao.value) return

  chartMotivacao?.destroy()

  chartMotivacao = new Chart(canvasMotivacao.value, {
    type: 'bar',
    data: {
      labels: dados.serie.map(s => s.mes),
      datasets: [
        { label: 'Corretiva',  data: dados.serie.map(s => s.corretivas),  backgroundColor: '#ef9a9a', stack: 'motivacao' },
        { label: 'Preventiva', data: dados.serie.map(s => s.preventivas), backgroundColor: '#fb8c00', stack: 'motivacao' },
        { label: 'Preditiva',  data: dados.serie.map(s => s.preditivas),  backgroundColor: '#43a047', stack: 'motivacao' },
      ]
    },
    options: {
      responsive:          true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position:  'top',
          labels:    { font: { size: 11 }, boxWidth: 10, padding: 12 }
        }
      },
      scales: {
        x: { stacked: true, grid: { display: false }, ticks: { font: { size: 10 } } },
        y: { stacked: true, beginAtZero: true,         ticks: { font: { size: 10 } } },
      }
    }
  })
}

// ── Estações ──────────────────────────────────────────────────────────────────
async function listarEstacoes() {
  const result = await estacaoService.buscarEstacoes()
  if (result?.statusCode === 200)
    listaEstacoes.value = result?.data?.data || []
}

// ── Lifecycle ─────────────────────────────────────────────────────────────────
onMounted(async () => {
  await listarEstacoes()
  await carregarTodos()
})
</script>

<template>
  <v-container fluid>
    <Snackbar
      v-model="retorno"
      :mensagem="mensagemRetorno"
      :sucesso="sucesso"
      @fechar="retorno = false"
    />

    <!-- ── Cabeçalho ──────────────────────────────────────────────── -->
    <div class="d-flex align-center mb-3">
      <font-awesome-icon icon="magnifying-glass-chart" class="text-primary mr-2" />
      <div class="page-title">Dashboard de manutenção</div>

      <v-spacer />

      <v-select
        v-model="estacaoId"
        :items="listaEstacoes"
        item-title="nome"
        item-value="id"
        label="Filtrar por estação"
        density="compact"
        variant="outlined"
        clearable
        hide-details
        style="max-width: 260px"
        @update:modelValue="carregarTodos"
      />
    </div>

    <v-divider class="mb-4" />

    <!-- ── Bloco 1: Status OS ─────────────────────────────────────── -->
    <div class="section-header"><span>Status das ordens de serviço</span></div>

    <v-row dense class="mb-4">
      <v-col
        v-for="cfg in [
          { chave: 'statusOs', campo: 'abertas',     label: 'Abertas',      cor: 'text-error'   },
          { chave: 'statusOs', campo: 'emAndamento', label: 'Em andamento', cor: 'text-warning' },
          { chave: 'statusOs', campo: 'concluidas',  label: 'Concluídas',   cor: 'text-success' },
          { chave: 'statusOs', campo: 'atrasadas',   label: 'Atrasadas',    cor: 'text-error'   },
        ]"
        :key="cfg.campo"
        cols="6" md="3"
      >
        <v-card variant="outlined" class="kpi-card">
          <v-card-text class="card-body">
            <div class="d-flex justify-space-between align-center mb-1">
              <span class="kpi-label">{{ cfg.label }}</span>
              <v-btn
                icon size="x-small" variant="text"
                :loading="blocos.statusOs.loading"
                @click="recarregarStatusOs"
              >
                <font-awesome-icon icon="rotate-right" style="font-size:11px" />
              </v-btn>
            </div>

            <v-skeleton-loader v-if="blocos.statusOs.loading" type="heading" />

            <template v-else-if="blocos.statusOs.erro">
              <span class="text-error text-caption">Falha ao carregar</span>
            </template>

            <template v-else>
              <div class="kpi-value" :class="cfg.cor">
                {{ blocos.statusOs.dados?.[cfg.campo] ?? 0 }}
              </div>
            </template>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- ── Bloco 2: MTTR e MTBF ───────────────────────────────────── -->
    <div class="section-header"><span>Indicadores de performance</span></div>

    <v-row dense class="mb-4">
      <v-col cols="12" md="6">
        <v-card variant="outlined" class="main-card">
          <v-card-text class="card-body">
            <div class="d-flex justify-space-between align-center">
              <div>
                <div class="kpi-label">MTTR — Tempo médio de reparo</div>
                <div
                  class="kpi-value mt-1"
                  v-if="!blocos.indicadores.loading && !blocos.indicadores.erro"
                >
                  {{ blocos.indicadores.dados?.mttrAtual ?? 0 }}h
                </div>
              </div>
              <v-btn
                icon size="x-small" variant="text"
                :loading="blocos.indicadores.loading"
                @click="recarregarIndicadores"
              >
                <font-awesome-icon icon="rotate-right" style="font-size:11px" />
              </v-btn>
            </div>

            <v-skeleton-loader v-if="blocos.indicadores.loading" type="image" height="110" class="mt-2" />
            <div v-else class="chart-wrap mt-2">
              <canvas ref="canvasMttr"></canvas>
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card variant="outlined" class="main-card">
          <v-card-text class="card-body">
            <div class="d-flex justify-space-between align-center">
              <div>
                <div class="kpi-label">MTBF — Tempo médio entre falhas</div>
                <div
                  class="kpi-value mt-1"
                  v-if="!blocos.indicadores.loading && !blocos.indicadores.erro"
                >
                  {{ blocos.indicadores.dados?.mtbfAtual ?? 0 }}h
                </div>
              </div>
              <v-btn
                icon size="x-small" variant="text"
                :loading="blocos.indicadores.loading"
                @click="recarregarIndicadores"
              >
                <font-awesome-icon icon="rotate-right" style="font-size:11px" />
              </v-btn>
            </div>

            <v-skeleton-loader v-if="blocos.indicadores.loading" type="image" height="110" class="mt-2" />
            <div v-else class="chart-wrap mt-2">
              <canvas ref="canvasMtbf"></canvas>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- ── Bloco 3: Disponibilidade ───────────────────────────────── -->
    <div class="section-header"><span>Disponibilidade operacional</span></div>

    <v-row dense class="mb-4">
      <v-col cols="12" md="3">
        <v-card variant="outlined" class="kpi-card">
          <v-card-text class="card-body text-center">
            <div class="d-flex justify-space-between align-center">
              <span class="kpi-label">Disponibilidade geral</span>
              <v-btn
                icon size="x-small" variant="text"
                :loading="blocos.disponibilidade.loading"
                @click="recarregarDisponibilidade"
              >
                <font-awesome-icon icon="rotate-right" style="font-size:11px" />
              </v-btn>
            </div>

            <v-skeleton-loader v-if="blocos.disponibilidade.loading" type="heading" class="mt-2" />

            <template v-else-if="!blocos.disponibilidade.erro">
              <div
                class="disponibilidade-geral mt-3"
                :class="corDisponibilidade(blocos.disponibilidade.dados?.geral ?? 0)"
              >
                {{ blocos.disponibilidade.dados?.geral ?? 0 }}%
              </div>
              <div class="kpi-label mt-1">Meta: 95%</div>
            </template>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="9">
        <v-card variant="outlined" class="main-card">
          <v-card-text class="card-body">
            <v-skeleton-loader v-if="blocos.disponibilidade.loading" type="list-item-three-line" />

            <template v-else-if="blocos.disponibilidade.dados?.ativos?.length">
              <div
                v-for="ativo in blocos.disponibilidade.dados.ativos"
                :key="ativo.tag"
                class="mb-3"
              >
                <div class="d-flex justify-space-between align-center mb-1">
                  <span class="text-body-2">{{ ativo.nomeAtivo }} ({{ ativo.tag }})</span>
                  <span
                    class="text-body-2 font-weight-bold"
                    :class="corDisponibilidade(ativo.disponibilidade)"
                  >
                    {{ ativo.disponibilidade }}%
                  </span>
                </div>
                <v-progress-linear
                  :model-value="ativo.disponibilidade"
                  :color="corProgressBar(ativo.disponibilidade)"
                  height="10"
                  rounded
                  bg-color="grey-lighten-3"
                />
              </div>
            </template>

            <span v-else class="text-caption text-medium-emphasis">Nenhum ativo encontrado</span>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- ── Bloco 4: Motivação + OS Atrasadas ──────────────────────── -->
    <div class="section-header"><span>Motivação das OS e atrasos</span></div>

    <v-row dense class="mb-4">
      <v-col cols="12" md="6">
        <v-card variant="outlined" class="main-card">
          <v-card-text class="card-body">
            <div class="d-flex justify-space-between align-center mb-2">
              <span class="kpi-label">Motivação mensal — últimos 6 meses</span>
              <v-btn
                icon size="x-small" variant="text"
                :loading="blocos.motivacao.loading"
                @click="recarregarMotivacao"
              >
                <font-awesome-icon icon="rotate-right" style="font-size:11px" />
              </v-btn>
            </div>

            <v-skeleton-loader v-if="blocos.motivacao.loading" type="image" height="180" />
            <div v-else class="chart-wrap-tall">
              <canvas ref="canvasMotivacao"></canvas>
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card variant="outlined" class="main-card">
          <v-card-text class="card-body">
            <div class="d-flex justify-space-between align-center mb-2">
              <span class="kpi-label">OS atrasadas</span>
              <v-btn
                icon size="x-small" variant="text"
                :loading="blocos.osAtrasadas.loading"
                @click="recarregarOsAtrasadas"
              >
                <font-awesome-icon icon="rotate-right" style="font-size:11px" />
              </v-btn>
            </div>

            <v-skeleton-loader v-if="blocos.osAtrasadas.loading" type="table-row@4" />

            <v-table v-else-if="blocos.osAtrasadas.dados?.length" density="compact">
              <thead>
                <tr>
                  <th>OS</th>
                  <th>Ativo</th>
                  <th>Atraso</th>
                  <th>Tipo</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="os in blocos.osAtrasadas.dados" :key="os.numero">
                  <td>{{ os.numero }}</td>
                  <td>{{ os.nomeAtivo }}</td>
                  <td>
                    <v-chip
                      :color="corChipAtraso(os.diasAtraso)"
                      size="small"
                      variant="tonal"
                      density="comfortable"
                    >
                      {{ os.diasAtraso }}d
                    </v-chip>
                  </td>
                  <td>
                    <v-chip size="small" variant="tonal" density="comfortable">
                      {{ os.motivacao }}
                    </v-chip>
                  </td>
                </tr>
              </tbody>
            </v-table>

            <span v-else-if="!blocos.osAtrasadas.loading" class="text-caption text-medium-emphasis">
              Nenhuma OS atrasada
            </span>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- ── Bloco 5: Custos + Estoque ──────────────────────────────── -->
    <div class="section-header"><span>Custos e estoque</span></div>

    <v-row dense>
      <v-col cols="12" sm="6" md="3">
        <v-card variant="outlined" class="kpi-card">
          <v-card-text class="card-body">
            <div class="d-flex justify-space-between align-center mb-1">
              <span class="kpi-label">Custo total do mês</span>
              <v-btn
                icon size="x-small" variant="text"
                :loading="blocos.custos.loading"
                @click="recarregarCustos"
              >
                <font-awesome-icon icon="rotate-right" style="font-size:11px" />
              </v-btn>
            </div>

            <v-skeleton-loader v-if="blocos.custos.loading" type="heading" />

            <template v-else-if="!blocos.custos.erro">
              <div class="kpi-value" style="font-size: 20px">
                {{ formatCurrency(blocos.custos.dados?.totalMes) }}
              </div>
              <div class="kpi-label mt-1">
                Média por OS: {{ formatCurrency(blocos.custos.dados?.medioPorOs) }}
              </div>
            </template>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card variant="outlined" class="kpi-card">
          <v-card-text class="card-body">
            <div class="d-flex justify-space-between align-center mb-1">
              <span class="kpi-label">Rupturas de estoque</span>
              <v-btn
                icon size="x-small" variant="text"
                :loading="blocos.estoque.loading"
                @click="recarregarEstoque"
              >
                <font-awesome-icon icon="rotate-right" style="font-size:11px" />
              </v-btn>
            </div>

            <v-skeleton-loader v-if="blocos.estoque.loading" type="heading" />

            <template v-else-if="!blocos.estoque.erro">
              <div class="kpi-value text-error">
                {{ blocos.estoque.dados?.rupturas ?? '—' }}
              </div>
              <div class="kpi-label mt-1 text-warning">
                {{ blocos.estoque.dados?.abaixoMinimo ?? 0 }} abaixo do mínimo
              </div>
            </template>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

  </v-container>
</template>

<style scoped>
/* ── Cabeçalho ──────────────────────────────────────────────────────── */
.page-title {
  font-size: 18px;
  font-weight: 600;
  line-height: 1.1;
}

/* ── Section header — padrão idêntico ao EquipamentoDetalhe.vue ────── */
.section-header {
  display: flex;
  align-items: center;
  gap: 10px;
  font-weight: 600;
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: rgba(0, 0, 0, 0.55);
  margin: 0 0 10px 0;
}

.section-header::after {
  content: "";
  flex: 1;
  height: 1px;
  background: rgba(0, 0, 0, 0.08);
}

/* ── Cards ──────────────────────────────────────────────────────────── */
.main-card  { height: 100%; }
.kpi-card   { height: 100%; }

.card-body {
  padding: 12px 14px !important;
}

/* ── Tipografia KPI ─────────────────────────────────────────────────── */
.kpi-label {
  font-size: 12px;
  font-weight: 500;
  color: rgba(0, 0, 0, 0.55);
}

.kpi-value {
  font-size: 28px;
  font-weight: 600;
  line-height: 1.15;
  letter-spacing: -0.5px;
}

.disponibilidade-geral {
  font-size: 40px;
  font-weight: 700;
  line-height: 1;
  letter-spacing: -1px;
}

/* ── Canvas Chart.js ────────────────────────────────────────────────── */
.chart-wrap      { position: relative; height: 110px; }
.chart-wrap-tall { position: relative; height: 190px; }

/* ── Barras de disponibilidade ─────────────────────────────────────── */
:deep(.v-progress-linear) {
  border-radius: 4px;
}

/* ── Tabela OS atrasadas ────────────────────────────────────────────── */
:deep(.v-table) {
  font-size: 12px;
}

:deep(.v-table th) {
  font-size: 11px !important;
  font-weight: 600 !important;
  color: rgba(0, 0, 0, 0.55) !important;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
</style>