<script setup>
import { ref, getCurrentInstance, defineProps, defineEmits, defineExpose, watch } from "vue";
import BaseButton from '@/components/base/BaseButton.vue';

const props = defineProps([
  "searchableFields",
  "fixedFields",
  "list",
  "gridOverflow",
  "filters",
  "hasCheckbox",
  "hasFixedColumns",
  "disableOrder",
  "fields",
  "gridTableId",
  "defaultMenuSearchObject",
  "filterType",
  "customHeaderLabel",
  "customButtonsList",
  "hideFilters",
  "gridResizable",
  "autofocusSearch",
  "fitParent"
]);
const emits = defineEmits([
  "listarItens",
  "selecionarItem",
  "selecionarItemMinimizado",
  "abrirFiltro",
  "botaoClick",
  "botaoOpcaoClick",
  "customButtonClick",
  "alterarOrdenacao",
  "alterarStatus"
]);
const filtersClone = ref(JSON.parse(JSON.stringify(props.filters)));
const listClone = ref(JSON.parse(JSON.stringify(props.list)));


watch(() => props.list, (newList) => {
  listClone.value = JSON.parse(JSON.stringify(newList));
});

const hasSelectedItem = ref(false);
const chevronSearch = ref({
  opened: false,
});
const search = ref(null);
const chips = ref([]);
const selectAll = ref(false);
const selectedItems = ref(0);
const windowWidth = ref(window.innerWidth);

window.addEventListener("resize", () => {
  windowWidth.value = window.innerWidth;
});

function selectThClass(field) {
  let returnClass = "";
  if (field.tipo == "botao" || field.tipo == "menu")
    returnClass = "td-button";
  else returnClass = "th-title pl-2 pr-2";

  if (field.ocultarResponsivo) returnClass += " hidden-sm-and-down";

  return returnClass;
}

function switchChanged(item, field) {
  emits("alterarStatus", {
    item,
    valor: item[field.valor], // true | false
    campo: field.valor
  });
}

