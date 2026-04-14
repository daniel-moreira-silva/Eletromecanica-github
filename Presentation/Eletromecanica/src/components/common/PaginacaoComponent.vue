<script setup>
import { ref, computed, defineProps, defineEmits } from "vue";

const props = defineProps(["totalPaginas", "paginaAtual", "totalItens"]);
const paginaAtualInterna = ref(JSON.parse(JSON.stringify(props.paginaAtual)));
const itensPorPagina = ref(10);
const refPaginaAtual = ref(null);


const emits = defineEmits(["alterarPagina", "alterarItensPorPagina"]);

function primeiraPagina() {
  paginaAtualInterna.value = 1;
  emits("alterarPagina", paginaAtualInterna.value);
}
function proximaPagina() {
  paginaAtualInterna.value =
    paginaAtualInterna.value < props.totalPaginas
      ? paginaAtualInterna.value + 1
      : props.totalPaginas;
  emits("alterarPagina", paginaAtualInterna.value);
}
function paginaAnterior() {
  paginaAtualInterna.value =
    paginaAtualInterna.value > 1 ? paginaAtualInterna.value - 1 : paginaAtualInterna.value;
  emits("alterarPagina", paginaAtualInterna.value);
}
function ultimaPagina() {
  paginaAtualInterna.value = props.totalPaginas;
  emits("alterarPagina", paginaAtualInterna.value);
}
function alterarPagina() {
  emits("alterarPagina", paginaAtualInterna.value);
  refPaginaAtual.value?.blur();
}
function alterarItensPorPagina() {
  emits("alterarItensPorPagina", itensPorPagina.value);
}

const pAtuaisList = computed(function () {
  let list = [];
  for (let i = 1; i <= props.totalPaginas; i++) {
    list.push(i);
  }
  return list;
})

</script>


<template>
  <div>
    <hr class="mb-2">
    <div class="container-paginacao">
      <div class="d-none d-sm-flex">
        <div class="d-flex align-center">
          Itens por página
          <v-select density="compact" flat variant="solo" v-model="itensPorPagina" :items="[10, 20, 50, 100]"
            :menu-props="{ maxHeight: '400', offsetY: true, top: false }" @update:modelValue="alterarItensPorPagina"
            hide-details></v-select>
        </div>
      </div>
      <div class="d-flex align-center justify-flex-end">
        <div class="ml-4 mr-4 d-flex align-center">
          <div>
            {{ totalItens == 0 ? 0 : !paginaAtualInterna ? 0 : paginaAtualInterna * itensPorPagina - itensPorPagina + 1
            }} -
            {{
              paginaAtualInterna * itensPorPagina > totalItens
                ? totalItens
                : paginaAtualInterna * itensPorPagina
            }}
            <span class="hidden-sm-and-down"> de {{ totalItens }} itens |</span>
          </div>
          <div style="width: 115px;" class="hidden-sm-and-down">
            <v-select class="pa-0 ma-0" v-model="paginaAtualInterna" :items="pAtuaisList"
              :menu-props="{ maxWidth: '100', maxHeight: '400', offsetOverflow: false, offsetY: true, top: true }"
              density="compact" variant="solo" flat hide-details @update:modelValue="alterarPagina" ref="refPaginaAtual">
            </v-select>
          </div>
          <div class="pagina-atual-descricao ml-1 ml-md-0 hidden-sm-and-down">
            de {{ totalPaginas == 1 ? totalPaginas + " página" : totalPaginas + " páginas" }}
          </div>
        </div>
        <v-tooltip text="Primeira página" location="bottom">
          <template v-slot:activator="{ props }">
            <v-btn icon variant="text" rounded="xl" color="primary" v-bind="props" @click="primeiraPagina">
              <font-awesome-icon icon="fa-angles-left" class="text-neutral" />
            </v-btn>
          </template>

        </v-tooltip>
        <v-tooltip text="Página anterior" location="bottom">
          <template v-slot:activator="{ props }">
            <v-btn icon variant="text" rounded="xl" color="primary" v-bind="props" @click="paginaAnterior">
              <font-awesome-icon icon="fa-angle-left" class="text-neutral" />
            </v-btn>
          </template>
        </v-tooltip>
        <v-tooltip text="Próxima página" location="bottom">
          <template v-slot:activator="{ props }">
            <v-btn icon variant="text" rounded="xl" color="primary" v-bind="props" @click="proximaPagina">
              <font-awesome-icon icon="fa-angle-right" class="text-neutral" />
            </v-btn>
          </template>
        </v-tooltip>
        <v-tooltip text="Última página" location="bottom">
          <template v-slot:activator="{ props }">
            <v-btn icon variant="text" rounded="xl" color="primary" v-bind="props" @click="ultimaPagina">
              <font-awesome-icon icon="fa-angles-right" class="text-neutral" />
            </v-btn>
          </template>
        </v-tooltip>
      </div>
    </div>
  </div>
</template>

<style scoped>
.container-paginacao {
  color: black;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.pagina-atual-select {
  width: min-content;
}

.pagina-atual-descricao {
  z-index: 1;
}

.v-btn--icon.v-btn--density-default {
  height: 25px !important;
  margin-right: 5px !important;
}

@media (max-width: 600px) {
  .container-paginacao {
    justify-content: flex-end;
  }
}
</style>