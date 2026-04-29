# Review Arquitetural — Eletromecanica

**Branch:** develop  
**Data:** 2026-04-22  
**Escopo:** Separação de camadas, acoplamento, coesão, aderência à Clean Architecture declarada, violações de SOLID e módulos candidatos a refatoração.

---

## Legenda de Severidade

| Severidade | Critério |
|---|---|
| **crítico** | Viola contratos arquiteturais fundamentais, introduz risco de segurança ou impossibilita evolução sem reescrita |
| **alto** | Cria acoplamento forte entre camadas erradas, viola SRP de forma ampla ou esconde bugs silenciosos |
| **médio** | Reduz coesão, duplica responsabilidades ou torna a camada mais difícil de testar/manter |
| **baixo** | Inconsistência de convenção, naming, redundância ou oportunidade de melhoria de legibilidade |

---

## Sumário Executivo

A arquitetura declara uma **Clean Architecture em 4 camadas** (Core → Data/Business → API). Na prática, três violações sistemáticas corroem esse contrato:

1. **Infraestrutura infiltrou o Business** — `DbConnection`, `IConfiguration`, I/O de arquivo e gerenciamento de transação aparecem diretamente nas classes de serviço, tornando a camada de negócio dependente de SQL Server e do sistema de arquivos.
2. **Core virou depósito** — modelos de leitura (read models), DTOs disfarçados de agregados e utilitários com lógica de negócio coexistem no núcleo, que deveria ser puro.
3. **Frontend não tem fronteira de estado** — autenticação, autorização, requisições HTTP e estado de UI estão distribuídos em router, fetch-service, storage-service e nos próprios componentes sem camada centralizadora.

---

## Achados

---

### [CRÍTICO-01] `DbConnection` injetado diretamente nos Services

**Arquivo(s):**
- `Business/Services/OrdemServicoService.cs:6`
- `Business/Services/DocumentoService.cs:6`
- `Worker/Services/ProcessamentoRegraPreventivaService.cs:10`

**Descrição:**  
A camada Business recebe `System.Data.SqlClient.SqlConnection` (infraestrutura de dados) via construtor. Isso inverte a regra de dependência da Clean Architecture: Business passa a depender de detalhes de persistência, não de abstrações. Qualquer troca de banco, mock em teste ou migração para repositório HTTP quebra todos esses services.

```csharp
// Business/Services/OrdemServicoService.cs:6
public OrdemServicoService(DbConnection connection, IConfiguration configuration, ...)
```

**Causa-raiz:** gerenciamento de transação manual. Os services abrem `transaction = connection.BeginTransaction()` porque não existe Unit of Work.

**Impacto:** impossibilita testes unitários dos services sem banco real; viola DIP (Dependency Inversion Principle).

---

### [CRÍTICO-02] Gerenciamento de transação espalhado por todas as camadas

**Arquivo(s):**
- `Business/Services/OrdemServicoService.cs:16–23`
- `Business/Services/DocumentoService.cs:136–154` e `168–181` e `191–216`
- `Worker/Services/ProcessamentoRegraPreventivaService.cs:48–124`
- `Business/Interfaces/IOrdemServicoService.cs:6` — `IDbTransaction` exposto em interface de negócio

**Descrição:**  
`IDbTransaction` aparece como parâmetro em interfaces de serviço e em repositórios, propagando um detalhe de infraestrutura por toda a cadeia de chamadas. Cada serviço cria, propaga e faz rollback da transação manualmente, duplicando o padrão try/catch/rollback em pelo menos 6 lugares.

```csharp
// Business/Interfaces/IOrdemServicoService.cs:6
Task<Guid> AddAsync(OrdemServico ordemServico, IDbTransaction? transaction = null);
```

**Impacto:** qualquer alteração na estratégia transacional exige tocar API de negócio; rollback inconsistente pode causar dados parcialmente gravados.

---

### [CRÍTICO-03] Lógica de I/O de arquivo e criptografia na camada Business

**Arquivo(s):**
- `Business/Services/DocumentoService.cs:38–60` — `Directory.CreateDirectory`, `File.Create`, SHA-256
- `API/Controllers/DocumentoController.cs:28–43` e `52–56` — checagem de `File.Exists`, leitura de path
- `API/Controllers/DocumentoController.cs:5` — `IOptions<DocumentStorageOptions>` injetado no controller

