<template>
  <div>
    <v-snackbar v-model="retorno" :timeout="-1" :color="selecionarCor()" min-width="fit-content">
      <v-progress-circular width="1" v-model="progresso" color="white" class="mr-1">
        <template v-slot:default>
          <font-awesome-icon :icon="selecionarIcone()" class="text-white" />
        </template>
      </v-progress-circular>
      {{ props.mensagemRetorno }}
    </v-snackbar>
  </div>
</template>

<script setup>
import { defineProps, ref, watch, defineEmits } from "vue";
const props = defineProps([
  "retorno",
  "timeout",
  "tipo",
  "mensagemRetorno",
]);
const emits = defineEmits(["ocultarRetorno"]);

const retorno = ref(false);
const progresso = ref(0);
watch(
  () => props.retorno,
  (newRetorno) => {
    retorno.value = !!newRetorno;
    if (retorno.value)
      setTimeout(() => {
        progresso.value = 100;
      }, 500);
    else
      progresso.value = 0;
    if (props.timeout && retorno.value == true) {
      setTimeout(() => {
        emits("ocultarRetorno");
      }, props.timeout);
    }
  }
);
function selecionarCor() {
  if (props.tipo == 'sucesso')
    return 'success';
  else if (props.tipo == 'erro')
    return 'error';
  else if (props.tipo == 'aviso')
    return 'warning';
  else
    return 'info';
}

function selecionarIcone() {
  if (props.tipo === 'sucesso') return ['fas', 'check-circle'];
  if (props.tipo === 'erro') return ['fas', 'times-circle'];
  if (props.tipo === 'aviso') return ['fas', 'exclamation-triangle'];
  return ['fas', 'circle-info'];
}
</script>

<style scoped>
.v-snackbar {
  z-index: 10001 !important;
  /*justify-content: right !important;*/
}

::v-deep .v-snackbar__content {
  font-size: 1rem;
  text-align: center;

}
</style>