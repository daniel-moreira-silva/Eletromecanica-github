# CLAUDE.md — Eletromecanica

## Project Overview
Maintenance management system (gestão de Ordens de Serviço) for pumping stations. Two deployable processes: **API** (ASP.NET Core 9) and **Worker** (background scheduler). One SPA frontend in Vue 3.

---

## Architecture — 4-Layer Clean Architecture

```
API  →  Business  →  Data  →  Core (no dependencies)
```

- **Core** (`Core/`): domain models, enums, interfaces, utils — never import other layers
- **Business** (`Business/`): services, DTOs, business rules — depends only on Core
- **Data** (`Data/`): repositories, SQL queries, external HTTP clients — depends only on Core
- **API** (`API/`): controllers, HTTP boundary — depends on Business + Core
- **Worker** (`Worker/`): hosted background service — depends on Business + Core

DI chain: `API.Extensions → Business.Extensions → Data.Extensions` — all Scoped.

---

## Key Conventions

### Backend
- **No EF Core.** All SQL via **Dapper** (raw SQL). Never introduce an ORM.
- **Repository pattern** — controllers/services never touch `SqlConnection` directly.
- Repositories accept optional `IDbTransaction` for explicit transactions.
- Services manage `SqlTransaction` with try/catch rollback.
- `DbUtils.EnsureOpenAsync()` centralizes safe connection opening.
- `SqlQueryBuilder` (in `Data/Utils/`) builds dynamic WHERE clauses.
- Enums serialized as camelCase strings (`JsonStringEnumConverter` + `CamelCaseNamingPolicy`).
- Logging via **NLog** (`ILogger` from NLog); helper methods on `BaseController`.
- One `SqlConnection` per request (Scoped).

### DTOs
- All API contracts in `Business/Dtos/` — separate `*Request` (input) from `*Dto` (output).
- Domain models (`Core/Models/`) never leak out of the API layer.

### Frontend
- Vue 3 + Vuetify 3 (Material Design).
- HTTP calls in `Presentation/Eletromecanica/src/services/` — organized by domain subfolder.
- `fetch-service.js` — base HTTP wrapper with Bearer JWT + AES encryption.
- `storage-service.js` — localStorage helper (encrypted user data via `crypto-js`).
- Auth: JWT → `localStorage` key `tokenSeguranca`; user data under `loginNovoSanegeo` (AES, key from `VUE_APP_CHAVE_SEGURANCA`).
- Route guard in `router/index.js` (`router.beforeResolve`) — decrypts user modules and redirects to `/acesso-negado` if route not permitted.

### Worker
- `SchedulerWorker` extends `BackgroundService`; uses **Cronos** to parse cron expressions.
- `SemaphoreSlim` prevents concurrent job runs.
- Default schedule: daily at 07:00, timezone `America/Sao_Paulo`.
- Entry point: `Worker/Program.cs` (Generic Host, no HTTP).

---

## File Map — Where to Find Things

| What | Where |
|---|---|
| Controllers (14) | `API/Controllers/` |
| Services (14) | `Business/Services/` |
| Service interfaces | `Business/Interfaces/` |
| DTOs (request/response) | `Business/Dtos/` |
| Domain models (aggregates) | `Core/Models/` |
| Enums | `Core/Models/Enums/` |
| Repositories (20) | `Data/Repositories/` |
| Repository interfaces | `Data/Interfaces/` |
| SQL scripts (schema + seed) | `Data/Scripts/` |
| External HTTP clients (Refit) | `Data/Clients/` |
| SQL utilities | `Data/Utils/` |
| Vue pages | `Presentation/Eletromecanica/src/views/` |
| Vue services (HTTP) | `Presentation/Eletromecanica/src/services/` |
| Vue components | `Presentation/Eletromecanica/src/components/` |
| Vue routes | `Presentation/Eletromecanica/src/router/index.js` |
| API config / conn strings | `API/appsettings.json` |
| Worker config / cron | `Worker/appsettings.json` |

---

## Domain Aggregates (Core)

