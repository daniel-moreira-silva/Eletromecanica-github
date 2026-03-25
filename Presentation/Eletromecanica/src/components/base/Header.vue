<template>
  <v-app-bar app flat class="header-bar">
    <!-- LOGO (esquerda) -->
    <v-img :src="logoSrc" class="ml-3 logo-header" />

    <v-spacer />

    <!-- BREADCRUMB DA ROTA ATUAL -->
    <div class="breadcrumbs">
      <template v-for="(crumb, idx) in breadcrumbs" :key="idx">
        <router-link v-if="crumb.path" :to="crumb.path" class="label-crumb">
          {{ crumb.label }}
        </router-link>
        <span v-else class="label-crumb">{{ crumb.label }}</span>
        <span v-if="idx < breadcrumbs.length - 1" class="mx-1"> / </span>
      </template>
    </div>

    <v-spacer />

    <!-- AVATAR com menu (direita) -->
    <v-menu offset-y>
      <template #activator="{ props }">
        <v-btn v-bind="props" icon>
          <v-avatar class="user-avatar--initials">
            <span>{{ initials }}</span>
          </v-avatar>
        </v-btn>
      </template>

      <v-list>
        <!--*<v-list-item @click="editarPerfil">
          <v-list-item-title>Editar Perfil</v-list-item-title>
        </v-list-item>-->
        <v-list-item @click="openDialog">
          <template #prepend>
            <font-awesome-icon icon="key" class="text-primary me-2" />
          </template>
          <v-list-item-title>Trocar Senha</v-list-item-title>
        </v-list-item>
        <v-list-item @click="toggleTheme">
          <template #prepend>
            <font-awesome-icon icon="palette" class="text-primary me-2" />
          </template>
          <v-list-item-title>
            {{ theme.global.current.value.dark ? 'Tema Claro' : 'Tema Escuro' }}
          </v-list-item-title>
        </v-list-item>
        <v-list-item @click="sair">
          <template #prepend>
            <font-awesome-icon icon="right-from-bracket" class="text-primary me-2" />
          </template>
          <v-list-item-title>Sair</v-list-item-title>
        </v-list-item>
      </v-list>

    </v-menu>
  </v-app-bar>

  <div justify="center">
    <v-dialog v-model="dialogSenha" class="form-dialog" max-width="500">
      <v-card>
        <v-card-text class="pa-4">
          <div class="d-flex align-center pb-2">
            <font-awesome-icon icon="lock" class="text-primary me-2" />
            <span class="title black--text">Editar Senha</span>
            <v-spacer />
            <font-awesome-icon icon="xmark" class="me-1 cursor-pointer" @click="closeDialog" />
          </div>
          <v-divider class="pb-4" />

          <v-form ref="form" @submit.prevent="submitSenha">
            <v-container class="pa-3 pt-2 container-modal">
              <v-row>
                <v-col cols="12" class="pa-0 pb-4">
                  <v-text-field v-model="senhaNova" label="Nova Senha" type="password" clearable density="compact"
                    hide-details="auto" variant="outlined" color="grey-darken-1" base-color="grey-darken-1"
                    :rules="[rules.required, rules.minLength]" />
                </v-col>
                <v-col cols="12" class="pa-0 pb-4">
                  <v-text-field v-model="senhaConfirmacao" label="Confirmar Senha" type="password" clearable
                    density="compact" hide-details="auto" variant="outlined" color="grey-darken-1"
                    base-color="grey-darken-1" :rules="[rules.required, rules.matchPassword]" />
                </v-col>
              </v-row>
            </v-container>
          </v-form>
          <small class="label-italic">
            Senha deve ter no mínimo 4 caracteres.
          </small>
        </v-card-text>

        <v-card-actions class="pt-0">
          <v-spacer />
          <BaseButton label="Cancelar" type="cancel" iconPosition="left" extraClass="me-2" @click="closeDialog" />
          <BaseButton label="Salvar" type="save" iconPosition="left" @click="submitSenha" />
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>

  <!-- SNACKBAR ------------------------------------------------------------------->
  <Snackbar :retorno="retorno" :timeout="3000" :tipo="sucesso ? 'sucesso' : 'erro'" :mensagemRetorno="mensagemRetorno"
    @ocultarRetorno="() => { retorno = false; }" />
  <!-- --------------------------------------------------------------------------- -->
