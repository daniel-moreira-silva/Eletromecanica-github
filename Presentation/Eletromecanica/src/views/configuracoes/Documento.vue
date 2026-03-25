<script setup>
import { ref, inject, onMounted, onBeforeUnmount, watch, computed,
  nextTick,
  defineProps,
  defineEmits,
} from "vue";

import Snackbar from "@/components/base/Snackbar.vue";
import DocumentoService from "@/services/configuracoes/documento-service.js";

const props = defineProps({
  entidadeId: { type: [String, Number], default: null },
  tipoEntidade: { type: [String, Number], default: null },
  disabled: { type: Boolean, default: false },
  loading: { type: Boolean, default: false },
});

const emit = defineEmits(["update:loading"]);

const endpoint = inject("endpoint");
const headerPadrao = inject("headerPadrao");
const chaveSeguranca = inject("chaveSeguranca");
const usuarioSeguranca = inject("usuarioSeguranca");

const documentoService = new DocumentoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);

const objectUrls = ref(new Set());

const retorno = ref(false);
const mensagemRetorno = ref(null);
const sucesso = ref(true);

const anexos = ref([]);

const anexoSelecionado = ref({});
const modalExclusao = ref(false);
const modalRenomear = ref(false);
const novoNome = ref("");

const fileInput = ref(null);
const formRenomear = ref(null);

const dropArea = ref(null);
const anexoContainer = ref(null);
const iconeNenhumAnexo = ref(null);

const modalTags = ref(false);
const tagsDisponiveis = ref([]);        // autocomplete
const tagsSelecionadas = ref([]);       // [{id, nome}]
const tagSearch = ref("");
const tagNova = ref("");
const anexoParaTag = ref(null);         // item selecionado para tag

const podeUsarArquivos = computed(() => {
  if (props.disabled) return false;
  return !!props.entidadeId;
});

const loading = computed({
  get: () => props.loading,
  set: (v) => emit("update:loading", v),
});

async function abrirModalTags(item) {
  anexoParaTag.value = item;
  modalTags.value = true;
  tagSearch.value = "";
  tagNova.value = "";
  tagsDisponiveis.value = [];

  // carrega tags disponiveis
  tagsDisponiveis.value = [];
  await buscarTags("");

  tagsSelecionadas.value = (item.tags || []).map(t => ({ id: t.id, nome: t.nome }));
}

async function criarESelecionarTag() {
  const nome = (tagNova.value || "").trim();
  if (!nome) return;

  // evita duplicar por nome na UI
  const jaTem = tagsSelecionadas.value.some(t => t.nome.toLowerCase() === nome.toLowerCase());
  if (jaTem) {
    tagNova.value = "";
    return;
  }

  loading.value = true;
  const resp = await documentoService.criarTag(nome);
  loading.value = false;

  if (resp?.statusCode === 200 && resp.data?.id) {
    tagsSelecionadas.value.push({ id: resp.data.id, nome: resp.data.nome });
    tagNova.value = "";
  } else {
    showSnack(resp?.data?.message || "Falha ao criar tag.", false);
  }
}

async function salvarTags() {
  const docId = anexoParaTag.value?.id;
  if (!docId) return;

  const tagIds = tagsSelecionadas.value.map(t => t.id);

  loading.value = true;
  const resp = await documentoService.salvarTagsDoDocumento(docId, tagIds);
  loading.value = false;

  if (resp?.statusCode === 200) {
    // Atualiza só o item em memória (sem listarItens)
    const idx = anexos.value.findIndex(x => x.id === docId);
    if (idx >= 0) anexos.value[idx].tags = [...tagsSelecionadas.value];

    showSnack(resp?.data?.message || "Tags atualizadas.", true);
    modalTags.value = false;
  } else {
    showSnack(resp?.data?.message || "Falha ao salvar tags.", false);
  }
}

