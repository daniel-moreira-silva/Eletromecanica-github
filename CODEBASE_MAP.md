# CODEBASE MAP — Eletromecanica

## 1. Stack Tecnológica

### Backend
| Componente | Tecnologia |
|---|---|
| Runtime | .NET 9.0 |
| Web Framework | ASP.NET Core 9.0 (minimal hosting) |
| Linguagem | C# (nullable references, implicit usings) |
| ORM / Data Access | Dapper 2.1.66 (raw SQL) |
| Banco de Dados | SQL Server (local: SQLEXPRESS / nuvem: Azure SQL) |
| SQL Client | Microsoft.Data.SqlClient 6.1.4 + System.Data.SqlClient 4.9.1 |
| HTTP Client externo | Refit 10.0.1 + Refit.HttpClientFactory |
| Cron / Agendamento | Cronos 0.11.1 |
| Documentação API | Swashbuckle.AspNetCore 6.2.3 (Swagger/OpenAPI) |
| Logging | NLog 4.7.13 + NLog.Web.AspNetCore 4.14.0 |

### Frontend
| Componente | Tecnologia |
|---|---|
| Framework | Vue 3.2.13 |
| Roteamento | Vue Router 4.5.1 |
| UI Framework | Vuetify 3.9.0 |
| Build Tool | Vue CLI 5.0.0 |
| Linguagem | JavaScript (ES6+) |
| Editor Rich-Text | CKEditor 5 (41.4.2) + Quill (@vueup/vue-quill 1.2.0) |
| Gráficos | Chart.js 4.5.1 |
| Mapas | vue3-google-map 0.27.0 |
| Criptografia (cliente) | crypto-js 4.2.0 + vue-cryptojs 2.4.7 |
| Ícones | FontAwesome 6.7.2 + @mdi/font 5.9.55 + @mdi/js 7.3.67 |
| Máscaras de Input | vue-the-mask 0.11.1 |
| Timeout de sessão | v-idle 1.0.3 |
| Estilos | Sass 1.89.2 |

---

## 2. Estrutura de Pastas

```
Eletromecanica/
├── API/                        # Projeto ASP.NET Core — camada de apresentação HTTP
│   ├── Controllers/            # 14 controllers REST (endpoints da aplicação)
│   ├── Extensions/             # Registro de DI da camada API
│   ├── Utils/                  # Utilitários de API
│   ├── Properties/             # Metadados de assembly
│   ├── Program.cs              # Entry point da API
│   └── appsettings.json        # Configurações (strings de conexão, CORS, etc.)
│
├── Business/                   # Camada de regras de negócio
│   ├── Services/               # 14 services com lógica de negócio
│   ├── Interfaces/             # Interfaces dos services (contratos)
│   ├── Dtos/                   # Data Transfer Objects (contratos de API)
│   └── Extensions/             # Registro de DI da camada Business
│
├── Core/                       # Núcleo do domínio — compartilhado por todas as camadas
│   ├── Models/                 # Modelos de domínio organizados por agregado
│   │   ├── DocumentAggregate/  # Modelos de documentos/arquivos
│   │   ├── EquipmentAggregate/ # Equipamento + componentes (Bomba, Motor, CLP, etc.)
│   │   ├── OrdemServicoAggregate/ # Ordens de Serviço + ServicoSolicitado/Executado
│   │   ├── FuncionarioAggregate/  # Funcionários e cargos
│   │   ├── DashboardAggregate/ # Métricas de dashboard
│   │   ├── GoogleMapAggregate/ # Modelos de geolocalização
│   │   ├── PaginateAggregate/  # Filtros e paginação
│   │   └── Enums/              # Enumerações do domínio
│   ├── Interfaces/             # Interfaces base (IRepository, etc.)
│   ├── Constants/              # Constantes e recursos compartilhados
│   └── Utils/                  # Utilitários do núcleo
│
├── Data/                       # Camada de acesso a dados
│   ├── Repositories/           # 20 repositórios concretos (implementam interfaces)
│   ├── Interfaces/             # Interfaces dos repositórios
│   ├── Clients/                # Clientes HTTP externos (ex: IGoogleMapClient via Refit)
│   ├── Utils/                  # DbUtils, SqlQueryBuilder, DocumentoUtils
│   ├── Extensions/             # Registro de DI da camada Data
│   └── Scripts/                # Scripts SQL de migração e carga de dados
│       ├── 00_script_inicial.sql  # Criação completa do schema
│       ├── carga.sql              # Carga de dados iniciais
│       ├── expurgo_dados.sql      # Limpeza de dados
│       └── massa_teste_*.sql      # Dados de teste
│
├── Worker/                     # Background service — processamento agendado
│   ├── Workers/                # SchedulerWorker (IHostedService / BackgroundService)
│   ├── Services/               # JobExecutor + ProcessamentoRegraPreventivaService
│   ├── Options/                # SchedulerOptions (configuração de cron e timezone)
│   ├── Program.cs              # Entry point do Worker
│   └── appsettings.json        # Configurações do agendador (cron, timezone SP)
│
├── Integration/                # Projeto de integração (atualmente mínimo)
│
└── Presentation/
    └── Eletromecanica/         # Aplicação Vue.js
        └── src/
            ├── main.js         # Inicialização do app Vue, plugins, segurança global
            ├── App.vue         # Componente raiz com layout
            ├── router/
            │   └── index.js    # Rotas e guards de autorização
            ├── components/     # Componentes reutilizáveis (base, common)
            ├── views/          # Páginas organizadas por domínio
            │   ├── configuracoes/    # Estação, Equipamento, Funcionário, etc.
            │   ├── ordem-servico/    # Gestão de Ordens de Serviço
            │   └── relatorios/       # Relatórios e dashboards
            ├── services/       # Chamadas HTTP organizadas por domínio
            │   ├── fetch-service.js      # Base HTTP com Bearer token + criptografia
            │   ├── storage-service.js    # Gerenciamento de localStorage
            │   ├── response.js           # Wrapper de resposta
            │   ├── configuracoes/
            │   ├── ordem-servico/
            │   ├── relatorios/
            │   └── seguranca/
            ├── composables/    # Composables Vue 3
            ├── plugins/        # Plugins Vue (Vuetify, FontAwesome)
            ├── themes/         # Configuração de temas
            └── assets/         # Imagens e estilos globais
```

