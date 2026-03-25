<template>
  <div class="login-page">
    <v-row class="fill-height ma-0 pa-0">
      <!-- imagem à esquerda (oculta no mobile) -->
      <v-col cols="12" md="8" class="login-left p-0 d-none d-md-flex">

        <!-- Rodapé sobre a imagem -->
        <div class="login-left-footer">
          <!-- copyright -->
          <span class="text-white text-caption"> Copyright &copy; {{ new Date().getFullYear() }} - Powered by
            Linedata</span>
          <!-- versão -->
          <span class="text-white text-caption">Versão {{ version }}</span>
        </div>
      </v-col>
      <!-- formulário à direita -->
      <v-col cols="12" md="4" class="login-right d-flex justify-center p-0">
        <div class="form-container">
          <!-- logo centralizado e aumentado -->
          <div class="logo-container">
            <v-img src="@/assets/images/logo-horizontal-color.webp" max-width="220" contain />
          </div>

          <!-- títulos com espaçamento reforçado -->
          <div class="text-center">
            <p class="login-title">Bem-vindo de volta</p>
            <p class="login-subtitle">Faça login para continuar</p>
          </div>

          <!-- formulário -->
          <v-form ref="loginForm" @submit.prevent="signIn">
            <!-- usuário -->
            <div class="field-group">
              <label class="field-label">
                <font-awesome-icon icon="user" class="me-2 text-primary" />Usuário
              </label>
              <v-text-field v-model="email" variant="outlined" density="compact" :rules="emailRules" hide-details />
            </div>

            <!-- senha -->
            <div>
              <label class="field-label">
                <font-awesome-icon icon="key" class="me-2 text-primary" />Senha
              </label>
              <v-text-field v-model="password" :type="showPassword ? 'text' : 'password'" variant="outlined"
                density="compact" :rules="passwordRules" hide-details>
                <template #append-inner>
                  <font-awesome-icon :icon="showPassword ? 'eye-slash' : 'eye'" class="cursor-pointer text-primary"
                    @click="showPassword = !showPassword" />
                </template>
              </v-text-field>
            </div>

            <!-- manter conectado -->
            <!--<div class="field-group lite-checkbox">
              <v-checkbox v-model="stayConnected" label="Manter conectado" density="compact" color="primary"
                hide-details />
            </div>-->

            <!-- botao acessar -->
            <v-btn type="submit" block color="primary" class="access-btn mb-0" :disabled="loading">
              Entrar
            </v-btn>

            <div class="forgot-password text-right">
              <a href="#" @click.prevent="showRecoverDialog = true" class="text-accent text-caption">
                Esqueci minha senha
              </a>
            </div>

            <!--
              <v-divider class="separator"></v-divider>

              <v-btn block color="white" class="mb-3"
                @click="notify('Autenticação via Google realizada com sucesso.', 'success')">
                <font-awesome-icon icon="fa-brands fa-google" class="me-2" /> Entrar com Google
              </v-btn>
              <v-btn block color="#007FFF" class="mb-3 text-white"
                @click="notify('Falha ao conectar com Azure. Tente novamente mais tarde.', 'error')">
                <font-awesome-icon icon="fa-brands fa-microsoft" class="me-2" /> Entrar com Azure
              </v-btn>
              <v-btn block color="#2F2F2F" class="mb-6 text-white"
                @click="notify('Suas credenciais irão expirar em 5 dias.', 'warning')">
                <font-awesome-icon icon="fa-brands fa-windows" class="me-2" /> Entrar com Microsoft
              </v-btn>

              <div class="text-center text-caption copyright">
                &copy; {{ new Date().getFullYear() }} Sanegeo. Todos os direitos reservados.
              </div>
              -->
          </v-form>
        </div>
      </v-col>
    </v-row>

    <!-- dialog de recuperar senha -->
    <v-dialog v-model="showRecoverDialog" max-width="600">
      <v-card class="pa-6">
        <v-card-title>
          <font-awesome-icon icon="key" class="text-primary me-2" />
          Recuperar Senha
          <font-awesome-icon icon="xmark" @click="showRecoverDialog = false"
            class="text-close float-right icon-clicavel me-2" title="Fechar" />
        </v-card-title>
        <v-divider class="pb-4"></v-divider>
        <v-card-text>
          <p> Deseja realmente excluir o anexo? </p>
        </v-card-text>
        <v-card-text>
          <div class="field-group">
            <label class="field-label">
              <font-awesome-icon icon="envelope" class="me-2 text-primary" />E-mail cadastrado
            </label>
            <v-text-field v-model="recoverEmail" variant="outlined" density="compact" :rules="emailRules"
              hide-details />
          </div>

        </v-card-text>
        <v-divider class="pb-4"></v-divider>
        <v-card-actions class="justify-end">
          <BaseButton label="Cancelar" type="cancel" iconPosition="left" @click="showRecoverDialog = false"
            extraClass="me-2" />
          <BaseButton label="Enviar" type="save" iconPosition="left" @click="submitRecovery" extraClass="me-2" />
        </v-card-actions>
      </v-card>
    </v-dialog>

    <LoadingOverlay :active="loading" />

    <Snackbar :retorno="snackRetorno" :timeout=3000 :tipo="snackTipo" :mensagemRetorno="snackMensagem"
      @ocultarRetorno="snackRetorno = false" />
  </div>