function getSearchableFields() {
  if (props.searchableFields) return props.searchableFields;
  else if (props.fixedFields)
    return props.fixedFields.filter((x) => x.tipo != "checkbox");
  else
    return props.fields.filter((x) => x.tipo != "botao" && x.tipo != "menu");
}
async function selectAllItems() {
  listClone.value.forEach((x) => {
    x.selecionado = selectAll.value;
  });
  selectedItems.value = listClone.value.filter((x) => {
    return x.selecionado;
  }).length;
  hasSelectedItem.value = false;
}
function selectSingleItem(item) {
  if (selectAll.value && item.selecionado == false) selectAll.value = false;
  else {
    const notSelected = listClone.value.filter((x) => {
      return !x.selecionado;
    });
    if (notSelected.length == 0) selectAll.value = true;
  }
  selectedItems.value = listClone.value.filter((x) => {
    return x.selecionado;
  }).length;
  if (selectedItems.value == 1) {
    hasSelectedItem.value = true;
  } else {
    hasSelectedItem.value = false;
  }
}
function selectTdClass(field, item) {
  let returnClass = "";
  if (
    (!field.editavel || !item.editando) &&
    props.gridOverflow == "vertical"
  )
    returnClass = "pl-2 pr-2 pt-1 pb-1";
  if (
    (!field.editavel || !item.editando) &&
    props.gridOverflow == "horizontal"
  )
    returnClass = "pl-2 pr-2 pt-1 pb-1 no-wrap";

  if (field.ocultarResponsivo)
    returnClass += " hidden-sm-and-down";

  if (field.tipo == "botao" || field.tipo == "menu")
    returnClass += " td-button";

  return returnClass;
}
function removeChip(item) {
  filtersClone.value.pagina = 1;
  const index = chips.value.indexOf(item);
  chips.value.splice(index, 1);
  filtersClone.value[item.campo] = null;
  emits("listarItens", filtersClone.value);
  selectedItems.value = 0;
  selectAll.value = false;
  hasSelectedItem.value = false;
}
function selectRow(item) {
  if (!props.hasCheckbox) {
    listClone.value.forEach((x) => {
      if (x != item) {
        x.selecionado = false;
      }
    });
    item.selecionado = true;
    hasSelectedItem.value = true;
  }
}
function editRow(item) {
  emits("selecionarItem", item);
}
function updateFieldModel(field) {
  field.modelFiltro = field.lista
    .filter((x) => {
      return x.selecionado == true;
    })
    .map((x) => {
      return x.id;
    });
}
function menuButtonClick(field) {
  const menuButton = document.getElementById("menu-filtro-" + field.valor);
  menuButton.click();
  menuButton.blur();
}
function closeFilter(field) {
  menuButtonClick(field);
  field.modelFiltro = field.modelUltimoValor;
}
function clearFilter(field) {
  field.filtrado = false;
  field.modelFiltro = null;
  field.lista.forEach((x) => {
    x.selecionado = false;
  });
  filtersClone.value[field.valorFiltroEmLista] = field.modelFiltro;
  menuButtonClick(field);
  emits("listarItens", filtersClone.value);
}
function applyFilter(field) {
  menuButtonClick(field);
  field.modelUltimoValor = field.modelFiltro;
  filtersClone.value[field.valorFiltroEmLista] = field.modelFiltro;
  field.filtrado =
    field.tipo == "lista"
      ? field.modelFiltro != null
      : field.modelFiltro.length != 0;
  emits("listarItens", filtersClone.value);
}
function setOrder(field, index, fixed) {
  if (props.hasFixedColumns && !fixed)
    index = index + props.fixedFields.length;

  if (field.tipo == "botao" || field.tipo == "menu" || props.disableOrder || field.desabilitarOrdenacao)
    return;
  filtersClone.value.pagina = 1;
  props.fields.forEach((x) => {
    if (x != field) {
      x.ordenado = null;
    }
  });
  props.fixedFields?.forEach((x) => {
    if (x != field) {
      x.ordenado = null;
    }
  });
  if (field.ordenado == null) {
    field.ordenado = true;
    filtersClone.value.ordenarPor = index;
    filtersClone.value.ordem = 0;
  } else if (field.ordenado == true) {
    field.ordenado = false;
    filtersClone.value.ordenarPor = index;
    filtersClone.value.ordem = 1;
  } else {
    field.ordenado = null;
    filtersClone.value.ordenarPor = 0;
    filtersClone.value.ordem = 0;
  }
  selectedItems.value = 0;
  selectAll.value = false;
  hasSelectedItem.value = false;
  emits("alterarOrdenacao", filtersClone.value);
}
function handleFocusOut(event) {
  if (!props.hasCheckbox) {
    const gridId = !props.gridTableId
      ? "#grid-table"
      : "#" + props.gridTableId;
    var gridTable = document.querySelector(gridId);
    if (!gridTable?.contains(event.target)) {
      listClone.value.forEach((x) => {
        x.selecionado = false;
      });
      hasSelectedItem.value = false;
    }
  }
}

function unselectAll() {
  listClone.value.forEach((x) => {
    x.selecionado = false;
  });
  hasSelectedItem.value = false;
}

