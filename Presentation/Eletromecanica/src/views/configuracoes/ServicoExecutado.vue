<script setup>
import { ref, inject, watch } from "vue";

import Grid from "@/components/common/GridComponent.vue";
import Paginacao from "@/components/common/PaginacaoComponent.vue";
import Loading from "@/components/base/LoadingOverlay.vue";
import BaseButton from "@/components/base/BaseButton.vue";
import Snackbar from "@/components/base/Snackbar.vue";
import ServicoExecutadoService from "@/services/configuracoes/servico-executado-service.js";

import { usePermissoesTela } from "@/composables/usePermissoesTela";
import { usePadraoPermissao } from "@/composables/usePadraoPermissao";

const endpoint = inject("endpoint");
const headerPadrao = inject("headerPadrao");
const chaveSeguranca = inject("chaveSeguranca");
const usuarioSeguranca = inject("usuarioSeguranca");

const servicoExecutadoService = new ServicoExecutadoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);

const lista = ref([]);
const totalPaginas = ref(1);
const totalItems = ref(0);

const modal = ref(false);
const modalDadosMestre = ref(false);

const gridComponent = ref(null);
const formulario = ref(null);

const inserindo = ref(true);
const item = ref({});

const loading = ref(false);
const retorno = ref(false);
const mensagemRetorno = ref(null);
const sucesso = ref(true);

// ✅ renomeado
const tabServicoExecutado = ref("dados");

const { hasPermission } = usePermissoesTela();
const canRead = hasPermission("Ler");

function handleFocusOut(event) {
  gridComponent.value.handleFocusOut(event);
}

const rawButtons = ref([
  {
    function: "inserir",
    customButtonIcon: "plus",
    customButtonDescription: "Inserir",
    color: "primary",
  },
  {
    function: "editar",
    customButtonIcon: "pen-to-square",
    customButtonDescription: "Editar",
    color: "primary",
    hasSelectedItem: true,
    hasMultipleAction: true,
  },
]);

const customButtonsList = usePadraoPermissao(rawButtons);

function customButtonClick(button) {
  if (button.function === "inserir") {
    modalDadosMestre.value = true;
    inserindo.value = true;
    item.value = {};
  } else if (button.function === "editar") {
    modalDadosMestre.value = true;
    inserindo.value = false;
    selecionarItem(button.selecionados[0]);
  }
}

function selecionarItem(selectedItem) {
  inserindo.value = false;
  item.value = JSON.parse(JSON.stringify(selectedItem));
  modalDadosMestre.value = true;
  gridComponent.value.unselectAll();
}

function onOcultarRetorno() {
  retorno.value = false;
}

async function salvar() {
  const validation = await formulario.value.validate();
  if (!validation.valid) return;

  const request = JSON.parse(JSON.stringify(item.value));

  loading.value = true;
  const resultado = await servicoExecutadoService.salvar(request);
  loading.value = false;

  retorno.value = true;

  if (resultado?.statusCode === 200) {
    modalDadosMestre.value = false;
    item.value = {};
    mensagemRetorno.value = resultado?.data.message;
    sucesso.value = true;
    listarItens();
  } else {
    mensagemRetorno.value = resultado?.data.message;
    sucesso.value = false;
  }
}

async function atualizar() {
  const validation = await formulario.value.validate();
  if (!validation.valid) return;

  const request = JSON.parse(JSON.stringify(item.value));

  loading.value = true;
  const resultado = await servicoExecutadoService.atualizar(request);
  loading.value = false;

  retorno.value = true;

  if (resultado?.statusCode === 200) {
    modalDadosMestre.value = false;
    item.value = {};
    mensagemRetorno.value = resultado?.data.message;
    sucesso.value = true;
    listarItens();
  } else {
    mensagemRetorno.value = resultado?.data.message;
    sucesso.value = false;
  }
}

async function alterarStatus({ item: itemGrid, valor }) {
  try {
    itemGrid.cor = valor ? null : "red";
    loading.value = true;

    const id = itemGrid?.id;
    const resp = await servicoExecutadoService.atualizarStatus(id, valor);

    loading.value = false;
    retorno.value = true;

    if (resp?.statusCode === 200) {
      mensagemRetorno.value = resp?.data?.message || "Status atualizado com sucesso.";
      sucesso.value = true;
    } else {
      mensagemRetorno.value = resp?.data?.message || "Falha ao atualizar status.";
      sucesso.value = false;
    }
  } catch (e) {
    loading.value = false;
    retorno.value = true;
    mensagemRetorno.value = "Falha ao atualizar status.";
    sucesso.value = false;
  } finally {
    await listarItens();
  }
}

