<template>
  <v-container fluid class="operacoes-container pa-0">
    <v-tabs v-model="tab" class="operacoes-tabs" grow>
      <v-tab value="os" class="operacoes-tab" :class="{ 'operacoes-tab--active': tab === 'os' }">
        <font-awesome-icon icon="clipboard-list" class="me-2 text-info" />
        Abertura de OS
      </v-tab>

      <v-tab value="reclamacao" class="operacoes-tab" :class="{ 'operacoes-tab--active': tab === 'reclamacao' }">
        <font-awesome-icon icon="bullhorn" class="me-2 text-info" />
        Consulta de Reclamações
      </v-tab>

      <v-tab value="consulta" class="operacoes-tab" :class="{ 'operacoes-tab--active': tab === 'consulta' }">
        <font-awesome-icon icon="search" class="me-2 text-info" />
        Consulta de OS
      </v-tab>
    </v-tabs>

    <v-window v-model="tab">
      <v-window-item value="os" class="operacoes-tab-item">
        <NovaOcorrencia />
      </v-window-item>

      <v-window-item value="reclamacao" class="operacoes-tab-item">
        <!-- <AberturaReclamacao /> -->
      </v-window-item>

      <v-window-item value="consulta" class="operacoes-tab-item">
        <ConsultaOrdemServico />
      </v-window-item>
    </v-window>
  </v-container>
</template>

<script setup>
import { ref } from 'vue'
import NovaOcorrencia from '@/views/NovaOrdemServico.vue'
import ConsultaOrdemServico from '@/views/ConsultaOrdemServico.vue'

const tab = ref('os')
</script>

<style scoped>
.operacoes-container {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.operacoes-tabs {
  position: sticky;
  top: 0;
  z-index: 10;
}

.operacoes-tab {
  text-transform: none !important;
  font-weight: 500;
  color: var(--v-theme-on-primary);
}

.operacoes-tab--active {
  background-color: var(--v-theme-surface) !important;
  color: var(--v-theme-primary) !important;
}

.operacoes-tab,
.operacoes-tab>.v-btn__content {
  padding-top: 12px !important;
  padding-bottom: 12px !important;
}
</style>