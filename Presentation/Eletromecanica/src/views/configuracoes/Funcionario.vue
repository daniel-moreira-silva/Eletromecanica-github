<script setup>
import { ref, inject, onMounted } from "vue";

import Grid from "@/components/common/GridComponent.vue";
import Paginacao from "@/components/common/PaginacaoComponent.vue";
import Loading from "@/components/base/LoadingOverlay.vue";
import BaseButton from "@/components/base/BaseButton.vue";
import Snackbar from "@/components/base/Snackbar.vue";

import FuncionarioService from "@/services/configuracoes/funcionario-service.js";
import CargoService from "@/services/configuracoes/cargo-service.js";
import SetorService from "@/services/configuracoes/setor-service.js";
import TipoFuncionarioService from "@/services/configuracoes/tipo-funcionario-service.js";

import { usePermissoesTela } from "@/composables/usePermissoesTela";
import { usePadraoPermissao } from "@/composables/usePadraoPermissao";

const endpoint = inject("endpoint");
const headerPadrao = inject("headerPadrao");
const chaveSeguranca = inject("chaveSeguranca");
const usuarioSeguranca = inject("usuarioSeguranca");

const funcionarioService = new FuncionarioService(
  endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca
);
const cargoService = new CargoService(
  endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca
);
const setorService = new SetorService(
  endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca
);
const tipoFuncionarioService = new TipoFuncionarioService(
  endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca
);

/* ---------- estado ---------- */
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

/* ---------- opções dos selects ---------- */
const cargosOptions = ref([]);
const setoresOptions = ref([]);
const tiposFuncionarioOptions = ref([]);

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
    customButtonIcon: "pencil",
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
    item.value = { terceirizado: false, ativo: true };
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

