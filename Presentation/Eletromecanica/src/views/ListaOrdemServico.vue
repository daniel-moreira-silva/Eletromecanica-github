<script setup>
/* global defineProps */
import { ref, computed, watch, inject } from 'vue'
import Grid from '@/components/common/GridComponent.vue'
import Paginacao from '@/components/common/PaginacaoComponent.vue'
import BaseButton from '@/components/base/BaseButton.vue'
//import { QuillEditor } from '@vueup/vue-quill'
import { useRouter } from 'vue-router'
import OcorrenciaService from '@/services/ordem-servico/nova-ordem-servico'
import Loading from '@/components/base/LoadingOverlay.vue'
import Snackbar from '@/components/base/Snackbar.vue'

const endpoint = inject('endpoint')
const headerPadrao = inject('headerPadrao')
const chaveSeguranca = inject('chaveSeguranca')
const usuarioSeguranca = inject('usuarioSeguranca')

const ocorrenciaService = new OcorrenciaService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
)

const props = defineProps({
  status: { type: String, required: false },
  count: { type: Number, required: false },
  readonly: { type: Boolean, required: false },
})

const ocorrencias = ref([])
const iniciarDialog = ref(false)
const confirmDialog = ref(false)
const cancelDialog = ref(false)
const currentItem = ref({ numero: '', endereco: '', servico: '' })
const confirmationNumber = ref('')
const sendConfirmDialog = ref(false)
const modalFilter = ref(false)

const selectedTeam = ref(null)
const selectedAdutora = ref(null)
const teamOptions = ['Equipe Alpha', 'Equipe Beta', 'Equipe Gamma']
const adutoraOptions = ['Adutora Norte', 'Adutora Sul', 'Adutora Leste']

// Conta quantas linhas estão marcadas (campo 'selecionado' na grid)
const selectedCount = computed(() =>
  ocorrencias.value.filter(i => i.selecionado).length
)

// Único botão “Ação”, do tipo menu, desabilitado quando não há seleção
const customButtonsList = computed(() => {
  const disabled = selectedCount.value === 0
  // monta as opções internas de menu (mesmas que antes eram botões separados)
  let opcoesMenu = []
  if (props.status === 'Solicitadas') {
    opcoesMenu = [
      { descricao: 'Enviar para equipe', icone: 'paper-plane', type: 'send' },
      { descricao: 'Enviar para destinatário', icone: 'share-square', type: 'sendToDest' },
      { descricao: 'Despacho programado', icone: 'calendar-alt', type: 'schedule' },
      { descricao: 'Imprimir selecionado', icone: 'print', type: 'print' }
    ]
  }
  else if (props.status === 'Distribuídas') {
    opcoesMenu = [
      { descricao: 'Devolver ocorrência', icone: 'reply', type: 'return' },
      { descricao: 'Imprimir selecionado', icone: 'print', type: 'print' }
    ]
  }
  else {
    opcoesMenu = [
      { descricao: 'Imprimir selecionado', icone: 'print', type: 'print' }
    ]
  }

  return [
    {
      // aqui alteramos apenas a label visível
      customButtonDescription: 'Ações para ocorrências',
      customButtonIcon: 'ellipsis-vertical',
      type: 'menu',
      enableIfHasSelectedItems: disabled,
      opcoesMenu
    }
  ]
})


