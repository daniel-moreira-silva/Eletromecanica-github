<script setup>
/* global defineProps, defineEmits */

import { computed, reactive, ref, watch } from 'vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { WizardPopService } from '@/services/ordem-servico/pop-wizard'

const props = defineProps({
  modelValue: { type: Boolean, required: true }
})

const emit = defineEmits(['update:modelValue', 'apply'])

const etapa = ref(1)

// Wizard sempre inicia do zero (regra do seu requisito)
const termoBusca = ref('')
const categoria = ref('')
const servico = ref('')
const detalhe = ref('')

// filtros por etapa (switches)
const filtrosServico = reactive({})
const filtrosDetalhe = reactive({})
const filtrosCategoria = reactive({})

// data carregada do “service” (mock backend)
const categoriaData = ref({ instrucoes: [], filtros: [], opcoes: [] })
const servicoData = ref({ instrucoes: [], filtros: [], opcoes: [] })
const detalheData = ref({ instrucoes: [], filtros: [], opcoes: [] })
const detalhamento = computed(() => {
  const opt = (detalheData.value?.opcoes || []).find(o => o?.value === detalhe.value)
  return opt?.detalhamento || ''
})

const normalizeLocal = (s = '') =>
  (s || '')
    .toString()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9\s]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()

const matchByTerm = (text, term) => {
  const t = normalizeLocal(text)
  const q = normalizeLocal(term)
  if (!q) return true
  const tokens = q.split(' ').filter(Boolean)
  return tokens.every(tok => t.includes(tok))
}

function getSwitchBucketByEtapa() {
  if (etapa.value === 1) return filtrosCategoria
  if (etapa.value === 2) return filtrosServico
  if (etapa.value === 3) return filtrosDetalhe
  return null
}

function getSwitchValue(id) {
  const bucket = getSwitchBucketByEtapa()
  return Boolean(bucket?.[id])
}

function setSwitchValue(id, v) {
  const bucket = getSwitchBucketByEtapa()
  if (!bucket) return
  bucket[id] = v
}

const etapaDataAtual = computed(() => {
  if (etapa.value === 1) return categoriaData.value
  if (etapa.value === 2) return servicoData.value
  if (etapa.value === 3) return detalheData.value
  return { instrucoes: [], filtros: [], opcoes: [] }
})

const getSwitchesFromData = (data) => {
  const fromInstrucoes = (data?.instrucoes || []).filter(i => i.type === 'switch')
  const fromFiltros = (data?.filtros || [])
  return [...fromInstrucoes, ...fromFiltros]
}

const etapaItemsOrdenados = computed(() => {
  const data = etapaDataAtual.value || { instrucoes: [], filtros: [] }
  const instr = data.instrucoes || []
  const hasMixed = instr.some(i => i.type === 'switch')
  if (hasMixed) return instr
  // fallback: mantém ordem de textos e adiciona switches ao final (enquanto backend não manda misturado)
  return [
    ...(instr || []),
    ...getSwitchesFromData(data).map(sw => ({ type: 'switch', ...sw }))
  ]
})

const itensEtapaFiltrados = computed(() => {
  const term = termoBusca.value || ''
  const list = etapaItemsOrdenados.value || []
  if (!term) return list
  return list.filter(item => {
    if (item.type === 'text') return matchByTerm(item.text || '', term)
    if (item.type === 'switch') return matchByTerm(item.label || '', term)
    return true
  })
})

function emitModelValue(v) {
  emit('update:modelValue', v)
}

function close() {
  emit('update:modelValue', false)
}

// reset total do wizard ao abrir
function resetWizard() {
  etapa.value = 1
  termoBusca.value = ''
  categoria.value = ''
  servico.value = ''
  detalhe.value = ''

  Object.keys(filtrosCategoria).forEach(k => delete filtrosCategoria[k])
  Object.keys(filtrosServico).forEach(k => delete filtrosServico[k])
  Object.keys(filtrosDetalhe).forEach(k => delete filtrosDetalhe[k])

  //loadCategoria()
  loadServico()
  //loadDetalhe()
}

// function loadCategoria() {
//   categoriaData.value = WizardPopService.getEtapaCategoria({ termo: '' })
//   applyDefaultFilters(getSwitchesFromData(categoriaData.value), filtrosCategoria)
// }

