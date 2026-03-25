// src/main.js
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

import '@mdi/font/css/materialdesignicons.css'

// FontAwesome
import { registerFontAwesome } from './plugins/fontawesome'

// Vuetify
import { createThemedVuetify } from './plugins/vuetify'

import '@/assets/styles/global.css'


// CKEditor do VUe
import '@vueup/vue-quill/dist/vue-quill.snow.css'

import CryptoJS from 'crypto-js'

// Plugin
import VueColumnsResizable from './plugins/vue-columns-resizable';

// Cria app Vue
const app = createApp(App)

// Configurações de segurança e endpoint (do .env)
const chaveSeguranca = process.env.VUE_APP_CHAVE_SEGURANCA
const endpoint = process.env.VUE_APP_BASE_ENDPOINT
const apiKeyMaps = process.env.VUE_APP_GOOGLE_KEY
const cidadeCliente = process.env.VUE_APP_CIDADE_CLIENTE
const estadoCliente = process.env.VUE_APP_ESTADO_CLIENTE
const usuarioSeguranca = {
  user: process.env.VUE_APP_USUARIO,
  password: process.env.VUE_APP_PASSWORD
}

// Injeta valores globais via provide
app.provide('chaveSeguranca', chaveSeguranca)
app.provide('endpoint', endpoint)
app.provide('headerPadrao', { 'Content-Type': 'application/json' })
app.provide('usuarioSeguranca', usuarioSeguranca)
app.provide('apiKeyMaps', apiKeyMaps)
app.provide('cidadeCliente', cidadeCliente)
app.provide('estadoCliente', estadoCliente)