const filtro = ref({
  pagina: 1,
  itensPagina: 10,
  codigo: null,
  descricao: null,
  ativo: null,
  ordenarPor: "Codigo",
  ordem: "Asc",
  todos: null,
});

function filtrar() {
  filtro.value.pagina = 1;
  listarItens();
}

function limparFiltro() {
  filtro.value.pagina = 1;
  filtro.value.ativo = null;
  filtro.value.codigo = "";
  filtro.value.descricao = "";
  filtro.value.todos = "";
  listarItens();
}

function alterarPagina(pagina) {
  filtro.value.pagina = pagina;
  listarItens();
}

function alterarItensPorPagina(itens) {
  filtro.value.itensPagina = itens;
  filtro.value.pagina = 1;
  listarItens();
}

function alterarOrdenacao(evento) {
  filtro.value.ordenarPor = evento.ordenarPor;
  filtro.value.ordem = evento.ordem;
  listarItens();
}

async function listarItens() {
  modal.value = false;

  loading.value = true;
  const result = await servicoExecutadoService.listar(filtro.value);
  loading.value = false;

  if (result?.statusCode === 200) {
    lista.value = (result?.data.data.lista || []).map((x) => ({
      ...x,
      cor: x.ativo === false || x.ativo === 0 ? "red" : null,
      selecionado: false,
    }));

    totalPaginas.value = result?.data.data.paginas;
    totalItems.value = result?.data.data.totalItens;
  } else {
    mensagemRetorno.value = result?.data.message;
    sucesso.value = false;
    retorno.value = true;
  }
}

listarItens();

const fields = ref([
  {
    descricao: "Código",
    valor: "codigo",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Descrição",
    valor: "descricao",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Status",
    valor: "ativo",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "switch",
    ordenado: null,
    ocultarResponsivo: false,
  },
]);

watch(modalDadosMestre, (abriu) => {
  if (abriu) tabServicoExecutado.value = "dados";
});
</script>