function applyDefaultFilters(filtersArray, targetReactive) {
  (filtersArray || []).forEach(f => {
    if (targetReactive[f.id] === undefined) {
      targetReactive[f.id] = Boolean(f.default)
    }
  })
}

function loadServico() {
  servicoData.value = WizardPopService.getEtapaServico({
    categoria: categoria.value,
    termo: '',
    filtros: filtrosServico
  })
  applyDefaultFilters(getSwitchesFromData(servicoData.value), filtrosServico)
}

// function loadDetalhe() {
//   detalheData.value = WizardPopService.getEtapaDetalhe({
//     categoria: categoria.value,
//     servico: servico.value,
//     termo: '',
//     filtros: filtrosDetalhe
//   })
//   applyDefaultFilters(getSwitchesFromData(detalheData.value), filtrosDetalhe)
// }

const canNext = computed(() => {
  if (etapa.value === 1) return !!categoria.value
  if (etapa.value === 2) return !!categoria.value && !!servico.value
  if (etapa.value === 3) return !!categoria.value && !!servico.value && !!detalhe.value
  return false
})

const canApply = computed(() => !!categoria.value && !!servico.value && !!detalhe.value)

function next() {
  if (etapa.value < 4 && canNext.value) etapa.value += 1
}

function prev() {
  if (etapa.value > 1) etapa.value -= 1
}

function apply() {
  if (!canApply.value) return

  emit('apply', {
    categoria: categoria.value,
    servico: servico.value,
    detalhe: detalhe.value
  })

  close()
}

// Watchers para recalcular opções/instruções dinamicamente
watch(
  () => props.modelValue,
  (open) => {
    if (open) resetWizard()
  }
)

watch(categoria, () => {
  // ao trocar categoria, zera seleções dependentes e recarrega etapa 2
  servico.value = ''
  detalhe.value = ''
  Object.keys(filtrosServico).forEach(k => delete filtrosServico[k])
  Object.keys(filtrosDetalhe).forEach(k => delete filtrosDetalhe[k])

  loadServico()
  //loadDetalhe()

  // mantém etapa coerente (se estiver avançado)
  if (etapa.value > 2) etapa.value = 2
})

watch(servico, () => {
  // ao trocar serviço, zera detalhe e recarrega etapa 3
  detalhe.value = ''
  Object.keys(filtrosDetalhe).forEach(k => delete filtrosDetalhe[k])

  //loadDetalhe()
  if (etapa.value > 3) etapa.value = 3
})

// filtros: ao mudar, recarrega as opções daquela etapa
watch(
  () => ({ ...filtrosServico }),
  () => loadServico(),
  { deep: true }
)

watch(
  () => ({ ...filtrosDetalhe }),
  //() => loadDetalhe(),
  { deep: true }
)
</script>