---

## 3. Pontos de Entrada

### API — `API/Program.cs`
- Hosting model mínimo do ASP.NET Core 9 (sem `Startup.cs`)
- Registra DI via `services.AddDependencyInjections(builder.Configuration)` (chain: API → Business → Data)
- CORS configurado com origens dinâmicas de `appsettings.json`
- Swagger habilitado com esquema de segurança Bearer JWT
- `app.UseAuthentication()` + `app.UseAuthorization()`
- Controladores mapeados via `app.MapControllers()`

### Controllers (14 total)

| Controller | Domínio |
|---|---|
| `OrdemServicoController` | Ordens de serviço |
| `EquipamentoController` | Equipamentos |
| `EstacaoController` | Estações de bombeamento |
| `DocumentoController` | Documentos / arquivos |
| `DashboardController` | Métricas e dashboards |
| `FuncionarioController` | Funcionários |
| `CargoController` | Cargos |
| `SetorController` | Setores |
| `TipoFuncionarioController` | Tipos de funcionário |
| `MotivoCancelamentoController` | Motivos de cancelamento de OS |
| `ServicoSolicitadoController` | Serviços solicitados |
| `ServicoExecutadoController` | Serviços executados |
| `GoogleMapController` | Geolocalização |
| `BaseController` | Classe base com logging (NLog) |

### Worker — `Worker/Program.cs`
- .NET Generic Host (sem HTTP)
- Registra `SchedulerWorker` como `IHostedService`
- Executa `ProcessamentoRegraPreventivaService` via `JobExecutor` na hora configurada
- Padrão: diariamente às 07:00, timezone America/Sao_Paulo

### Frontend — `src/main.js` + `src/router/index.js`

**Rotas definidas (Vue Router):**

| Rota | Tela |
|---|---|
| `/` | Dashboard (home) |
| `/acesso-negado` | Acesso negado |
| `/estacoes` | Listagem de estações |
| `/equipamentos` | Listagem de equipamentos |
| `/equipamentos/:id` | Detalhe de equipamento |
| `/servicos-solicitados` | Serviços solicitados |
| `/servicos-executados` | Serviços executados |
| `/motivos-cancelamento` | Motivos de cancelamento |
| `/funcionario` | Funcionários |
| `/ocorrencia-tabs` | Ordens de serviço (com tabs) |
| `/nova-ocorrencia` | Nova ordem de serviço |
| `/consulta-ordem-servico` | Consulta de OS |
| `/detalhar-ordem-servico/:id` | Detalhe de OS |
| `/dashboard` | Dashboard explícito |
| `/operacao` | Operações |

**Guard de Rota (`router.beforeResolve`):**
- Lê token criptografado em `localStorage` (`loginNovoSanegeo`)
- Decripta com AES (chave via `VUE_APP_CHAVE_SEGURANCA`)
- Constrói lista de rotas permitidas a partir dos módulos/telas do usuário
- Redireciona para `/acesso-negado` se não autorizado
- Redireciona para `/` se não autenticado

---

## 4. Dependências Críticas

### Backend — NuGet