function openFilter() {
  emits("abrirFiltro");
}
function removeFocus(event) {
  event.target.blur();
}
function executeSearch(event) {
  filtersClone.value.pagina = 1;
  if (this.search) {
    const selectedItems = [];
    props.fields.forEach((x) => {
      if (x.selecionado) {
        includeNewChip(x);
        selectedItems.push(x);
      }
      x.selecionado = false;
    });
    if (props.fixedFields) {
      props.fixedFields.forEach((x) => {
        if (x.selecionado) {
          includeNewChip(x);
          selectedItems.push(x);
        }
        x.selecionado = false;
      });
    }
    if (props.searchableFields) {
      props.searchableFields.forEach((x) => {
        if (x.selecionado) {
          includeNewChip(x);
          selectedItems.push(x);
        }
        x.selecionado = false;
      });
    }
    if (selectedItems.length == 0) {
      if (props.defaultMenuSearchObject)
        includeNewChip(props.defaultMenuSearchObject);
      else includeNewChip({ descricao: "Todos", valor: "todos" });
    }

    this.search = null;
  }
  event.target.blur();
  selectedItems.value = 0;
  selectAll.value = false;
  hasSelectedItem.value = false;
}
function includeNewChip(item) {
  const existingChip = chips.value.filter((y) => {
    return y.descricao == item.descricao;
  });
  if (existingChip.length > 0) {
    const index = chips.value.indexOf(existingChip[0]);
    chips.value[index] = {
      campo: item.valor,
      descricao: item.descricao,
      valor: search.value,
    };
  } else {
    chips.value.push({
      campo: item.valor,
      descricao: item.descricao,
      valor: search.value,
    });
  }
  filtersClone.value[item.valor] = search.value;
  emits("listarItens", filtersClone.value);
}
function buttonClick(item) {
  emits("botaoClick", item);
}
function optionButtonClick(item, opcao) {
  const result = {
    item: item,
    opcao: opcao,
  };
  emits("botaoOpcaoClick", result);
}
function customButtonClick(item) {
  if (item.hasMultipleAction) {
    const selecionados = listClone.value
      .filter((x) => {
        return x.selecionado;
      })
    item.selecionados = selecionados;
  }
  emits("customButtonClick", item);
}
function resetAfterExclude() {
  selectedItems.value = 0;
  selectAll.value = false;
  hasSelectedItem.value = false;
}
function addCustomIcon(index, icone) {
  listClone.value[index].iconePersonalizado = icone;
  const instance = getCurrentInstance();
  instance?.proxy?.$forceUpdate();
}
function removeCustomIcon(index) {
  listClone.value[index].iconePersonalizado = null;
  const instance = getCurrentInstance();
  instance?.proxy?.$forceUpdate();
}

function selecionarClasseTitulo(field) {
  let classe =
    field.tipo == 'botao' ||
      field.tipo == 'menu' ||
      field.desabilitarOrdenacao ||
      props.disableOrder
      ? 'title-without-click'
      : 'title-click';

  if (field.tipo == 'numero')
    classe += ' text-right';

  return classe;
}

// NOVO: devolve as linhas atualmente selecionadas (da cópia interna)
function getSelectedRows() {
  return (listClone.value || []).filter(x => x.selecionado);
}

defineExpose({
  handleFocusOut,
  unselectAll,
  resetAfterExclude,
  addCustomIcon,
  removeCustomIcon,
  getSelectedRows
});



</script>