**Descrição:**  
`DocumentoService` cria diretórios, grava arquivos, calcula hash SHA-256 e gerencia caminhos de disco — tudo dentro da camada de negócio. O controller complementa verificando existência de arquivo diretamente. Isso acopla Business e API ao sistema de arquivos local, impedindo, por exemplo, migrar armazenamento para S3/Azure Blob sem reescrever a service.

```csharp
// Business/Services/DocumentoService.cs:42
Directory.CreateDirectory(rootPath);

// Business/Services/DocumentoService.cs:51
using var sha256 = SHA256.Create();
```

**Impacto:** Business não é testável sem sistema de arquivos; violação clara de SRP e DIP.

---

### [CRÍTICO-04] Dados de teste com JWT hardcoded em `main.js` de produção

**Arquivo(s):**
- `Presentation/Eletromecanica/src/main.js:60–470`

**Descrição:**  
O arquivo de inicialização do app Vue contém um bloco de ~410 linhas que cria um usuário de teste, monta um objeto de login completo e armazena um token JWT criptografado em `localStorage`. Esse código executa em produção.

```js
// main.js:65 (aproximado)
const usuarioTeste = { id: '<uuid-fixo>', nome: 'Teste', ... }
// ...
localStorage.setItem('loginNovoSanegeo', CryptoJS.AES.encrypt(...))
```

**Impacto:** risco de segurança direto — qualquer usuário que inspecione o bundle tem acesso ao token de teste; dados de fixture poluem localStorage real de usuários; dificulta identificar comportamento legítimo nos logs.

---

### [CRÍTICO-05] Domínio recebido como contrato HTTP no `OrdemServicoController`

**Arquivo(s):**
- `API/Controllers/OrdemServicoController.cs:10` — `Post([FromBody] OrdemServico ordemServico)`
- `API/Controllers/OrdemServicoController.cs:29` — `UpdateAsync([FromBody] OrdemServico ordemServico)`
- `API/Controllers/OrdemServicoController.cs:17` — retorno direto do modelo de domínio

**Descrição:**  
O controller recebe e devolve `OrdemServico` (modelo do Core) como contrato de API. Qualquer mudança interna no agregado altera automaticamente o contrato público da API sem revisão consciente. DTOs existem justamente para isolar esse contrato.

**Impacto:** breaking change silencioso a cada refatoração de domínio; expõe campos internos (FKs, IDs técnicos) desnecessariamente.

---

### [ALTO-01] `InjectionService` registrado duas vezes no container DI

**Arquivo(s):**
- `Business/Extensions/ServiceExtensions.cs:19` e `23` — `IFuncionarioService` registrado como Scoped duas vezes
- `Data/Extensions/ServiceExtension.cs:19` e `29` — mesmo serviço registrado novamente

**Descrição:**  
O Microsoft DI container usa o último registro quando há duplicatas de uma interface. O registro duplicado indica que a cadeia de extensão (`API → Business → Data`) não tem controle de quais registros cada camada faz, gerando sobreposições silenciosas.

**Impacto:** comportamento imprevisível se as duas instâncias forem configuradas de forma diferente; registros duplicados de Scoped desperdiçam memória.

---

### [ALTO-02] `ProcessamentoRegraPreventivaService` com 9 dependências — violação grave de SRP

**Arquivo(s):**
- `Worker/Services/ProcessamentoRegraPreventivaService.cs:3–11`

**Descrição:**  
O construtor recebe `IRegraPreventivaRepository`, `IOrdemServicoRepository`, `IServicoSolicitadoRepository`, `IEstacaoRepository`, `IRegraPrevativaService`, `IOrdemServicoService`, `DbConnection`, `ILogger` — 9 dependências. A classe executa 5 operações distintas: analisar execuções, criar OS preventivas, criar novas regras, persistir tudo em transação e logar.

```csharp
// Worker/Services/ProcessamentoRegraPreventivaService.cs:3–11
public ProcessamentoRegraPreventivaService(
    IRegraPreventivaRepository regraPreventivaRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IServicoSolicitadoRepository servicoSolicitadoRepository,
    IEstacaoRepository estacaoRepository,
    IRegraPrevativaService regraPrevativaService,
    IOrdemServicoService ordemServicoService,
    DbConnection connection,
    ILogger logger)
```

