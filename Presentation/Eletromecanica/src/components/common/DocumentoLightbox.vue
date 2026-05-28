<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  modelValue:         { type: Boolean,  required: true },
  documentos:         { type: Array,    required: true },
  indice:             { type: Number,   default: 0 },
  fnCarregarPreview:  { type: Function, required: true },
  fnBaixar:           { type: Function, required: true },
})

const emit = defineEmits(['update:modelValue'])

const visualizadorIndex    = ref(0)
const visualizadorCarregando = ref(false)
const lightboxThumbsRef    = ref(null)

const visualizadorDocumento  = computed(() => props.documentos[visualizadorIndex.value] ?? null)
const visualizadorTemAnterior = computed(() => visualizadorIndex.value > 0)
const visualizadorTemProximo  = computed(() => visualizadorIndex.value < props.documentos.length - 1)

// ── Helpers ────────────────────────────────────────────────────────────────
function getIconeDocumento(nomeArquivo) {
  if (!nomeArquivo) return 'file'
  const idx = nomeArquivo.lastIndexOf('.')
  const ext = idx >= 0 ? nomeArquivo.slice(idx).toLowerCase() : ''
  if (['.jpg', '.jpeg', '.png', '.webp'].includes(ext)) return 'image'
  if (ext === '.pdf') return 'file-pdf'
  if (['.doc', '.docx'].includes(ext)) return 'file-word'
  if (['.xls', '.xlsx', '.csv'].includes(ext)) return 'file-excel'
  if (ext === '.txt') return 'file-lines'
  if (ext === '.mp3') return 'file-audio'
  if (ext === '.mp4') return 'file-video'
  return 'file'
}

function formatarTamanho(bytes) {
  const v = Number(bytes || 0)
  if (v < 1024) return `${v} B`
  if (v < 1024 * 1024) return `${(v / 1024).toFixed(1)} KB`
  return `${(v / (1024 * 1024)).toFixed(2)} MB`
}

// ── Navigation ─────────────────────────────────────────────────────────────
function scrollThumbAtivo() {
  setTimeout(() => {
    const container = lightboxThumbsRef.value
    if (!container) return
    const active = container.querySelector('.lightbox-thumb--ativo')
    if (active) active.scrollIntoView({ block: 'nearest', inline: 'center', behavior: 'smooth' })
  }, 60)
}

async function carregarDocumentoAtual() {
  const doc = visualizadorDocumento.value
  if (!doc) return
  if (!doc.previewUrl) {
    visualizadorCarregando.value = true
    await props.fnCarregarPreview(doc)
    visualizadorCarregando.value = false
  }
  // Pré-carrega vizinhos em background
  const next = props.documentos[visualizadorIndex.value + 1]
  const prev = props.documentos[visualizadorIndex.value - 1]
  if (next && !next.previewUrl) props.fnCarregarPreview(next)
  if (prev && !prev.previewUrl) props.fnCarregarPreview(prev)
  scrollThumbAtivo()
}

function fecharVisualizador() {
  emit('update:modelValue', false)
}

async function irParaProximo() {
  if (!visualizadorTemProximo.value) return
  visualizadorIndex.value++
  await carregarDocumentoAtual()
}

async function irParaAnterior() {
  if (!visualizadorTemAnterior.value) return
  visualizadorIndex.value--
  await carregarDocumentoAtual()
}

async function irParaIndice(i) {
  if (i < 0 || i >= props.documentos.length || i === visualizadorIndex.value) return
  visualizadorIndex.value = i
  await carregarDocumentoAtual()
}

// ── Keyboard ───────────────────────────────────────────────────────────────
function onKeydown(e) {
  if (e.key === 'ArrowRight') irParaProximo()
  else if (e.key === 'ArrowLeft') irParaAnterior()
  else if (e.key === 'Escape') fecharVisualizador()
}

// ── Touch swipe ────────────────────────────────────────────────────────────
let touchStartX = 0
let touchStartY = 0

function onTouchStart(e) {
  touchStartX = e.touches[0].clientX
  touchStartY = e.touches[0].clientY
}

function onTouchEnd(e) {
  const dx = e.changedTouches[0].clientX - touchStartX
  const dy = e.changedTouches[0].clientY - touchStartY
  if (Math.abs(dx) < Math.abs(dy) || Math.abs(dx) < 40) return
  if (dx < 0) irParaProximo()
  else irParaAnterior()
}

