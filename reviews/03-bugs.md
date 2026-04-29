# Review de Bugs — branch develop

---

## [ALTO] Arquivo físico deletado antes da transação DB confirmar

**Arquivo:** `Business/Services/DocumentoService.cs:142-150`

**Problema:** `DeleteByIdAsync` apaga o arquivo do disco _antes_ de executar o `DELETE` no banco. Se o delete do DB falhar, o `RollbackAsync` desfaz o registro mas o arquivo já não existe mais no storage — estado inconsistente irrecuperável.

**Código atual:**
```csharp
if (File.Exists(fullPath))
    File.Delete(fullPath);           // ← arquivo deletado primeiro

var result = await documentoRepository.DeleteByIdAsync(id, transaction, cancellationToken);

if (result)
{
    await transaction.CommitAsync(cancellationToken);
    return true;
}

await transaction.RollbackAsync(cancellationToken); // arquivo já sumiu
```

**Correção sugerida:**
```csharp
var result = await documentoRepository.DeleteByIdAsync(id, transaction, cancellationToken);

if (!result)
{
    await transaction.RollbackAsync(cancellationToken);
    return false;
}

await transaction.CommitAsync(cancellationToken);

// deleta físico somente após commit
if (File.Exists(fullPath))
    File.Delete(fullPath);

return true;
```

**Justificativa:** Operações de arquivo não são transacionais. A única ordem segura é: commit do DB → delete do arquivo. Se o delete físico falhar após o commit, o arquivo fica órfão (aceitável), mas o registro continua correto. A ordem atual pode deixar o banco com um registro que aponta para um arquivo inexistente.

**Esforço:** baixo

---

## [ALTO] Endpoints PATCH retornam `ErrorMessage` em caso de sucesso

**Arquivo:** `API/Controllers/OrdemServicoController.cs:132,150,170,189`

**Problema:** Os quatro endpoints de transição de status (`iniciar`, `devolver`, `despachar`, `cancelar`) retornam `Ok(new ErrorMessage(...))` no caminho de sucesso. O cliente recebe HTTP 200 mas o corpo do JSON sugere erro, o que quebra qualquer consumidor que inspecione o tipo da resposta.

**Código atual:**
```csharp
// linha 132 (iniciar)
return Ok(new ErrorMessage("Ordem de serviço iniciada com sucesso.", iniciar.OrdemServicoId));

// linha 150 (devolver)
return Ok(new ErrorMessage("Ordem de serviço devolvida com sucesso.", devolver.OrdemServicoId));

// linha 170 (despachar)
return Ok(new ErrorMessage("Ordem de serviço despachada com sucesso.", despachar.OrdemServicoId));

// linha 189 (cancelar)
return Ok(new ErrorMessage("Ordem de serviço cancelada com sucesso.", cancelamento.OrdemServicoId));
```

**Correção sugerida:**
```csharp
return Ok(new SuccessMessage("Ordem de serviço iniciada com sucesso.", iniciar.OrdemServicoId));
// idem para devolver, despachar e cancelar
```

**Justificativa:** O padrão do restante do controller (e.g. `Post`, `UpdateAsync`) usa `SuccessMessage` para 200 OK. Usar `ErrorMessage` em respostas de sucesso é semânticamente errado e pode fazer o frontend (ou logs) tratar operações bem-sucedidas como falha.

**Esforço:** baixo

---

## [MÉDIO] Ordenação por `Codigo` usa coluna `DataSolicitacao` — switch errado

**Arquivo:** `Data/Repositories/OrdemServicoRepository.cs:191`

**Problema:** No switch de `orderBy` de `PaginatedGetAsync`, o case `EOrdemServico.Codigo` está mapeado para `"OS.DataSolicitacao"` em vez de `"OS.Codigo"`. Isso faz com que ordenar por "Código" produza o mesmo resultado de ordenar por "Data de Solicitação". O filtro padrão em `ListaOrdemServico.vue` usa `ordenarPor: 'Codigo'`, então _todas_ as listagens estão ordenando por data, não por código.

**Código atual:**
```csharp
EOrdemServico.Numero => "OS.Numero",
EOrdemServico.Codigo => "OS.DataSolicitacao",   // ← errado
// ...
EOrdemServico.DataSolicitacao => "OS.DataSolicitacao",
```

**Correção sugerida:**
```csharp
EOrdemServico.Codigo => "OS.Codigo",
```

**Justificativa:** `Codigo` e `DataSolicitacao` ficam com o mesmo target — o valor do enum `Codigo` nunca ordena pelo campo correto.

**Esforço:** baixo

---

## [MÉDIO] Concatenação de `Observacao` produz NULL quando campo é NULL no banco

**Arquivo:** `Data/Repositories/OrdemServicoRepository.cs:398-402`

**Problema:** O SQL de cancelamento usa `Observacao + '<BR><BR>' + @Observacao` no branch `ELSE`. Em SQL Server, qualquer concatenação com `NULL` retorna `NULL`. Se a `Observacao` da OS for `NULL`, o resultado final também será `NULL` — a observação do cancelamento é perdida silenciosamente.