**Impacto:** impossível testar cada responsabilidade isoladamente; alta probabilidade de side-effects entre as responsabilidades.

---

### [ALTO-03] `[FromServices]` em parâmetros de action — antipadrão de DI

**Arquivo(s):**
- `API/Controllers/EquipamentoController.cs:156` — `GetAllByEquipamentoIdAsync([FromServices] IComplementoEquipamentoService ...)`
- `API/Controllers/EquipamentoController.cs:173` — `Post([FromServices] IRegraPreventivaService ...)`
- `API/Controllers/EquipamentoController.cs:195` — `Put([FromServices] IRegraPreventivaService ...)`
- `API/Controllers/EquipamentoController.cs:217` — `Delete([FromServices] IRegraPreventivaService ...)`

**Descrição:**  
`[FromServices]` injeta serviços diretamente em parâmetros de action em vez de pelo construtor. Isso torna as dependências do controller invisíveis, dificulta testes (não há como injetar mocks via construtor) e viola o princípio de que o construtor deve declarar todas as dependências.

**Impacto:** testes precisam configurar o ServiceProvider inteiro; IDE não consegue rastrear o uso do serviço normalmente.

---

### [ALTO-04] `JobExecutor` acessa repositório diretamente — bypassa a service layer

**Arquivo(s):**
- `Worker/Services/JobExecutor.cs:3–5` e `7–30`

**Descrição:**  
O `JobExecutor` injeta `IRegraPreventivaRepository` e o consulta diretamente para obter as regras a processar, sem passar pela service layer. Isso cria um segundo caminho de acesso a dados que não segue as mesmas regras de negócio e logging que a service layer garantiria.

```csharp
// Worker/Services/JobExecutor.cs:3
public JobExecutor(IRegraPreventivaRepository regraPreventivaRepository, ...)
```

---

### [ALTO-05] Autenticação e autorização dispersas no frontend sem camada dedicada

**Arquivo(s):**
- `Presentation/Eletromecanica/src/router/index.js:90–138` — lógica de auth no guard de rota
- `Presentation/Eletromecanica/src/services/fetch-service.js:17–31` e `71–79` — decrypt de login e refresh de token no cliente HTTP
- `Presentation/Eletromecanica/src/services/storage-service.js:4–11` — validação de token como base de serviço

**Descrição:**  
Não existe um `AuthService` dedicado. A lógica de autenticação está fragmentada: descriptografar o login acontece no guard do router (linha 100), no fetch-service (linha 28) e em qualquer componente que precise verificar permissões. O refresh automático de token vive dentro do cliente HTTP.

```js
// router/index.js:100
const loginDecriptado = CryptoJS.AES.decrypt(localStorage.getItem('loginNovoSanegeo'), ...)

// fetch-service.js:28
const loginDecriptado = CryptoJS.AES.decrypt(...)
```

**Impacto:** mudança na estratégia de auth exige tocar router, fetch-service e storage-service; lógica duplicada aumenta chance de inconsistência.

---

### [ALTO-06] `ILogger` com tipo errado no `OrdemServicoController`

**Arquivo(s):**
- `API/Controllers/OrdemServicoController.cs:7`

**Descrição:**  
O construtor declara `ILogger<EstacaoController>` em vez de `ILogger<OrdemServicoController>`. Todos os logs desse controller aparecem categorizados sob `EstacaoController`, tornando o rastreamento de erros em produção enganoso.

```csharp
// API/Controllers/OrdemServicoController.cs:7
public OrdemServicoController(IOrdemServicoService service, ILogger<EstacaoController> logger)
```

---

### [ALTO-07] `OrdemServicoList` é um DTO vivendo no Core como modelo de domínio

**Arquivo(s):**
- `Core/Models/OrdemServicoAggregate/OrdemServicoList.cs:1–58`

**Descrição:**  
A classe contém propriedades de apresentação formatadas, computed properties que constroem strings de exibição e cálculos de negócio (linhas 26, 28, 57). Trata-se de um read model / DTO de listagem disfarçado de entidade de domínio. O Core não deve ter conhecimento de formatação de UI.

```csharp
// Core/Models/OrdemServicoAggregate/OrdemServicoList.cs:34 (aproximado)
public string DataAberturaFormatada => DataAbertura.ToString("dd/MM/yyyy");
```