// ── Lifecycle ──────────────────────────────────────────────────────────────
watch(() => props.modelValue, async (aberto) => {
  if (aberto) {
    visualizadorIndex.value = props.indice
    document.addEventListener('keydown', onKeydown)
    await carregarDocumentoAtual()
  } else {
    document.removeEventListener('keydown', onKeydown)
  }
})
</script>

<template>
  <Teleport to="body">
    <Transition name="lightbox-fade">
      <div
        v-if="modelValue"
        class="lightbox-overlay"
        @click.self="fecharVisualizador"
        @touchstart.passive="onTouchStart"
        @touchend.passive="onTouchEnd"
      >
        <!-- Top bar -->
        <div class="lightbox-topbar">
          <div class="lightbox-topbar__filename" :title="visualizadorDocumento?.nomeOriginal">
            <font-awesome-icon :icon="getIconeDocumento(visualizadorDocumento?.nomeOriginal)" class="me-2" />
            {{ visualizadorDocumento?.nomeOriginal }}
          </div>
          <div class="lightbox-topbar__counter">
            {{ visualizadorIndex + 1 }} / {{ documentos.length }}
          </div>
          <div class="lightbox-topbar__actions">
            <button class="lightbox-action-btn" title="Baixar" @click="fnBaixar(visualizadorDocumento)">
              <font-awesome-icon icon="download" />
            </button>
            <button class="lightbox-action-btn" title="Fechar (Esc)" @click="fecharVisualizador">
              <font-awesome-icon icon="xmark" />
            </button>
          </div>
        </div>

        <!-- Main: arrows + viewer -->
        <div class="lightbox-main">
          <button
            class="lightbox-arrow lightbox-arrow--prev"
            :class="{ 'lightbox-arrow--disabled': !visualizadorTemAnterior }"
            @click="irParaAnterior"
            aria-label="Anterior"
          >
            <font-awesome-icon icon="chevron-left" />
          </button>

          <div class="lightbox-viewer">
            <div v-if="visualizadorCarregando" class="lightbox-loading">
              <div class="lightbox-spinner"></div>
            </div>
            <template v-else-if="visualizadorDocumento">
              <!-- Imagem -->
              <img
                v-if="visualizadorDocumento.imagem && visualizadorDocumento.previewUrl"
                :src="visualizadorDocumento.previewUrl"
                :alt="visualizadorDocumento.nomeOriginal"
                class="lightbox-image"
                draggable="false"
              />
              <!-- PDF: ocupa toda a área -->
              <iframe
                v-else-if="visualizadorDocumento.pdf && visualizadorDocumento.previewUrl"
                :src="visualizadorDocumento.previewUrl"
                class="lightbox-pdf"
                frameborder="0"
              />
              <!-- Áudio -->
              <div v-else-if="visualizadorDocumento.audio && visualizadorDocumento.previewUrl" class="lightbox-media-center">
                <font-awesome-icon icon="file-audio" class="lightbox-media-icon" />
                <p class="lightbox-media-label">{{ visualizadorDocumento.nomeOriginal }}</p>
                <audio controls :src="visualizadorDocumento.previewUrl" class="lightbox-audio" />
              </div>
              <!-- Vídeo -->
              <video
                v-else-if="visualizadorDocumento.video && visualizadorDocumento.previewUrl"
                :src="visualizadorDocumento.previewUrl"
                class="lightbox-video"
                controls
              />
              <!-- Sem preview -->
              <div v-else class="lightbox-no-preview">
                <font-awesome-icon :icon="getIconeDocumento(visualizadorDocumento.nomeOriginal)" class="lightbox-no-preview-icon" />
                <p class="lightbox-no-preview-label">Visualização não disponível para este tipo de arquivo</p>
                <button class="lightbox-download-btn" @click="fnBaixar(visualizadorDocumento)">
                  <font-awesome-icon icon="download" class="me-2" />
                  Baixar arquivo
                </button>
              </div>
            </template>
          </div>

          <button
            class="lightbox-arrow lightbox-arrow--next"
            :class="{ 'lightbox-arrow--disabled': !visualizadorTemProximo }"
            @click="irParaProximo"
            aria-label="Próximo"
          >
            <font-awesome-icon icon="chevron-right" />
          </button>
        </div>

        <!-- Footer: meta + tags + thumbnails -->
        <div class="lightbox-footer">
          <div class="lightbox-footer__meta">
            <div class="lightbox-footer__desc">
              {{ visualizadorDocumento?.descricao || 'Sem descrição' }}
            </div>
            <div class="lightbox-footer__info">
              <span>{{ formatarTamanho(visualizadorDocumento?.tamanhoBytes) }}</span>
              <span v-if="visualizadorDocumento?.dataCriacaoFormatada"> • {{ visualizadorDocumento.dataCriacaoFormatada }}</span>
              <span v-if="visualizadorDocumento?.publico === false"> • Privado</span>
              <span v-if="visualizadorDocumento?.fotoExecucao"> •
                <font-awesome-icon icon="camera" class="me-1" />Foto de execução
              </span>
            </div>
            <div v-if="visualizadorDocumento?.tags?.length" class="lightbox-footer__tags">
              <span class="lightbox-tags-label">Tags</span>
              <v-chip
                v-for="t in visualizadorDocumento.tags"
                :key="t.id"
                size="small"
                variant="tonal"
                color="primary"
                class="lightbox-tag-chip"
              >
                {{ t.nome }}
              </v-chip>
            </div>
          </div>

          <!-- Thumbnails strip -->
          <div class="lightbox-thumbs" ref="lightboxThumbsRef">
            <button
              v-for="(doc, i) in documentos"
              :key="doc.id"
              class="lightbox-thumb"
              :class="{ 'lightbox-thumb--ativo': i === visualizadorIndex }"
              @click="irParaIndice(i)"
              :title="doc.nomeOriginal"
            >
              <img v-if="doc.imagem && doc.previewUrl" :src="doc.previewUrl" draggable="false" />
              <font-awesome-icon v-else :icon="getIconeDocumento(doc.nomeOriginal)" />
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.lightbox-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background: rgba(0, 0, 0, 0.94);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* Top bar */
.lightbox-topbar {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 16px;
  background: rgba(0, 0, 0, 0.55);
  border-bottom: 1px solid rgba(255, 255, 255, 0.07);
  flex-shrink: 0;
  min-height: 52px;
}

