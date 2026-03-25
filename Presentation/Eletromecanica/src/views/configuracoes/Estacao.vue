<script setup>
import { ref, inject, watch, computed } from "vue";
import { useRouter } from "vue-router";

import Grid from "@/components/common/GridComponent.vue";
import Paginacao from "@/components/common/PaginacaoComponent.vue";
import Loading from "@/components/base/LoadingOverlay.vue";
import BaseButton from "@/components/base/BaseButton.vue";
import Snackbar from "@/components/base/Snackbar.vue";
import { GoogleMap, AdvancedMarker } from "vue3-google-map";

import EstacaoService from "@/services/configuracoes/estacao-service.js";
import EquipamentoService from "@/services/configuracoes/equipamento-service.js";
import GoogleMapsService from "@/services/ordem-servico/google-maps-service";

import { usePermissoesTela } from "@/composables/usePermissoesTela";
import { usePadraoPermissao } from "@/composables/usePadraoPermissao";

import DocumentoTab from "@/views/configuracoes/Documento.vue";

const endpoint = inject("endpoint");
const headerPadrao = inject("headerPadrao");
const chaveSeguranca = inject("chaveSeguranca");
const usuarioSeguranca = inject("usuarioSeguranca");
const apiKey = inject("apiKeyMaps");

const estacaoService = new EstacaoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);

const equipamentoService = new EquipamentoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);

const googleMapsService = new GoogleMapsService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);

const router = useRouter();

const lista = ref([]);
const listaTipoEstacao = ref([]);

const totalPaginas = ref(1);
const totalItems = ref(0);

const modal = ref(false);
const modalDadosMestre = ref(false);
const modalConfirmarVisualizacao = ref(false);

const gridComponent = ref(null);
const gridEquipamentosRef = ref(null);
const formulario = ref(null);

const loading = ref(false);

const retorno = ref(false);
const mensagemRetorno = ref(null);
const sucesso = ref(true);

const tabEstacao = ref("dados");
const inserindo = ref(true);

const { hasPermission } = usePermissoesTela();
const canRead = computed(() => hasPermission("Ler"));

const novoItem = () => ({
  id: null,
  nome: "",
  tipoEstacaoId: null,
  tipoEstacao: "",
  observacoes: "",
  endereco: "",
  bairro: "",
  numero: "",
  complemento: "",
  pontoReferencia: "",
  lat: "",
  long: "",
  ativo: true,
  equipamentos: []
});

const item = ref(novoItem());
const equipamentoSelecionado = ref(null);

const stationMapCenter = ref({ lat: -21.7617922, lng: -43.3439923 });
const stationMapZoom = ref(12);
const stationMapMarker = ref(null);

function normalizarItemEstacao(origem = {}) {
  return {
    ...novoItem(),
    ...origem,
    observacoes: origem?.observacoes ?? "",
    endereco: origem?.endereco ?? "",
    bairro: origem?.bairro ?? "",
    numero: origem?.numero ?? "",
    complemento: origem?.complemento ?? "",
    pontoReferencia: origem?.pontoReferencia ?? "",
    lat: origem?.lat ?? "",
    long: origem?.long ?? "",
    equipamentos: Array.isArray(origem?.equipamentos) ? origem.equipamentos : []
  };
}

function atualizarMapaDaEstacao() {
  if (!item.value?.lat || !item.value?.long) {
    stationMapMarker.value = null;
    stationMapCenter.value = { lat: -21.7617922, lng: -43.3439923 };
    stationMapZoom.value = 12;
    return;
  }

  const lat = parseFloat(item.value.lat);
  const lng = parseFloat(item.value.long);

  if (Number.isNaN(lat) || Number.isNaN(lng)) {
    stationMapMarker.value = null;
    stationMapCenter.value = { lat: -21.7617922, lng: -43.3439923 };
    stationMapZoom.value = 12;
    return;
  }

  stationMapCenter.value = { lat, lng };
  stationMapMarker.value = { lat, lng };
  stationMapZoom.value = 16;
}

