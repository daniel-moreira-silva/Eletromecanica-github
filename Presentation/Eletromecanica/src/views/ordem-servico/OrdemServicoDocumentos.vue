<script setup>
/* global defineProps, defineEmits, defineExpose */
import { ref, inject, computed, watch, onBeforeUnmount } from 'vue'
import BaseButton from '@/components/base/BaseButton.vue'
import LoadingOverlay from '@/components/base/LoadingOverlay.vue'
import DocumentoService from '@/services/configuracoes/documento-service'

const ENTIDADE_TIPO_ORDEM_SERVICO = 1

const props = defineProps({
  modelValue: { type: Boolean, required: true },
  ordemServico: { type: Object, required: false, default: null },
})

const emit = defineEmits([
  'update:modelValue',
  'atualizado',
])

const endpoint = inject('endpoint')
const headerPadrao = inject('headerPadrao')
const chaveSeguranca = inject('chaveSeguranca')
const usuarioSeguranca = inject('usuarioSeguranca')

const documentoService = new DocumentoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
)

const loadingDocumentos = ref(false)
const documentoEdicaoDialog = ref(false)
const excluirDocumentoDialog = ref(false)
const documentosOrdemServico = ref([])
const filaDocumentos = ref([])
const documentoInputRef = ref(null)
const documentoDragOver = ref(false)
const documentoEdicaoForm = ref(null)
const documentoParaExcluir = ref(null)

// tags
const modalTags = ref(false)
const tagsDisponiveis = ref([])
const tagsSelecionadas = ref([])
const tagSearch = ref('')
const tagNova = ref('')
const documentoParaTag = ref(null)

const dragDocumentoId = ref(null)
const dragDocumentoOrigemIndex = ref(null)
const reordenandoDocumentos = ref(false)
const listVersion = ref(0)
const dragOverDocumentoId = ref(null)
const documentoListaRef = ref(null)

let autoScrollRaf = null
const AUTO_SCROLL_ZONE = 80  // px da borda que ativa o scroll
const AUTO_SCROLL_SPEED = 8  // px por frame

function pararAutoScroll() {
  if (autoScrollRaf) {
    cancelAnimationFrame(autoScrollRaf)
    autoScrollRaf = null
  }
}

function tickAutoScroll(container, direction) {
  container.scrollTop += direction * AUTO_SCROLL_SPEED
  autoScrollRaf = requestAnimationFrame(() => tickAutoScroll(container, direction))
}

function handleDragOverLista(e) {
  if (!dragDocumentoId.value) return
  e.preventDefault()

  const container = documentoListaRef.value
  if (!container) return

  const rect = container.getBoundingClientRect()
  const y = e.clientY

  const nearTop = y - rect.top < AUTO_SCROLL_ZONE
  const nearBottom = rect.bottom - y < AUTO_SCROLL_ZONE

  if (nearTop) {
    if (!autoScrollRaf) tickAutoScroll(container, -1)
  } else if (nearBottom) {
    if (!autoScrollRaf) tickAutoScroll(container, 1)
  } else {
    pararAutoScroll()
  }
}

function onDropLista(e) {
  if (!dragDocumentoId.value || reordenandoDocumentos.value) {
    onDragEndDocumento()
    return
  }

  const cards = [...(documentoListaRef.value?.querySelectorAll('.documento-card') || [])]
  const total = documentosOrdemServico.value.length

  if (!cards.length || !total) { onDragEndDocumento(); return }

  // Encontra o índice de destino pelo centro vertical de cada card
  let destinoIndex = total - 1
  for (let i = 0; i < cards.length; i++) {
    const rect = cards[i].getBoundingClientRect()
    if (e.clientY < rect.top + rect.height / 2) {
      destinoIndex = i
      break
    }
  }

  onDropDocumentoSalvo(destinoIndex)
}

const opcoesSimNao = [
  { text: 'Sim', value: true },
  { text: 'Não', value: false },
]

const documentoEdicao = ref({
  id: '',
  nomeOriginal: '',
  descricao: '',
  publico: true,
})

const ExtensoesDocumentoOS = [
  '.pdf',
  '.doc',
  '.docx',
  '.xls',
  '.xlsx',
  '.csv',
  '.jpg',
  '.jpeg',
  '.png',
  '.webp',
  '.mp3',
  '.mp4',
  '.txt',
]

const TamanhoMaximoDocumentoBytes = 20 * 1024 * 1024

const dialog = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})

function gerarIdLocal() {
  return `${Date.now()}-${Math.random().toString(36).slice(2, 10)}`
}

function getMensagemRetornoApi(resp, fallback = 'Operação realizada.') {
  return resp?.data?.message || resp?.data?.mensagem || fallback
}

function isImagem(mime) {
  return (mime || '').toLowerCase().startsWith('image/')
}

function isPdf(mime) {
  return (mime || '').toLowerCase() === 'application/pdf'
}

function getExtensao(nomeArquivo) {
  if (!nomeArquivo || typeof nomeArquivo !== 'string') return ''
  const idx = nomeArquivo.lastIndexOf('.')
  return idx >= 0 ? nomeArquivo.slice(idx).toLowerCase() : ''
}

function formatarTamanho(bytes) {
  const valor = Number(bytes || 0)
  if (valor < 1024) return `${valor} B`
  if (valor < 1024 * 1024) return `${(valor / 1024).toFixed(1)} KB`
  return `${(valor / (1024 * 1024)).toFixed(2)} MB`
}

