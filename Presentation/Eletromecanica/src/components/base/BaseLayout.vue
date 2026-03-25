<template>
  <!-- BaseLayout.vue: layout wrapper sem <v-app> para evitar provedores aninhados -->

  <!-- Header (application top bar) -->
  <slot name="header" />

  <!-- Conteúdo principal -->
  <v-main>
    <v-container fluid class="pa-0">
      <v-row no-gutters>
        <!-- Sidebar (Menu) -->
        <v-col cols="auto">
          <slot name="menu" />
        </v-col>

        <!-- Main view -->
        <v-col>
          <slot />
        </v-col>
      </v-row>
    </v-container>
  </v-main>

  <!-- Footer (ex.: idle watcher) -->
  <slot name="footer" />
</template>

<script setup>
// BaseLayout.vue
// Slots:
// - header: recebe Header component
// - menu: recebe Menu component
// - default: recebe conteúdo principal via <router-view>
// - footer: recebe elementos adicionais, como idle watcher
</script>

<style scoped>
.pa-0 {
  padding: 0 !important;
}

.no-gutters {
  margin: 0;
}
</style>