async function buscarTags(search) {
  const resp = await documentoService.listarTags(search);
  if (resp?.statusCode === 200) {
    tagsDisponiveis.value = (resp.data || []).map(t => ({ id: t.id, nome: t.nome }));
  }
}

function isImagem(mime) {
  return (mime || "").toLowerCase().startsWith("image/");
}

function isPdf(mime) {
  return (mime || "").toLowerCase() === "application/pdf";
}

function showSnack(msg, isSuccess = true) {
  retorno.value = true;
  mensagemRetorno.value = msg;
  sucesso.value = isSuccess;
}

function addObjectUrl(url) {
  objectUrls.value.add(url);
}

function revokeAllPreviews() {
  for (const url of objectUrls.value) {
    URL.revokeObjectURL(url);
  }
  objectUrls.value.clear();
}

async function carregarPreview(item) {
  if (!item?.id || item.previewUrl) return;

  const resp = await documentoService.viewDocumentoBlob(item.id);
  if (resp?.statusCode !== 200 || !(resp.data instanceof Blob)) return;

  const url = URL.createObjectURL(resp.data);
  item.previewUrl = url;
  addObjectUrl(url);
}

async function baixarAnexo(item) {
  if (!item?.id) return;

  const resp = await documentoService.downloadDocumentoBlob(item.id);
  if (resp?.statusCode !== 200 || !(resp.data instanceof Blob)) {
    showSnack("Não foi possível baixar o arquivo.", false);
    return;
  }

  const url = URL.createObjectURL(resp.data);
  addObjectUrl(url);

  const a = document.createElement("a");
  a.href = url;
  a.download = item.nomeOriginal || "download";
  a.click();
}

async function abrirAnexo(item) {
  if (!item?.id) return;

  if (!item.previewUrl) {
    await carregarPreview(item);
  }

  if (item.previewUrl) {
    window.open(item.previewUrl, "_blank");
    return;
  }

  showSnack("Não foi possível abrir o arquivo.", false);
}

function fecharModalRenomear() {
  novoNome.value = "";
  anexoSelecionado.value = {};
  modalRenomear.value = false;
}

function fecharModalExclusao() {
  anexoSelecionado.value = {};
  modalExclusao.value = false;
}

async function listarItens() {
  if (!podeUsarArquivos.value) {
    anexos.value = [];
    return;
  }

  loading.value = true;
  const result = await documentoService.listarDocumentosPorEntidade(props.entidadeId);
  loading.value = false;

  if (result?.statusCode === 200) {
    const lista = result?.data ?? [];

    revokeAllPreviews();

    anexos.value = lista.map((x) => {
      const mime = x.mimeType || "application/octet-stream";
      return {
        ...x,
        menu: false,
        imagem: isImagem(mime),
        pdf: isPdf(mime),
        previewUrl: null, // <- novo
      };
    });

    // Se quisermos preview apenas pra imagens descomentar filter
    anexos.value
      //.filter(a => a.imagem)
      .slice(0, 12) // limite para não explodir requisições
      .forEach((a) => carregarPreview(a));
  } else {
    showSnack(result?.data?.message || "Falha ao listar arquivos.", false);
  }

  await nextTick();
  adicionarListeners();
}

function adicionarAnexo() {
  if (!podeUsarArquivos.value) return;
  fileInput.value?.click?.();
}

function filesUploaded() {
  const files = [...(fileInput.value?.files || [])];
  if (!files.length) return;

  incluirAnexos(files);

  fileInput.value.value = "";
}

async function incluirAnexos(files) {
  if (!props.entidadeId) return;

  loading.value = true;

  try {
    for (const file of files) {
      const form = new FormData();
      form.append("EntidadeId", props.entidadeId);
      form.append("EntidadeTipo", props.tipoEntidade); // tem que bater com o enum ETipoDocumento no backend
      form.append("Tipo", 0); // opcional: Manual, Foto, Laudo, Projeto
      form.append("Descricao", "");
      form.append("ObservacoesVinculo", "");
      form.append("Arquivo", file);

      await documentoService.adicionarDocumento(form);
    }

    await listarItens();
    showSnack("Arquivos adicionados com sucesso.", true);
  } catch (e) {
    showSnack("Falha ao adicionar arquivos.", false);
  } finally {
    loading.value = false;
  }
}