function getStatusChipColor(status) {
  if (status === 'enviado') return 'success'
  if (status === 'enviando') return 'info'
  if (status === 'validado') return 'primary'
  if (status === 'erro') return 'error'
  return 'grey'
}

function getStatusLabel(status) {
  if (status === 'enviado') return 'Enviado'
  if (status === 'enviando') return 'Enviando'
  if (status === 'validado') return 'Validado'
  if (status === 'erro') return 'Erro'
  return 'Pendente'
}

function getIconeDocumento(nomeArquivo) {
  const extensao = getExtensao(nomeArquivo)
  if (['.jpg', '.jpeg', '.png', '.webp'].includes(extensao)) return 'image'
  if (extensao === '.pdf') return 'file-pdf'
  if (['.doc', '.docx'].includes(extensao)) return 'file-word'
  if (['.xls', '.xlsx', '.csv'].includes(extensao)) return 'file-excel'
  if (extensao === '.txt') return 'file-lines'
  if (extensao === '.mp3') return 'file-audio'
  if (extensao === '.mp4') return 'file-video'
  return 'file'
}

function validarArquivoSelecionado(file) {
  if (!file) return { valido: false, mensagem: 'Arquivo inválido.' }

  const extensao = getExtensao(file.name)
  if (!ExtensoesDocumentoOS.includes(extensao)) {
    return { valido: false, mensagem: 'Extensão do arquivo não permitida.' }
  }
  if (file.size > TamanhoMaximoDocumentoBytes) {
    return { valido: false, mensagem: 'O arquivo excede o tamanho máximo permitido de 20,00 MB.' }
  }
  return { valido: true, mensagem: null }
}

function validarItemFila(item) {
  if (!props.ordemServico?.id) return { valido: false, mensagem: "O campo 'Ordem de Serviço' é obrigatório." }
  if (!item?.nomeArquivo) return { valido: false, mensagem: 'Nome do arquivo obrigatório.' }
  if (!item?.arquivo) return { valido: false, mensagem: 'Arquivo obrigatório.' }

  const extensao = getExtensao(item.nomeArquivo)
  if (!ExtensoesDocumentoOS.includes(extensao)) return { valido: false, mensagem: 'Extensão do arquivo não permitida.' }
  if (Number(item.tamanho || 0) > TamanhoMaximoDocumentoBytes) return { valido: false, mensagem: 'O arquivo excede o tamanho máximo permitido de 20,00 MB.' }

  return { valido: true, mensagem: null }
}

function abrirSeletorArquivos() {
  documentoInputRef.value?.click()
}

function onDragOver(e) {
  e.preventDefault()
  documentoDragOver.value = true
}

function onDragLeave() {
  documentoDragOver.value = false
}

function onDrop(e) {
  e.preventDefault()
  documentoDragOver.value = false
  const files = Array.from(e?.dataTransfer?.files || [])
  adicionarNaFila(files)
}

function onInputChange(e) {
  const files = Array.from(e?.target?.files || [])
  adicionarNaFila(files)
  if (e?.target) e.target.value = ''
}

function adicionarNaFila(files) {
  for (const file of files) {
    const validacao = validarArquivoSelecionado(file)
    const extensao = getExtensao(file?.name || '')
    const ehImagem = ['.jpg', '.jpeg', '.png', '.webp'].includes(extensao)
    const ehPdf = extensao === '.pdf'

    let previewUrl = null
    if (validacao.valido && (ehImagem || ehPdf) && file) {
      previewUrl = URL.createObjectURL(file)
      objectUrls.value.add(previewUrl)
    }

    const item = {
      localId: gerarIdLocal(),
      descricao: '',
      publico: true,
      ordem: 0,
      nomeArquivo: file?.name || '',
      extensao,
      tamanho: file?.size || 0,
      arquivo: validacao.valido ? file : null,
      previewUrl,
      ehImagem,
      ehPdf,
      status: validacao.valido ? 'pendente' : 'erro',
      mensagemErro: validacao.valido ? null : validacao.mensagem,
    }

    filaDocumentos.value.push(item)
  }
}

function removerDaFila(localId) {
  const item = filaDocumentos.value.find(i => i.localId === localId)
  if (item?.previewUrl) {
    URL.revokeObjectURL(item.previewUrl)
    objectUrls.value.delete(item.previewUrl)
  }
  filaDocumentos.value = filaDocumentos.value.filter(i => i.localId !== localId)
}

function limparFila() {
  for (const item of filaDocumentos.value) {
    if (item.previewUrl) {
      URL.revokeObjectURL(item.previewUrl)
      objectUrls.value.delete(item.previewUrl)
    }
  }
  filaDocumentos.value = []
}

async function listarDocumentos() {
  if (!props.ordemServico?.id) {
    documentosOrdemServico.value = []
    return
  }

  loadingDocumentos.value = true
  const result = await documentoService.listarDocumentosPorEntidade(props.ordemServico.id)
  loadingDocumentos.value = false

  if (result?.statusCode === 200) {
    revokeAllPreviews()
    const mime = x => x.mimeType || 'application/octet-stream'
    documentosOrdemServico.value = (result.data ?? []).map(x => ({
      ...x,
      imagem: isImagem(mime(x)),
      pdf: isPdf(mime(x)),
      previewUrl: null,
    }))
    listVersion.value++
    emit('atualizado')
    return
  }

  if (result?.statusCode === 404) {
    documentosOrdemServico.value = []
    emit('atualizado')
    return
  }

  documentosOrdemServico.value = []
}