</template>

<script setup>
import { ref, inject, getCurrentInstance } from 'vue'
import { useRouter } from 'vue-router'
import UsuarioService from '@/services/seguranca/usuario-service'
import CryptoJS from 'crypto-js'
import LoadingOverlay from '@/components/base/LoadingOverlay.vue'
import Snackbar from '@/components/base/Snackbar.vue'
import BaseButton from '@/components/base/BaseButton.vue'

// Importa versão direto do package.json
import { version } from '../../package.json'


// Campos do formulário
const email = ref('')
const password = ref('')
const showPassword = ref(false)
//const stayConnected = ref(false)

// Estado UI
const loading = ref(false)
const snackRetorno = ref(false)
const snackMensagem = ref('')
const snackTipo = ref('info')
const showRecoverDialog = ref(false)
const recoverEmail = ref('')
const loginForm = ref(null)

// Regras de validação
const emailRules = [
  v => !!v || 'Informe um e-mail válido!',
  v => /.+@.+\..+/.test(v) || 'Informe um e-mail válido!'
]
const passwordRules = [v => !!v || 'Informe a senha!']

// Contexto global e navegação
const { appContext } = getCurrentInstance()
const global = appContext.config.globalProperties
const router = useRouter()
const chaveSeguranca = inject('chaveSeguranca')

// Snackbar helper
function notify(msg, type = 'info') {
  const map = { success: 'sucesso', error: 'erro', warning: 'aviso', info: 'info' }
  snackTipo.value = map[type] ?? 'info'
  snackMensagem.value = msg
  snackRetorno.value = true
}

// Recuperação de senha
function submitRecovery() {
  showRecoverDialog.value = false
  notify('Se este e-mail estiver cadastrado, você receberá instruções para recuperar sua senha.', 'info')
}

// Login
async function signIn() {
  const valido = await loginForm.value?.validate()
  if (!valido?.valid) {
    const erroEmail = emailRules.map(r => r(email.value)).find(m => m !== true)
    const erroSenha = passwordRules.map(r => r(password.value)).find(m => m !== true)
    if (erroEmail && erroSenha) notify('Todos os campos são de preenchimento obrigatório', 'warning')
    else if (erroEmail) notify(erroEmail, 'warning')
    else notify(erroSenha, 'warning')
    return
  }
  loading.value = true
  const service = new UsuarioService(
    global.$endpoint,
    global.$headerPadrao,
    chaveSeguranca,
    global.$usuarioSeguranca
  )
  const resultado = await service.login({ email: email.value, senha: password.value })
  loading.value = false
  if (resultado.statusCode === 200) {
    const criptografado = CryptoJS.AES.encrypt(JSON.stringify(resultado.data), chaveSeguranca).toString()
    localStorage.setItem('loginNovoSanegeo', criptografado)
    const rotaNavegacao = localStorage.getItem('rotaNavegacao')
    if (rotaNavegacao) { localStorage.removeItem('rotaNavegacao'); await router.push(rotaNavegacao) }
    else await router.push('/painel-manobras')
  } else {
    notify(resultado?.data?.message || 'Erro ao fazer login. Contacte o administrador do sistema.', 'error')
  }
}
</script>

<style scoped>
.login-page {
  height: 100vh;
  background-color: var(--v-theme-background);
}

.login-left {
  background-image: url('@/assets/images/background-login.webp');
  background-size: cover;
  background-position: center;
  position: relative;
}

.form-container {
  width: 100%;
  max-width: 90%;
  margin: 0 auto;
  padding: 0 24px;
}

.logo-container {
  display: flex;
  justify-content: center;
  margin: 60px 0 60px 0;
}

.login-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--v-theme-on-surface);
}

.login-subtitle {
  font-size: 13px;
  font-weight: 100;
  color: var(--v-theme-neutral);
  margin-bottom: 55px;
}

.field-group {
  margin-bottom: 16px;
}

.field-label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: rgba(0, 0, 0, 0.6);
  margin-bottom: 4px;
}

/* Checkbox suave e menor */
.lite-checkbox ::v-deep(.v-input--selection-controls .v-selection-control__input) {
  width: 14px;
  height: 14px;
}

::v-deep(.v-icon) {
  --v-icon-size-multiplier: 0.75 !important;
}

.lite-checkbox ::v-deep(.v-label) {
  font-size: 14px;
  color: rgba(0, 0, 0, 0.6);
  margin-left: 4px;
}

.access-btn {
  margin-top: 50px;
}

/* Botões sem uppercase */
::v-deep(.v-btn .v-btn__content) {
  text-transform: none !important;
}

.v-icon {
  --v-icon-size-multiplier: 0.75;
}

.login-right {
  background-color: var(--v-theme-surface);
}

.login-left-footer {
  position: absolute;
  bottom: 16px;
  left: 0;
  right: 0;
  display: flex;
  justify-content: space-between;
  padding: 0 24px;
  color: var(--v-theme-on-surface);
}

/* Link “Esqueci minha senha” */
.forgot-password a {
  text-decoration: none;
  cursor: pointer;
}

/* Mobile: imagem oculta e form full width */
@media (max-width: 960px) {
  .login-left {
    display: none !important;
  }

  .form-container {
    width: 100% !important;
    padding: 0 16px;
  }
}
</style>