| Pacote | Versão | Propósito |
|---|---|---|
| Dapper | 2.1.66 | Micro-ORM para SQL manual |
| Microsoft.Data.SqlClient | 6.1.4 | Driver SQL Server moderno |
| System.Data.SqlClient | 4.9.1 | Driver SQL Server legado |
| Refit | 10.0.1 | HTTP client declarativo (APIs externas) |
| Cronos | 0.11.1 | Parsing de expressões cron no Worker |
| NLog + NLog.Web | 4.7.13 / 4.14.0 | Logging estruturado |
| Swashbuckle.AspNetCore | 6.2.3 | Documentação Swagger/OpenAPI |
| Microsoft.Extensions.Hosting | 9.0.7 | Generic Host para o Worker |

### Frontend — npm

| Pacote | Versão | Propósito |
|---|---|---|
| vue | 3.2.13 | Framework reativo |
| vue-router | 4.5.1 | Roteamento SPA |
| vuetify | 3.9.0 | UI components Material Design |
| chart.js | 4.5.1 | Gráficos |
| @ckeditor/ckeditor5-build-classic | 41.4.2 | Editor rich-text |
| crypto-js | 4.2.0 | Criptografia AES no cliente |
| vue3-google-map | 0.27.0 | Integração Google Maps |
| v-idle | 1.0.3 | Timeout de sessão por inatividade |
| sass | 1.89.2 | Pré-processador CSS |

---

## 5. Padrões Arquiteturais

### Layering — Clean Architecture adaptada (4 camadas)

```
┌─────────────────────────────────────────────┐
│  API  (Controllers, DTOs de request/response)│  ← HTTP boundary
├─────────────────────────────────────────────┤
│  Business  (Services, regras de negócio)     │  ← Orquestração
├─────────────────────────────────────────────┤
│  Data  (Repositories, SQL, clientes HTTP)    │  ← Persistência
├─────────────────────────────────────────────┤
│  Core  (Models, Enums, Interfaces, Utils)    │  ← Domínio puro
└─────────────────────────────────────────────┘
```

- **Core** não depende de nenhuma outra camada
- **Data** e **Business** dependem apenas de **Core**
- **API** depende de **Business** e **Core**
- Comunicação entre camadas via **interfaces** (nunca implementações concretas)

### Padrões Identificados

**Repository Pattern**
- Toda persistência fica em repositórios concretos (`Data/Repositories/`)
- Controllers e Services nunca acessam banco diretamente
- Repositórios recebem `IDbTransaction` como parâmetro opcional para transações

**Service Layer**
- Services em `Business/Services/` orquestram casos de uso completos
- Injetam múltiplos repositórios via construtor
- Gerenciam transações explícitas (`SqlTransaction`) com rollback em caso de falha

**DTO (Data Transfer Object)**
- Contratos de entrada/saída da API isolados dos modelos de domínio
- `Business/Dtos/` contém requests e dtos de resposta
- Desacopla API de modelos internos

**Aggregate Pattern (DDD)**
- Modelos organizados por agregados em `Core/Models/`:
  - `EquipmentAggregate` — Equipamento (raiz) + Bomba, Motor, CLP, Nobreak, MedidorVazao
  - `OrdemServicoAggregate` — OrdemServico (raiz) + ServicoSolicitado, ServicoExecutado
  - `DocumentAggregate`, `FuncionarioAggregate`, etc.

**Filter/Specification Pattern**
- Filtros tipados em `Core/Models/PaginateAggregate/Filters/`
- Repositórios aceitam filtros para construção dinâmica de queries
- `SqlQueryBuilder` utilitário auxilia na montagem de SQL dinâmico

**Dependency Injection com cadeia de extensões**
```
API.Extensions.AddDependencyInjections()
  └─> Business.Extensions.AddBusinessServices()
        └─> Data.Extensions.AddDataServices()
```
- Todos os services e repositories registrados como **Scoped**
- `SqlConnection` registrado como Scoped (uma conexão por request HTTP)

**Scheduled Job Pattern (Worker)**
- `SchedulerWorker` herda de `BackgroundService`
- Cronos calcula o próximo horário de execução a partir da expressão cron
- Semáforo (`SemaphoreSlim`) impede execuções concorrentes

**HTTP Client Abstraction (Refit)**
- `IGoogleMapClient` define o contrato da API externa
- Refit gera a implementação automaticamente
- Registrado no container via `AddRefitClient<T>()`

### Convenções Adicionais

- **Enums como string no JSON**: `JsonStringEnumConverter` + `CamelCaseNamingPolicy`
- **Logging**: NLog injetado via `ILogger` do NLog, métodos de log encapsulados no `BaseController`
- **Gestão de conexão**: `DbUtils.EnsureOpenAsync()` centraliza abertura segura da conexão
- **Segurança no frontend**: dados do usuário logado criptografados com AES em `localStorage`; token JWT enviado em cada request via `Authorization: Bearer`
- **Autenticação**: fluxo de login gera JWT → armazenado em `localStorage` (`tokenSeguranca`) → renovação automática em 401
