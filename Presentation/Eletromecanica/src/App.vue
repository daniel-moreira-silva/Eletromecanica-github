<script setup>
import { computed } from 'vue'
//import { useRoute, useRouter } from 'vue-router'
import { useRouter } from 'vue-router'
import Vidle from 'v-idle'
import { useTheme } from 'vuetify'

import BaseLayout from '@/components/base/BaseLayout.vue'
import Header from '@/components/base/Header.vue'
import Menu from '@/components/base/Menu.vue'

//Rodas onde o menu deverá ficar oculto
//const hideComponentsRoutes = ['/', '/Login', '/recuperar-senha']

//const route = useRoute()
const router = useRouter()

// Pega o tema corrente ('light' ou 'dark')
const { global: themeGlobal } = useTheme()
const themeClass = computed(() => `theme-${themeGlobal.current.value}`)

//const showLayoutComponents = computed(() => !hideComponentsRoutes.includes(route.path))

function onIdle() {
  localStorage.removeItem('loginNovoSanegeo')
  router.push({ path: '/' }).catch(failure => {
    console.error('Idle logout failed:', failure)
  })
}
</script>

<template>
  <!-- 1 único <v-app> para TODA a aplicação -->
  <v-app :class="themeClass">

    <!--<BaseLayout v-if="showLayoutComponents">-->
    <BaseLayout>
      <!-- Header -->
      <template #header>
        <Header />
      </template>

      <!-- Menu lateral -->
      <template #menu>
        <Menu />
      </template>

      <!-- Conteúdo principal -->
      <router-view />

      <!-- Idle watcher fica no footer do layout -->
      <template #footer>
        <Vidle @idle="onIdle" :duration="1200" class="d-none" />
      </template>
    </BaseLayout>

    <!-- rotas que já defini para nãoaparecer header irão continuar sem header/menu -->
    <!--<router-view v-else />-->

  </v-app>
</template>


<style></style>