function fecharDialog() {
  dialog.value = false
  documentoEdicaoDialog.value = false
  excluirDocumentoDialog.value = false
  documentoParaExcluir.value = null
  limparFila()
}

async function enviarFila({ somenteComErro = false } = {}) {
  const itens = filaDocumentos.value.filter(item => {
    if (somenteComErro) return item.status === 'erro'
    return item.status === 'pendente' || item.status === 'erro'
  })

  if (!itens.length) return

  loadingDocumentos.value = true

  let proximaOrdem = documentosOrdemServico.value.length + 1

  for (const item of itens) {
    const validacao = validarItemFila(item)

    if (!validacao.valido) {
      item.status = 'erro'
      item.mensagemErro = validacao.mensagem
      continue
    }

    item.status = 'validado'
    item.mensagemErro = null

    const form = new FormData()
    form.append('EntidadeId', props.ordemServico.id)
    form.append('EntidadeTipo', ENTIDADE_TIPO_ORDEM_SERVICO)
    form.append('Descricao', item.descricao?.trim() || '')
    form.append('Publico', !!item.publico)
    form.append('Ordem', proximaOrdem)
    form.append('Arquivo', item.arquivo, item.nomeArquivo)

    item.status = 'enviando'
    const result = await documentoService.adicionarDocumento(form)

    if (result?.statusCode === 200) {
      item.status = 'enviado'
      item.mensagemErro = null
      proximaOrdem++
    } else {
      item.status = 'erro'
      item.mensagemErro = getMensagemRetornoApi(result, 'Falha ao enviar documento.')
    }
  }

  loadingDocumentos.value = false
  await listarDocumentos()
  filaDocumentos.value = filaDocumentos.value.filter(item => item.status === 'erro')
}

function abrirEdicao(item) {
  documentoEdicao.value = {
    id: item?.id ?? '',
    nomeOriginal: item?.nomeOriginal ?? '',
    descricao: item?.descricao ?? '',
    publico: item?.publico !== false,
  }
  documentoEdicaoDialog.value = true
}

async function salvarEdicao() {
  const validation = await documentoEdicaoForm.value?.validate()
  if (!validation?.valid) return

  loadingDocumentos.value = true

  const request = {
    id: documentoEdicao.value.id,
    nomeOriginal: documentoEdicao.value.nomeOriginal,
    descricao: documentoEdicao.value.descricao?.trim() || '',
    publico: !!documentoEdicao.value.publico,
  }

  const result = await documentoService.atualizarDocumento(request)
  loadingDocumentos.value = false

  if (result?.statusCode === 200) {
    documentoEdicaoDialog.value = false
    await listarDocumentos()
  }
}

function abrirExcluir(item) {
  documentoParaExcluir.value = item
  excluirDocumentoDialog.value = true
}

async function excluirDocumento() {
  if (!documentoParaExcluir.value?.id) return

  loadingDocumentos.value = true
  const result = await documentoService.excluirDocumento(documentoParaExcluir.value.id)
  loadingDocumentos.value = false

  if (result?.statusCode === 200) {
    excluirDocumentoDialog.value = false
    documentoParaExcluir.value = null
    await listarDocumentos()
  }
}

async function baixarDocumento(item) {
  if (!item?.id) return

  const resp = await documentoService.downloadDocumentoBlob(item.id)
  if (resp?.statusCode !== 200 || !(resp.data instanceof Blob)) return

  const url = URL.createObjectURL(resp.data)
  objectUrls.value.add(url)
  const a = document.createElement('a')
  a.href = url
  a.download = item.nomeOriginal || 'download'
  a.click()
}

const objectUrls = ref(new Set())

function revokeAllPreviews() {
  for (const url of objectUrls.value) URL.revokeObjectURL(url)
  objectUrls.value.clear()
}

async function carregarPreview(item) {
  if (!item?.id || item.previewUrl) return

  const resp = await documentoService.viewDocumentoBlob(item.id)
  if (resp?.statusCode !== 200 || !(resp.data instanceof Blob)) return

  const url = URL.createObjectURL(resp.data)
  item.previewUrl = url
  objectUrls.value.add(url)
}

async function abrirDocumento(item) {
  if (!item?.id) return

  if (!item.previewUrl) await carregarPreview(item)

  if (item.previewUrl) window.open(item.previewUrl, '_blank')
}

onBeforeUnmount(() => {
  revokeAllPreviews()
  pararAutoScroll()
})

// ── drag-and-drop reordenação ─────────────────────────────────────────────────

function onDragStartDocumento(doc, index) {
  if (reordenandoDocumentos.value) return
  dragDocumentoId.value = doc?.id || null
  dragDocumentoOrigemIndex.value = index
}

function onDragEndDocumento() {
  dragDocumentoId.value = null
  dragDocumentoOrigemIndex.value = null
  dragOverDocumentoId.value = null
  pararAutoScroll()
}

function isDragging(doc) {
  return dragDocumentoId.value && dragDocumentoId.value === doc?.id
}