<template>
  <div class="grid-component" :class="{ 'fit-parent': props.fitParent }">
    <div class="mb-2 d-flex" id="filter-chips">
      <div v-for="(item, index) in chips" :key="index">
        <v-chip class="ma-1" close outlined color="primary" @click:close="removeChip(item)">
          {{ item.descricao + ": " + item.valor }}
        </v-chip>
      </div>
    </div>
    <div class="d-flex flex-column flex-md-row flex-wrap justify-space-between" id="grid-action-buttons"
      v-if="filterType == 'search'">
      <div class="d-flex flex-wrap">
        <template v-if="customButtonsList && customButtonsList.length > 0">
          <div v-for="(item, index) in customButtonsList" :key="index" class="d-inline-block">
            <!-- se item.opcoesMenu existe, renderiza menu -->
            <v-menu v-if="item.opcoesMenu" offset="10" location="end" :close-on-content-click="true">
              <template #activator="{ props: menuProps }">
                <BaseButton v-bind="menuProps" :label="item.customButtonDescription" :type="item.type || 'next'"
                  :icon="item.customButtonIcon" :color="item.color" class="text-base-button mr-3" :disabled="(item.enableIfHasSelectedItems ? selectedItems === 0 : false) ||
                    (item.enableIfHasSingleItem ? selectedItems !== 1 : false) ||
                    (item.hasSelectedItem ? !hasSelectedItem : false)" />
              </template>
              <v-list>
                <v-list-item v-for="(opcao, i) in item.opcoesMenu" :key="i"
                  @click="customButtonClick({ ...item, opcao })">
                  <v-list-item-title class="d-flex align-center">
                    <!-- Se vier imagem, usa imagem; senão mantém FontAwesome -->
                    <img v-if="opcao.imagem" :src="opcao.imagem" class="menu-img" />
                    <font-awesome-icon v-else :icon="opcao.icone" class="me-2" />
                    {{ opcao.descricao }}
                  </v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
            <!-- caso contrário, botão simples como antes -->
            <BaseButton v-else :label="item.customButtonDescription" :type="item.type || 'next'"
              :icon="item.customButtonIcon" :color="item.color" class="text-base-button mr-3"
              @click="customButtonClick(item)" :disabled="(item.enableIfHasSelectedItems ? selectedItems === 0 : false) ||
                (item.enableIfHasSingleItem ? selectedItems !== 1 : false) ||
                (item.hasSelectedItem ? !hasSelectedItem : false)" />
          </div>
        </template>
        <span v-if="selectedItems >= 1" class="pt-1">
          {{
            selectedItems == 1
              ? selectedItems + " item selecionado"
              : selectedItems + " itens selecionados"
          }}
        </span>
      </div>
      <div class="pr-2 pr-sm-0 mb-2 search-bar-container" id="search-bar-container" v-if="!hideFilters">
        <div class="search-bar">
          <v-menu offset="10" location="bottom" :close-on-content-click="false" v-model="chevronSearch.opened">
            <template v-slot:activator="{ props }">
              <font-awesome-icon v-bind="props" :icon="'angle-down'" :class="[
                'mt-1 mx-1',
                chevronSearch.opened ? 'rotate-180 text-primary' : 'text-grey'
              ]" />
            </template>
            <v-card class="max-height-search-box">
              <v-card-text>
                <div v-for="(item, index) in getSearchableFields()" :key="index">
                  <v-checkbox class="mt-2 mb-2" v-model="item.selecionado" :label="item.descricao"
                    :value="item.selecionado" hide-details density="compact"></v-checkbox>
                </div>
              </v-card-text>
            </v-card>
          </v-menu>
          <v-text-field label="Procurar" hide-details append-icon="search" clearable
            v-on:keyup.esc="removeFocus($event)" v-on:keyup.enter="executeSearch($event)"
            @click:append="executeSearch($event)" v-model="search" density="compact" :autofocus="autofocusSearch"
            variant="underlined"></v-text-field>
        </div>
      </div>
    </div>
    <div class="d-flex justify-space-between"
      v-if="filterType == 'popup' || (customButtonsList && customButtonsList.length > 0)">
      <div class="d-flex align-center">
        <span v-if="customHeaderLabel">
          {{ customHeaderLabel }}
        </span>
        <span class="ml-4" v-if="customHeaderLabel && secondCustomHeaderLabel">
          {{ secondCustomHeaderLabel }}
        </span>
        <template v-if="customButtonsList && customButtonsList.length > 0">
          <div v-for="(item, index) in customButtonsList" :key="index" class="d-inline-block">
            <!-- se item.opcoesMenu existe, renderiza menu -->
            <v-menu v-if="item.opcoesMenu" offset="10" location="end" :close-on-content-click="true">
              <template #activator="{ props: menuProps }">
                <BaseButton v-bind="menuProps" :label="item.customButtonDescription" :type="item.type || 'next'"
                  :icon="item.customButtonIcon" :color="item.color" class="text-base-button mr-3" :disabled="(item.enableIfHasSelectedItems ? selectedItems === 0 : false) ||
                    (item.enableIfHasSingleItem ? selectedItems !== 1 : false) ||
                    (item.hasSelectedItem ? !hasSelectedItem : false)" />
              </template>
              <v-list>
                <v-list-item v-for="(opcao, i) in item.opcoesMenu" :key="i"
                  @click="customButtonClick({ ...item, opcao })">
                  <v-list-item-title class="d-flex align-center">
                    <!-- Se vier imagem, usa imagem; senão mantém FontAwesome -->
                    <img v-if="opcao.imagem" :src="opcao.imagem" class="menu-img" />
                    <font-awesome-icon v-else :icon="opcao.icone" class="me-2" />
                    {{ opcao.descricao }}
                  </v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
            <!-- caso contrário, botão simples como antes -->
            <BaseButton v-else :label="item.customButtonDescription" :type="item.type || 'next'"
              :icon="item.customButtonIcon" :color="item.color" class="text-base-button mr-3"
              @click="customButtonClick(item)" :disabled="(item.enableIfHasSelectedItems ? selectedItems === 0 : false) ||
                (item.enableIfHasSingleItem ? selectedItems !== 1 : false) ||
                (item.hasSelectedItem ? !hasSelectedItem : false)" />
          </div>
        </template>
      </div>
      <div v-if="filterType == 'popup' && !hideFilters">
        <BaseButton label="Filtrar" type="search" iconPosition="left" @click="openFilter" />
      </div>
    </div>
    <v-divider class="mt-3" color="white"></v-divider>
    <div :class="gridOverflow == 'vertical'
      ? 'grid-container'
      : 'grid-container-horizontal'
      " id="grid-container">
      <table :class="!hasFixedColumns ? 'grid' : 'grid grid-with-fixed-column'"
        v-columns-resizable="gridResizable ? true : false">
        <thead>
          <tr>
            <th v-if="fixedFields">
              <div class="first-column-container">
                <div :class="field.tipo == 'checkbox'
                  ? 'pa-0 d-flex justify-center'
                  : 'pl-2 pr-2 d-flex justify-space-between'
                  " v-for="(field, innerIndex) in fixedFields" :key="innerIndex"
                  :style="'width: ' + field.largura + 'px'">
                  <span v-if="field.tipo != 'checkbox'" class="span-fixed-field-title"
                    @click="setOrder(field, innerIndex, true)">
                    {{ field.descricao }}
                  </span>
                  <font-awesome-icon v-if="field.tipo !== 'checkbox' && field.ordenado !== null"
                    :icon="field.ordenado ? 'arrow-up' : 'arrow-down'" class="text-grey-lighten-2 mx-1" />

                  <v-checkbox v-if="field.tipo == 'checkbox'" class="pa-0 mt-0" v-model="selectAll" :label="null"
                    hide-details @change="selectAllItems()" density="compact"></v-checkbox>
                </div>
              </div>
            </th>
            <th scope="col" :class="selectThClass(field)" v-for="(field, innerIndex) in fields" :key="innerIndex">
              <div class="title-container">
                <span v-if="field.tipo != 'checkbox'" :class="selecionarClasseTitulo(field)"
                  @click="setOrder(field, innerIndex, false)">{{ field.descricao }}</span>
                <v-checkbox v-if="field.tipo == 'checkbox'" class="pa-0 mt-0" v-model="selectAll" :label="null"
                  hide-details @change="selectAllItems()" density="compact"></v-checkbox>
                <div>
                  <font-awesome-icon v-if="field.ordenado !== null" :icon="field.ordenado ? 'arrow-up' : 'arrow-down'"
                    class="text-grey-lighten-2 mx-1" />
                  <v-menu offset="10" location="bottom" v-if="field.filtravel" :close-on-content-click="false">
                    <template v-slot:activator="{ props }">
                      <font-awesome-icon v-bind="props" :id="'menu-filtro-' + field.valor"
                        :icon="field.filtrado ? 'sliders' : 'filter'"
                        :class="field.filtrado ? 'text-primary mx-1' : 'text-grey-lighten-2 mx1'" />
                    </template>
                    <v-card class="filter-card">
                      <v-card-text class="pb-0">
                        <div class="filter-options">
                          <div v-if="field.tipo != 'lista'">
                            <div v-for="(itemLista, indiceItemLista) in field.lista" :key="indiceItemLista">
                              <v-checkbox class="mt-2 mb-2" v-model="itemLista.selecionado" :label="itemLista.descricao"
                                :value="itemLista.itemValue" hide-details
                                @change="updateFieldModel(field)"></v-checkbox>
                            </div>
                          </div>
                          <div v-if="field.tipo == 'lista'">
                            <v-radio-group v-model="field.modelFiltro" class="pa-0">
                              <v-radio v-for="itemLista in field.lista" :key="itemLista" :label="itemLista.text"
                                :value="itemLista.value"></v-radio>
                            </v-radio-group>
                          </div>
                        </div>
                        <div>
                          <BaseButton :disabled="!field.filtrado" class="btn-clear-filter" type="clear"
                            :label="`Limpar filtro de ${field.descricao}`" @click="clearFilter(field)" />
                        </div>
                        <hr class="mt-6" />
                      </v-card-text>
                      <v-card-actions>
                        <v-spacer></v-spacer>
                        <BaseButton class="" type="cancel" label="Cancelar" @click="closeFilter(field)" />
                        <BaseButton class="" type="save" label="Aplicar" @click="applyFilter(field)" />
                      </v-card-actions>
                    </v-card>
                  </v-menu>
                </div>
              </div>
            </th>
          </tr>
        </thead>
        <tbody @focusout="handleFocusOut" :id="!gridTableId ? 'grid-table' : gridTableId">
          <tr v-for="(item, index) in listClone" :key="index" :class="item.selecionado ? 'selected-item' : ''">
            <td v-if="fixedFields" @click="selectRow(item)" @dblclick="editRow(item, index, innerIndex)">
              <div class="first-column-container">
                <div class="pl-2 pr-2" v-for="(field, innerIndex) in fixedFields" :key="innerIndex"
                  :style="'width: ' + field.largura + 'px'">
                  <span v-if="field.tipo != 'checkbox'" class="span-fixed-field">
                    {{ item[field.valor] }}
                  </span>
                  <v-checkbox v-if="field.tipo == 'checkbox'" class="pa-0 mt-0" v-model="item.selecionado" :label="null"
                    hide-details density="compact" @change="selectSingleItem(item)"></v-checkbox>
                </div>
              </div>
            </td>
            <td @click="selectRow(item)" @dblclick="editRow(item, index, innerIndex)"
              v-for="(field, innerIndex) in fields" :key="innerIndex" :class="selectTdClass(field, item)">
              <div class="text-center" v-if="field.tipo == 'icone' && item[field.valor]">
                <font-awesome-icon :icon="item[field.valor]?.mdiIcon"
                  :style="{ color: item[field.valor]?.color || undefined }" />
              </div>
              <font-awesome-icon class="td-custom-icon mx-1" v-if="field.tipo == 'menu' && item.iconePersonalizado"
                :icon="item.iconePersonalizado" />
              <v-checkbox v-if="field.tipo == 'checkbox'" class="pa-0 mt-0" v-model="item.selecionado" :label="null"
                hide-details density="compact" @change="selectSingleItem(item)"></v-checkbox>

              <!-- Novo: campo do tipo switch para status (ex.: ativo/inativo) -->
              <v-switch v-if="field.tipo == 'switch'" class="switch-thin" v-model="item[field.valor]" density="compact"
                hide-details color="success" inset :ripple="false" @update:modelValue="() => switchChanged(item, field)"
                @click.stop />

              <BaseButton v-if="field.tipo === 'botao'" :icon="field.valor" :label="''" type="next"
                class="text-primary remove-border transparencia" @click="buttonClick(item)" />

              <v-menu offset="10" location="end" :close-on-content-click="true" v-model="item.exibirMenu"
                v-if="field.tipo == 'menu'" class="text-center">
                <template v-slot:activator="{ props }">
                  <font-awesome-icon v-bind="props" :icon="field.valor" class="text-primary mx-1" />
                </template>

                <v-list>
                  <div v-if="!hasCustomOptionsItem">
                    <v-list-item v-for="(opcao, index) in field.opcoesMenu" :key="index"
                      @click="optionButtonClick(item, opcao)">
                      <v-list-item-title :class="opcao.classe">
                        <font-awesome-icon :icon="opcao.icone" :class="['mx-1', opcao.classe]" />
                        {{ opcao.descricao }}</v-list-item-title>
                    </v-list-item>
                  </div>
                  <div v-if="hasCustomOptionsItem">
                    <v-list-item v-for="(opcao, index) in item.opcoesMenu" :key="index"
                      @click="optionButtonClick(item, opcao)">
                      <v-list-item-title :class="opcao.classe">
                        <font-awesome-icon :icon="opcao.icone" :class="['mx-1', opcao.classe]" />
                        {{ opcao.descricao }}</v-list-item-title>
                    </v-list-item>
                  </div>
                </v-list>
              </v-menu>

              <div :class="(!field.editavel || !item.editando) && field.tipo != 'icone'
                ? 'grid-text-div'
                : 'hide'
                " :style="item.cor ? 'color: ' + item.cor + ' !important;' : ''" v-if="field.tipo == 'texto'">
                {{ item[field.valor] }}
              </div>
              <div :class="(!field.editavel || !item.editando) && field.tipo != 'icone'
                ? 'grid-number-div'
                : 'hide'
                " :style="item.cor ? 'color: ' + item.cor + ' !important;' : ''" v-if="field.tipo == 'numero'">
                {{ item[field.valor] }}
              </div>
              <div class="d-flex" v-if="field.tipo == 'imagem'">
                <img v-if="item[field.valor]" :src="item[field.valor]" alt="thumb" class="thumb-image" />
                <font-awesome-icon v-if="!item[field.valor]" icon="user-circle mx-1" :style="{ fontSize: '50px' }" />
              </div>
              <div class="d-flex" v-if="field.tipo == 'banner'">
                <img v-if="item[field.valor]" :src="item[field.valor]" alt="banner" class="banner-image" />
                <font-awesome-icon v-if="!item[field.valor]" icon="image" :style="{ fontSize: '50px' }" />
              </div>
              <input type="text" v-model="item[field.valor]"
                :class="field.editavel && item.editando ? 'edit-field' : 'hide'"
                :id="'item-field-' + index + '-' + innerIndex" />
            </td>
          </tr>
          <tr v-if="listClone.length == 0">
            <td :colspan="fields.length + 1" class="text-center red--text" style="position: relative">
              <div class="no-content-container">Nenhum registro encontrado</div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