async function onStationMapClick(e) {
  const lat = e.latLng.lat().toString();
  const lng = e.latLng.lng().toString();

  item.value.lat = lat;
  item.value.long = lng;

  loading.value = true;
  const result = await googleMapsService.buscarEndereco(lat, lng);
  loading.value = false;

  if (result?.statusCode === 200) {
    const data = result?.data?.data || {};

    item.value.endereco = data.rua || item.value.endereco;
    item.value.numero = data.numero || item.value.numero;
    item.value.bairro = data.bairro || item.value.bairro;
  } else {
    retorno.value = true;
    sucesso.value = false;
    mensagemRetorno.value =
      result?.data?.message || "Falha ao obter endereço a partir do mapa.";
  }

  atualizarMapaDaEstacao();
}

async function localizarEnderecoDaEstacao() {
  const query = [
    item.value.endereco,
    item.value.numero,
    item.value.bairro
  ]
    .filter(Boolean)
    .join(", ");

  if (!query) {
    retorno.value = true;
    sucesso.value = false;
    mensagemRetorno.value = "Informe ao menos endereço para localizar no mapa.";
    return;
  }

  loading.value = true;
  const result = await googleMapsService.buscarEnderecoTextoLivre(query, null, null);
  loading.value = false;

  if (result?.statusCode === 200) {
    const data = result?.data?.data || {};

    item.value.lat = String(data.lat ?? "");
    item.value.long = String(data.long ?? "");

    if (!item.value.endereco) item.value.endereco = data.rua || "";
    if (!item.value.numero) item.value.numero = data.numero || "";
    if (!item.value.bairro) item.value.bairro = data.bairro || "";

    atualizarMapaDaEstacao();
  } else {
    retorno.value = true;
    sucesso.value = false;
    mensagemRetorno.value =
      result?.data?.message || "Falha ao localizar endereço da estação.";
  }
}

function handleFocusOut(event) {
  gridComponent.value?.handleFocusOut?.(event);
}