function onDragEnterDocumento(doc) {
  if (!dragDocumentoId.value || reordenandoDocumentos.value) return
  dragOverDocumentoId.value = doc?.id || null
}

function onDragLeaveDocumento(doc) {
  if (dragOverDocumentoId.value === doc?.id) dragOverDocumentoId.value = null
}

function isDragOver(doc) {
  return dragOverDocumentoId.value && dragOverDocumentoId.value === doc?.id
}

async function onDropDocumentoSalvo(destinoIndex) {
  if (
    reordenandoDocumentos.value ||
    dragDocumentoOrigemIndex.value === null ||
    destinoIndex === null ||
    dragDocumentoOrigemIndex.value === destinoIndex
  ) {
    onDragEndDocumento()
    return
  }

  const origemIdx = dragDocumentoOrigemIndex.value
  onDragEndDocumento()

  // Mover item no array e aplicar ordens sequenciais
  const novaLista = [...documentosOrdemServico.value]
  const [itemMovido] = novaLista.splice(origemIdx, 1)
  novaLista.splice(destinoIndex, 0, itemMovido)

  const atualizacoes = novaLista.map((item, idx) => ({ id: item.id, ordem: idx + 1 }))

  // Aplicar visualmente de imediato
  documentosOrdemServico.value = novaLista.map((item, idx) => ({ ...item, ordem: idx + 1 }))

  reordenandoDocumentos.value = true

  const listaOriginal = [...documentosOrdemServico.value]

  try {
    const result = await documentoService.atualizarOrdemDocumento(atualizacoes)
    if (result?.statusCode !== 200) {
      documentosOrdemServico.value = listaOriginal
      await listarDocumentos()
    }
  } catch {
    documentosOrdemServico.value = listaOriginal
    await listarDocumentos()
  } finally {
    reordenandoDocumentos.value = false
  }
}

// ── tags ──────────────────────────────────────────────────────────────────────
async function buscarTags(search) {
  const resp = await documentoService.listarTags(search)
  if (resp?.statusCode === 200) {
    tagsDisponiveis.value = (resp.data || []).map(t => ({ id: t.id, nome: t.nome }))
  }
}

async function abrirModalTags(item) {
  documentoParaTag.value = item
  tagSearch.value = ''
  tagNova.value = ''
  tagsDisponiveis.value = []
  tagsSelecionadas.value = (item.tags || []).map(t => ({ id: t.id, nome: t.nome }))
  await buscarTags('')
  modalTags.value = true
}

async function criarESelecionarTag() {
  const nome = (tagNova.value || '').trim()
  if (!nome) return

  const jaTem = tagsSelecionadas.value.some(t => t.nome.toLowerCase() === nome.toLowerCase())
  if (jaTem) { tagNova.value = ''; return }

  const resp = await documentoService.criarTag(nome)
  if (resp?.statusCode === 200 && resp.data?.id) {
    tagsSelecionadas.value.push({ id: resp.data.id, nome: resp.data.nome })
    tagNova.value = ''
  }
}

async function salvarTags() {
  const docId = documentoParaTag.value?.id
  if (!docId) return

  const tagIds = tagsSelecionadas.value.map(t => t.id)
  const resp = await documentoService.salvarTagsDoDocumento(docId, tagIds)

  if (resp?.statusCode === 200) {
    const idx = documentosOrdemServico.value.findIndex(x => x.id === docId)
    if (idx >= 0) documentosOrdemServico.value[idx].tags = [...tagsSelecionadas.value]
    modalTags.value = false
  }
}

let tagTimer = null
watch(tagSearch, val => {
  clearTimeout(tagTimer)
  tagTimer = setTimeout(() => buscarTags(val), 250)
})

watch(
  () => props.modelValue,
  async (aberto) => {
    if (aberto) {
      limparFila()
      await listarDocumentos()
    }
  }
)

defineExpose({ listarDocumentos })
</script>