.lightbox-topbar__filename {
  flex: 1;
  min-width: 0;
  color: #fff;
  font-size: 14px;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  opacity: 0.88;
}

.lightbox-topbar__counter {
  color: rgba(255, 255, 255, 0.5);
  font-size: 13px;
  white-space: nowrap;
  flex-shrink: 0;
  letter-spacing: 0.3px;
}

.lightbox-topbar__actions {
  display: flex;
  gap: 6px;
  flex-shrink: 0;
}

.lightbox-action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border: none;
  background: rgba(255, 255, 255, 0.08);
  color: rgba(255, 255, 255, 0.7);
  border-radius: 6px;
  cursor: pointer;
  font-size: 15px;
  transition: background 0.15s, color 0.15s;
}

.lightbox-action-btn:hover {
  background: rgba(255, 255, 255, 0.16);
  color: #fff;
}

/* Main area */
.lightbox-main {
  flex: 1;
  display: flex;
  align-items: stretch;
  min-height: 0;
}

/* Navigation arrows */
.lightbox-arrow {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  width: 60px;
  border: none;
  background: transparent;
  color: rgba(255, 255, 255, 0.45);
  font-size: 22px;
  cursor: pointer;
  transition: color 0.15s, background 0.15s;
  user-select: none;
}

.lightbox-arrow:hover:not(.lightbox-arrow--disabled) {
  color: #fff;
  background: rgba(255, 255, 255, 0.06);
}

.lightbox-arrow--disabled {
  opacity: 0.15;
  cursor: default;
  pointer-events: none;
}

/* Viewer */
.lightbox-viewer {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 0;
  overflow: hidden;
  padding: 12px 0;
}

/* Loading spinner */
.lightbox-loading {
  display: flex;
  align-items: center;
  justify-content: center;
}

.lightbox-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(255, 255, 255, 0.12);
  border-top-color: rgba(255, 255, 255, 0.65);
  border-radius: 50%;
  animation: lightbox-spin 0.75s linear infinite;
}