</template>

<script setup>
import { ref, computed, onUnmounted, watch, inject } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTheme } from 'vuetify'
import { useUserMenu } from '@/composables/useUserMenu'
import CryptoJS from 'crypto-js'
import UsuarioService from '@/services/seguranca/usuario-service'
import BaseButton from '@/components/base/BaseButton.vue'
import Snackbar from '@/components/base/Snackbar.vue'

// Menu e permissões dinâmicas
const { filteredMenu } = useUserMenu()

// URLs dos logos para light/dark e desktop/mobile
const logoFullLight = new URL('@/assets/images/logo-horizontal-color.webp', import.meta.url).href
const logoFullDark = new URL('@/assets/images/logo-horizontal-icon-color.webp', import.meta.url).href
const logoMiniLight = new URL('@/assets/images/logo-color.webp', import.meta.url).href
const logoMiniDark = new URL('@/assets/images/logo-icon-color.webp', import.meta.url).href

const chaveSeguranca = inject('chaveSeguranca')
const endpoint = inject('endpoint')
const headerPadrao = inject('headerPadrao')
const usuarioSeguranca = inject('usuarioSeguranca')
const usuarioService = new UsuarioService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)

// Theme e responsividade
const theme = useTheme()
const isMobile = ref(window.innerWidth < 768)
function updateIsMobile() {
  isMobile.value = window.innerWidth < 768
}
window.addEventListener('resize', updateIsMobile)
onUnmounted(() => window.removeEventListener('resize', updateIsMobile))

// Computa src do logo conforme tema e tamanho de tela
const logoSrc = computed(() => {
  const dark = theme.global.current.value.dark
  return isMobile.value
    ? (dark ? logoMiniDark : logoMiniLight)
    : (dark ? logoFullDark : logoFullLight)
})

// Roteamento
const route = useRoute()
const router = useRouter()

// Computa breadcrumbs, incluindo módulo e tela dinâmicos
const breadcrumbs = computed(() => {
  const path = route.path

  // Dashboard
  if (path === '/painel-manobra') {
    return [{ label: 'Dashboard', path: '/painel-manobras' }]
  }

  // Caso especial: perfil atribuição de telas
  if (/^\/perfil\/[\w-]+\/telas$/.test(path)) {
    const raw = route.query.descricao || ''
    const desc = decodeURIComponent(raw.replace(/\+/g, ' '))
    return [
      { label: 'Segurança', path: null },
      { label: desc || 'Telas', path: null }
    ]
  }

  // Busca em filteredMenu
  for (const mod of filteredMenu.value) {
    const child = mod.children.find(c => c.value === path)
    if (child) {
      return [
        { label: mod.title, path: null },
        { label: child.title, path: null }
      ]
    }
  }

  // Fallback genérico
  const segments = path.split('/').filter(Boolean)
  let acc = ''
  let result = segments.map((seg, idx) => {
    acc += `/${seg}`
    return {
      label: seg.replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
      path: idx < segments.length - 1 ? acc : null
    }
  })
  return result
})

// Alterna tema
const toggleTheme = () => {
  theme.global.name.value = theme.global.current.value.dark ? 'light' : 'dark'
}

// Avatar/menu actions
//const avatarUrl = new URL('@/assets/images/user.png', import.meta.url).href
//const editarPerfil = () => router.push('/perfil')

const decryptedLogin = localStorage.getItem('loginNovoSanegeo')
const loginData = decryptedLogin
  ? JSON.parse(CryptoJS.AES.decrypt(decryptedLogin, chaveSeguranca).toString(CryptoJS.enc.Utf8))
  : null

const usuarioId = computed(() => {
  // vem de loginData.data.usuario.usuarioId (JSON de login)
  return loginData?.data?.usuario?.usuarioId || null
})