**Impacto:** o Core passa a depender indiretamente de decisões de apresentação; qualquer mudança de formato exige alterar o coração do domínio.

---

### [MÉDIO-01] `IOrdemServicoRepository` com 18 métodos — Fat Interface

**Arquivo(s):**
- `Data/Interfaces/IOrdemServicoRepository.cs:1–22`

**Descrição:**  
A interface acumula operações de leitura paginada, geolocalização, mudanças de estado de domínio (`CancelarOrdemServicoAsync`, `IniciarOrdemServicoAsync`, `DespacharOrdemServicoAsync`) e utilitários de sequência. Viola ISP (Interface Segregation Principle): clientes que precisam apenas ler OS precisam depender de toda a interface.

**Sugestão de divisão:**
- `IOrdemServicoReadRepository` — consultas e paginação
- `IOrdemServicoWriteRepository` — persistência e transições de estado
- `IOrdemServicoGeoRepository` — operações de geolocalização

---

### [MÉDIO-02] `IDocumentoService` mistura documentos e tags — violação de ISP

**Arquivo(s):**
- `Business/Interfaces/IDocumentoService.cs:1–15`
- `Data/Interfaces/IDocumentoRepository.cs:1–17`

**Descrição:**  
Ambas as interfaces agrupam operações de documento (CRUD, upload, download) com operações de tag (listar tags, setar tags, buscar). Tags são uma responsabilidade separada e poderiam evoluir independentemente.

---

### [MÉDIO-03] `DocumentoDto` com operadores de conversão implícita e lógica de apresentação

**Arquivo(s):**
- `Business/Dtos/DocumentoDto.cs:11` — propriedade `DataCriacaoFormatada`
- `Business/Dtos/DocumentoDto.cs:18–51` — operadores `implicit` para/de domínio

**Descrição:**  
Um DTO não deveria conter lógica de formatação (`DataCriacaoFormatada`) nem operadores de conversão implícita com o modelo de domínio. Conversão implícita torna o fluxo de dados opaco e pode causar conversões não intencionais silenciosas.

```csharp
// Business/Dtos/DocumentoDto.cs:18
public static implicit operator DocumentoDto(Documento documento) => new() { ... };
```

---

### [MÉDIO-04] `DbUtils.MontarBase` contém lógica de negócio na camada de dados

**Arquivo(s):**
- `Data/Utils/DbUtils.cs:11–30`

**Descrição:**  
O método `MontarBase()` usa `Constantes.OrdemServicoStatusFinalizada` e `Constantes.OrdemServicoStatusCancelada` para calcular intervalos de datas e construir filtros de query. Isso é regra de negócio (quais status são "finalizados") embutida em utilitário de dados.

```csharp
// Data/Utils/DbUtils.cs:13–14
var statusFinalizados = new[] {
    Constantes.OrdemServicoStatusFinalizada,
    Constantes.OrdemServicoStatusCancelada
};
```

---

### [MÉDIO-05] `SetTagsAsync` com padrão N+1 — DELETE + INSERT em loop

**Arquivo(s):**
- `Data/Repositories/DocumentoRepository.cs:211–223`

**Descrição:**  
A operação de salvar tags faz um `DELETE` geral e depois itera sobre a coleção fazendo um `INSERT` por tag. Para documentos com muitas tags isso gera N round-trips ao banco.

```csharp
// Data/Repositories/DocumentoRepository.cs:215–223
await connection.ExecuteAsync("DELETE FROM DocumentoTag WHERE DocumentoId = @id", ...);
foreach (var tagId in tagIds)
{
    await connection.ExecuteAsync("INSERT INTO DocumentoTag ...", ...);
}
```

**Solução:** usar `INSERT INTO ... VALUES (...),(...),...` ou `SqlBulkCopy`.

---

### [MÉDIO-06] `IConfiguration` injetado diretamente no `OrdemServicoService`

**Arquivo(s):**
- `Business/Services/OrdemServicoService.cs:7`

**Descrição:**  
`IConfiguration` expõe toda a árvore de configuração da aplicação a um serviço de negócio. O correto é criar uma classe `Options` tipada e injetá-la via `IOptions<T>`, restringindo o serviço apenas às configurações que ele precisa.

---

### [MÉDIO-07] `ListaOrdemServico.vue` com 18+ refs locais e instanciação direta de services