<style scoped>
.grid-component {
  display: flex;
  flex-flow: column;
  z-index: 1;
  position: relative;
}

#filter-chips {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  flex-wrap: wrap;
}

.grid-container {
  overflow-y: auto;
  overflow-x: hidden;
  z-index: 1;
  position: relative;
}

.grid-container-horizontal {
  overflow-y: auto;
  overflow-x: auto;
  z-index: 1;
}

.no-wrap {
  white-space: nowrap;
}

.grid {
  width: 100%;
  text-align: left;
  color: #3d3d3d;
  border-spacing: 0;
  border-collapse: collapse;
}

.grid thead {
  position: sticky;
  top: 0;
  z-index: 1;
  background: #fff;
}

.grid th {
  border-bottom: 1.5px solid rgba(0, 0, 0, 0.12);
  font-size: 1.1rem;
  padding-top: 8px;
  padding-bottom: 8px;
  font-weight: normal;
  color: black;
}

.grid tr {
  border-bottom: 0.5px solid rgba(0, 0, 0, 0.12);
}

.grid tbody tr:hover {
  background-color: #eee;
}

.grid tbody tr:hover .first-column-container {
  background-color: #eee;
}

.grid tbody tr td {
  height: 35px;
  color: black;
  font-size: 0.9rem;
}