// Computa iniciais (primeira e última letra)
const nomeUsuario = computed(() => loginData?.data?.usuario?.nome || '')
const initials = computed(() => {
  const parts = nomeUsuario.value.split(' ').filter(Boolean)
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase()
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
})

// Dialog de troca de senha
const dialogSenha = ref(false)
const senhaNova = ref('')
const senhaConfirmacao = ref('')
const form = ref(null)
const retorno = ref(false)
const sucesso = ref(false)
const mensagemRetorno = ref('')

const rules = {
  // Campo não pode ser vazio
  required: v => !!v || 'Campo obrigatório',

  // Tamanho mínimo de 4 caracteres
  minLength: v => (v && v.length >= 4) || 'Senha deve ter pelo menos 4 caracteres',

  // Confirmação deve bater com a senha nova
  matchPassword: v => v === senhaNova.value || 'Senhas não coincidem'
}

const sair = () => { localStorage.removeItem('loginNovoSanegeo'); router.push('/') }

function clearForm() {
  senhaNova.value = ''
  senhaConfirmacao.value = ''
  form.value?.resetValidation()
}
function openDialog() { dialogSenha.value = true }
function closeDialog() { dialogSenha.value = false }

// Limpa campos sempre que o dialog fecha
watch(dialogSenha, val => { if (!val) clearForm() })

async function submitSenha() {
  if (!form.value) return

  // Vuetify 3: validate() pode retornar boolean ou { valid }
  const result = await form.value.validate()
  const valid = typeof result === 'boolean' ? result : result?.valid

  if (!valid) {
    retorno.value = true
    sucesso.value = false
    mensagemRetorno.value = 'Dados inválidos!'
    return
  }

  if (!usuarioId.value) {
    retorno.value = true
    sucesso.value = false
    mensagemRetorno.value = 'Não foi possível identificar o usuário logado. Faça login novamente.'
    return
  }

  try {
    const resposta = await usuarioService.alterarSenha(
      usuarioId.value,
      senhaNova.value,
      senhaConfirmacao.value
    )

    retorno.value = true

    if (resposta?.statusCode === 200) {
      sucesso.value = true
      mensagemRetorno.value =
        resposta?.data?.message || 'Senha atualizada com sucesso'
      retorno.value = true

      clearForm()
      closeDialog()
    } else {
      sucesso.value = false
      mensagemRetorno.value =
        resposta?.data?.message || 'Erro ao atualizar senha'
      retorno.value = true
    }
  } catch (e) {
    retorno.value = true
    sucesso.value = false
    mensagemRetorno.value = 'Erro ao atualizar senha. Contacte o Administrador.'
  }
}
</script>

<style scoped>
.header-bar {
  background-color: rgb(var(--v-theme-surface, 255, 255, 255)) !important;
  color: rgb(var(--v-theme-on-surface, 0, 0, 0)) !important;
  box-shadow: none !important;
}

.breadcrumbs {
  display: flex;
  align-items: center;
  user-select: none;
}

.breadcrumbs a {
  text-decoration: none;
  color: inherit !important;
}

.label-crumb {
  font-size: 15px;
  font-weight: 500;
  color: rgb(var(--v-theme-menu, 69, 85, 96)) !important;
}

.logo-header {
  max-width: 150px;
}

.user-avatar--initials {
  background-color: rgb(var(--v-theme-primary));
  color: rgb(var(--v-theme-on-primary));
  font-weight: bold;
  display: flex;
  align-items: center;
  justify-content: center;
}

.user-avatar--initials:active {
  animation: pulse 0.3s ease-out;
  border-color: rgba(var(--v-theme-on-primary), 0.5);
}

.label-italic {
  font-style: italic;
}

@keyframes pulse {
  0% {
    transform: scale(1);
  }

  50% {
    transform: scale(1.05);
  }

  100% {
    transform: scale(1);
  }
}

@media (max-width: 768px) {
  .logo-header {
    max-width: 75px;
  }

  .label-crumb {
    font-size: 12px;
    font-weight: 100;
    color: rgb(var(--v-theme-menu, 69, 85, 96)) !important;
  }
}
</style>
