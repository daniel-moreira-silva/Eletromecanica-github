using Core.Models.FuncionarioAggregate;

namespace API.Controllers;

[ApiController]
[Route("setores")]
public class SetorController(ILogger<SetorController> logger, ISetorService service) : BaseController(logger)
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Setor setor, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateDescriptionIsDuplicatedAsync(setor.Descricao, null, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um setor cadastrado com esta descrição."));

            var result = await service.AddAsync(setor, cancellationToken);

            if (result != Guid.Empty)
                return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", setor));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao adicionar setor"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao adicionar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] Setor setor, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateDescriptionIsDuplicatedAsync(setor.Descricao, setor.Id, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um setor cadastrado com esta descrição."));

            var result = await service.UpdateAsync(setor, cancellationToken);
            if (result)
                return Ok(new SuccessMessage("Edição efetuada com sucesso.", setor));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao atualizar setor"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar setor.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetAllAsync(cancellationToken);

            if (result is null)
                return NotFound(new ErrorMessage("Setores não encontrados."));

            return Ok(new SuccessMessage("Setores retornados com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(new ErrorMessage("Setor não encontrado.", id));

            return Ok(new SuccessMessage("Setor retornado com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPatch("status")]
    public async Task<IActionResult> AtualizaStatusAsync([FromQuery] Guid id, [FromQuery] bool ativo, CancellationToken cancellationToken)
    {
        try
        {
            bool resultado = await service.UpdateStatusAsync(id, ativo, cancellationToken);

            if (!resultado)
                return NotFound(new ErrorMessage("Setor não encontrado.", id));

            return Ok(new SuccessMessage("Status atualizado com sucesso.", id));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar status.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista")]
    public async Task<IActionResult> GetPaginatedAsync([FromBody] SetorFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PaginatedGetAsync(filter, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar setores.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }
}