.title-container {
  display: flex;
  justify-content: space-between;
}

.title-container span {
  white-space: nowrap;
}

.title-container div {
  width: max-content;
  text-align: right;
}

.search-bar-container {
  display: flex;
  justify-content: flex-end;
}

.rotate-180 {
  transform: rotate(-180deg);
}

.edit-field {
  height: 26px;
  width: calc(100% - 8px);
  margin: 0;
  color: #3d3d3d;
  padding: 0 4px;
  margin: 4px;
}

.edit-field:focus {
  outline: 1px solid rgba(0, 0, 0, 0.3);
  border-radius: 2px;
}

.edit-field:focus-visible {
  outline: 1px solid rgba(0, 0, 0, 0.3);
  border-radius: 2px;
}

.search-bar {
  display: flex;
  align-items: center;
  width: 300px;
  max-width: 300px;
}

.selected-item,
.selected-item .first-column-container {
  background-color: #dedede;
}

.selected-item:hover,
.selected-item:hover .first-column-container {
  background-color: #dedede !important;
}

.hide {
  display: none;
}

.title-click {
  cursor: pointer;
  user-select: none;
  -moz-user-select: none;
  -webkit-user-select: none;
}

.title-without-click {
  user-select: none;
  -moz-user-select: none;
  -webkit-user-select: none;
}