// grid definitions
const fields = [
  { descricao: '', valor: 'selecionado', tipo: 'checkbox', filtravel: false, ordenado: null, selecionado: null },
  { descricao: 'Nº Ocorrência', valor: 'codigo', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Endereço', valor: 'endereco', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Bairro', valor: 'bairro', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Serviço', valor: 'servicoSolicitado', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Data', valor: 'dataFormatada', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Equipe', valor: 'equipe', tipo: 'texto', filtravel: false, ordenado: null },
  { descricao: 'Status', valor: 'status', tipo: 'texto', filtravel: false, ordenado: null },
  {
    descricao: 'Ações', valor: 'ellipsis', tipo: 'menu', filtravel: false, ordenado: null, class: 'text-left',
    opcoesMenu: props.readonly ? [
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' }
    ] : [
      { descricao: 'Iniciar ordem serviço', icone: 'share-square', classe: 'text-left' },
      { descricao: 'Detalhar ordem serviço', icone: 'info-circle', classe: 'text-left' },
      { descricao: 'Histórico', icone: 'history', classe: 'text-left' },
      { descricao: 'Arquivos', icone: 'file-alt', classe: 'text-left' },
      { descricao: 'Finalizar ordem serviço', icone: 'flag-checkered', classe: 'text-left' }
    ]
  }
]

const filtros = ref({
  pagina: 1,
  itensPagina: 10,
  numero: null,
  equipe: null
})

// events
const router = useRouter()
async function handleOptionClick({ item, opcao }) {
  if (opcao.descricao === 'Iniciar ordem serviço') {
    currentItem.value = item
    iniciarDialog.value = true
  } else if (opcao.descricao === 'Detalhar ordem serviço') {
    // navega para a página de detalhes, passando o número da ordem serviço
    await router.push({ name: 'DetalharOrdemServico', params: { id: item.id } })
  }
}

function filtrar() {
  filtros.value.pagina = 1
  modalFilter.value = false
  listarItens();
}

function limparFiltro() {
  filtros.value.pagina = 1
  filtros.value.numero = null
  filtros.value.equipe = null
}

function closeModal() {
  iniciarDialog.value = false
  confirmDialog.value = false
}

function submitStart() {
  confirmationNumber.value = currentItem.value.numero
  confirmDialog.value = true
  iniciarDialog.value = false
  resetForm()
}

function resetForm() {
  selectedTeam.value = null
  selectedAdutora.value = null

}

function prepareConfirmSend() {
  confirmationNumber.value = currentItem.value.numero
  sendConfirmDialog.value = true
}

// ação confirmada no diálogo de confirmação
function confirmSend() {
  sendConfirmDialog.value = false
  submitStart()
}

function handleCancelConfirmed() {
  // limpa formulário
  selectedTeam.value = null
  selectedAdutora.value = null

  // fecha dialogs
  iniciarDialog.value = false
  cancelDialog.value = false
}

const searchEmp = ref('')
const selectedEmployees = ref([])

const canSend = computed(() =>
  !!selectedTeam.value && selectedEmployees.value.length > 0
)
const teamEmployeesMap = {
  'Equipe Alpha': ['Amauri Silva', 'Bruno Costa', 'Carlos Dias'],
  'Equipe Beta': ['Diego Ferreira', 'Evandro Gomes', 'Fernando Lopes'],
  'Equipe Gamma': ['Gabriel Rocha', 'Henrique Martins', 'Mario Cruz Cruz']
}

// todos os funcionários disponíveis
const allEmployees = Object.values(teamEmployeesMap).flat()

// Ao selecionar equipe, pré-marca funcionários e reseta filtro
watch(selectedTeam, team => {

  selectedEmployees.value = team ? (teamEmployeesMap[team] || []) : []
  searchEmp.value = ''
})

function removeEmployee(emp) {
  selectedEmployees.value = selectedEmployees.value.filter(e => e !== emp)
}

const mensagemRetorno = ref(null)
const sucesso = ref(true)
const retorno = ref(null)
const totalPaginas = ref(null);
const totalItems = ref(null);
const loading = ref(true);
async function listarItens() {
  loading.value = true
  const result = await ocorrenciaService.listar(filtros.value)
  loading.value = false
  if (result?.statusCode === 200) {
    ocorrencias.value = (result?.data?.data.lista || []).map((x) => ({ ...x, dataFormatada: new Date(x.dataSolicitacao).toLocaleDateString('pt-BR')}))
    totalPaginas.value = result.data.data.paginas
    totalItems.value = result.data.data.totalItens
  } else {
    mensagemRetorno.value = result.data.message
    sucesso.value = false
    retorno.value = true
  }
}

// inicialização
listarItens()

</script>

<template>
  <div>
    <Loading :active="loading" />
    <Grid :fields="fields" :list="ocorrencias" :filters="filtros" gridType="responsive" filterType="popup"
      gridOverflow="horizontal" :gridResizable="false" :hasCheckbox="true" :customButtonsList="customButtonsList"
      @listarItens="listarItens($event)" @botaoOpcaoClick="handleOptionClick"
      @abrirFiltro="modalFilter = true" />
    <Paginacao :totalPaginas="totalPaginas" :paginaAtual="filtros.pagina"
      :totalItens="totalItems" @alterarItensPorPagina="val => filtros.value.itensPagina = val"
      @alterarPagina="val => filtros.value.pagina = val" />

    <!-- Filter Modal -->
    <v-dialog v-model="modalFilter" max-width="600" persistent scrim="rgba(0,0,0,0.7)">
      <v-card>
        <v-card-text class="pa-4">
          <div class="d-flex align-center mb-2">
            <font-awesome-icon icon="search" class="text-primary me-2" />
            <span class="text-h6">Filtrar ocorrências</span>
            <v-spacer />
            <font-awesome-icon icon="xmark" class="cursor-pointer" @click="modalFilter = false" />
          </div>
          <v-divider />
          <v-form>
            <v-container class="pa-3">
              <v-row>
                <v-col cols="12" class="pb-4">
                  <v-text-field density="comfortable" v-model="filtros.codigo" label="Nº Ocorrência" clearable hide-details
                    variant="outlined" />
                </v-col>
                <v-col cols="12">
                  <v-autocomplete density="comfortable" v-model="filtros.equipe" :items="teamOptions" label="Equipe" clearable dense
                    variant="outlined" />
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

    <!-- Modal Iniciar Ocorrência -->
    <v-dialog v-model="iniciarDialog" max-width="800" height="100%" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-4">
        <v-card-title>
          <font-awesome-icon icon="play" class="text-primary me-2" />
          Iniciar Ocorrência
          <font-awesome-icon icon="xmark" @click="closeModal" class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-0" />
        <v-card-text>
          <p class="mb-1"><strong>Nº O.S:</strong> {{ currentItem.numero }}</p>
          <p class="mb-1"><strong>Serviço Solicitado:</strong> {{ currentItem.servico }}</p>
          <p class="mb-1"><strong>Endereço:</strong> {{ currentItem.endereco }}</p>
          <v-divider class="mx-2 my-2" />
          <v-autocomplete v-model="selectedTeam" :items="teamOptions" label="Equipe" variant="solo" clearable
            density="comfortable" single-line hide-details class="selecao-equipe mb-3" />
          <v-autocomplete v-model="selectedAdutora" :items="adutoraOptions" label="Adutora" variant="solo" clearable
            density="comfortable" single-line hide-details class="selecao-equipe" />
          <v-row class="box-listas" v-if="selectedTeam">
            <v-col cols="12">
              <div class="mb-2"><strong>Todos os funcionários</strong></div>
              <!-- campo de filtro -->
              <v-text-field v-model="searchEmp" clearable dense variant="underlined" hide-details
                placeholder="Filtrar funcionários">
                <template #prepend-inner>
                  <font-awesome-icon icon="search" class="text-primary me-2" />
                </template>
              </v-text-field>
              <!-- chips com ícone de fechar -->
              <div class="d-flex flex-wrap gap-2 mb-3">
                <v-chip v-for="emp in selectedEmployees" :key="emp" closable @click:close="removeEmployee(emp)"
                  class="chips-autocomplete">
                  {{ emp }}
                </v-chip>
              </div>
              <!-- lista filtrada de checkboxes -->
              <div style="max-height:180px; overflow-y:auto;">
                <v-checkbox
                  v-for="emp in allEmployees.filter(e => e.toLowerCase().includes((searchEmp || '').toLowerCase()))"
                  :key="emp" v-model="selectedEmployees" :value="emp" :label="emp" dense hide-details class="mb-0" />
              </div>
            </v-col>
          </v-row>
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Cancelar ocorrência" type="cancel" @click="cancelDialog = true" />
          <BaseButton label="Programar ocorrência" type="time" :disabled="!canSend" />
          <BaseButton label="Enviar ocorrência" type="save" :disabled="!canSend" @click="prepareConfirmSend" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal Confirmar Envio -->
    <v-dialog v-model="sendConfirmDialog" max-width="450" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-2">
        <v-card-title>
          <font-awesome-icon icon="paper-plane" class="text-primary me-4" />
          <span class="text-h6">Confirmar envio?</span>
          <font-awesome-icon icon="xmark" @click="sendConfirmDialog = false"
            class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-4" />
        <v-card-text class="pb-4">
          <p>Deseja realmente enviar a ocorrência <strong>{{ confirmationNumber }}</strong> para a equipe: <strong>{{
            selectedTeam }}</strong>?</p>
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Não" type="cancel" extraClass="me-2" @click="sendConfirmDialog = false" />
          <BaseButton label="Sim, enviar" type="confirm" @click="confirmSend" />
        </v-card-actions>
      </v-card>
    </v-dialog>


    <!-- Modal de Confirmação -->
    <v-dialog v-model="confirmDialog" max-width="450" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-2">
        <v-card-title>
          <font-awesome-icon icon="clipboard-check" class="text-primary me-2" />
          <span class="text-h6">Ocorrência enviada!</span>
          <font-awesome-icon icon="xmark" @click="closeModal" class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-4" />
        <v-card-text class="pb-4">
          <p><strong>Ocorrência enviada à equipe: </strong> {{ selectedTeam }}</p>
          <p><strong>Nº O.S:</strong> {{ confirmationNumber }}</p>
          <p><strong>Endereço:</strong> {{ currentItem.endereco }}</p>
          <p><strong>Tipo serviço:</strong> {{ currentItem.servico }}</p>
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Fechar" type="confirm" @click="closeModal" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal de Cancelamento -->
    <v-dialog v-model="cancelDialog" max-width="450" persistent scrim="rgba(0,0,0,0.7)">
      <v-card class="pa-2">
        <v-card-title>
          <font-awesome-icon icon="times-circle" class="text-primary me-2" />
          <span class="text-h6">Cancelar ocorrência?</span>
          <font-awesome-icon icon="xmark" @click="cancelDialog = false"
            class="text-close float-right icon-clicavel me-2" />
        </v-card-title>
        <v-divider class="pb-4" />
        <v-card-text class="pb-4">
          <p>Deseja realmente cancelar esta ocorrência?</p>
        </v-card-text>
        <v-divider class="pb-4" />
        <v-card-actions class="justify-end">
          <BaseButton label="Não" type="cancel" extraClass="me-2" @click="cancelDialog = false" />
          <BaseButton label="Sim, cancelar" type="confirm" @click="handleCancelConfirmed" />
        </v-card-actions>
      </v-card>
    </v-dialog>
    <Snackbar :retorno="retorno" :timeout="3000" :tipo="sucesso ? 'sucesso' : 'erro'"
        :mensagemRetorno="mensagemRetorno" @ocultarRetorno="() => { retorno = false }" />
  </div>
</template>

<style scoped>
.grid-component {
  height: calc(100vh - 215px) !important;
}

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