<template>
  <v-dialog :model-value="modelValue" @update:modelValue="emitModelValue" class="pop-dialog">
    <v-card class="pa-4 pop-card">

      <v-card-title class="pb-2">
        <div class="d-flex align-center w-100">
          <font-awesome-icon icon="clipboard-question" class="text-primary me-2" />
          <div class="d-flex flex-column">
            <span class="text-h6">POP - Procedimento Operacional Padrão</span>
          </div>

          <v-spacer />
          <font-awesome-icon icon="xmark" class="text-close cursor-pointer" title="Fechar" @click="close" />
        </div>
      </v-card-title>

      <v-divider class="mb-4" />

      <div class="pop-scroll">
        <!-- Busca global (opcional) -->
        <v-row class="mb-2 section-busca justify-center" dense>
          <v-col cols="12" md="8">
            <v-text-field v-model="termoBusca" label="Buscar termo (ex.: fiscalização, esgoto, horário)"
              variant="filled" clearable />
          </v-col>

          <v-col cols="12" md="4" />

        </v-row>

        <!-- Instruções + perguntas (misturadas e na ordem) — ocultar na etapa 4 -->
        <div v-if="etapa !== 4" class="pop-scroll-lista">
          <div class="d-flex align-center mb-2">
            <font-awesome-icon icon="comment-dots" class="text-primary me-2" />
            <span class="text-subtitle-1">Instruções e perguntas (POP)</span>
            <v-spacer />
            <v-chip label size="small" variant="tonal">Etapa {{ etapa }}/4</v-chip>
          </div>

          <v-divider />

          <div v-if="itensEtapaFiltrados.length" class="section-pops">
            <div v-for="(it, idx) in itensEtapaFiltrados" :key="it.type === 'switch' ? it.id : `txt-${idx}`"
              class="py-1">
              <div v-if="it.type === 'text'" class="text-body-2 d-flex align-start">
                <font-awesome-icon :icon="it.icon || 'comment-dots'" class="me-2 mt-1" />
                <div>{{ it.text }}</div>
              </div>

              <div v-else-if="it.type === 'switch'" class="pop-item pop-item--switch">
                <v-switch :model-value="getSwitchValue(it.id)" :label="it.label" inset color="success" hide-details
                  density="compact" class="switch-thin pop-switch"
                  @update:modelValue="(v) => setSwitchValue(it.id, v)" />

              </div>
            </div>
          </div>

          <v-alert v-else type="info" variant="tonal" class="mb-3">
            Nenhum item encontrado para o termo informado.
          </v-alert>
        </div>

        <v-divider />

        <!-- Stepper do wizard (sempre inicia do zero) -->
        <v-stepper v-model="etapa" solo
          :class="['pop-stepper', 'section-stepper', { 'section-stepper--final': etapa === 4 }]">
          <v-stepper-header>
            <v-stepper-item :value="1" title="Categoria" />
            <v-stepper-item :value="2" title="Tipo do problema" />
            <v-stepper-item :value="3" title="Detalhe" />
            <v-stepper-item :value="4" title="Conclusão" />
          </v-stepper-header>

          <v-stepper-window class="mt-4">
            <!-- Etapa 1 -->
            <v-stepper-window-item :value="1">
              <v-card class="py-2">
                <v-row class="justify-center py-0">
                  <v-col cols="12" md="8" class="pt-2 pb-2">
                    <v-autocomplete v-model="categoria" label="Tipo do problema (Serviço)" hide-details
                      :items="categoriaData.opcoes" variant="filled" clearable />
                  </v-col>
                </v-row>
              </v-card>
            </v-stepper-window-item>

            <!-- Etapa 2 -->
            <v-stepper-window-item :value="2">
              <v-card class="py-2">
                <v-row class="justify-center py-0">
                  <v-col cols="12" md="8" class="pt-2 pb-2">
                    <v-autocomplete v-model="servico" :items="servicoData.opcoes" hide-details
                      label="Tipo do problema (Serviço)" variant="filled" clearable :disabled="!categoria" />
                  </v-col>
                </v-row>
              </v-card>
            </v-stepper-window-item>

            <!-- Etapa 3 -->
            <v-stepper-window-item :value="3">
              <v-card class="py-2">
                <v-row class="justify-center py-0">
                  <v-col cols="12" md="8" class="pt-2 pb-2">
                    <v-autocomplete v-model="detalhe" :items="detalheData.opcoes" item-title="title" item-value="value"
                      hide-details label="Detalhe do problema" variant="filled" clearable :disabled="!servico" />
                  </v-col>
                </v-row>
              </v-card>
            </v-stepper-window-item>

            <!-- Conclusão -->
            <v-stepper-window-item :value="4">
              <v-card class="pa-4">
                <v-alert type="info" variant="tonal" class="mb-4">
                  Confira e aplique no atendimento
                </v-alert>
                <v-row>
                  <v-col cols="12" md="4">
                    <v-text-field label="Categoria" :model-value="categoria" variant="solo-filled" readonly />
                  </v-col>
                  <v-col cols="12" md="4">
                    <v-text-field label="Serviço" :model-value="servico" variant="solo-filled" readonly />
                  </v-col>
                  <v-col cols="12" md="4">
                    <v-text-field label="Detalhe" :model-value="detalhe" variant="solo-filled" readonly />
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12" md="12">
                    <v-textarea label="Detalhamento" :model-value="detalhamento" class="text-detalhamento"
                      variant="plain" readonly rows="10" auto-grow />
                  </v-col>
                </v-row>
              </v-card>
            </v-stepper-window-item>
          </v-stepper-window>
        </v-stepper>
      </div>
      <v-divider />

      <v-card-actions class="justify-space-between">
        <BaseButton label="Voltar" type="back" iconPosition="left" :disabled="etapa === 1" @click="prev" />

        <div class="d-flex align-center pop-actions">
          <BaseButton label="Cancelar" type="cancel" iconPosition="left" @click="close" />

          <BaseButton v-if="etapa < 4" label="Avançar" type="next" iconPosition="right" :disabled="!canNext"
            @click="next" />

          <BaseButton v-else label="Aplicar" type="save" iconPosition="left" :disabled="!canApply" @click="apply" />
        </div>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.switch-thin :deep(.v-switch__track) {
  height: 20px;
  width: 20px;
  border-radius: 9999px;
}