**Código atual:**
```sql
Observacao = CASE WHEN OBSERVACAO LIKE '' THEN @Observacao
             ELSE Observacao + '<BR><BR>' + @Observacao END
```

**Correção sugerida:**
```sql
Observacao = CASE
    WHEN OBSERVACAO IS NULL OR OBSERVACAO = '' THEN @Observacao
    ELSE Observacao + '<BR><BR>' + @Observacao
END
```

**Justificativa:** `NULL LIKE ''` avalia como `NULL` (não `TRUE`), portanto o branch `ELSE` é executado e `NULL + '<BR><BR>' + @Observacao` → `NULL`. A condição deve verificar explicitamente `IS NULL`.

**Esforço:** baixo

---

## [MÉDIO] `using` síncrono em transação assíncrona em `OrdemServicoService`

**Arquivo:** `Business/Services/OrdemServicoService.cs:17`

**Problema:** `AddAsync` usa `using var transaction` (dispose síncrono) enquanto `DocumentoService` e o próprio `OrdemServicoService` em `DeleteByIdAsync` usam `await using`. `DbTransaction` implementa `IAsyncDisposable`; usar `Dispose()` síncrono em vez de `DisposeAsync()` pode bloquear a thread em providers que fazem cleanup assíncrono (ex: Npgsql/SQL Server em modo async).

**Código atual:**
```csharp
using var transaction = await connection.BeginTransactionAsync(cancellationToken);
```

**Correção sugerida:**
```csharp
await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
```

**Justificativa:** Consistência com o restante do codebase e respeito ao contrato `IAsyncDisposable` de `DbTransaction`. Evita bloqueio de thread em providers que realizam I/O durante o dispose.

**Esforço:** baixo

---

## [BAIXO] Mistura de `DateTime.Now` e `DateTime.UtcNow` na criação da OS

**Arquivo:** `Business/Services/OrdemServicoService.cs:45,48` e `Data/Repositories/OrdemServicoRepository.cs:248`

**Problema:** `CreateOrdemServicoAsync` seta `DataSolicitacao = DateTime.Now` (hora local) e `Ano = DateTime.UtcNow.Year` (UTC). `GetNextNumberOSAynsc` usa `DateTime.Now.Year`. Se o servidor estiver em UTC±0 não há efeito, mas em qualquer outro fuso horário os três valores podem divergir perto da virada do ano — resultando em `Ano` errado no `Codigo` da OS e numeração incorreta.

**Código atual:**
```csharp
// OrdemServicoService.cs
ordemServico.DataSolicitacao = DateTime.Now;      // local
ordemServico.Ano = DateTime.UtcNow.Year;          // UTC

// OrdemServicoRepository.cs:248
new { Ano = DateTime.Now.Year }                   // local novamente
```

**Correção sugerida:** Escolher uma única referência (preferencialmente `DateTime.UtcNow`) e usá-la consistentemente nos três lugares.

```csharp
var agora = DateTime.UtcNow;
ordemServico.DataSolicitacao = agora;
ordemServico.Ano = agora.Year;
// no repository:
new { Ano = DateTime.UtcNow.Year }
```

**Justificativa:** Misturar `Now` e `UtcNow` é um edge case que só manifesta perto de 00h em timezones não-UTC, mas pode gerar `Codigo` como `123/2024/0` quando o ano correto seria `2025`.

**Esforço:** baixo

---

## [BAIXO] Snapshot `listaOriginal` capturado após atualização otimista no drag-and-drop

**Arquivo:** `Presentation/Eletromecanica/src/views/ordem-servico/OrdemServicoDocumentos.vue:559`

**Problema:** `listaOriginal` é declarado _depois_ que `documentosOrdemServico.value` já foi substituído pela lista reordenada. O rollback em caso de erro da API restaura o estado já modificado (no-op visual), não o estado anterior ao drag.

**Código atual:**
```js
// linha 555 – atualização otimista
documentosOrdemServico.value = novaLista.map((item, idx) => ({ ...item, ordem: idx + 1 }))

reordenandoDocumentos.value = true

// linha 559 – BUG: snapshot da lista JÁ modificada
const listaOriginal = [...documentosOrdemServico.value]

try {
  const result = await documentoService.atualizarOrdemDocumento(atualizacoes)
  if (result?.statusCode !== 200) {
    documentosOrdemServico.value = listaOriginal   // não reverte nada
    await listarDocumentos()
  }
```

**Correção sugerida:** Capturar o snapshot antes da atualização otimista:

```js
// capturar ANTES do splice/atualização
const listaOriginal = documentosOrdemServico.value.map(x => ({ ...x }))

const novaLista = [...documentosOrdemServico.value]
// ... splice e reordenação ...
documentosOrdemServico.value = novaLista.map((item, idx) => ({ ...item, ordem: idx + 1 }))
```

**Justificativa:** Com a correção, a lista reverte imediatamente para o estado pré-drag enquanto `listarDocumentos()` busca os dados do servidor, evitando um flash visual com a ordem errada. Atualmente o rollback imediato é uma no-op; `listarDocumentos()` resolve, mas há uma janela de alguns segundos onde a UI mostra estado inconsistente.

**Esforço:** baixo