//Temporário
let data = {
    message: '',
    data: {
    token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3Njg4NzU0MTYsImlzcyI6IkFsZ3VtSXNzdWVyIiwiYXVkIjoiQWxndW1hQXVkaWVuY2UifQ.z_IqrAXzUuxp_SWEt8QTuLNPx8H6MXMwuovrAel5cQI",
    usuario: {
      usuarioId: "62f4295c-7763-42cf-b33a-5ca851f0b1aa",
      perfilId: "661adc84-2391-4704-a2a2-8cb2b71c9166",
      email: "teste3@email.com",
      nome: "teste3",
      ativo: true,
      modulos: [
        {
          id: "3a7a6de8-4ea3-41dd-a4f5-6e5b76f26a88",
          descricao: "Configurações",
          urlControle: "/configuracoes",
          ordenacao: 1,
          ativo: true,
          ativoFormatado: "Ativo",
          telas: [
            {
              id: "111f7a76-08a7-4e28-a03b-f2b12789f0dc",
              moduloId: "3a7a6de8-4ea3-41dd-a4f5-6e5b76f26a88",
              descricao: "Estações",
              url: "/estacoes",
              ordenacao: 1,
              ativo: true,
              exibirMenu: true,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesPerfil: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesUsuario: [
                {
                    id: "001bdb25-26d3-435e-91f9-e25e78be615d",
                    usuarioId: "62f4295c-7763-42cf-b33a-5ca851f0b1aa",
                    telaId: "111f7a76-08a7-4e28-a03b-f2b12789f0dc",
                    permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
                    dataAtribuicao: "2025-11-25T09:28:14.983",
                    dataInicio: "2025-11-25T00:00:00",
                    dataExpiracao: "2026-11-25T00:00:00",
                    permissaoExtra: false,
                    observacao: "",
                    dataAtribuicaoFormatada: "25/11/2025",
                    dataInicioFormatada: "25/11/2025",
                    dataExpiracaoFormatada: "25/11/2026"
                }
              ]
            },
            {
              id: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
              moduloId: "3a7a6de8-4ea3-41dd-a4f5-6e5b76f26a88",
              descricao: "Equipamentos",
              url: "/equipamentos",
              ordenacao: 2,
              ativo: true,
              exibirMenu: true,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesPerfil: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesUsuario: [
                {
                  id: "1b91bf49-5c07-49ba-b4fd-9f472400d678",
                  usuarioId: "62f4295c-7763-42cf-b33a-5ca851f0b1aa",
                  telaId: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
                  permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
                  dataAtribuicao: "2025-11-25T09:28:15.027",
                  dataInicio: "2025-11-25T00:00:00",
                  dataExpiracao: "2026-11-25T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "25/11/2025",
                  dataInicioFormatada: "25/11/2025",
                  dataExpiracaoFormatada: "25/11/2026"
                }
              ]
            },
            {
                    id: "c672fc6b-3a46-4bd6-8512-80d414a825ef",
                    moduloId: "3a7a6de8-4ea3-41dd-a4f5-6e5b76f26a88",
                    descricao: "Criar/Editar Equipamento",
                    url: "/equipamentos/:id",
                    ordenacao: 0,
                    ativo: true,
                    exibirMenu: false,
                    ativoFormatado: "Ativo",
                    permissaoDisponivel: "[Criar,Editar,Ler]",
                    permissoesPerfil: null,
                    permissoesUsuario: [
                        {
                            id: "5be50e38-fc30-4fd6-83e7-fc5de7bacdd4",
                            usuarioId: "62f4295c-7763-42cf-b33a-5ca851f0b1aa",
                            telaId: "c672fc6b-3a46-4bd6-8512-80d414a825ef",
                            permissaoDisponivel: "[Criar,Editar,Ler]",
                            dataAtribuicao: "2025-11-25T09:28:14.78",
                            dataInicio: "2025-11-25T00:00:00",
                            dataExpiracao: "2026-11-25T00:00:00",
                            permissaoExtra: false,
                            observacao: "",
                            dataAtribuicaoFormatada: "25/11/2025",
                            dataInicioFormatada: "25/11/2025",
                            dataExpiracaoFormatada: "25/11/2026"
                        }
                    ]
            },
            {
              id: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
              moduloId: "3a7a6de8-4ea3-41dd-a4f5-6e5b76f26a88",
              descricao: "Serviços Solicitados",
              url: "/servicos-solicitados",
              ordenacao: 3,
              ativo: true,
              exibirMenu: true,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesPerfil: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesUsuario: [
                {
                  id: "1b91bf49-5c07-49ba-b4fd-9f472400d678",
                  usuarioId: "62f4295c-7763-42cf-b33a-5ca851f0b1aa",
                  telaId: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
                  permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
                  dataAtribuicao: "2025-11-25T09:28:15.027",
                  dataInicio: "2025-11-25T00:00:00",
                  dataExpiracao: "2026-11-25T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "25/11/2025",
                  dataInicioFormatada: "25/11/2025",
                  dataExpiracaoFormatada: "25/11/2026"
                }
              ]
            },
            {
              id: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
              moduloId: "3a7a6de8-4ea3-41dd-a4f5-6e5b76f26a88",
              descricao: "Serviços Executados",
              url: "/servicos-executados",
              ordenacao: 4,
              ativo: true,
              exibirMenu: true,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesPerfil: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesUsuario: [
                {
                  id: "1b91bf49-5c07-49ba-b4fd-9f472400d678",
                  usuarioId: "62f4295c-7763-42cf-b33a-5ca851f0b1aa",
                  telaId: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
                  permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
                  dataAtribuicao: "2025-11-25T09:28:15.027",
                  dataInicio: "2025-11-25T00:00:00",
                  dataExpiracao: "2026-11-25T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "25/11/2025",
                  dataInicioFormatada: "25/11/2025",
                  dataExpiracaoFormatada: "25/11/2026"
                }
              ]
            },
          ]
        },
        {
          id: "3a7a6de8-4ea3-41dd-a4f5-6e5b76f26a88",
          descricao: "Atendimento / Solicitações",
          urlControle: "/ocorrencia-tabs",
          ordenacao: 2,
          ativo: true,
          ativoFormatado: "Ativo",
          telas: [
            {
              id: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
              moduloId: "3a7a6de8-4ea3-41dd-a4f5-6e5b76f26a88",
              descricao: "Ordens de Serviço",
              url: "/ocorrencia-tabs",
              ordenacao: 1,
              ativo: true,
              exibirMenu: false,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesPerfil: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesUsuario: [
                {
                  id: "1b91bf49-5c07-49ba-b4fd-9f472400d678",
                  usuarioId: "62f4295c-7763-42cf-b33a-5ca851f0b1aa",
                  telaId: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
                  permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
                  dataAtribuicao: "2025-11-25T09:28:15.027",
                  dataInicio: "2025-11-25T00:00:00",
                  dataExpiracao: "2026-11-25T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "25/11/2025",
                  dataInicioFormatada: "25/11/2025",
                  dataExpiracaoFormatada: "25/11/2026"
                }
              ]
            },
            {
              id: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
              moduloId: "3a7a6de8-4ea3-41dd-a4f5-6e5b76f26a88",
              descricao: "Detalhar Ordem Serviço",
              url: "/detalhar-ordem-servico/:numero",
              ordenacao: 0,
              ativo: true,
              exibirMenu: false,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesPerfil: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesUsuario: [
                {
                  id: "1b91bf49-5c07-49ba-b4fd-9f472400d678",
                  usuarioId: "62f4295c-7763-42cf-b33a-5ca851f0b1aa",
                  telaId: "ddc8d0b4-90d0-4604-8a3e-fa472bb89978",
                  permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
                  dataAtribuicao: "2025-11-25T09:28:15.027",
                  dataInicio: "2025-11-25T00:00:00",
                  dataExpiracao: "2026-11-25T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "25/11/2025",
                  dataInicioFormatada: "25/11/2025",
                  dataExpiracaoFormatada: "25/11/2026"
                }
              ]
            }
          ]
        }
      ]
    }
  }
}
const criptografado = CryptoJS.AES.encrypt(JSON.stringify(data), chaveSeguranca).toString()
localStorage.setItem('loginNovoSanegeo', criptografado)

// Exposição via globalProperties (this.$...)
app.config.globalProperties.$chaveSeguranca = chaveSeguranca
app.config.globalProperties.$endpoint = endpoint
app.config.globalProperties.$headerPadrao = { 'Content-Type': 'application/json' }
app.config.globalProperties.$usuarioSeguranca = usuarioSeguranca

// Registro global do componente FontAwesome
registerFontAwesome(app)

// Instância Vuetify com tema dinâmico (light/dark) conforme armazenamento
const themeMode = localStorage.getItem('themeMode') || 'light'
const vuetify = createThemedVuetify(themeMode)

// Uso de plugins e montagem do app
app
  .use(router)
  .use(vuetify)
  .use(VueColumnsResizable)
  .mount('#app')