<template>
  <v-container fluid>
    <div @click="handleFocusOut($event)">
      <Loading :active="loading" v-if="loading" />

      <div class="pa-0">
        <Grid
          class="grid-component"
          ref="gridComponent"
          :fields="fields"
          :list="canRead ? lista : []"
          :filters="canRead ? filtro : {}"
          :hideFilters="!canRead"
          gridType="responsive"
          :filterType="canRead ? 'popup' : null"
          gridOverflow="horizontal"
          :gridResizable="true"
          @listarItens="listarItens($event)"
          @selecionarItem="selecionarItem($event)"
          @botaoClick="() => {}"
          @abrirFiltro="canRead && (modal = true)"
          :hasCheckbox="false"
          :customButtonsList="customButtonsList"
          @customButtonClick="customButtonClick($event)"
          @alterarOrdenacao="alterarOrdenacao($event)"
          @alterarStatus="alterarStatus($event)"
        />

        <Paginacao
          v-if="canRead"
          id="paginacao"
          :totalPaginas="totalPaginas"
          :paginaAtual="filtro.pagina"
          :totalItens="totalItems"
          @alterarItensPorPagina="alterarItensPorPagina($event)"
          @alterarPagina="alterarPagina($event)"
        />
      </div>

      <!-- MODAL FILTRO -->
      <div justify="center">
        <v-dialog v-model="modal" class="form-dialog">
          <v-card>
            <v-card-text class="pa-4">
              <div class="d-flex align-center pb-2">
                <font-awesome-icon icon="search" color="primary" class="me-1" />
                <span class="title black--text"> Filtrar </span>
                <v-spacer></v-spacer>
                <font-awesome-icon
                  icon="xmark"
                  color="close"
                  class="me-1 cursor-pointer"
                  @click="modal = false"
                />
              </div>

              <v-divider class="pb-4"></v-divider>

              <v-form>
                <v-container class="pa-3 pt-2 container-modal">
                  <v-row>
                    <v-col cols="12" class="pa-0 pb-4">
                      <v-text-field
                        v-model="filtro.codigo"
                        label="Código"
                        clearable
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      />
                    </v-col>

                    <v-col cols="12" class="pa-0 pb-4">
                      <v-text-field
                        v-model="filtro.descricao"
                        label="Descrição"
                        clearable
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      />
                    </v-col>

                    <v-col cols="12" class="pa-0 pb-4">
                      <v-select
                        :items="[
                          { text: 'Todos', value: null },
                          { text: 'Ativo', value: true },
                          { text: 'Inativo', value: false },
                        ]"
                        v-model="filtro.ativo"
                        item-title="text"
                        item-value="value"
                        label="Status"
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      />
                    </v-col>
                  </v-row>
                </v-container>
              </v-form>
            </v-card-text>

            <v-card-actions class="pt-0">
              <v-spacer></v-spacer>

              <BaseButton
                label="Limpar filtro"
                type="clear"
                iconPosition="left"
                @click="limparFiltro"
                extraClass="me-2"
              />

              <BaseButton
                label="Cancelar"
                type="cancel"
                iconPosition="left"
                @click="modal = false"
              />

              <BaseButton
                label="Filtrar"
                type="save"
                iconPosition="left"
                @click="filtrar"
              />
            </v-card-actions>
          </v-card>
        </v-dialog>
      </div>

      <!-- MODAL CADASTRO/EDIÇÃO -->
      <div justify="center">
        <v-dialog v-model="modalDadosMestre" class="form-dialog">
          <v-card>
            <v-card-text class="pa-4">
              <div class="d-flex align-center pb-2">
                <font-awesome-icon
                  :icon="inserindo ? 'plus' : 'pen-to-square'"
                  class="text-primary mr-1"
                />
                <span class="title black--text">
                  {{ inserindo ? "Inserir Serviço Executado" : "Editar Serviço Executado" }}
                </span>
                <v-spacer></v-spacer>
                <font-awesome-icon
                  icon="xmark"
                  class="text-black cursor-pointer"
                  @click="modalDadosMestre = false"
                />
              </div>

              <v-tabs
                slider-transition="grow"
                color="primary"
                v-model="tabServicoExecutado"
                density="compact"
              >
                <v-tab value="dados" text="Dados"></v-tab>
                <v-tab value="checklist" text="Check List"></v-tab>
              </v-tabs>

              <v-tabs-window v-model="tabServicoExecutado">
                <v-tabs-window-item value="dados">
                  <v-form ref="formulario">
                    <v-container class="pa-3 pt-6 container-modal">
                      <v-row>
                        <v-col cols="12" class="pa-1 mt-2">
                          <v-text-field
                            v-model="item.codigo"
                            label="Código"
                            clearable
                            density="compact"
                            variant="outlined"
                            color="grey-darken-1"
                            base-color="grey-darken-1"
                            :rules="[(v) => !!v || 'Código é um campo obrigatório']"
                          />
                        </v-col>
                      </v-row>

                      <v-row>
                        <v-col cols="12" class="pa-1">
                          <v-text-field
                            v-model="item.descricao"
                            label="Descrição"
                            clearable
                            density="compact"
                            variant="outlined"
                            color="grey-darken-1"
                            base-color="grey-darken-1"
                            :rules="[(v) => !!v || 'Descrição é um campo obrigatório']"
                          />
                        </v-col>
                      </v-row>
                    </v-container>
                  </v-form>
                </v-tabs-window-item>

                <v-tabs-window-item value="checklist">
                  <div></div>
                </v-tabs-window-item>
              </v-tabs-window>

              <v-card-actions class="pt-0">
                <v-spacer></v-spacer>

                <BaseButton
                  label="Cancelar"
                  type="cancel"
                  iconPosition="left"
                  @click="modalDadosMestre = false"
                />

                <BaseButton
                  v-if="(inserindo && hasPermission('Criar')) || (!inserindo && hasPermission('Editar'))"
                  label="Salvar"
                  type="save"
                  iconPosition="left"
                  @click="inserindo ? salvar() : atualizar()"
                />
              </v-card-actions>
            </v-card-text>
          </v-card>
        </v-dialog>
      </div>

      <Snackbar
        :retorno="retorno"
        :timeout="3000"
        :tipo="sucesso ? 'sucesso' : 'erro'"
        :mensagemRetorno="mensagemRetorno"
        @ocultarRetorno="onOcultarRetorno()"
      />
    </div>
  </v-container>
</template>

<style scoped>
.app-container {
  height: 100%;
  min-height: 0;
  display: flex;
  flex-direction: column;
}
</style>