<template>
  <v-dialog v-model="dialog" fullscreen persistent scrim="rgba(0,0,0,0.7)">
    <v-card class="pa-2 documentos-dialog-card" rounded="0">
      <LoadingOverlay :active="loadingDocumentos" />

      <v-card-title>
        <font-awesome-icon icon="file-alt" class="text-primary me-2" />
        Documentos da Ordem de Serviço
        <font-awesome-icon icon="xmark" @click="fecharDialog"
          class="text-close float-right icon-clicavel me-2" />
      </v-card-title>

      <v-divider class="pb-0" />

      <v-card-text>
        <div class="documentos-header mb-4">
          <div class="documentos-header__left">
            <div class="documentos-header__info" v-if="ordemServico?.servico">
              <span class="documentos-header__label">
                Serviço:
                <strong class="text-black">{{ ordemServico.servico }}</strong>
              </span>
            </div>
            <div class="documentos-header__info">
              <span class="documentos-header__label">
                Endereço:
                <strong class="text-black">{{ ordemServico?.endereco }}</strong>
              </span>
            </div>
          </div>

          <div class="documentos-header__right">
            <span class="documentos-header__os-label">Nº O.S.</span>
            <span class="documentos-header__os-value">{{ ordemServico?.codigo || '—' }}</span>
          </div>
        </div>

        <v-row>
          <!-- Documentos cadastrados -->
          <v-col cols="12" lg="7">
            <v-card variant="outlined" class="altura-card">
              <v-card-title class="text-subtitle-1 d-flex align-center">
                <font-awesome-icon icon="folder-open" class="text-primary me-2" />
                Documentos cadastrados
              </v-card-title>

              <v-divider />

              <v-card-text class="pa-0">
                <div v-if="!documentosOrdemServico.length" class="pa-4 text-medium-emphasis">
                  Nenhum documento vinculado a esta ordem de serviço.
                </div>

                <div v-else class="documento-lista" ref="documentoListaRef" @dragover="handleDragOverLista" @drop="onDropLista">
                  <div class="documento-lista-grid">
                    <div
                      v-for="(doc, index) in documentosOrdemServico"
                      :key="`${doc.id}-${listVersion}`"
                      class="documento-card"
                      :class="{
                        'documento-card--dragging': isDragging(doc),
                        'documento-card--dragover': isDragOver(doc),
                        'documento-card--reordering': !!dragDocumentoId && !isDragging(doc),
                      }"
                      draggable="true"
                      @dragstart="onDragStartDocumento(doc, index)"
                      @dragend="onDragEndDocumento"
                      @dragenter.prevent="onDragEnterDocumento(doc)"
                      @dragleave="onDragLeaveDocumento(doc)"
                      @dragover.prevent
                      @drop.stop="onDropDocumentoSalvo(index)"
                    >
                      <!-- Preview -->
                      <div
                        class="documento-card-preview"
                        v-intersect="(isIntersecting) => { if (isIntersecting) carregarPreview(doc) }"
                      >
                        <img
                          v-if="doc.imagem && doc.previewUrl"
                          :src="doc.previewUrl"
                          :alt="doc.nomeOriginal"
                        />
                        <div v-else-if="doc.pdf && doc.previewUrl" class="documento-card-preview-embed">
                          <embed :src="doc.previewUrl" scrolling="no" />
                        </div>
                        <font-awesome-icon
                          v-else
                          :icon="getIconeDocumento(doc.nomeOriginal)"
                          class="documento-card-preview-icone"
                        />
                      </div>

                      <!-- Conteúdo -->
                      <div class="documento-card-body">
                        <div
                          class="documento-card-titulo documento-card-titulo--download"
                          :class="{ 'documento-card-titulo--disabled': reordenandoDocumentos }"
                          @click="!reordenandoDocumentos && abrirDocumento(doc)"
                        >
                          <span :title="doc.nomeOriginal">{{ doc.nomeOriginal }}</span>
                        </div>

                        <div class="documento-card-linha">
                          <strong>Descrição</strong>
                          <span>{{ doc.descricao || 'Não informada' }}</span>
                        </div>

                        <div class="documento-card-linha-dupla">
                          <div class="documento-card-info-box">
                            <strong>Tamanho</strong>
                            <span>{{ formatarTamanho(doc.tamanhoBytes) }}</span>
                          </div>
                          <div class="documento-card-info-box">
                            <strong>Data</strong>
                            <span>{{ doc.dataCriacaoFormatada || '-' }}</span>
                          </div>
                        </div>

                        <div class="documento-card-linha">
                          <strong>Público</strong>
                          <span>{{ doc.publico ? 'Sim' : 'Não' }}</span>
                        </div>

                        <div v-if="doc.tags?.length" class="documento-card-tags">
                          <v-chip
                            v-for="t in doc.tags"
                            :key="t.id"
                            size="x-small"
                            variant="tonal"
                            color="primary"
                          >
                            {{ t.nome }}
                          </v-chip>
                        </div>

                        <div class="documento-card-acoes">
                          <v-btn icon variant="text" color="primary" title="Tags" @click="abrirModalTags(doc)">
                            <font-awesome-icon icon="tags" />
                          </v-btn>
                          <v-btn icon variant="text" color="primary" title="Visualizar" @click="abrirDocumento(doc)">
                            <font-awesome-icon icon="eye" />
                          </v-btn>
                          <v-btn icon variant="text" color="primary" title="Download" @click="baixarDocumento(doc)">
                            <font-awesome-icon icon="download" />
                          </v-btn>
                          <v-btn icon variant="text" color="primary" title="Editar" @click="abrirEdicao(doc)">
                            <font-awesome-icon icon="pencil" />
                          </v-btn>
                          <v-btn icon variant="text" color="error" title="Excluir" @click="abrirExcluir(doc)">
                            <font-awesome-icon icon="trash" />
                          </v-btn>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </v-card-text>
            </v-card>
          </v-col>

          <!-- Anexar novos documentos -->
          <v-col cols="12" lg="5">
            <v-card variant="outlined" class="altura-card">
              <v-card-title class="text-subtitle-1 d-flex align-center">
                <font-awesome-icon icon="upload" class="text-primary me-2" />
                Anexar novos documentos
              </v-card-title>

              <v-divider />

              <v-card-text>
                <div
                  class="upload-dropzone"
                  :class="{ 'is-dragover': documentoDragOver }"
                  @click="abrirSeletorArquivos"
                  @dragenter.prevent="documentoDragOver = true"
                  @dragover="onDragOver"
                  @dragleave="onDragLeave"
                  @drop="onDrop"
                >
                  <input
                    ref="documentoInputRef"
                    type="file"
                    class="d-none"
                    multiple
                    accept=".pdf,.doc,.docx,.xls,.xlsx,.csv,.jpg,.jpeg,.png,.webp,.mp3,.mp4,.txt"
                    @change="onInputChange"
                  />

                  <div class="upload-droparea">
                    <font-awesome-icon icon="upload" class="upload-dropicon" />
                    <div class="upload-droptext">
                      <span>Arraste e solte ou </span>
                      <span class="upload-browse" role="button" tabindex="0"
                        @click.stop="abrirSeletorArquivos"
                        @keydown.enter.stop="abrirSeletorArquivos">
                        procurar arquivos
                      </span>
                    </div>
                    <div class="upload-subtext">
                      PDF, documento, planilhas, imagens e arquivos de áudio e vídeo • até 20MB por arquivo
                    </div>
                  </div>
                </div>

                <div class="d-flex justify-space-between align-center mt-4 mb-2">
                  <div class="text-subtitle-2">Fila de envio</div>
                  <div class="text-caption">{{ filaDocumentos.length }} documento(s)</div>
                </div>

                <div v-if="!filaDocumentos.length" class="text-medium-emphasis text-body-2">
                  Nenhum documento selecionado.
                </div>

                <div v-else class="fila-documentos">
                  <div class="fila-documentos-grid">
                    <div
                      v-for="doc in filaDocumentos"
                      :key="doc.localId"
                      class="fila-documento-item fila-documento-card"
                    >
                      <!-- Preview da fila -->
                      <div class="fila-documento-preview">
                        <img
                          v-if="doc.ehImagem && doc.previewUrl"
                          :src="doc.previewUrl"
                          :alt="doc.nomeArquivo"
                        />
                        <div v-else-if="doc.ehPdf && doc.previewUrl" class="fila-documento-preview-embed">
                          <embed :src="doc.previewUrl" scrolling="no" />
                        </div>
                        <font-awesome-icon
                          v-else
                          :icon="getIconeDocumento(doc.nomeArquivo)"
                          class="fila-documento-preview-icone"
                        />
                      </div>

                      <div class="fila-documento-titulo">
                        <span :title="doc.nomeArquivo">{{ doc.nomeArquivo }}</span>
                      </div>

                      <div class="fila-documento-top-meta">
                        <span class="fila-documento-tamanho">{{ formatarTamanho(doc.tamanho) }}</span>

                        <div class="fila-documento-top-actions">
                          <v-chip size="small" :color="getStatusChipColor(doc.status)" variant="flat">
                            {{ getStatusLabel(doc.status) }}
                          </v-chip>

                          <v-btn
                            v-if="doc.status !== 'enviando'"
                            icon variant="text" color="error"
                            title="Remover da fila"
                            @click="removerDaFila(doc.localId)"
                          >
                            <font-awesome-icon icon="trash" />
                          </v-btn>
                        </div>
                      </div>

                      <v-text-field
                        v-model="doc.descricao"
                        label="Descrição"
                        density="compact"
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                        hide-details="auto"
                        class="fila-documento-descricao"
                      />

                      <v-select
                        :items="opcoesSimNao"
                        v-model="doc.publico"
                        item-title="text"
                        item-value="value"
                        label="Público"
                        density="compact"
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                        hide-details
                        class="fila-documento-publico"
                      />

                      <div v-if="doc.mensagemErro" class="upload-error mt-2">
                        {{ doc.mensagemErro }}
                      </div>
                    </div>
                  </div>
                </div>
              </v-card-text>

              <v-divider />

              <v-card-actions class="justify-end">
                <BaseButton
                  label="Limpar fila"
                  type="clear"
                  iconPosition="left"
                  extraClass="me-2"
                  @click="limparFila"
                  :disabled="!filaDocumentos.length"
                />

                <BaseButton
                  label="Reenviar falhas"
                  type="clear"
                  iconPosition="left"
                  extraClass="me-2"
                  @click="enviarFila({ somenteComErro: true })"
                  :disabled="!filaDocumentos.some(item => item.status === 'erro')"
                />

                <BaseButton
                  label="Enviar documentos"
                  type="save"
                  iconPosition="left"
                  @click="enviarFila()"
                  :disabled="!filaDocumentos.some(item => item.status === 'pendente' || item.status === 'erro')"
                />
              </v-card-actions>
            </v-card>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>
  </v-dialog>

  <!-- Modal Editar documento -->
  <v-dialog v-model="documentoEdicaoDialog" max-width="520" persistent scrim="rgba(0,0,0,0.7)">
    <v-card class="pa-2">
      <v-card-title>
        <font-awesome-icon icon="pencil" class="text-primary me-2" />
        Editar documento
        <font-awesome-icon icon="xmark" @click="documentoEdicaoDialog = false"
          class="text-close float-right icon-clicavel me-2" />
      </v-card-title>

      <v-divider class="pb-4" />

      <v-card-text>
        <v-form ref="documentoEdicaoForm">
          <v-text-field
            v-model="documentoEdicao.descricao"
            label="Descrição"
            density="compact"
            variant="outlined"
            color="grey-darken-1"
            base-color="grey-darken-1"
            class="mb-4"
          />

          <v-select
            :items="opcoesSimNao"
            v-model="documentoEdicao.publico"
            item-title="text"
            item-value="value"
            label="Público"
            density="compact"
            variant="outlined"
            color="grey-darken-1"
            base-color="grey-darken-1"
          />
        </v-form>
      </v-card-text>

      <v-divider class="pb-4" />

      <v-card-actions class="justify-end">
        <BaseButton label="Cancelar" type="cancel" iconPosition="left" extraClass="me-2"
          @click="documentoEdicaoDialog = false" />
        <BaseButton label="Salvar" type="save" iconPosition="left" @click="salvarEdicao" />
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Modal Excluir documento -->
  <v-dialog v-model="excluirDocumentoDialog" max-width="450" persistent scrim="rgba(0,0,0,0.7)">
    <v-card class="pa-2">
      <v-card-title>
        <font-awesome-icon icon="trash" class="text-primary me-2" />
        Excluir documento?
        <font-awesome-icon icon="xmark" @click="excluirDocumentoDialog = false"
          class="text-close float-right icon-clicavel me-2" />
      </v-card-title>

      <v-divider class="pb-4" />

      <v-card-text class="pb-4">
        <p>
          Deseja realmente excluir o documento
          <strong>{{ documentoParaExcluir?.nomeOriginal }}</strong>?
        </p>
      </v-card-text>

      <v-divider class="pb-4" />

      <v-card-actions class="justify-end">
        <BaseButton label="Não" type="cancel" extraClass="me-2" @click="excluirDocumentoDialog = false" />
        <BaseButton label="Sim, excluir" type="confirm" @click="excluirDocumento" />
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Modal Tags -->
  <v-dialog v-model="modalTags" max-width="700" persistent scrim="rgba(0,0,0,0.7)">
    <v-card class="pa-2">
      <v-card-title>
        <font-awesome-icon icon="tags" class="text-primary me-2" />
        Tags do documento
        <font-awesome-icon icon="xmark" @click="modalTags = false" class="text-close float-right icon-clicavel me-2" />
      </v-card-title>

      <v-divider class="pb-4" />

      <v-card-text>
        <!-- Tags selecionadas -->
        <div class="mb-3">
          <v-chip
            v-for="t in tagsSelecionadas"
            :key="t.id"
            class="ma-1"
            closable
            color="primary"
            variant="tonal"
            @click:close="tagsSelecionadas = tagsSelecionadas.filter(x => x.id !== t.id)"
          >
            {{ t.nome }}
          </v-chip>
          <div v-if="!tagsSelecionadas.length" class="text-caption text-grey">
            Nenhuma tag adicionada.
          </div>
        </div>

        <!-- Autocomplete -->
        <v-autocomplete
          v-model="tagsSelecionadas"
          :items="tagsDisponiveis"
          item-title="nome"
          item-value="id"
          label="Buscar e selecionar tags"
          density="compact"
          variant="outlined"
          multiple
          return-object
          chips
          closable-chips
          v-model:search="tagSearch"
          no-data-text="Nenhuma tag encontrada"
          class="mb-3"
        />

        <!-- Criar nova tag -->
        <div class="d-flex align-center">
          <v-text-field
            v-model="tagNova"
            label="Criar nova tag"
            density="compact"
            variant="outlined"
            class="me-2 flex-grow-1"
            hide-details
            @keydown.enter.prevent="criarESelecionarTag"
          />
          <v-btn color="primary" variant="text" density="compact" height="40" @click="criarESelecionarTag">
            <font-awesome-icon icon="plus" class="me-1" />
            Criar
          </v-btn>
        </div>
      </v-card-text>

      <v-divider class="pb-2" />

      <v-card-actions class="justify-end">
        <BaseButton label="Cancelar" type="cancel" extraClass="me-2" @click="modalTags = false" />
        <BaseButton label="Salvar" type="save" @click="salvarTags" />
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.documento-lista {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 12px;
}