**Arquivo(s):**
- `Presentation/Eletromecanica/src/views/ListaOrdemServico.vue:20–39` — `new OrdemServicoService(...)` diretamente no componente
- `Presentation/Eletromecanica/src/views/ListaOrdemServico.vue:60–107` — 18+ `ref()` locais
- `Presentation/Eletromecanica/src/views/ListaOrdemServico.vue:124–150+` — lógica de menu e transição de estado no componente

**Descrição:**  
O componente instancia serviços diretamente (acoplamento forte a implementações concretas), gerencia estado complexo com refs locais sem store centralizado e contém lógica de quais ações estão disponíveis por status — que é regra de negócio, não de UI.

**Impacto:** impossível testar o componente sem instanciar o serviço HTTP real; estado distribuído dificulta debugging.

---

### [MÉDIO-08] `EquipamentoController` mistura dois domínios distintos

**Arquivo(s):**
- `API/Controllers/EquipamentoController.cs:172–236` — endpoints de `RegraPreventivaEquipamento`
- `API/Controllers/EquipamentoController.cs:1–170` — endpoints de `Equipamento`

**Descrição:**  
Regras preventivas são um agregado distinto (existem no Worker, têm repositório próprio, têm service próprio). Concentrar seus endpoints no `EquipamentoController` viola SRP e cria um controller com dois contextos delimitados diferentes.

**Sugestão:** mover os endpoints de regra preventiva para `RegraPreventivaController`.

---

### [MÉDIO-09] `API/Extensions/ServiceExtensions.cs` é uma passagem sem valor

**Arquivo(s):**
- `API/Extensions/ServiceExtensions.cs:1–9`

**Descrição:**  
O arquivo inteiro é:

```csharp
public static void AddDependencyInjections(this IServiceCollection services, IConfiguration config)
    => services.AddBusinessServices(config);
```

Adiciona uma camada de indireção sem encapsulamento algum. Se a API precisar registrar serviços próprios futuramente, o método já existe — mas atualmente não acrescenta nada.

---

### [BAIXO-01] Typo em método de interface pública

**Arquivo(s):**
- `Data/Interfaces/IOrdemServicoRepository.cs` — `GetNextNumberOSAynsc` (deveria ser `Async`)

**Impacto:** baixo, mas integra o contrato público da interface — corrigir exige atualizar todos os implementadores e chamadores.

---

### [BAIXO-02] `IRepository<T>` base com `UpdateStatusAsync` genérico demais

**Arquivo(s):**
- `Core/Interfaces/Repositories/IRepository.cs`

**Descrição:**  
A interface base genérica expõe `UpdateStatusAsync`, mas nem todos os agregados têm o conceito de "status". Isso força repositórios de entidades sem status a implementar um método que não faz sentido para elas.

---

### [BAIXO-03] Dois clientes SQL mantidos em paralelo

**Arquivo(s):**
- `API/API.csproj` — `System.Data.SqlClient 4.9.1` (legado, descontinuado)
- `Data/Data.csproj` — `Microsoft.Data.SqlClient 6.1.4` (atual)

**Descrição:**  
Ambos coexistem. `System.Data.SqlClient` foi descontinuado pela Microsoft. O projeto Worker ainda usa o pacote legado. Deve-se migrar tudo para `Microsoft.Data.SqlClient`.

---

### [BAIXO-04] Três bibliotecas de ícones carregadas simultaneamente

**Arquivo(s):**
- `Presentation/Eletromecanica/package.json` — `@mdi/font`, `@mdi/js`, `@fortawesome/fontawesome-free`

**Descrição:**  
MDI Font (CSS) e MDI JS são distribuições diferentes do mesmo conjunto de ícones Material Design; carregar ambas duplica o bundle sem necessidade. FontAwesome adiciona uma terceira família. Consolidar em uma única fonte de ícones reduz o bundle.

---

### [BAIXO-05] `IFormFile` em DTO da camada Business

**Arquivo(s):**
- `Business/Dtos/AdicionarDocumentoRequest.cs:15`

**Descrição:**  
`IFormFile` é um tipo do namespace `Microsoft.AspNetCore.Http`, que é infraestrutura ASP.NET Core. Ao usá-lo em um DTO da camada Business, o projeto Business passa a referenciar o framework web — o que não é ideal para uma camada que deveria ser independente de transporte.

---

