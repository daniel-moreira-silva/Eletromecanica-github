<script setup>
import { ref, computed, watch, reactive, inject } from 'vue'
import { QuillEditor } from '@vueup/vue-quill'
import { GoogleMap, AdvancedMarker } from 'vue3-google-map'
import BaseButton from '@/components/base/BaseButton.vue'
import LoadingOverlay from '@/components/base/LoadingOverlay.vue'
import NovaOrdemServicoService from '@/services/ordem-servico/nova-ordem-servico'
import ServicoSolicitadoService from '@/services/configuracoes/servico-solicitado-service'
import EstacaoService from "@/services/configuracoes/estacao-service.js"
import EquipamentoService from "@/services/configuracoes/equipamento-service.js"
import Snackbar from '@/components/base/Snackbar.vue'

const endpoint = inject('endpoint')
const headerPadrao = inject('headerPadrao')
const chaveSeguranca = inject('chaveSeguranca')
const usuarioSeguranca = inject('usuarioSeguranca')
const apiKey = inject('apiKeyMaps')

const ordemServicoService = new NovaOrdemServicoService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
const servicoSolicitadoService = new ServicoSolicitadoService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
const estacaoService = new EstacaoService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
const equipamentoService = new EquipamentoService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)

const step = ref(1)
const loading = ref(false)

const retorno = ref(false)
const mensagemRetorno = ref(null)
const sucesso = ref(true)

const showConfirmationDialog = ref(false)
const confirmationNumber = ref('')

const fallbackCenter = ref({ lat: -21.7617922, lng: -43.3439923 })
const fallbackZoom = 12

/**
 * STEP 1: Estação + Equipamentos + Serviços
 */
const estacaoSelecionada = ref(null)
const estacoesOptions = ref([])

const estacaoEndereco = reactive({
  endereco: '',
  numero: '',
  bairro: '',
  complemento: '',
  pontoReferencia: '',
  lat: '',
  long: ''
})

const equipamentosSelecionados = ref([])
const equipamentosOptions = ref([])

const servicosSelecionados = ref([])
const servicosOptions = ref([])

const menuEquipamentos = ref(false)
const menuServicos = ref(false)

async function listarEstacoes() {
  loading.value = true
  const result = await estacaoService.buscarEstacoes()
  loading.value = false

  if (result?.statusCode === 200) {
    estacoesOptions.value = result?.data?.data || []
  } else {
    mensagemRetorno.value = result?.data?.message || 'Falha ao listar estações.'
    sucesso.value = false
    retorno.value = true
  }
}

async function listarEquipamentosPorEstacao(estacaoId) {
  if (!estacaoId) return

  loading.value = true
  const result = await equipamentoService.listarEquipamentosPorEstacao(estacaoId, null)
  loading.value = false

  if (result?.statusCode === 200) {
    equipamentosOptions.value = (result?.data?.data || []).map((x) => ({
      ...x,
      tagnome: `${x.tag} | ${x.nome}`
    }))
  } else {
    mensagemRetorno.value = result?.data?.message || 'Falha ao listar equipamentos da estação.'
    sucesso.value = false
    retorno.value = true
  }
}

async function listarServicosSolicitados() {
  loading.value = true
  const result = await servicoSolicitadoService.buscarTodos(false)
  loading.value = false

  if (result?.statusCode === 200) {
    servicosOptions.value = result?.data?.data || []
  } else {
    mensagemRetorno.value = result?.data?.message || 'Falha ao listar serviços solicitados.'
    sucesso.value = false
    retorno.value = true
  }
}

watch(menuEquipamentos, (abriu) => {
  if (abriu) menuServicos.value = false
})

watch(menuServicos, (abriu) => {
  if (abriu) menuEquipamentos.value = false
})

/**
 * STEP 2: Localização da estação
 */
const mapMarkers = ref([])
const zoom = ref(null)
const center = ref(null)

const modalDetalheMapa = ref(false)
const modalMarker = ref({
  type: 'endereco',
  title: '',
  subtitle: '',
  fields: [],
  servicos: [],
  servicosTotal: 0
})

