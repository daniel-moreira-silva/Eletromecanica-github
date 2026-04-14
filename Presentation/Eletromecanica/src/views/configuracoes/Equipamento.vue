<script setup>
import { ref, inject, computed } from "vue";
import { useRouter } from "vue-router";

import Grid from "@/components/common/GridComponent.vue";
import Paginacao from "@/components/common/PaginacaoComponent.vue";
import Loading from "@/components/base/LoadingOverlay.vue";
import BaseButton from "@/components/base/BaseButton.vue";
import Snackbar from "@/components/base/Snackbar.vue";

import EquipamentoService from "@/services/configuracoes/equipamento-service.js";
import EstacaoService from "@/services/configuracoes/estacao-service.js";

import { usePermissoesTela } from "@/composables/usePermissoesTela";
import { usePadraoPermissao } from "@/composables/usePadraoPermissao";

const endpoint = inject("endpoint");
const headerPadrao = inject("headerPadrao");
const chaveSeguranca = inject("chaveSeguranca");
const usuarioSeguranca = inject("usuarioSeguranca");

const equipamentoService = new EquipamentoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);

const estacaoService = new EstacaoService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
);

const router = useRouter();

const lista = ref([]);
const listaTipoEquipamento = ref([]);
const listaEstacoes = ref([]);

const totalPaginas = ref(1);
const totalItems = ref(0);

const modal = ref(false);
const gridComponent = ref(null);

const loading = ref(false);

const retorno = ref(false);
const mensagemRetorno = ref(null);
const sucesso = ref(true);

const { hasPermission } = usePermissoesTela();
const canRead = computed(() => hasPermission("Ler"));

function handleFocusOut(event) {
  gridComponent.value?.handleFocusOut?.(event);
}

const rawButtons = [
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
];

const customButtonsList = usePadraoPermissao(rawButtons);

async function customButtonClick(button) {
  if (button.function == "inserir") {
    await router.push({ name: "EquipamentoDetalhe", params: { id: "novo" } });
  } else if (button.function == "editar") {
    const id = button.selecionados[0]?.id;
    await router.push({ name: "EquipamentoDetalhe", params: { id } });
  }
}

async function selecionarItem(selectedItem) {
  const id = selectedItem?.id;
  await router.push({ name: "EquipamentoDetalhe", params: { id } });
}

async function alterarStatus({ item: itemGrid, valor }) {
  try {
    const corAnterior = itemGrid.cor;
    itemGrid.cor = valor ? null : "red";
    
    loading.value = true;
    const id = itemGrid?.id;
    const resp = await equipamentoService.atualizarStatus(id, valor);
    loading.value = false;
    retorno.value = true;
    if (resp?.statusCode === 200) {
      mensagemRetorno.value = resp?.data?.message || "Status atualizado com sucesso.";
      sucesso.value = true;
    } else {
      itemGrid.cor = corAnterior;
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
  tag: null,
  fabricante: null,
  modelo: null,
  numeroSerie: null,
  tipoEquipamento: null,
  estacao: null,
  ativo: null
});

function filtrar() {
  filtro.value.pagina = 1;
  listarItens();
}

function limparFiltro() {
  Object.assign(filtro.value, {
    pagina: 1,
    itensPagina: filtro.value.itensPagina,
    tag: null,
    fabricante: null,
    modelo: null,
    numeroSerie: null,
    tipoEquipamento: null,
    estacao: null,
    ativo: null,
  });
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

function onOcultarRetorno() {
  retorno.value = false;
}

async function listarItens() {
  modal.value = false;
  loading.value = true;

  let result = await equipamentoService.listar(filtro.value);

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

async function listarTiposEquipamento() {
  let result = await equipamentoService.listarTiposEquipamento();
  listaTipoEquipamento.value = result?.data.data;
}

listarTiposEquipamento();

async function listarEstacoes() {
  let result = await estacaoService.buscarEstacoes();
  listaEstacoes.value = result?.data.data;
}

listarEstacoes();

const fields = [
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
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Tag",
    valor: "tag",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Estação",
    valor: "estacao",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Fabricante",
    valor: "fabricante",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Modelo",
    valor: "modelo",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: false,
  },
  {
    descricao: "Número de série",
    valor: "numeroSerie",
    selecionado: null,
    editavel: true,
    filtravel: false,
    tipo: "texto",
    ordenado: null,
    ocultarResponsivo: true,
  },
  {
    descricao: "Tipo Equipamento",
    valor: "tipoEquipamento",
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
];
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
                        v-model="filtro.tag"
                        label="Tag"
                        clearable
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      ></v-text-field>
                    </v-col>

                    <v-col cols="12" class="pa-0 pb-4">
                      <v-select
                        :items="listaEstacoes"
                        v-model="filtro.estacaoId"
                        item-title="nome"
                        item-value="id"
                        label="Estação"
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      ></v-select>
                    </v-col>

                    <v-col cols="12" class="pa-0 pb-4">
                      <v-text-field
                        v-model="filtro.fabricante"
                        label="Fabricante"
                        clearable
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      ></v-text-field>
                    </v-col>

                    <v-col cols="12" class="pa-0 pb-4">
                      <v-text-field
                        v-model="filtro.modelo"
                        label="Modelo"
                        clearable
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      ></v-text-field>
                    </v-col>

                    <v-col cols="12" class="pa-0 pb-4">
                      <v-text-field
                        v-model="filtro.numeroSerie"
                        label="Número de série"
                        clearable
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      ></v-text-field>
                    </v-col>

                    <v-col cols="12" class="pa-0 pb-4">
                      <v-select
                        :items="listaTipoEquipamento"
                        v-model="filtro.tipoEquipamentoId"
                        item-title="nome"
                        item-value="id"
                        label="Tipo Equipamento"
                        density="compact"
                        hide-details
                        variant="outlined"
                        color="grey-darken-1"
                        base-color="grey-darken-1"
                      ></v-select>
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
                      ></v-select>
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

<style scoped></style>