### [BAIXO-06] Dois editores rich-text no bundle frontend

**Arquivo(s):**
- `Presentation/Eletromecanica/package.json` — `@ckeditor/ckeditor5-build-classic 41.4.2` e `@vueup/vue-quill 1.2.0`

**Descrição:**  
CKEditor e Quill são bibliotecas pesadas com funcionalidades sobrepostas. Se as duas são efetivamente usadas em telas diferentes, pode ser intencional — mas deve ser verificado se não é um caso de adição incremental sem remoção do anterior.

---

## Mapa de Prioridades

| # | Severidade | Achado | Arquivo Principal |
|---|---|---|---|
| 01 | **crítico** | `DbConnection` em services de negócio | `Business/Services/OrdemServicoService.cs:6` |
| 02 | **crítico** | `IDbTransaction` em interfaces de negócio | `Business/Interfaces/IOrdemServicoService.cs:6` |
| 03 | **crítico** | I/O de arquivo e crypto em `DocumentoService` | `Business/Services/DocumentoService.cs:38–60` |
| 04 | **crítico** | JWT de teste hardcoded em `main.js` produção | `src/main.js:60–470` |
| 05 | **crítico** | Modelo de domínio como contrato HTTP | `API/Controllers/OrdemServicoController.cs:10` |
| 06 | **alto** | `IFuncionarioService` registrado duas vezes no DI | `Business/Extensions/ServiceExtensions.cs:19,23` |
| 07 | **alto** | `ProcessamentoRegraPreventivaService` com 9 deps | `Worker/Services/ProcessamentoRegraPreventivaService.cs:3` |
| 08 | **alto** | `[FromServices]` em parâmetros de action | `API/Controllers/EquipamentoController.cs:156,173,195,217` |
| 09 | **alto** | `JobExecutor` acessa repositório diretamente | `Worker/Services/JobExecutor.cs:3` |
| 10 | **alto** | Auth dispersa sem `AuthService` no frontend | `src/router/index.js:90` + `fetch-service.js:28` |
| 11 | **alto** | `ILogger<EstacaoController>` em `OrdemServicoController` | `API/Controllers/OrdemServicoController.cs:7` |
| 12 | **alto** | `OrdemServicoList` como DTO no Core | `Core/Models/OrdemServicoAggregate/OrdemServicoList.cs:1` |
| 13 | **médio** | Fat Interface `IOrdemServicoRepository` (18 métodos) | `Data/Interfaces/IOrdemServicoRepository.cs:1` |
| 14 | **médio** | `IDocumentoService` mistura documentos e tags | `Business/Interfaces/IDocumentoService.cs:1` |
| 15 | **médio** | Conversão implícita e formatação em `DocumentoDto` | `Business/Dtos/DocumentoDto.cs:11,18` |
| 16 | **médio** | Regra de negócio em `DbUtils.MontarBase` | `Data/Utils/DbUtils.cs:11` |
| 17 | **médio** | N+1 em `SetTagsAsync` | `Data/Repositories/DocumentoRepository.cs:215` |
| 18 | **médio** | `IConfiguration` em service de negócio | `Business/Services/OrdemServicoService.cs:7` |
| 19 | **médio** | `ListaOrdemServico.vue` instancia services e gerencia 18+ refs | `src/views/ListaOrdemServico.vue:20` |
| 20 | **médio** | `EquipamentoController` com dois domínios | `API/Controllers/EquipamentoController.cs:172` |
| 21 | **médio** | `API/Extensions/ServiceExtensions.cs` passagem sem valor | `API/Extensions/ServiceExtensions.cs:1` |
| 22 | **baixo** | Typo `GetNextNumberOSAynsc` | `Data/Interfaces/IOrdemServicoRepository.cs` |
| 23 | **baixo** | `IRepository<T>.UpdateStatusAsync` genérico demais | `Core/Interfaces/Repositories/IRepository.cs` |
| 24 | **baixo** | Dois drivers SQL em paralelo | `API.csproj` + `Worker.csproj` |
| 25 | **baixo** | Três bibliotecas de ícones no bundle | `package.json` |
| 26 | **baixo** | `IFormFile` em DTO do Business | `Business/Dtos/AdicionarDocumentoRequest.cs:15` |
| 27 | **baixo** | Dois editores rich-text (CKEditor + Quill) | `package.json` |