function generateMapMarkersFromEstacao() {
  if (!estacaoEndereco.lat || !estacaoEndereco.long) {
    mapMarkers.value = []
    center.value = fallbackCenter.value
    zoom.value = fallbackZoom
    return
  }

  const lat = parseFloat(estacaoEndereco.lat)
  const lng = parseFloat(estacaoEndereco.long)

  if (Number.isNaN(lat) || Number.isNaN(lng)) {
    mapMarkers.value = []
    center.value = fallbackCenter.value
    zoom.value = fallbackZoom
    return
  }

  mapMarkers.value = [
    {
      id: 'estacao',
      lat,
      lng,
      icon: 'map-marker-alt',
      class: 'map-marker-selected',
      details: { ...estacaoEndereco },
      type: 'endereco'
    }
  ]

  center.value = { lat, lng }
  zoom.value = 15
}

watch(estacaoSelecionada, async (nova) => {
  equipamentosSelecionados.value = []
  equipamentosOptions.value = []

  Object.assign(estacaoEndereco, {
    endereco: nova?.endereco || '',
    numero: nova?.numero || '',
    bairro: nova?.bairro || '',
    complemento: nova?.complemento || '',
    pontoReferencia: nova?.pontoReferencia || '',
    lat: nova?.lat || '',
    long: nova?.long || ''
  })

  generateMapMarkersFromEstacao()

  if (nova?.id) {
    await listarEquipamentosPorEstacao(nova.id)
  }
})

function safeStr(v) {
  return v === null || v === undefined ? '' : String(v)
}

function buildEnderecoFields(details = {}) {
  return [
    { label: 'Endereço', value: safeStr(details.endereco) },
    { label: 'Número', value: safeStr(details.numero) },
    { label: 'Bairro', value: safeStr(details.bairro) },
    { label: 'Complemento', value: safeStr(details.complemento) },
    { label: 'Ponto de referência', value: safeStr(details.pontoReferencia) }
  ].filter(x => x.value)
}

function openMarkerModal(marker) {
  if (!marker?.type) return

  const type = marker.type
  const details = marker.details ? JSON.parse(JSON.stringify(marker.details)) : {}

  modalMarker.value = {
    type,
    title: 'Detalhes da localização',
    subtitle: safeStr(details.endereco),
    fields: buildEnderecoFields(details),
    servicos: [],
    servicosTotal: 0
  }

  modalDetalheMapa.value = true
}

function closeMarkerModal() {
  modalDetalheMapa.value = false
}

function handleMarkerClick(marker) {
  openMarkerModal(marker)
}

watch(step, () => {
  if (modalDetalheMapa.value) closeMarkerModal()
})

/**
 * STEP 3: Solicitante
 */
const claimant = reactive({
  name: '',
  phone: '',
  cpf: '',
  email: '',
  complement: '',
  reference: ''
})

/**
 * STEP 4: Observações
 */
const observacoes = ref('')

/**
 * STEP 5: Resumo
 */
const resumo = computed(() => ({
  estacao: estacaoSelecionada.value,
  equipamentos: equipamentosSelecionados.value || [],
  servicos: servicosSelecionados.value || []
}))

/**
 * Validação por step
 */
const podeAvancar = computed(() => {
  if (step.value === 1) {
    const hasEstacao = Boolean(estacaoSelecionada.value?.id)
    const hasServicos = Array.isArray(servicosSelecionados.value) && servicosSelecionados.value.length > 0
    return hasEstacao && hasServicos
  }

  if (step.value === 2) {
    return Boolean(estacaoSelecionada.value?.id)
  }

  if (step.value === 3) {
    return Boolean(claimant.name && claimant.cpf)
  }

  if (step.value === 4) {
    return true
  }

  return false
})

const stepEditable = (n) => n < step.value || (n === step.value + 1 && podeAvancar.value)

function avancar() {
  if (podeAvancar.value) step.value++
}

function voltar() {
  if (step.value > 1) step.value--
}

function removerEquipamento(id) {
  equipamentosSelecionados.value = equipamentosSelecionados.value.filter(x => x.id !== id)
}

