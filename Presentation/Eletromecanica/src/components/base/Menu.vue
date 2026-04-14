<template>
  <v-navigation-drawer expand-on-hover permanent rail app v-model:rail="isRail" color="primary" class="menu-drawer">
    <v-list nav density="comfortable" class="menu-list">

      <!-- Dashboard -->
      <v-list-item title="Dashboard" dense clickable @click="onChildClick({ value: '/' })"
        :class="['cursor-pointer', { 'active-item': isActive('/') }]">
        <template #prepend>
          <v-icon icon="fas fa-chart-line" class="group-icon" />
        </template>
      </v-list-item>

      <v-divider class="mx-2 my-2" />

      <!-- MÓDULOS DINÂMICOS, FILTRADOS POR PERMISSÕES -->
      <template v-for="(mod, idx) in filteredMenu" :key="mod.title">
        <!-- Se houver subitens, exibe grupo expansível -->
        <template v-if="mod.children?.length">
          <v-list-item dense clickable @click="toggleGroup(idx)"
            :class="['group-item', { 'active-group': isGroupActive(idx) }]">
            <template #prepend>
              <v-icon :icon="mod.icon" class="group-icon" />
            </template>
            <v-list-item-title>{{ mod.title }}</v-list-item-title>
            <template #append>
              <v-icon icon="fas fa-chevron-down" class="expand-icon" :class="{ rotate: openedGroup === idx }" />
            </template>
          </v-list-item>

          <v-expand-transition>
            <div v-show="openedGroup === idx" class="subitem-wrapper">
              <v-list-item v-for="child in mod.children" :key="child.value" dense clickable @click="onChildClick(child)"
                :class="['cursor-pointer', 'subitem', { 'active-item': isActive(child.value) }]">
                <template #title>
                  {{ child.title }}
                </template>
              </v-list-item>
            </div>
          </v-expand-transition>
        </template>

        <!-- Se não tiver subitens, clicável direto para a URL do módulo -->
        <template v-else>
          <v-list-item dense clickable @click="onChildClick({ value: mod.value })"
            :class="['cursor-pointer', { 'active-item': isActive(mod.value) }]">
            <template #prepend>
              <v-icon :icon="mod.icon" class="group-icon" />
            </template>
            <v-list-item-title>{{ mod.title }}</v-list-item-title>
          </v-list-item>
        </template>
      </template>
    </v-list>
  </v-navigation-drawer>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useUserMenu } from '@/composables/useUserMenu'

// Menu dinâmico
const { filteredMenu } = useUserMenu()

const router = useRouter()
const route = useRoute()

// Estado rail e grupos
const isRail = ref(true)
const openedGroup = ref(-1)

// Verifica se rota é ativa
function isActive(path) {
  return route.path === path || route.path.startsWith(path + '/')
}

// Alterna expansão de grupo
function toggleGroup(idx) {
  openedGroup.value = openedGroup.value === idx ? -1 : idx
}

// Navega e fecha grupo
async function onChildClick(item) {
  openedGroup.value = -1
  if (item.value) await router.push(item.value)
}

// Índice de grupo ativo
const activeGroupIndex = computed(() =>
  filteredMenu.value.findIndex(mod =>
  (mod.children?.length
    ? mod.children.some(child => isActive(child.value))
    : mod.value === route.path)
  )
)

// Verifica se grupo está ativo
function isGroupActive(idx) {
  return openedGroup.value === idx || activeGroupIndex.value === idx
}
</script>

<style scoped>
.menu-drawer {
  border-right: none;
  background-color: rgb(var(--v-theme-surface, 255, 255, 255)) !important;
  box-shadow: 1px 1px 2px -1px rgba(165, 165, 165, 0.7) !important;
  color: rgb(var(--v-theme-menu, 69, 85, 96)) !important;
}

.menu-list {
  padding-top: 10px;
  margin-top: 10px;
}

.v-list-item {
  min-height: 35px !important;
  padding: 4px 0 !important;
}

.v-list-item:hover {
  background-color: rgba(62, 121, 247, 0.05) !important;
  transition: background-color 0.2s;
}

.group-icon,
.menu-list .v-list-item .v-icon {
  font-size: 16px !important;
  color: rgb(var(--v-theme-menu, 69, 85, 96)) !important;
  opacity: 1 !important;
}

.menu-list .v-list-item-title {
  color: rgb(var(--v-theme-menu, 69, 85, 96)) !important;
  font-weight: 500 !important;
  font-size: 13px !important;
  margin: 10px 0 !important;
}

/* Itens ativos */
.active-item {
  background-color: rgba(62, 121, 247, 0.1) !important;
  border-right: 3px solid rgb(var(--v-theme-primary, 51, 102, 255)) !important;
}

.active-item .v-list-item-title {
  color: rgb(var(--v-theme-primary, 51, 102, 255)) !important;
  font-weight: 600 !important;
}

/* Grupo ativo */
.active-group {
  background-color: rgba(62, 121, 247, 0.1) !important;
  border-right: 3px solid rgb(var(--v-theme-primary, 51, 102, 255)) !important;
}

.subitem {
  color: rgb(var(--v-theme-menu, 69, 85, 96)) !important;
  margin-left: 50px !important;
}

.active-item .bullet-marker {
  color: rgb(var(--v-theme-on-surface, 0, 0, 0)) !important;
  font-weight: bold;
}

.expand-icon.rotate {
  transform: rotate(180deg);
}

.v-list-item--nav {
  padding-inline: 12px !important;
}

.v-navigation-drawer--is-hovering {
  width: 300px !important;
}
</style>