function confirmarExclusaoAnexo(item) {
  anexoSelecionado.value = { id: item.id, nomeOriginal: item.nomeOriginal };
  modalExclusao.value = true;
}

function confirmarTrocaNomeAnexo(item) {
  anexoSelecionado.value = item;
  modalRenomear.value = true;
  novoNome.value = item.nomeOriginal.replace(item.extensao, "");
}

async function renomearAnexo() {
  const valido = await formRenomear.value?.validate?.();
  if (!valido?.valid) return;

  modalRenomear.value = false;

  const payload = {
    ...anexoSelecionado.value,
    nomeOriginal: novoNome.value.trim().concat(anexoSelecionado.value.extensao)
  };

  loading.value = true;
  const result = await documentoService.atualizarDocumento(payload);
  loading.value = false;

  if (result?.statusCode === 200) {
    showSnack(result?.data?.message || "Arquivo renomeado.", true);
    fecharModalRenomear();
    listarItens();
  } else {
    showSnack(result?.data?.message || "Falha ao renomear.", false);
  }
}

async function excluirAnexo() {
  modalExclusao.value = false;

  const idx = anexos.value.findIndex((x) => x.id === anexoSelecionado.value.id);
  if (idx < 0) return;

  const id = anexos.value[idx].id;

  loading.value = true;
  const result = await documentoService.excluirDocumento(id);
  loading.value = false;

  if (result?.statusCode === 200) {
    anexos.value.splice(idx, 1);
    showSnack(result?.data?.message || "Arquivo excluído.", true);
    fecharModalExclusao();
    await nextTick();
    adicionarListeners();
  } else {
    showSnack(result?.data?.message || "Falha ao excluir.", false);
  }
}

function preventDefaults(e) {
  e.preventDefault();
  e.stopPropagation();
}

function handleDrop(e) {
  if (!podeUsarArquivos.value) return;
  const files = [...(e.dataTransfer?.files || [])];
  if (files.length) incluirAnexos(files);
}

let listenersAtivos = false;

function addDnDListeners(el) {
  if (!el) return;

  ["dragenter", "dragover", "dragleave", "drop"].forEach((evt) => {
    el.addEventListener(evt, preventDefaults);
  });

  ["dragenter", "dragover"].forEach((evt) => {
    el.addEventListener(evt, () => {
      el.classList.add("highlight");
      iconeNenhumAnexo.value?.classList?.add("icone-on-drop");
    });
  });

  ["dragleave", "drop"].forEach((evt) => {
    el.addEventListener(evt, () => {
      el.classList.remove("highlight");
      iconeNenhumAnexo.value?.classList?.remove("icone-on-drop");
    });
  });

  el.addEventListener("drop", handleDrop);
}

function removeDnDListeners(el) {
  if (!el) return;

  ["dragenter", "dragover", "dragleave", "drop"].forEach((evt) => {
    el.removeEventListener(evt, preventDefaults);
  });

  el.removeEventListener("drop", handleDrop);
}

function adicionarListeners() {
  if (listenersAtivos) return;

  const drop = dropArea.value;
  const cont = anexoContainer.value;

  // só faz sentido ter listeners quando for possível usar arquivos
  if (!podeUsarArquivos.value) return;

  // quando não tem anexo, dropArea existe; quando tem, cont existe
  if (drop) addDnDListeners(drop);
  if (cont) addDnDListeners(cont);

  listenersAtivos = true;
}

function removerListeners() {
  const drop = dropArea.value;
  const cont = anexoContainer.value;

  removeDnDListeners(drop);
  removeDnDListeners(cont);

  listenersAtivos = false;
}