function limparEquipamentosSelecionados() {
  equipamentosSelecionados.value = []
}

function removerServico(id) {
  servicosSelecionados.value = servicosSelecionados.value.filter(x => x.id !== id)
}

function limparServicosSelecionados() {
  servicosSelecionados.value = []
}

function onlyDigits(v = '') {
  return (v || '').toString().replace(/\D/g, '')
}

function buildTelefoneE164BR(rawPhone = '') {
  const digits = onlyDigits(rawPhone)
  if (!digits) return ''
  return `+55${digits}`
}

/**
 * Resets
 */
function resetStep1() {
  estacaoSelecionada.value = null
  equipamentosSelecionados.value = []
  equipamentosOptions.value = []
  servicosSelecionados.value = []
}

function resetStep2() {
  Object.assign(estacaoEndereco, {
    endereco: '',
    numero: '',
    bairro: '',
    complemento: '',
    pontoReferencia: '',
    lat: '',
    long: ''
  })

  mapMarkers.value = []
  center.value = null
  zoom.value = null
}

function resetStep3() {
  Object.assign(claimant, {
    name: '',
    phone: '',
    cpf: '',
    email: '',
    complement: '',
    reference: ''
  })
}

function resetStep4() {
  observacoes.value = ''
}

/**
 * Salvar OS
 */
async function salvar() {
  const payload = {
    EstacaoId: estacaoSelecionada.value?.id,

    Equipamentos: (equipamentosSelecionados.value || []).map(e => ({
      OrdemServicoId: null,
      EquipamentoId: e.id
    })),

    servicosSolicitados: (servicosSelecionados.value || []).map(s => ({
      servicoSolicitadoId: s.id
    })),

    nome: claimant.name,
    telefone: buildTelefoneE164BR(claimant.phone),
    numeroDocumento: claimant.cpf,
    email: claimant.email,
    observacao: observacoes.value
  }

  loading.value = true
  const result = await ordemServicoService.criarOrdemServico(payload)
  loading.value = false

  if (result?.statusCode === 200) {
    confirmationNumber.value = result?.data?.data?.codigo || ''
    showConfirmationDialog.value = true

    step.value = 1
    resetStep1()
    resetStep2()
    resetStep3()
    resetStep4()
  } else {
    mensagemRetorno.value = result?.data?.message || 'Falha ao criar Ordem de Serviço.'
    sucesso.value = false
    retorno.value = true
  }
}

listarEstacoes()
listarServicosSolicitados()
</script>