/* ---------- CRUD ---------- */
async function salvar() {
  const validation = await formulario.value.validate();
  if (!validation.valid) return;

  const request = JSON.parse(JSON.stringify(item.value));

  loading.value = true;
  const resultado = await funcionarioService.salvar(request);
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
  const resultado = await funcionarioService.atualizar(request);
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
    const resp = await funcionarioService.atualizarStatus(id, valor);

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

/* ---------- filtro / paginação ---------- */
const filtro = ref({
  pagina: 1,
  itensPagina: 10,
  codigo: null,
  nome: null,
  cargoId: null,
  setorId: null,
  tipoFuncionarioId: null,
  terceirizado: null,
  ativo: null,
  ordenarPor: "Nome", // enum backend: Codigo | Nome | Cargo | Setor | TipoFuncionario
  ordem: "Asc",       // enum backend: Asc | Desc
  todos: null,
});

function filtrar() {
  filtro.value.pagina = 1;
  listarItens();
}

function limparFiltro() {
  filtro.value.pagina = 1;
  filtro.value.codigo = "";
  filtro.value.nome = "";
  filtro.value.cargoId = null;
  filtro.value.setorId = null;
  filtro.value.tipoFuncionarioId = null;
  filtro.value.terceirizado = null;
  filtro.value.ativo = null;
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
  const result = await funcionarioService.listar(filtro.value);
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

/* ---------- carrega catálogos ---------- */
async function carregarCatalogos() {
  const [cargosResp, setoresResp, tiposResp] = await Promise.all([
    cargoService.buscarTodos(),
    setorService.buscarTodos(),
    tipoFuncionarioService.buscarTodos(),
  ]);

  if (cargosResp?.statusCode === 200) {
    cargosOptions.value = cargosResp?.data?.data || [];
  }
  if (setoresResp?.statusCode === 200) {
    setoresOptions.value = setoresResp?.data?.data || [];
  }
  if (tiposResp?.statusCode === 200) {
    tiposFuncionarioOptions.value = tiposResp?.data?.data || [];
  }
}

onMounted(async () => {
  await carregarCatalogos();
  await listarItens();
});

/* ---------- configuração do Grid ---------- */
const fields = ref([
  {
    descricao: "Matrícula",
    valor: "codigo",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Nome",
    valor: "nome",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Cargo",
    valor: "cargo",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: true,
  },
  {
    descricao: "Setor",
    valor: "setor",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: true,
  },
  {
    descricao: "Tipo",
    valor: "tipoFuncionario",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: true,
  },
  {
    descricao: "Terceirizado",
    valor: "terceirizadoDescricao",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: true,
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
                    <v-col cols="12" md="6" class="pa-0 pb-4 pr-md-2">
                      <v-text-field
                        v-model="filtro.codigo"
                        label="Matrícula"
                        clearable
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      />
                    </v-col>

                    <v-col cols="12" md="6" class="pa-0 pb-4 pl-md-2">
                      <v-text-field
                        v-model="filtro.nome"
                        label="Nome"
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
                        v-model="filtro.cargoId"
                        :items="cargosOptions"
                        item-title="descricao"
                        item-value="id"
                        label="Cargo"
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
                        v-model="filtro.setorId"
                        :items="setoresOptions"
                        item-title="descricao"
                        item-value="id"
                        label="Setor"
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
                        v-model="filtro.tipoFuncionarioId"
                        :items="tiposFuncionarioOptions"
                        item-title="descricao"
                        item-value="id"
                        label="Tipo de Funcionário"
                        clearable
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      />
                    </v-col>

                    <v-col cols="12" md="6" class="pa-0 pb-4 pr-md-2">
                      <v-select
                        :items="[
                          { text: 'Todos', value: null },
                          { text: 'Sim', value: true },
                          { text: 'Não', value: false },
                        ]"
                        v-model="filtro.terceirizado"
                        item-title="text"
                        item-value="value"
                        label="Terceirizado"
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      />
                    </v-col>

                    <v-col cols="12" md="6" class="pa-0 pb-4 pl-md-2">
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
                  :icon="inserindo ? 'plus' : 'pencil'"
                  class="text-primary mr-1"
                />
                <span class="title black--text">
                  {{ inserindo ? "Inserir Funcionário" : "Editar Funcionário" }}
                </span>
                <v-spacer></v-spacer>
                <font-awesome-icon
                  icon="xmark"
                  class="text-black cursor-pointer"
                  @click="modalDadosMestre = false"
                />
              </div>

              <v-divider class="pb-4"></v-divider>

              <v-form ref="formulario">
                <v-container class="pa-3 pt-2 container-modal">
                  <!-- Identificação -->
                  <v-row>
                    <v-col cols="12" md="4" class="pa-1">
                      <v-text-field
                        v-model="item.codigo"
                        label="Matrícula"
                        clearable
                        density="compact"
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                        :rules="[(v) => !!v || 'Matrícula é obrigatória']"
                      />
                    </v-col>
                    <v-col cols="12" md="8" class="pa-1">
                      <v-text-field
                        v-model="item.nome"
                        label="Nome"
                        clearable
                        density="compact"
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                        :rules="[(v) => !!v || 'Nome é obrigatório']"
                      />
                    </v-col>
                  </v-row>

                  <!-- Vínculo -->
                  <v-row>
                    <v-col cols="12" md="4" class="pa-1">
                      <v-select
                        v-model="item.cargoId"
                        :items="cargosOptions"
                        item-title="descricao"
                        item-value="id"
                        label="Cargo"
                        density="compact"
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                        :rules="[(v) => !!v || 'Cargo é obrigatório']"
                      />
                    </v-col>
                    <v-col cols="12" md="4" class="pa-1">
                      <v-select
                        v-model="item.setorId"
                        :items="setoresOptions"
                        item-title="descricao"
                        item-value="id"
                        label="Setor"
                        density="compact"
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                        :rules="[(v) => !!v || 'Setor é obrigatório']"
                      />
                    </v-col>
                    <v-col cols="12" md="4" class="pa-1">
                      <v-select
                        v-model="item.tipoFuncionarioId"
                        :items="tiposFuncionarioOptions"
                        item-title="descricao"
                        item-value="id"
                        label="Tipo de Funcionário"
                        density="compact"
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                        :rules="[(v) => !!v || 'Tipo é obrigatório']"
                      />
                    </v-col>
                  </v-row>

                  <!-- Terceirização -->
                  <v-row>
                    <v-col cols="12" class="pa-1">
                      <div
                        class="terceirizado-toggle"
                        :class="item.terceirizado ? 'state-active' : 'state-inactive'"
                        @click="item.terceirizado = !item.terceirizado"
                      >
                        <div class="terceirizado-toggle__icon">
                          <font-awesome-icon
                            :icon="item.terceirizado ? 'building' : 'user-tie'"
                            :class="item.terceirizado ? 'icon-active' : 'icon-inactive'"
                          />
                        </div>
                        <div class="terceirizado-toggle__content">
                          <span class="terceirizado-toggle__label">
                            {{ item.terceirizado ? 'Terceirizado' : 'Funcionário Interno' }}
                          </span>
                          <span class="terceirizado-toggle__desc">
                            {{ item.terceirizado ? 'Vínculo com empresa terceirizada' : 'Clique para marcar como terceirizado' }}
                          </span>
                        </div>
                        <v-switch
                          v-model="item.terceirizado"
                          density="compact"
                          hide-details
                          color="primary"
                          class="terceirizado-toggle__switch"
                          @click.stop
                        />
                      </div>
                    </v-col>
                  </v-row>
                </v-container>
              </v-form>

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
.terceirizado-toggle {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  border-radius: 4px;
  cursor: pointer;
  transition: border-color 0.2s, background-color 0.2s;
  user-select: none;
}

/* ── inactive: dashed border, muted colors ── */
.terceirizado-toggle.state-inactive {
  border: 1px dashed rgba(0, 0, 0, 0.28);
  background-color: rgba(0, 0, 0, 0.02);
}

.terceirizado-toggle.state-inactive:hover {
  border-color: rgba(0, 0, 0, 0.5);
  background-color: rgba(0, 0, 0, 0.04);
}

/* ── active: solid primary border + tinted background ── */
.terceirizado-toggle.state-active {
  border: 1px solid rgb(var(--v-theme-primary));
  background-color: rgba(var(--v-theme-primary), 0.06);
}

.terceirizado-toggle.state-active:hover {
  background-color: rgba(var(--v-theme-primary), 0.1);
}

/* ── icon ── */
.terceirizado-toggle__icon {
  font-size: 18px;
  width: 24px;
  text-align: center;
  flex-shrink: 0;
}

.icon-inactive {
  color: rgba(0, 0, 0, 0.3);
}

.icon-active {
  color: rgb(var(--v-theme-primary));
}

/* ── text ── */
.terceirizado-toggle__content {
  display: flex;
  flex-direction: column;
  flex: 1;
  line-height: 1.3;
}

.terceirizado-toggle__label {
  font-size: 13px;
  font-weight: 500;
}

.state-inactive .terceirizado-toggle__label {
  color: rgba(0, 0, 0, 0.45);
}

.state-active .terceirizado-toggle__label {
  color: rgb(var(--v-theme-primary));
}

.terceirizado-toggle__desc {
  font-size: 11px;
  color: rgba(0, 0, 0, 0.4);
}

.terceirizado-toggle__switch {
  flex-shrink: 0;
}
</style>