<template>
  <v-btn v-bind="attrs" variant="text" density="comfortable" :color="config.color" :disabled="props.disabled"
    class="text-base-button">
    <!-- Ícone à esquerda -->
    <font-awesome-icon v-if="config.icon && props.iconPosition === 'left'" :icon="config.icon"
      :class="['me-3', `text-${config.color}`]" />

    <!-- Label do botão -->
    {{ props.label }}

    <!-- Ícone à direita -->
    <font-awesome-icon v-if="config.icon && props.iconPosition === 'right'" :icon="config.icon"
      :class="['ml-3', `text-${config.color}`]" />
  </v-btn>
</template>

<script setup>
/* eslint-disable no-undef */
import { computed, useAttrs } from 'vue'

const props = defineProps({
  // Texto exibido no botão
  label: { type: String, required: true },
  // Tipo pré-definido: 'back', 'next', 'save', etc.
  type: { type: String, required: true },
  // Desabilita o botão */
  disabled: { type: Boolean, default: false },
  // Posição do ícone: 'left' ou 'right'
  iconPosition: {
    type: String,
    default: 'left',
    validator: (val) => ['left', 'right'].includes(val),
  },
  // Sobreposições opcionais de cor e ícone
  color: { type: String, default: null },
  icon: { type: [String, Array], default: null },
})

const attrs = useAttrs()

const config = computed(() => {
  const map = {
    back: { color: 'accent', icon: ['fas', 'angle-left'] },
    next: { color: 'primary', icon: ['fas', 'angle-right'] },
    save: { color: 'success', icon: ['fas', 'check'] },
    cancel: { color: 'error', icon: ['fas', 'xmark'] },
    confirm: { color: 'success', icon: ['fas', 'check'] },
    close: { color: 'grey', icon: ['fas', 'xmark'] },
    clear: { color: 'grey', icon: ['fas', 'filter-circle-xmark'] },
    remove: { color: 'accent', icon: ['fas', 'eraser'] },
    atrib: { color: 'primary', icon: ['fas', 'list-check'] },
    search: { color: 'primary', icon: ['fas', 'search'] },
    wpp: { color: 'success', icon: ['fab', 'whatsapp'] },
    send: { color: 'primary', icon: ['fas', 'paper-plane'] },
    return: { color: 'primary', icon: ['fas', 'reply'] },
    print: { color: 'primary', icon: ['fas', 'print'] },
    time: { color: 'primary', icon: ['fas', 'clock-rotate-left'] },
  }
  const base = map[props.type] || { color: 'primary', icon: null }
  return {
    color: props.color || base.color,
    icon: props.icon || base.icon,
  }
})
</script>

<style scoped>
.v-btn--disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.text-base-button {
  font-weight: 300;
  cursor: pointer;
}

.v-btn {
  text-transform: none;
}
</style>