<template>
  <LoadingOverlay :active="loading" />

  <v-container fluid class="nova-ocorrencia__container">
    <v-stepper v-model="step" class="flex-grow-1 d-flex flex-column">
      <v-stepper-header>
        <v-stepper-item :value="1" title="Estação, equipamentos e serviços" :editable="stepEditable(1)" :disabled="!stepEditable(1)" :complete="step > 1" />
        <v-stepper-item :value="2" title="Localização da estação" :editable="stepEditable(2)" :disabled="!stepEditable(2)" :complete="step > 2" />
        <v-stepper-item :value="3" title="Solicitante" :editable="stepEditable(3)" :disabled="!stepEditable(3)" :complete="step > 3" />
        <v-stepper-item :value="4" title="Observações" :editable="stepEditable(4)" :disabled="!stepEditable(4)" :complete="step > 4" />
        <v-stepper-item :value="5" title="Visão geral" :editable="stepEditable(5)" :disabled="!stepEditable(5)" :complete="step > 5" />
      </v-stepper-header>

      <v-stepper-window class="flex-grow-1 overflow-y-auto mx-3">
        <v-stepper-window-item :value="1">
          <v-row class="justify-center">
            <v-col cols="12" md="7" class="px-0 pb-0">
              <v-autocomplete
                v-model="estacaoSelecionada"
                :items="estacoesOptions"
                return-object
                label="Estação *"
                clearable
                variant="solo-filled"
                item-title="nome"
                item-value="id"
                no-data-text="Nenhuma estação encontrada"
              />

              <v-autocomplete
                v-model="equipamentosSelecionados"
                v-model:menu="menuEquipamentos"
                :items="equipamentosOptions"
                :disabled="!estacaoSelecionada?.id || menuServicos"
                multiple
                return-object
                chips
                closable-chips
                clearable
                hide-selected
                variant="solo-filled"
                :label="estacaoSelecionada?.id ? 'Equipamentos (opcional)' : 'Selecione uma estação para carregar os equipamentos'"
                item-title="tagnome"
                item-value="id"
                no-data-text="Nenhum equipamento encontrado"
                class="mt-3"
                attach="body"
              >
                <template #chip="{ props, item }">
                  <v-chip v-bind="props" size="small" class="me-1">
                    {{ item?.raw?.tag || 'SEM TAG' }}
                  </v-chip>
                </template>
              </v-autocomplete>

              <v-card v-if="equipamentosSelecionados?.length && !menuEquipamentos" class="mt-3" variant="elevated" elevation="0" border rounded="lg">
                <v-card-title class="d-flex align-center">
                  <font-awesome-icon icon="microchip" class="me-2 text-primary" />
                  Equipamentos selecionados
                  <v-spacer />
                  <v-chip size="small" variant="flat">{{ equipamentosSelecionados.length }}</v-chip>
                </v-card-title>

                <v-divider />

                <v-card-text class="pa-0">
                  <v-list density="compact">
                    <v-list-item v-for="e in equipamentosSelecionados" :key="e.id">
                      <v-list-item-title class="text-body-2">{{ e.tagnome }}</v-list-item-title>
                      <template #append>
                        <v-tooltip text="Remover equipamento" location="top">
                          <template #activator="{ props }">
                            <v-btn v-bind="props" icon variant="text" size="small" @click="removerEquipamento(e.id)">
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
                  <v-btn variant="text" @click="limparEquipamentosSelecionados">Limpar seleção</v-btn>
                </v-card-actions>
              </v-card>

              <v-autocomplete
                v-model="servicosSelecionados"
                v-model:menu="menuServicos"
                :items="servicosOptions"
                label="Serviços Solicitados *"
                multiple
                return-object
                chips
                closable-chips
                clearable
                hide-selected
                variant="solo-filled"
                item-title="descricao"
                item-value="id"
                no-data-text="Nenhum serviço encontrado"
                class="mt-6"
                attach="body"
                :disabled="menuEquipamentos"
              >
                <template #chip="{ props, item }">
                  <v-chip v-bind="props" size="small" class="me-1">
                    <font-awesome-icon icon="wrench" class="me-2" />
                    {{ item?.raw?.codigo }}
                  </v-chip>
                </template>
              </v-autocomplete>

              <v-card v-if="servicosSelecionados?.length && !menuServicos" class="mt-3" variant="elevated" elevation="0" border rounded="lg">
                <v-card-title class="d-flex align-center">
                  <font-awesome-icon icon="list-check" class="me-2 text-primary" />
                  Serviços selecionados
                  <v-spacer />
                  <v-chip size="small" variant="flat">{{ servicosSelecionados.length }}</v-chip>
                </v-card-title>

                <v-divider />

                <v-card-text class="pa-0">
                  <v-list density="compact">
                    <v-list-item v-for="s in servicosSelecionados" :key="s.id">
                      <v-list-item-title class="text-body-2">{{ s.descricao }}</v-list-item-title>

                      <template #append>
                        <v-tooltip text="Remover serviço" location="top">
                          <template #activator="{ props }">
                            <v-btn v-bind="props" icon variant="text" size="small" @click="removerServico(s.id)">
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
                  <v-btn variant="text" @click="limparServicosSelecionados">Limpar seleção</v-btn>
                </v-card-actions>
              </v-card>
            </v-col>
          </v-row>
        </v-stepper-window-item>

        <v-stepper-window-item :value="2">
          <v-card class="mb-4">
            <v-card-title>
              <font-awesome-icon icon="map-marker-alt" class="me-2" />
              Localização da estação
            </v-card-title>

            <v-card-text>
              <v-row>
                <v-col cols="12" md="6">
                  <v-text-field label="Endereço" :model-value="estacaoEndereco.endereco" variant="solo-filled" readonly />
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field label="Número" :model-value="estacaoEndereco.numero" variant="solo-filled" readonly />
                </v-col>
              </v-row>

              <v-row>
                <v-col cols="12" md="6">
                  <v-text-field label="Bairro" :model-value="estacaoEndereco.bairro" variant="solo-filled" readonly />
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field label="Complemento" :model-value="estacaoEndereco.complemento" variant="solo-filled" readonly />
                </v-col>
              </v-row>

              <v-row>
                <v-col cols="12">
                  <v-text-field label="Ponto de referência" :model-value="estacaoEndereco.pontoReferencia" variant="solo-filled" readonly />
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>

          <v-card class="mt-2 map-card">
            <v-card-title>
              <font-awesome-icon icon="map-marker-alt" class="me-2" />
              Mapa da estação
            </v-card-title>
            <v-card-text class="pa-0">
              <div class="map-container">
                <GoogleMap
                  :api-key="apiKey"
                  class="map-iframe"
                  :center="center || fallbackCenter"
                  :zoom="zoom || fallbackZoom"
                  mapId="MAPA_ORDEM_SERVICO"
                >
                  <AdvancedMarker
                    v-for="m in mapMarkers"
                    :key="m.id"
                    :options="{ position: { lat: m.lat, lng: m.lng } }"
                    @click="handleMarkerClick(m)"
                  >
                    <template #content>
                      <font-awesome-icon :icon="m.icon" :class="m.class" />
                    </template>
                  </AdvancedMarker>
                </GoogleMap>
              </div>
            </v-card-text>
          </v-card>
        </v-stepper-window-item>

        <v-stepper-window-item :value="3">
          <h4 class="d-flex align-center mb-5">
            <font-awesome-icon icon="bullhorn" class="text-primary me-2" />
            Solicitante
          </h4>

          <div class="justify-center mx-1">
            <v-row class="justify-center">
              <v-col cols="12" md="12">
                <v-text-field v-model="claimant.name" label="Nome *" variant="solo-filled" />
              </v-col>
            </v-row>

            <v-row class="justify-center">
              <v-col cols="12" md="4">
                <v-text-field v-model="claimant.phone" v-mask="['####-####', '#####-####', '(##)####-####','(##)#####-####']" label="Telefone" variant="solo-filled" />
              </v-col>
              <v-col cols="12" md="4">
                <v-text-field v-model="claimant.cpf" v-mask="['###.###.###-##', '##.###.###/####-##']" label="CPF/CNPJ *" variant="solo-filled" />
              </v-col>
              <v-col cols="12" md="4">
                <v-text-field v-model="claimant.email" label="E-mail" variant="solo-filled" />
              </v-col>
            </v-row>

            <v-row class="justify-center">
              <v-col cols="12" md="6">
                <v-text-field v-model="claimant.complement" label="Complemento" variant="solo-filled" />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="claimant.reference" label="Ponto de referência" variant="solo-filled" />
              </v-col>
            </v-row>
          </div>
        </v-stepper-window-item>

        <v-stepper-window-item :value="4">
          <v-row>
            <v-col cols="12">
              <QuillEditor v-model:content="observacoes" contentType="html" theme="snow" toolbar="full" class="quill-editor" />
            </v-col>
          </v-row>
        </v-stepper-window-item>

        <v-stepper-window-item :value="5">
          <v-card class="mb-4" v-if="resumo.estacao">
            <v-card-title><font-awesome-icon icon="circle-info" class="me-2" />Estação</v-card-title>
            <v-card-text>
              <v-text-field label="Estação" :model-value="resumo.estacao.nome" variant="solo-filled" readonly />
            </v-card-text>
          </v-card>

          <v-card class="mb-4">
            <v-card-title><font-awesome-icon icon="circle-info" class="me-2" />Equipamentos</v-card-title>
            <v-card-text>
              <div v-if="resumo.equipamentos.length">
                <v-list density="compact">
                  <v-list-item v-for="e in resumo.equipamentos" :key="e.id">
                    <v-list-item-title>{{ e.tagnome }}</v-list-item-title>
                  </v-list-item>
                </v-list>
              </div>
              <div v-else class="text-body-2"><em>OS aberta para a estação inteira (sem equipamentos).</em></div>
            </v-card-text>
          </v-card>

          <v-card class="mb-4" v-if="resumo.servicos.length">
            <v-card-title><font-awesome-icon icon="circle-info" class="me-2" />Serviços</v-card-title>
            <v-card-text>
              <v-list density="compact">
                <v-list-item v-for="s in resumo.servicos" :key="s.id">
                  <v-list-item-title>{{ s.descricao }}</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-card-text>
          </v-card>

          <v-card class="mb-4">
            <v-card-title><font-awesome-icon icon="circle-info" class="me-2" />Localização da estação</v-card-title>
            <v-card-text>
              <v-row>
                <v-col cols="12" md="6">
                  <v-text-field label="Endereço" :model-value="estacaoEndereco.endereco" variant="solo-filled" readonly />
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field label="Número" :model-value="estacaoEndereco.numero" variant="solo-filled" readonly />
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <v-text-field label="Bairro" :model-value="estacaoEndereco.bairro" variant="solo-filled" readonly />
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field label="Complemento" :model-value="estacaoEndereco.complemento" variant="solo-filled" readonly />
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12">
                  <v-text-field label="Ponto de referência" :model-value="estacaoEndereco.pontoReferencia" variant="solo-filled" readonly />
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>

          <v-card class="mb-4">
            <v-card-title><font-awesome-icon icon="circle-info" class="me-2" />Solicitante</v-card-title>
            <v-card-text>
              <v-row>
                <v-col cols="12" md="6"><v-text-field label="Nome" v-model="claimant.name" variant="solo-filled" readonly /></v-col>
                <v-col cols="12" md="6"><v-text-field label="CPF/CNPJ" v-model="claimant.cpf" variant="solo-filled" readonly /></v-col>
              </v-row>
            </v-card-text>
          </v-card>

          <v-card class="mb-4">
            <v-card-title><font-awesome-icon icon="circle-info" class="me-2" />Observações</v-card-title>
            <v-card-text>
              <div v-if="observacoes && observacoes.trim()" class="ql-snow quill-readonly">
                <div class="ql-editor ql-editor-readonly" v-html="observacoes"></div>
              </div>
              <div v-else class="text-body-2"><em>Sem observações.</em></div>
            </v-card-text>
          </v-card>
        </v-stepper-window-item>
      </v-stepper-window>
    </v-stepper>

    <div class="nova-ocorrencia__footer d-flex justify-space-between w-100 pt-4 px-4">
      <BaseButton label="Voltar" type="back" iconPosition="left" :disabled="step === 1" @click="voltar" />
      <BaseButton v-if="step !== 5" label="Avançar" type="next" iconPosition="right" :disabled="!podeAvancar" @click="avancar" :class="{ 'pulse-effect': podeAvancar }" />
      <BaseButton v-else label="Finalizar" type="save" @click="salvar" />
    </div>

    <div justify="center">
      <v-dialog v-model="showConfirmationDialog" max-width="400">
        <v-card class="pa-4">
          <v-card-title>
            <div class="d-flex align-center pb-2">
              <font-awesome-icon icon="clipboard-check" class="text-primary me-2" />
              <span class="text-h6">Ordem de Serviço Registrada</span>
              <font-awesome-icon icon="xmark" @click="showConfirmationDialog = false" class="text-close float-right icon-clicavel ms-auto" title="Fechar" />
            </div>
          </v-card-title>
          <v-divider class="pb-4" />
          <v-card-text class="pb-4">
            <p>OS registrada com sucesso!</p>
            <p><strong>Protocolo:</strong> {{ confirmationNumber }}</p>
          </v-card-text>
        </v-card>
      </v-dialog>
    </div>

    <div justify="center">
      <v-dialog v-model="modalDetalheMapa" max-width="520">
        <v-card class="pa-4" rounded="lg">
          <v-card-title class="d-flex align-center">
            <font-awesome-icon icon="circle-info" class="text-primary me-2" />
            <span class="text-h6">{{ modalMarker.title }}</span>
            <v-spacer />
            <v-btn icon variant="text" @click="closeMarkerModal">
              <font-awesome-icon icon="xmark" />
            </v-btn>
          </v-card-title>

          <v-card-subtitle v-if="modalMarker.subtitle" class="pb-2">
            {{ modalMarker.subtitle }}
          </v-card-subtitle>

          <v-divider class="my-2" />

          <v-card-text class="pt-2">
            <v-list density="compact" class="pa-0">
              <v-list-item v-for="(f, idx) in modalMarker.fields" :key="idx" class="px-0">
                <v-row class="w-100" no-gutters>
                  <v-col cols="5" class="text-caption text-medium-emphasis">
                    {{ f.label }}
                  </v-col>
                  <v-col cols="7" class="text-body-2 text-right">
                    {{ f.value }}
                  </v-col>
                </v-row>
              </v-list-item>
            </v-list>
          </v-card-text>

          <v-divider class="my-2" />

          <v-card-actions class="justify-end">
            <BaseButton label="OK" type="confirm" iconPosition="left" extraClass="me-2" @click="closeMarkerModal" />
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>

    <Snackbar
      :retorno="retorno"
      :timeout="3000"
      :tipo="sucesso ? 'sucesso' : 'erro'"
      :mensagemRetorno="mensagemRetorno"
      @ocultarRetorno="() => { retorno = false }"
    />
  </v-container>