.documento-lista-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 16px;
  align-items: start;
}

.documento-card {
  border: 1px solid rgb(0 0 0 / 17%);
  border-radius: 8px;
  background: #fff;
  min-width: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 2px 2px 5px -5px;
  cursor: grab;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease,
    border-color 0.18s ease,
    background-color 0.18s ease;
}

.documento-card:active {
  cursor: grabbing;
}

.documento-card-preview {
  width: 100%;
  height: 140px;
  background: #f3f4f6;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  flex-shrink: 0;
  border-bottom: 1px solid rgb(0 0 0 / 8%);
}

.documento-card-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.documento-card-preview-embed {
  width: 100%;
  height: 100%;
}

.documento-card-preview-embed embed {
  width: 100%;
  height: 100%;
  border: 0;
  pointer-events: none;
}

.documento-card-preview-icone {
  font-size: 52px;
  color: rgb(var(--v-theme-primary));
  opacity: 0.55;
}

.documento-card-body {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px;
}

.documento-card-titulo,
.documento-card-titulo--download {
  display: flex;
  align-items: center;
  font-weight: 600;
  min-width: 0;
}

.documento-card-titulo--download {
  cursor: pointer;
}

.documento-card-titulo--download:hover {
  color: rgb(var(--v-theme-primary));
  text-decoration: underline;
}