function equipamentosHandleFocusOut(event) {
  gridEquipamentosRef.value?.handleFocusOut?.(event);
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

function onSelecionarComponente(itemSelecionado) {
  equipamentoSelecionado.value = itemSelecionado;
  modalConfirmarVisualizacao.value = true;
}

async function confirmarVisualizacao() {
  const id = equipamentoSelecionado.value?.id;
  modalConfirmarVisualizacao.value = false;

  if (!id) {
    equipamentoSelecionado.value = null;
    return;
  }

  try {
    loading.value = true;
    await router.push({ name: "EquipamentoDetalhe", params: { id } });
  } finally {
    loading.value = false;
    equipamentoSelecionado.value = null;
  }
}

function fecharModalVisualizacao() {
  modalConfirmarVisualizacao.value = false;
  equipamentoSelecionado.value = null;
}

async function customButtonClick(button) {
  if (button.function === "inserir") {
    inserindo.value = true;
    item.value = normalizarItemEstacao();
    modalDadosMestre.value = true;
    atualizarMapaDaEstacao();
    return;
  }

  if (button.function === "editar") {
    await selecionarItem(button.selecionados[0]);
  }
}

function montarRequest() {
  return {
    id: item.value.id,
    nome: item.value.nome,
    tipoEstacaoId: item.value.tipoEstacaoId,
    observacoes: item.value.observacoes,
    endereco: item.value.endereco,
    bairro: item.value.bairro,
    numero: item.value.numero,
    complemento: item.value.complemento,
    pontoReferencia: item.value.pontoReferencia,
    lat: item.value.lat,
    long: item.value.long,
    ativo: item.value.ativo
  };
}

async function persistir(acao) {
  const validation = await formulario.value?.validate?.();
  if (!validation?.valid) return;

  const request = montarRequest();

  loading.value = true;
  const resultado = await acao(request);
  loading.value = false;
  retorno.value = true;

  if (resultado?.statusCode === 200) {
    modalDadosMestre.value = false;
    item.value = normalizarItemEstacao();
    atualizarMapaDaEstacao();
    mensagemRetorno.value = resultado?.data?.message || "Registro salvo com sucesso.";
    sucesso.value = true;
    await listarItens();
  } else {
    mensagemRetorno.value = resultado?.data?.message || "Falha ao salvar.";
    sucesso.value = false;
  }
}

const salvar = () => persistir((req) => estacaoService.salvar(req));
const atualizar = () => persistir((req) => estacaoService.atualizar(req));

async function alterarStatus({ item: itemGrid, valor }) {
  try {
    const corAnterior = itemGrid.cor;
    itemGrid.cor = valor ? null : "red";

    loading.value = true;
    const id = itemGrid?.id;
    const resp = await estacaoService.atualizarStatus(id, valor);
    loading.value = false;
    retorno.value = true;

    if (resp?.statusCode === 200) {
      mensagemRetorno.value =
        resp?.data?.message || "Status atualizado com sucesso.";
      sucesso.value = true;
    } else {
      itemGrid.cor = corAnterior;
      mensagemRetorno.value =
        resp?.data?.message || "Falha ao atualizar status.";
      sucesso.value = false;
    }
  } catch {
    loading.value = false;
    retorno.value = true;
    mensagemRetorno.value = "Falha ao atualizar status.";
    sucesso.value = false;
  } finally {
    await listarItens();
  }
}

async function selecionarItem(selectedItem) {
  inserindo.value = false;
  item.value = normalizarItemEstacao(structuredClone(selectedItem));
  modalDadosMestre.value = true;
  gridComponent.value?.unselectAll?.();

  atualizarMapaDaEstacao();
  await carregarEquipamentosDaEstacao(item.value.id);
}

async function carregarEquipamentosDaEstacao(estacaoId) {
  item.value.equipamentos = [];
  if (!estacaoId) return;

  loading.value = true;
  const resp = await equipamentoService.listarEquipamentosPorEstacao(estacaoId, null);
  loading.value = false;

  if (resp?.statusCode === 200) {
    item.value.equipamentos = (resp?.data?.data || []).map((x) => ({
      ...x,
      selecionado: false
    }));
  } else {
    retorno.value = true;
    sucesso.value = false;
    mensagemRetorno.value =
      resp?.data?.message || "Falha ao carregar equipamentos da estação.";
  }
}

const filtro = ref({
  pagina: 1,
  itensPagina: 10,
  nome: null,
  endereco: null,
  tipoEstacaoId: null,
  ativo: null,
});

function filtrar() {
  filtro.value.pagina = 1;
  listarItens();
}

function limparFiltro() {
  filtro.value.pagina = 1;
  filtro.value.ativo = null;
  filtro.value.nome = "";
  filtro.value.endereco = "";
  filtro.value.tipoEstacaoId = "";
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

  const result = await estacaoService.listar(filtro.value);

  loading.value = false;

  if (result?.statusCode === 200) {
    lista.value = (result?.data?.data?.lista || []).map((x) => ({
      ...x,
      cor: x.ativo === false || x.ativo === 0 ? "red" : null,
      selecionado: false,
    }));

    totalPaginas.value = result?.data?.data?.paginas || 1;
    totalItems.value = result?.data?.data?.totalItens || 0;
  } else {
    mensagemRetorno.value = result?.data?.message || "Falha ao listar estações.";
    sucesso.value = false;
    retorno.value = true;
  }
}

async function listarTiposEstacao() {
  const result = await estacaoService.listarTiposEstacao();

  if (result?.statusCode === 200) {
    listaTipoEstacao.value = result?.data?.data || [];
  } else {
    listaTipoEstacao.value = [];
  }
}

const fields = ref([
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
    descricao: "Tipo Estação",
    valor: "tipoEstacao",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Endereço",
    valor: "endereco",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Observações",
    valor: "observacoes",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    desabilitarOrdenacao: true,
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

const equipmentFields = ref([
  {
    descricao: "Equip. Principal",
    valor: "equipamentoPrincipal",
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
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
    desabilitarOrdenacao: true,
  },
  {
    descricao: "Tag",
    valor: "tag",
    selecionado: null,
    editavel: false,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
    desabilitarOrdenacao: true,
  },
]);

watch(modalDadosMestre, (abriu) => {
  if (abriu) {
    tabEstacao.value = "dados";
    atualizarMapaDaEstacao();
  }
});

function onOcultarRetorno() {
  retorno.value = false;
}

listarItens();
listarTiposEstacao();
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

      <div justify="center">
        <v-dialog v-model="modal" class="form-dialog">
          <v-card>
            <v-card-text class="pa-4">
              <div class="d-flex align-center pb-2">
                <font-awesome-icon icon="search" color="primary" class="me-1" />
                <span class="title black--text">Filtrar</span>
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
                        :items="listaTipoEstacao"
                        v-model="filtro.tipoEstacaoId"
                        item-title="nome"
                        item-value="id"
                        label="Tipo Estação"
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      />
                    </v-col>

                    <v-col cols="12" class="pa-0 pb-4">
                      <v-text-field
                        v-model="filtro.endereco"
                        label="Endereço"
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

      <div justify="center">
        <v-dialog v-model="modalDadosMestre" max-width="960" class="form-dialog">
          <v-card color="white">
            <div @click="equipamentosHandleFocusOut($event)">
              <v-card-text class="pa-4">
                <div class="d-flex align-center pb-2">
                  <font-awesome-icon
                    :icon="inserindo ? 'plus' : 'pencil'"
                    class="text-primary mr-1"
                  />
                  <span class="title black--text">
                    {{ inserindo ? "Inserir Estação" : "Editar Estação" }}
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
                  v-model="tabEstacao"
                  density="compact"
                >
                  <v-tab value="dados" text="Dados"></v-tab>
                  <v-tab value="equipamentos" text="Equipamentos"></v-tab>
                  <v-tab value="arquivos" text="Arquivos"></v-tab>
                </v-tabs>

                <v-tabs-window v-model="tabEstacao">
                  <v-tabs-window-item value="dados">
                    <v-form ref="formulario">
                      <v-container class="pa-3 pt-6 container-modal">
                        <v-row>
                          <v-col cols="12" md="6" class="pa-1 mt-2">
                            <v-text-field
                              v-model="item.nome"
                              label="Nome"
                              clearable
                              density="compact"
                              variant="outlined"
                              color="grey-darken-1"
                              base-color="grey-darken-1"
                              :rules="[(v) => !!v || 'Nome é um campo obrigatório']"
                            />
                          </v-col>

                          <v-col cols="12" md="6" class="pa-1 mt-2">
                            <v-select
                              :items="listaTipoEstacao"
                              v-model="item.tipoEstacaoId"
                              item-title="nome"
                              item-value="id"
                              label="Tipo estação"
                              clearable
                              density="compact"
                              variant="outlined"
                              color="grey-darken-1"
                              :rules="[(v) => !!v || 'Tipo estação é obrigatório']"
                            />
                          </v-col>
                        </v-row>

                        <v-row>
                          <v-col cols="12" md="8" class="pa-1">
                            <v-text-field
                              v-model="item.endereco"
                              label="Endereço"
                              clearable
                              density="compact"
                              variant="outlined"
                              color="grey-darken-1"
                              base-color="grey-darken-1"
                              :rules="[(v) => !!v || 'Endereço é obrigatório']"
                            />
                          </v-col>

                          <v-col cols="12" md="4" class="pa-1">
                            <v-text-field
                              v-model="item.numero"
                              label="Número"
                              clearable
                              density="compact"
                              variant="outlined"
                              color="grey-darken-1"
                              base-color="grey-darken-1"
                            />
                          </v-col>
                        </v-row>

                        <v-row>
                          <v-col cols="12" md="6" class="pa-1">
                            <v-text-field
                              v-model="item.bairro"
                              label="Bairro"
                              clearable
                              density="compact"
                              variant="outlined"
                              color="grey-darken-1"
                              base-color="grey-darken-1"
                            />
                          </v-col>

                          <v-col cols="12" md="6" class="pa-1">
                            <v-text-field
                              v-model="item.complemento"
                              label="Complemento"
                              clearable
                              density="compact"
                              variant="outlined"
                              color="grey-darken-1"
                              base-color="grey-darken-1"
                            />
                          </v-col>
                        </v-row>

                        <v-row>
                          <v-col cols="12" class="pa-1">
                            <v-text-field
                              v-model="item.pontoReferencia"
                              label="Ponto de referência"
                              clearable
                              density="compact"
                              variant="outlined"
                              color="grey-darken-1"
                              base-color="grey-darken-1"
                            />
                          </v-col>
                        </v-row>

                        <v-row>
                          <v-col cols="12" class="pa-1">
                            <v-text-field
                              v-model="item.observacoes"
                              label="Observações"
                              clearable
                              density="compact"
                              variant="outlined"
                              color="grey-darken-1"
                              base-color="grey-darken-1"
                            />
                          </v-col>
                        </v-row>

                        <v-row>
                          <v-col cols="12" class="pa-1 d-flex justify-end">
                            <BaseButton
                              label="Atualizar mapa"
                              type="next"
                              iconPosition="left"
                              @click="localizarEnderecoDaEstacao"
                            />
                          </v-col>
                        </v-row>

                        <v-row>
                          <v-col cols="12" class="pa-1">
                            <v-card variant="outlined">
                              <v-card-title>
                                <font-awesome-icon icon="map-marker-alt" class="me-2" />
                                Localização da estação
                              </v-card-title>

                              <v-card-text class="pa-0">
                                <div class="map-wrapper">
                                  <GoogleMap
                                    :api-key="apiKey"
                                    style="width: 100%; height: 100%;"
                                    :center="stationMapCenter"
                                    :zoom="stationMapZoom"
                                    mapId="MAPA_ESTACAO"
                                    @click="onStationMapClick"
                                  >
                                    <AdvancedMarker
                                      v-if="stationMapMarker"
                                      :options="{ position: stationMapMarker }"
                                    />
                                  </GoogleMap>
                                </div>
                              </v-card-text>
                            </v-card>
                          </v-col>
                        </v-row>
                      </v-container>
                    </v-form>
                  </v-tabs-window-item>

                  <v-tabs-window-item value="equipamentos">
                    <div class="app-container">
                      <Grid
                        class="grid-component"
                        ref="gridEquipamentosRef"
                        gridTableId="grid-equipamentos"
                        :fields="equipmentFields"
                        :list="item?.equipamentos || []"
                        :fitParent="true"
                        :filters="{}"
                        :hideFilters="true"
                        :filterType="canRead ? 'popup' : null"
                        gridOverflow="horizontal"
                        @selecionarItem="onSelecionarComponente"
                        :customButtonsList="[]"
                      />
                    </div>
                  </v-tabs-window-item>

                  <v-tabs-window-item value="arquivos">
                    <div class="section-block">
                      <div class="section-header">
                        <span>Arquivos</span>
                      </div>

                      <v-card variant="outlined" class="pa-3">
                        <DocumentoTab
                          v-model:loading="loading"
                          :entidadeId="item.id"
                          tipoEntidade="ESTACAO"
                        />
                      </v-card>
                    </div>
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

                <v-dialog v-model="modalConfirmarVisualizacao" max-width="520">
                  <v-card>
                    <v-card-text class="pa-5">
                      <div class="d-flex align-center mb-3">
                        <v-spacer />
                        <font-awesome-icon
                          icon="xmark"
                          class="text-grey cursor-pointer"
                          @click="fecharModalVisualizacao"
                        />
                      </div>

                      <div class="text-body-1">
                        Deseja visualizar o equipamento
                        <b>{{ equipamentoSelecionado?.nome }}</b> -
                        <b>{{ equipamentoSelecionado?.tag }}</b>?
                      </div>
                    </v-card-text>

                    <v-card-actions class="pa-4 pt-0">
                      <v-spacer />
                      <BaseButton label="Não" type="cancel" iconPosition="left" @click="fecharModalVisualizacao" />
                      <BaseButton label="Sim" type="save" iconPosition="left" @click="confirmarVisualizacao" />
                    </v-card-actions>
                  </v-card>
                </v-dialog>
              </v-card-text>
            </div>
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

.map-wrapper {
  height: 320px;
  width: 100%;
}
</style>