</template>

<style scoped>
.v-stepper-item__title { white-space: normal; }

.nova-ocorrencia__container {
  width: 100%;
  height: calc(83vh);
  display: flex;
  flex-direction: column;
}

.nova-ocorrencia__footer {
  border-top: 1px solid #ccc;
  background: #fff;
}

.map-container {
  position: relative;
  width: 100%;
  height: 400px;
  background: #fff;
  border-radius: 0 !important;
  margin: 30px 10px 20px 0 !important;
  padding: 0 !important;
}

.map-iframe {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 1;
  border: none;
}

.map-marker-selected {
  position: absolute;
  color: #d32f2f;
  font-size: 24px;
  transform: translate(-50%, -100%);
  z-index: 2;
}

.map-card {
  border-radius: 0;
  transition: box-shadow 0.3s ease;
}

:deep(.quill-editor) { background: #fff; border-radius: 8px; }
:deep(.ql-editor) { height: calc(80vh - 290px) !important; }

:deep(.quill-readonly .ql-editor-readonly) {
  min-height: 120px;
  padding: 12px;
  border: 1px solid #E0E0E0;
  border-radius: 8px;
  background: #FAFAFA;
  white-space: normal;
  overflow-wrap: anywhere;
}

@keyframes pulse {
  0% { transform: scale(1); box-shadow: 0 0 0 rgba(0,0,0,0); }
  50% { transform: scale(1.05); box-shadow: 0 0 8px rgba(25,118,210,0.6); }
  100% { transform: scale(1); box-shadow: 0 0 0 rgba(0,0,0,0); }
}

.pulse-effect { animation: pulse 0.6s ease-in-out; }

:deep(.v-chip) { border-radius: 999px; }

.servicos-scroll {
  max-height: 140px;
  overflow: auto;
  padding-right: 6px;
}

:deep(.v-stepper-item--complete .v-stepper-item__avatar.v-avatar) {
  background-color: #0c5510 !important;
  border-color: #388e3c !important;
  color: white !important;
  opacity: 0.8;
}

.v-stepper-header {
  position: sticky;
  top: 0;
  z-index: 10;
  background: white;
  flex-shrink: 0;
}

.v-container {
  padding: 0;
}

.text-body-2 {
  margin-top: 20px;
}
</style>

<script>
import { mask } from "vue-the-mask";
export default { directives: { mask } };
</script>