watch(
  () => props.entidadeId,
  async () => {
    removerListeners();
    await listarItens();
  }
);

let tagTimer = null;

watch(tagSearch, (val) => {
  clearTimeout(tagTimer);
  tagTimer = setTimeout(() => buscarTags(val), 250);
});

onMounted(async () => {
  await listarItens();
});

onBeforeUnmount(() => {
  removerListeners();
  revokeAllPreviews();
});
</script>

<template>
  <div class="arquivos-tab">
    <v-alert v-if="!podeUsarArquivos" type="info" variant="tonal" class="mb-3">
      Para anexar arquivos, primeiro salve o item para gerar o ID.
    </v-alert>

    <div ref="anexoContainer" class="anexo-container">
      <input
        type="file"
        ref="fileInput"
        class="hidden"
        @change="filesUploaded"
        multiple
        :disabled="!podeUsarArquivos"
      />

      <div class="novo-anexo" v-if="anexos.length > 0">
        <v-tooltip text="Adicionar novos anexos" location="start">
          <template #activator="{ props: tp }">
            <v-btn
              icon
              variant="text"
              rounded="xl"
              color="primary"
              v-bind="tp"
              @click="adicionarAnexo"
              :disabled="!podeUsarArquivos"
            >
              <v-icon>plus</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
      </div>

      <v-row v-if="anexos.length > 0" no-gutters class="anexos-grid">
        <v-col
          v-for="(item, index) in anexos"
          :key="index"
          cols="12"
          sm="6"
          md="4"
          lg="4"
          class="anexo-col"
        >
          <v-card class="pa-2 anexo-card">
            <div class="anexo-menu">
              <v-menu
                offset="10"
                location="start"
                :close-on-content-click="true"
                v-model="item.menu"
                class="text-center"
              >
                <template #activator="{ props: mp }">
                  <v-icon v-bind="mp" color="white">chevron-down</v-icon>
                </template>

                <v-card class="max-height-search-box">
                  <v-card-text class="pa-0">
                    <v-list class="pa-0">
                      <v-list-item @click="abrirModalTags(item)">
                        <v-list-item-title class="d-flex align-center">
                          <v-icon color="primary" class="mr-1">
                            plus
                          </v-icon>
                          Add Tag
                        </v-list-item-title>
                      </v-list-item>
                      <v-list-item
                        @click="abrirAnexo(item)"
                        v-if="item.imagem || item.pdf"
                      >
                        <v-list-item-title class="d-flex align-center">
                          <v-icon left color="primary">
                            mdi:mdi-folder-open-outline
                          </v-icon>
                          Abrir
                        </v-list-item-title>
                      </v-list-item>

                      <v-list-item @click="baixarAnexo(item)">
                        <v-list-item-title class="d-flex align-center">
                          <v-icon color="primary" class="mr-1">
                            mdi:mdi-cloud-download-outline
                          </v-icon>
                          Download
                        </v-list-item-title>
                      </v-list-item>


                      <v-list-item @click="confirmarTrocaNomeAnexo(item)">
                        <v-list-item-title class="d-flex align-center">
                          <v-icon class="mr-1" color="primary">
                            mdi:mdi-pencil-outline
                          </v-icon>
                          Renomear
                        </v-list-item-title>
                      </v-list-item>

                      <v-list-item @click="confirmarExclusaoAnexo(item)">
                        <v-list-item-title class="d-flex align-center" color="red">
                          <v-icon class="mr-1" color="red">
                            mdi:mdi-trash-can-outline
                          </v-icon>
                          Excluir
                        </v-list-item-title>
                      </v-list-item>
                    </v-list>
                  </v-card-text>
                </v-card>
              </v-menu>
            </div>

            <div class="preview-area">
              <img
                :src="item.previewUrl"
                alt="imagem"
                v-if="item.imagem && item.previewUrl"
              />

              <div class="anexo-embed-container" v-else-if="item.pdf && item.previewUrl">
                <embed :src="item.previewUrl" scrolling="no" />
              </div>

              <v-icon class="anexo-icone" v-else>
                mdi:mdi-file-document-multiple-outline
              </v-icon>
            </div>

            <div class="mt-1" v-if="item.tags?.length">
              <v-chip
                v-for="t in item.tags"
                :key="t.id"
                size="x-small"
                class="ma-1"
                variant="tonal"
                color="primary"
              >
                {{ t.nome }}
              </v-chip>
            </div>


            <v-tooltip :text="item.nomeOriginal" location="bottom">
              <template #activator="{ props: np }">
                <div class="nome-anexo" v-bind="np">
                  {{ item.nomeOriginal }}
                </div>
              </template>
            </v-tooltip>
          </v-card>
        </v-col>
      </v-row>

      <!-- Vazio -->
      <div class="nenhum-anexo" ref="dropArea" v-if="anexos.length === 0">
        <div class="without-pointer icone-container" ref="iconeNenhumAnexo">
          <v-icon icon="mdi:mdi-cloud-upload-outline" class="icone-nenhum-anexo" />
        </div>

        <div class="texto-nenhum-anexo without-pointer">Arraste arquivos aqui</div>
        <div class="mt-4 without-pointer">- ou -</div>

        <div class="mt-2">
          <v-btn
            variant="text"
            color="primary"
            @click="adicionarAnexo"
            :disabled="!podeUsarArquivos"
          >
            <v-icon class="mr-1">plus</v-icon>
            Clique para abrir o explorador de arquivos
          </v-btn>
        </div>
      </div>
    </div>

    <!-- Modal exclusão -->
    <v-dialog v-model="modalExclusao" max-width="600px">
      <v-card>
        <v-card-text class="pa-4">
          <div class="d-flex align-center pb-2">
            <v-icon color="red" class="mr-1">trash-can-outline</v-icon>
            <span class="title black--text">Excluir anexo</span>
            <v-spacer />
            <v-icon color="black" @click="fecharModalExclusao">close</v-icon>
          </div>
          <v-divider class="pb-4" />
          <p>
            Deseja realmente excluir o anexo "<b>{{ anexoSelecionado.nomeOriginal }}</b
            >"?
          </p>
        </v-card-text>
        <v-card-actions class="pt-0">
          <v-spacer />
          <v-btn color="grey" text right class="text-caption" @click="fecharModalExclusao">
            <v-icon class="text-caption">close</v-icon>NÃO
          </v-btn>
          <v-btn color="red" text class="text-caption" @click="excluirAnexo">
            <v-icon class="text-caption">check</v-icon>SIM
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal renomear -->
    <v-dialog v-model="modalRenomear" max-width="600px">
      <v-card>
        <v-card-text class="pa-4">
          <div class="d-flex align-center pb-2">
            <v-icon color="primary" class="mr-1">pencil</v-icon>
            <span class="title black--text">Renomear anexo</span>
            <v-spacer />
            <v-icon color="black" @click="fecharModalRenomear">close</v-icon>
          </div>
          <v-divider class="pb-4" />

          <v-form ref="formRenomear" class="pt-4">
            <v-text-field
              label="Nome do arquivo (sem extensão)"
              v-model="novoNome"
              density="compact"
              variant="outlined"
              color="grey-darken-1"
              base-color="grey-darken-1"
              :rules="[
                (v) =>
                  !!String(v || '').trim() || 'O campo nome do arquivo é obrigatório',
              ]"
              @keydown.enter.prevent
              autofocus
            />
          </v-form>
        </v-card-text>

        <v-card-actions class="pt-0">
          <v-spacer />
          <v-btn color="red" text right class="text-caption" @click="fecharModalRenomear">
            <v-icon class="text-caption">close</v-icon>NÃO
          </v-btn>
          <v-btn color="primary" text class="text-caption" @click="renomearAnexo">
            <v-icon class="text-caption">check</v-icon>SIM
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Modal tags -->
    <v-dialog v-model="modalTags" max-width="700px">
      <v-card>
        <v-card-text class="pa-4">
          <div class="d-flex align-center pb-2">
            <v-icon color="primary" class="mr-1">mdi:mdi-tag-outline</v-icon>
            <span class="title black--text">Tags do arquivo</span>
            <v-spacer />
            <v-icon color="black" @click="modalTags = false">close</v-icon>
          </div>

          <v-divider class="pb-4" />

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

            <div v-if="tagsSelecionadas.length === 0" class="text-caption text-grey">
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
          />

          <!-- Criar nova -->
          <div class="d-flex align-center mt-3">
            <v-text-field
              v-model="tagNova"
              label="Criar nova tag"
              density="compact"
              variant="outlined"
              class="mr-2 flex-grow-1"
              hide-details
              @keydown.enter.prevent="criarESelecionarTag"
            />

            <v-btn
              color="primary"
              variant="text"
              density="compact"
              height="40"
              @click="criarESelecionarTag"
            >
              <v-icon class="mr-1">mdi:mdi-plus</v-icon>
              Criar
            </v-btn>
          </div>
        </v-card-text>

        <v-card-actions class="pt-0">
          <v-spacer />
          <v-btn color="grey" variant="text" @click="modalTags = false">Cancelar</v-btn>
          <v-btn color="primary" variant="text" @click="salvarTags">Salvar</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <Snackbar
      :retorno="retorno"
      :timeout="3000"
      :tipo="sucesso ? 'sucesso' : 'erro'"
      :mensagemRetorno="mensagemRetorno"
      @ocultarRetorno="() => (retorno = false)"
    />
  </div>
