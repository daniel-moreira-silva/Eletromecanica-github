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

// Status Ordem de Serviço
const statusOrdemServico = {
  solicitada:   'E4011FE2-1CFC-46A4-9BB7-02F1E154C00E',
  iniciada:     '2CF3AF37-18CD-4DF6-BE22-91B4DE792B25',
  emAndamento:  '132AE55D-A544-439E-8D07-CDC643CE4E78',
  finalizada:   '8330AEC6-5E4D-4B67-8620-0C13FAEC04C5',
  cancelada:    'FC34601C-803F-4DA4-B4C9-0843AA1E8E4A',
  pendente:     '642A2B4C-1B28-4857-9FB9-C652B889392B',
}

// Injeta valores globais via provide
app.provide('chaveSeguranca', chaveSeguranca)
app.provide('endpoint', endpoint)
app.provide('headerPadrao', { 'Content-Type': 'application/json' })
app.provide('usuarioSeguranca', usuarioSeguranca)
app.provide('apiKeyMaps', apiKeyMaps)
app.provide('cidadeCliente', cidadeCliente)
app.provide('estadoCliente', estadoCliente)
app.provide('statusOrdemServico', statusOrdemServico)

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
            {
              id: "0e6b00f1-b2b9-4aaa-b1cc-242136f03cc4",
              moduloId: "bc728d1d-c586-45d1-a49e-5445add6fbdb",
              descricao: "Motivo Cancelamento",
              url: "/motivos-cancelamento",
              ordenacao: 5,
              ativo: true,
              exibirMenu: true,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesPerfil: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesUsuario: [
                {
                  id: "790a5cbf-aa46-4fdf-a080-14b9b26a4ce7",
                  usuarioId: "275f7e8d-93d6-4065-8f48-2ad83b8feedd",
                  telaId: "0e6b00f1-b2b9-4aaa-b1cc-242136f03cc4",
                  permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
                  dataAtribuicao: "2026-04-01T10:35:53.07",
                  dataInicio: "2026-04-01T00:00:00",
                  dataExpiracao: "2027-04-01T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "01/04/2026",
                  dataInicioFormatada: "01/04/2026",
                  dataExpiracaoFormatada: "01/04/2027"
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
        },
        {
          id: "024e975c-5beb-4597-a3da-076266c8c206",
          descricao: "Gestão de serviços e Equipes",
          urlControle: "/operacao",
          ordenacao: 2,
          ativo: true,
          ativoFormatado: "Ativo",
          telas: [
            {
              id: "2aed7cf0-6be9-4071-ad3b-b7c0d618d70c",
              moduloId: "024e975c-5beb-4597-a3da-076266c8c206",
              descricao: "Detalhar Ocorrência",
              url: "/detalhar-ocorrencia/:numero",
              ordenacao: 2,
              ativo: false,
              exibirMenu: false,
              ativoFormatado: "Inativo",
              permissaoDisponivel: "[Criar,Editar,Ler,Exportar,Excluir]",
              permissoesPerfil: null,
              permissoesUsuario: [
                {
                  id: "40a2b9d3-a563-46e2-8bdd-1a69af016e4d",
                  usuarioId: "275f7e8d-93d6-4065-8f48-2ad83b8feedd",
                  telaId: "2aed7cf0-6be9-4071-ad3b-b7c0d618d70c",
                  permissaoDisponivel: "[Criar,Editar,Ler,Exportar,Excluir]",
                  dataAtribuicao: "2026-04-01T10:35:58.043",
                  dataInicio: "2026-04-01T00:00:00",
                  dataExpiracao: "2027-04-01T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "01/04/2026",
                  dataInicioFormatada: "01/04/2026",
                  dataExpiracaoFormatada: "01/04/2027"
                }
              ]
            },
            {
              id: "e7016281-0447-4aeb-8640-1bf50dad2dfb",
              moduloId: "024e975c-5beb-4597-a3da-076266c8c206",
              descricao: "Detalhes da Ocorrência",
              url: "/detalhes-ocorrencia/:numero",
              ordenacao: 2,
              ativo: true,
              exibirMenu: false,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Ação,Excluir,Exportar,Ler,Editar,Criar]",
              permissoesPerfil: null,
              permissoesUsuario: [
                {
                  id: "38faca76-045e-4193-ad41-8722e32877aa",
                  usuarioId: "275f7e8d-93d6-4065-8f48-2ad83b8feedd",
                  telaId: "e7016281-0447-4aeb-8640-1bf50dad2dfb",
                  permissaoDisponivel: "[Ação,Criar,Editar,Excluir,Exportar,Ler]",
                  dataAtribuicao: "2026-04-01T10:35:52.78",
                  dataInicio: "2026-04-01T00:00:00",
                  dataExpiracao: "2027-04-01T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "01/04/2026",
                  dataInicioFormatada: "01/04/2026",
                  dataExpiracaoFormatada: "01/04/2027"
                }
              ]
            },
            {
              id: "7edbd684-a056-4cf4-9ea1-67e545e80746",
              moduloId: "024e975c-5beb-4597-a3da-076266c8c206",
              descricao: "Operações",
              url: "/operacao",
              ordenacao: 2,
              ativo: true,
              exibirMenu: false,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesPerfil: null,
              permissoesUsuario: [
                {
                  id: "2ce585fc-be71-479d-8aa6-3e793448707c",
                  usuarioId: "275f7e8d-93d6-4065-8f48-2ad83b8feedd",
                  telaId: "7edbd684-a056-4cf4-9ea1-67e545e80746",
                  permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
                  dataAtribuicao: "2026-04-01T10:35:55.423",
                  dataInicio: "2026-04-01T00:00:00",
                  dataExpiracao: "2027-04-01T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "01/04/2026",
                  dataInicioFormatada: "01/04/2026",
                  dataExpiracaoFormatada: "01/04/2027"
                }
              ]
            },
            {
              id: "4c75f69e-a739-41f4-9d7f-9444c1b381ee",
              moduloId: "024e975c-5beb-4597-a3da-076266c8c206",
              descricao: "Lista Ocorrências",
              url: "/lista-ocorrencia",
              ordenacao: 2,
              ativo: true,
              exibirMenu: false,
              ativoFormatado: "Ativo",
              permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
              permissoesPerfil: null,
              permissoesUsuario: [
                {
                  id: "41b2a647-139f-4302-8590-7e5bb9c42a23",
                  usuarioId: "275f7e8d-93d6-4065-8f48-2ad83b8feedd",
                  telaId: "4c75f69e-a739-41f4-9d7f-9444c1b381ee",
                  permissaoDisponivel: "[Criar,Editar,Excluir,Exportar,Ler]",
                  dataAtribuicao: "2026-04-01T10:35:57.147",
                  dataInicio: "2026-04-01T00:00:00",
                  dataExpiracao: "2027-04-01T00:00:00",
                  permissaoExtra: false,
                  observacao: "",
                  dataAtribuicaoFormatada: "01/04/2026",
                  dataInicioFormatada: "01/04/2026",
                  dataExpiracaoFormatada: "01/04/2027"
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
app.config.globalProperties.$statusOrdemServico = statusOrdemServico

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