.switch-thin :deep(.v-switch__thumb) {
  width: 12px;
  height: 12px;
  box-shadow: none;
}

.pop-dialog :deep(.v-overlay__content) {
  height: 98vh;
  max-height: 98vh;
  width: 900px;
  max-width: 900px;
}

.pop-card {
  height: 860px;
  max-height: 98vh;
  display: flex;
  flex-direction: column;
}

.pop-scroll {
  flex: 1;
  overflow: hidden;
  padding: 0.5rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-height: 0;
}

.pop-scroll-lista {
  flex: 1;
  overflow: hidden;
  padding: 0.5rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-height: 0;
}

.pop-scroll :deep(.v-divider) {
  flex: 0 0 auto;
}

.pop-actions {
  gap: 10px;
}

.pop-stepper {
  box-shadow: none !important;
  border: none !important;
  background: transparent !important;
}

.pop-stepper :deep(.v-stepper) {
  box-shadow: none !important;
  border: none !important;
  background: transparent !important;
}

/* Stepper mais compacto */
.pop-stepper :deep(.v-stepper-header) {
  padding: 0;
  min-height: 44px;
}

.pop-stepper :deep(.v-stepper-item) {
  padding: 0 6px;
}

.pop-stepper :deep(.v-stepper-item__title) {
  font-size: 0.85rem;
  line-height: 1.1;
}

.pop-stepper :deep(.v-stepper-window) {
  margin-top: 8px !important;
}

.pop-stepper :deep(.v-stepper-header) {
  background: transparent;
  box-shadow: none !important;
  border: none !important;
}

.pop-stepper :deep(.v-stepper-item) {
  background: transparent !important;
}

.pop-stepper :deep(.v-stepper-item--selected),
.pop-stepper :deep(.v-stepper-item--complete) {
  background: transparent !important;
}

.pop-stepper :deep(.v-stepper-item__avatar) {
  box-shadow: none !important;
}

.pop-stepper :deep(.v-stepper-window-item) {
  padding-top: 0 !important;
  padding-bottom: 0 !important;
}

/* Switch com mesma “cara” de instrução */
.pop-item {
  font-size: 0.875rem;
  /* equivalente ao text-body-2 do Vuetify */
  line-height: 1.25rem;
}

::v-deep(.v-textarea textarea) {
  font-size: 1rem !important;
}

.pop-switch :deep(.v-label) {
  font-size: inherit !important;
  line-height: inherit !important;
  opacity: 1;
}

.v-input--density-compact {
  --v-input-control-height: 20px;
}

/* Seção 1: sempre do mesmo tamanho (não rola) */
.section-busca {
  flex: 0 0 60px;
  /* ajuste fino aqui */
  overflow: hidden;
}

/* Seção 2: ocupa o “miolo” e é a ÚNICA que rola (quando tiver muito conteúdo) */
.section-pops {
  flex: 1 1 auto;
  overflow-y: auto;
  min-height: 0;
  padding-right: 5px;
}

/* Seção 3: sempre do mesmo tamanho (não depende de tela) */
.section-stepper {
  flex: 0 1 140px;
  /* pode encolher se faltar espaço, sem esmagar a seção 2 */
  /* ajuste fino aqui (altura fixa do wizard) */
  overflow: hidden;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.section-stepper :deep(.v-card) {
  box-shadow: none !important;
}

/* Se o conteúdo do stepper crescer, ele rola dentro dele (sem afetar as 3 seções) */
.section-stepper :deep(.v-stepper-window) {
  flex: 1 1 auto;
  overflow-y: auto;
  min-height: 0;
}

.section-stepper--final {
  flex: 0 0 450px;
  /* ajuste fino aqui */
}

.section-stepper--final :deep(.v-stepper-window) {
  overflow-y: auto;
}
</style>