.filter-card {
  max-width: fit-content;
}

.btn-clear-filter {
  width: 100%;
}

.grid-text-div {
  user-select: none;
  -moz-user-select: none;
  -webkit-user-select: none;
}

.grid-number-div {
  text-align: right;
  user-select: none;
  -moz-user-select: none;
  -webkit-user-select: none;
}

.excluir-sub-item {
  cursor: pointer;
  margin-top: -12px;
}

.filter-options {
  max-height: 200px;
  overflow-y: auto;
  border: 1px solid #e4e4e4;
  padding: 0px 12px;
  border-radius: 5px;
}

.td-button {
  width: 60px !important;
  text-align: center;
}

.td-button .title-container {
  justify-content: center;
}

.th-title {}

.minimized-items-menu {
  display: flex;
  align-items: center;
  cursor: pointer;
  color: #225fac;
  z-index: 1;
  border-radius: 3px;
}

.minimized-items-menu:hover {
  background-color: #225fac14;
}

.minimized-items-button-container {
  position: relative;
  height: 40px;
  vertical-align: middle;
  line-height: 40px;
}

.max-height-search-box {
  max-height: 400px;
  overflow: auto;
}

.no-content-container {
  /* position: fixed;
  margin-top: -10px;
  left: calc(50% - 115px);
  width: 230px;
  text-align: center;
  line-height: 20px; */
  margin-left: 14px;
  text-align: left;
}