</template>

<style scoped>
.arquivos-tab {
  min-height: 260px;
}

.anexo-container {
  width: 100%;
  overflow-x: hidden;
  overflow-y: auto;
  box-sizing: border-box;
  position: relative;
  max-height: min(70vh, 560px);
  padding-top: 36px;
}

.anexos-grid {
  margin: 0 !important;
}

.anexo-col {
  padding: 8px !important;
  box-sizing: border-box;
}

.nenhum-anexo {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 320px;
  flex-direction: column;
  color: #b0b0b0;
  cursor: default;
  border: 1px dashed rgba(0, 0, 0, 0.15);
  border-radius: 10px;
}

.without-pointer {
  pointer-events: none;
}

.icone-nenhum-anexo {
  font-size: 80px;
  color: #b0b0b0;
  cursor: default;
}

.icone-container {
  transition: 0.3s ease;
}

.icone-on-drop {
  transform: translateY(-12px);
}

.texto-nenhum-anexo {
  font-size: 22px;
  padding-top: 6px;
  text-align: center;
  line-height: 26px;
}

.highlight {
  background-color: #efefef;
}

.hidden {
  display: none;
}

.nome-anexo {
  font-size: 12px;
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow: hidden;
  display: block;
  cursor: default;
  user-select: none;
}

.anexo-card {
  height: 244px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.preview-area {
  flex: 1;
  min-height: 0;
  width: 100%;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-area img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.anexo-icone {
  font-size: 160px;
  text-align: center;
  height: 208px;
  width: 100%;
}

.anexo-embed-container {
  width: 100%;
  height: 100%;
}

.anexo-embed-container embed {
  width: 100%;
  height: 100%;
  border: 0;
}

.anexo-menu {
  position: absolute;
  right: 0px;
  top: 0px;
  width: 100%;
  height: 210px;
  display: flex;
  align-items: flex-start;
  justify-content: flex-end;
  background-image: linear-gradient(
    45deg,
    transparent,
    transparent,
    transparent,
    transparent,
    transparent,
    #00000054
  );
  padding: 4px;
  z-index: 1;
}

.novo-anexo {
  position: absolute;
  right: 0px;
  top: 6px;
  z-index: 2;
}
</style>
