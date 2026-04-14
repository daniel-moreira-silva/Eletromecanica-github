<script setup>
import { ref, inject, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Grid from '@/components/common/GridComponent.vue'
import Paginacao from '@/components/common/PaginacaoComponent.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import Loading from '@/components/base/LoadingOverlay.vue'
import Snackbar from '@/components/base/Snackbar.vue'
import OrdemServicoService from '@/services/ordem-servico/ordem-servico-service'

const endpoint = inject('endpoint')
const headerPadrao = inject('headerPadrao')
const chaveSeguranca = inject('chaveSeguranca')
const usuarioSeguranca = inject('usuarioSeguranca')

const router = useRouter()

const ordemServicoService = new OrdemServicoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
)

// ── state ─────────────────────────────────────────────────────────────────────
const loading = ref(false)
const modalFilter = ref(false)
const mensagemRetorno = ref(null)
const sucesso = ref(true)
const retorno = ref(null)

const lista = ref([])
const totalPaginas = ref(0)
const totalItens = ref(0)
const pagina = ref(1)
const itensPagina = ref(10)

const filtros = ref({ numero: null, todos: null })
const filtrosAplicados = ref({})

// ── grid setup ────────────────────────────────────────────────────────────────
const fields = [
  { descricao: 'Nº O.S.', valor: 'codigo', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Data', valor: 'dataFormatada', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Status', valor: 'status', tipo: 'texto', filtravel: false, ordenado: null },
  {
    descricao: 'Ações',
    valor: 'ellipsis',
    tipo: 'menu',
    filtravel: false,
    ordenado: null,
    opcoesMenu: [
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
    ],
  },
]

// ── fetch ─────────────────────────────────────────────────────────────────────
async function listarItens() {
  if (loading.value) return
  loading.value = true

  const result = await ordemServicoService.listar({
    ...filtrosAplicados.value,
    statusId: [],
    pagina: pagina.value,
    itensPagina: itensPagina.value,
  })

  loading.value = false

  if (result?.statusCode === 200) {
    lista.value = (result.data.data.lista || []).map(x => ({
      ...x,
      dataFormatada: new Date(x.dataSolicitacao).toLocaleDateString('pt-BR'),
    }))
    totalPaginas.value = result.data.data.paginas
    totalItens.value = result.data.data.totalItens
  } else {
    mensagemRetorno.value = result?.data?.message || 'Erro ao carregar ordens de serviço.'
    sucesso.value = false
    retorno.value = true
    lista.value = []
  }
}

onMounted(listarItens)

// ── pagination ────────────────────────────────────────────────────────────────
function alterarPagina(val) {
  pagina.value = val
  listarItens()
}

function alterarItensPorPagina(val) {
  itensPagina.value = val
  pagina.value = 1
  listarItens()
}

// ── filter ────────────────────────────────────────────────────────────────────
function filtrar() {
  filtrosAplicados.value = {
    numero: filtros.value.numero || null,
    todos: filtros.value.todos || null,
  }
  pagina.value = 1
  modalFilter.value = false
  listarItens()
}

function limparFiltro() {
  filtros.value = { numero: null, todos: null }
}

// ── row actions ───────────────────────────────────────────────────────────────
async function handleOptionClick({ item, opcao }) {
  if (opcao.descricao === 'Detalhar ordem serviço') {
    await router.push({ name: 'DetalharOrdemServico', params: { id: item.id } })
  }
}
</script>

<template>
  <div class="pa-4">
    <Loading :active="loading" />

    <Grid
      :fields="fields"
      :list="lista"
      :filters="{ pagina, itensPagina }"
      filterType="popup"
      gridOverflow="horizontal"
      :gridResizable="false"
      :hasCheckbox="false"
      @botaoOpcaoClick="handleOptionClick"
      @abrirFiltro="modalFilter = true"
    />

    <Paginacao
      :totalPaginas="totalPaginas"
      :paginaAtual="pagina"
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
                    v-model="filtros.numero"
                    label="Nº O.S."
                    clearable
                    hide-details
                    variant="outlined"
                  />
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    density="comfortable"
                    v-model="filtros.todos"
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
.grid-component {
  height: calc(100vh - 194px) !important;
}
</style>