.thumb-image {
  width: 50px;
  height: 50px;
  border-radius: 100%;
  object-fit: cover;
  user-select: none;
  -moz-user-select: none;
  -webkit-user-select: none;
}

.banner-image {
  width: 85px;
  height: 50px;
  user-select: none;
  -moz-user-select: none;
  -webkit-user-select: none;
}

@media (max-width: 600px) {
  .search-bar {
    align-items: center;
    display: flex;
    width: 100%;
    max-width: 100%;
  }

  .grid-header-button-label {
    display: none;
  }
}

/* ------------------------------------- tabela com duplo scroll-------------------------------------  */
.grid-with-fixed-column {
  border-collapse: separate;
}

.grid-with-fixed-column tfoot,
.grid-with-fixed-column tfoot th,
.grid-with-fixed-column tfoot td {
  position: -webkit-sticky;
  position: sticky;
  bottom: 0;
  z-index: 4;
}

.grid-with-fixed-column td:first-child,
.grid-with-fixed-column th:first-child {
  position: -webkit-sticky;
  position: sticky;
  left: 0;
  z-index: 0;
  /* background-color: white; */
  border-right: 1.5px solid rgba(0, 0, 0, 0.12);
}

.grid-with-fixed-column thead th:first-child,
.grid-with-fixed-column tfoot th:first-child {
  z-index: 5;
  /* background-color: white; */
}

.grid-with-fixed-column th {
  font-size: 0.9rem !important;
}

.grid-with-fixed-column tbody tr td {
  font-size: 0.8rem !important;
}

.first-column-container {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  background-color: white;
}

.first-column-container div {
  white-space: normal;
}

.span-fixed-field-title {
  cursor: pointer;
  user-select: none;
  -moz-user-select: none;
  -webkit-user-select: none;
}

.span-fixed-field {
  cursor: default;
  user-select: none;
  -moz-user-select: none;
  -webkit-user-select: none;
}

.td-custom-icon {
  font-size: 1.2rem;
  color: #aaa;
  margin-left: -18px;
}

.remove-border {
  box-shadow: unset !important;
}

.transparencia {
  background: transparent !important;
}

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

.switch-thin :deep(.v-switch__thumb) {
  transform: translateX(0) !important;
}

.switch-thin :deep(.v-selection-control) {
  margin: 0;
}

.v-btn {
  text-transform: none !important;
}

.menu-img {
  width: 18px;
  height: 18px;
  object-fit: contain;
  margin-right: 8px;
}

.grid-component.fit-parent {
  height: 100%;
  min-height: 0;
  display: flex;
  flex-direction: column;
}

.grid-component.fit-parent .grid-container,
.grid-component.fit-parent .grid-container-horizontal {
  flex: 1 1 auto;
  min-height: 0;
  overflow-y: auto;
}
</style>