.documento-card-titulo--disabled {
  pointer-events: none;
  opacity: 0.7;
}

.documento-card-titulo-icone {
  flex: 0 0 auto;
}

.documento-card-titulo span {
  min-width: 0;
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.documento-card-linha {
  display: flex;
  flex-direction: column;
  font-size: 12px;
  color: #666;
  gap: 2px;
  min-height: 42px;
}

.documento-card-linha strong,
.documento-card-info-box strong {
  color: #3f3f3f;
  font-size: 12px;
}

.documento-card-linha span {
  overflow: hidden;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.documento-card-linha-dupla {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.documento-card-info-box {
  display: flex;
  flex-direction: column;
  font-size: 12px;
  color: #666;
  gap: 2px;
  min-width: 0;
}

.documento-card-info-box span {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.documento-card-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.documento-card-acoes {
  display: flex;
  align-items: center;
  justify-content: left;
  gap: 4px;
  margin-top: 4px;
  border-top: 1px solid rgb(0 0 0 / 6%);
  padding-top: 4px;
}

.documento-card--dragging {
  opacity: 0.92;
  transform: scale(1.02);
  box-shadow: 0 10px 24px rgba(0, 0, 0, 0.16);
  border-color: rgb(var(--v-theme-primary));
  background: #f8fbff;
  z-index: 2;
}

.documento-card--dragover {
  border-color: rgb(var(--v-theme-primary));
  background: rgba(var(--v-theme-primary), 0.06);
  box-shadow: inset 0 0 0 1px rgba(var(--v-theme-primary), 0.18);
}

.documento-card--reordering {
  transform: translateY(0);
}

.upload-dropzone {
  border-radius: 10px;
  cursor: pointer;
}

.upload-dropzone.is-dragover {
  outline: 2px dashed var(--v-theme-primary);
  outline-offset: 3px;
}

.upload-droparea {
  border: 2px dashed rgba(0, 0, 0, 0.18);
  border-radius: 10px;
  padding: 10px 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  user-select: none;
}

.upload-dropicon {
  font-size: 26px;
  color: rgb(var(--v-theme-primary));
}

.upload-droptext {
  font-size: 14px;
  color: rgb(var(--v-theme-primary));
}

.upload-browse {
  color: rgb(var(--v-theme-primary));
  text-decoration: underline;
  cursor: pointer;
  font-weight: 600;
}

.upload-subtext {
  font-size: 12px;
  color: rgb(var(--v-theme-primary));
}

.fila-documentos {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 4px;
}

.fila-documentos-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
  align-items: start;
}

.fila-documento-item {
  border: 1px solid rgb(0 0 0 / 0%);
  border-radius: 10px;
  padding: 14px;
  background: #fff;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.fila-documento-card {
  background: #eeeeeea6;
}

.fila-documento-preview {
  width: 100%;
  height: 110px;
  background: #f3f4f6;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  flex-shrink: 0;
  border-radius: 6px;
  border: 1px solid rgb(0 0 0 / 8%);
}

.fila-documento-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  border-radius: 6px;
}

.fila-documento-preview-embed {
  width: 100%;
  height: 100%;
}

.fila-documento-preview-embed embed {
  width: 100%;
  height: 100%;
  border: 0;
  pointer-events: none;
}

.fila-documento-preview-icone {
  font-size: 40px;
  color: rgb(var(--v-theme-primary));
  opacity: 0.55;
}

.fila-documento-titulo {
  display: flex;
  align-items: center;
  font-weight: 600;
  min-width: 0;
}

.fila-documento-titulo-icone {
  flex: 0 0 auto;
}

.fila-documento-titulo span {
  min-width: 0;
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.fila-documento-top-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.fila-documento-tamanho {
  font-size: 12px;
  color: #666;
  white-space: nowrap;
}

.fila-documento-top-actions {
  display: flex;
  align-items: center;
  gap: 4px;
}

.fila-documento-descricao,
.fila-documento-publico {
  margin-top: 2px;
  display: block !important;
}

.upload-error {
  font-size: 12px;
  color: rgb(var(--v-theme-error));
}

.documentos-dialog-card {
  height: 100vh;
  overflow-y: auto;
  background: #fff;
}

.altura-card {
  height: 75vh;
  display: flex;
  flex-direction: column;
}

.altura-card :deep(.v-card-text) {
  flex: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.documentos-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 24px;
  padding: 4px 0 8px 0;
}

.documentos-header__left {
  min-width: 0;
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.documentos-header__info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.documentos-header__label {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  color: rgba(0, 0, 0, 0.55);
}

.documentos-header__right {
  min-width: 170px;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  justify-content: flex-start;
}

.documentos-header__os-label {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  color: rgba(0, 0, 0, 0.55);
}

.documentos-header__os-value {
  font-size: 22px;
  font-weight: 800;
  line-height: 1.1;
  color: rgba(0, 0, 0, 0.88);
}

::v-deep(.v-btn--icon) {
  border-radius: 10%;
  width: calc(var(--v-btn-height) + 0px);
  height: calc(var(--v-btn-height) + 0px);
}

::v-deep(.v-card) {
  border-color: #d4d4d4;
}

:deep(.v-icon--size-default) {
  font-size: calc(var(--v-icon-size-multiplier) * 1em);
}

@media (max-width: 1280px) {
  .documento-lista-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 960px) {
  .fila-documentos-grid {
    grid-template-columns: 1fr;
  }

  .documento-lista-grid {
    grid-template-columns: 1fr;
  }

  .documentos-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .documentos-header__right {
    align-items: flex-start;
    min-width: unset;
  }
}
</style>