@keyframes lightbox-spin {
  to { transform: rotate(360deg); }
}

/* Image */
.lightbox-image {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  border-radius: 4px;
  box-shadow: 0 6px 40px rgba(0, 0, 0, 0.6);
  user-select: none;
  display: block;
}

/* PDF: fills full viewer area */
.lightbox-pdf {
  width: 100%;
  height: 100%;
  border: none;
  background: #fff;
  border-radius: 3px;
  display: block;
}

/* Video */
.lightbox-video {
  max-width: 100%;
  max-height: 100%;
  border-radius: 4px;
  box-shadow: 0 6px 40px rgba(0, 0, 0, 0.6);
}

/* Audio */
.lightbox-media-center {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
  color: #fff;
}

.lightbox-media-icon {
  font-size: 72px;
  opacity: 0.35;
}

.lightbox-media-label {
  font-size: 14px;
  opacity: 0.65;
  margin: 0;
  text-align: center;
}

.lightbox-audio {
  width: 360px;
  max-width: 90vw;
}

/* No preview */
.lightbox-no-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 18px;
  color: rgba(255, 255, 255, 0.65);
  text-align: center;
  padding: 24px;
}

.lightbox-no-preview-icon {
  font-size: 80px;
  opacity: 0.25;
}

.lightbox-no-preview-label {
  font-size: 14px;
  margin: 0;
  opacity: 0.65;
}

.lightbox-download-btn {
  display: inline-flex;
  align-items: center;
  padding: 10px 22px;
  background: rgba(255, 255, 255, 0.1);
  color: #fff;
  border: 1px solid rgba(255, 255, 255, 0.18);
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: background 0.15s;
}

.lightbox-download-btn:hover {
  background: rgba(255, 255, 255, 0.18);
}

/* Footer */
.lightbox-footer {
  flex-shrink: 0;
  background: rgba(0, 0, 0, 0.55);
  border-top: 1px solid rgba(255, 255, 255, 0.07);
}

.lightbox-footer__meta {
  display: flex;
  flex-direction: column;
  gap: 3px;
  padding: 8px 16px 6px;
}

.lightbox-footer__desc {
  color: rgba(255, 255, 255, 0.82);
  font-size: 13px;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.lightbox-footer__info {
  color: rgba(255, 255, 255, 0.4);
  font-size: 12px;
}

.lightbox-footer__tags {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px;
  margin-top: 5px;
}

.lightbox-tags-label {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: rgba(255, 255, 255, 0.38);
  margin-right: 2px;
  flex-shrink: 0;
}

.lightbox-tag-chip {
  opacity: 0.85;
}

/* Thumbnails strip */
.lightbox-thumbs {
  display: flex;
  gap: 6px;
  padding: 6px 16px 12px;
  overflow-x: auto;
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 255, 255, 0.18) transparent;
}

.lightbox-thumbs::-webkit-scrollbar {
  height: 4px;
}

.lightbox-thumbs::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.18);
  border-radius: 2px;
}

.lightbox-thumb {
  flex-shrink: 0;
  width: 56px;
  height: 56px;
  border-radius: 6px;
  overflow: hidden;
  cursor: pointer;
  border: 2px solid transparent;
  background: rgba(255, 255, 255, 0.07);
  display: flex;
  align-items: center;
  justify-content: center;
  color: rgba(255, 255, 255, 0.45);
  font-size: 20px;
  transition: border-color 0.15s, opacity 0.15s;
  opacity: 0.5;
  padding: 0;
}

.lightbox-thumb:hover {
  opacity: 0.8;
  border-color: rgba(255, 255, 255, 0.35);
}

.lightbox-thumb--ativo {
  border-color: #fff;
  opacity: 1;
}

.lightbox-thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

/* Fade transition */
.lightbox-fade-enter-active,
.lightbox-fade-leave-active {
  transition: opacity 0.2s ease;
}

.lightbox-fade-enter-from,
.lightbox-fade-leave-to {
  opacity: 0;
}

@media (max-width: 600px) {
  .lightbox-arrow {
    width: 40px;
    font-size: 18px;
  }

  .lightbox-topbar__filename {
    font-size: 12px;
  }
}
</style>