| Aggregate | Root | Children |
|---|---|---|
| `EquipmentAggregate` | `Equipamento` | Bomba, Motor, CLP, Nobreak, MedidorVazao, RegraPreventiva |
| `OrdemServicoAggregate` | `OrdemServico` | ServicoSolicitado, ServicoExecutado, OrdemServicoList |
| `DocumentAggregate` | `Documento` | — |
| `FuncionarioAggregate` | `Funcionario` | Cargo, Setor, TipoFuncionario |
| `DashboardAggregate` | metrics models | — |
| `PaginateAggregate` | filter models | Filters/ subfolder |

---

## Controllers Quick Reference

`OrdemServico`, `Equipamento`, `Estacao`, `Documento`, `Dashboard`, `Funcionario`, `Cargo`, `Setor`, `TipoFuncionario`, `MotivoCancelamento`, `ServicoSolicitado`, `ServicoExecutado`, `GoogleMap`, `BaseController` (base class with NLog).

---

## Frontend Routes

`/` dashboard · `/estacoes` · `/equipamentos` · `/equipamentos/:id` · `/servicos-solicitados` · `/servicos-executados` · `/motivos-cancelamento` · `/funcionario` · `/ocorrencia-tabs` · `/nova-ocorrencia` · `/consulta-ordem-servico` · `/detalhar-ordem-servico/:id` · `/dashboard` · `/operacao` · `/acesso-negado`

---

## Commands

| Task | Command |
|---|---|
| Run API | `dotnet run --project API` |
| Run Worker | `dotnet run --project Worker` |
| Run Frontend | `cd Presentation/Eletromecanica && npm run serve` |
| Build Frontend | `cd Presentation/Eletromecanica && npm run build` |
| Lint Frontend | `cd Presentation/Eletromecanica && npm run lint` |

---

## OrdemServico — Status Lifecycle

Status is stored as `StatusId` (Guid FK to a status table) + `Status` (string label). There is no C# status enum — statuses are DB-driven.

**Service operations that change status:**

| Operation | Method |
|---|---|
| Create | `AddAsync` |
| Update (general fields) | `UpdateAsync` |
| Dispatch | `DespacharOrdemServicoAsync(id, funcionarioId, dataDespachoProgramado)` |
| Start execution | `IniciarOrdemServicoAsync(id, funcionarioId)` |
| Return / devolution | `DevolverOrdemServicoAsync(id, observacaoDevolucao)` |
| Cancel | `CancelarOrdemServicoAsync(id, motivoCancelamentoId, observacao)` |
| Update priority | `AtualizarPrioridadeAsync(id, EPrioridade)` |

**Sub-OS:** An OS can have children (`SubOS`, `OrdemServicoPaiId`). Use `GetSubOsListAsync` to list them.

---

## EOrdemServico Enum

`EOrdemServico` is **not** a status enum — it is a column-selector enum used by `SqlQueryBuilder` for dynamic ORDER BY / filter clauses. Status is always a Guid FK.

---

## EPrioridade

`EPrioridade` (Baixa / Media / Alta / Critica) replaced the deleted `EPrioridadeOS`. Always use `EPrioridade`.

---

## Frontend HTTP Layer

- `fetch-service.js` handles all HTTP globally: injects `Bearer` token + `Usuario` header (from AES-decrypted localStorage).
- On `401` with `erroToken`, auto-refreshes the token and retries once.
- On network error, returns `statusCode: 400` with a generic message.
- Supports `FormData` (auto-removes `Content-Type`) and blob responses.
- Each domain service extends or composes `FetchService` — never call `fetch()` directly.

---

## Tech Stack Summary

**Backend:** .NET 9 / ASP.NET Core 9 / C# / Dapper / SQL Server (SQLEXPRESS local, Azure SQL prod) / NLog / Swagger / Refit / Cronos

**Frontend:** Vue 3.2 / Vue Router 4 / Vuetify 3.9 / Chart.js / CKEditor 5 / Quill / crypto-js / vue3-google-map / Sass

---

## Database
- Schema created by `Data/Scripts/00_script_inicial.sql`
- Seed data in `Data/Scripts/carga.sql`
- No migrations framework — schema changes go directly in SQL scripts
