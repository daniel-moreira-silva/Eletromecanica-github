namespace API.Controllers;

[ApiController]
[Route("motivos-cancelamento")]
public class MotivoCancelamentoController(ILogger<MotivoCancelamentoController> logger, IMotivoCancelamentoService service) : BaseController(logger)
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] MotivoCancelamento motivo, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateCodeIsDuplicatedAsync(motivo.Codigo, null, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um motivo de cancelamento cadastrado com este código."));

            var result = await service.AddAsync(motivo, cancellationToken);

            if (result != Guid.Empty)
                return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", motivo));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao adicionar motivo de cancelamento"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao adicionar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] MotivoCancelamento motivo, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateCodeIsDuplicatedAsync(motivo.Codigo, motivo.Id, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um motivo de cancelamento cadastrado com este código."));

            var result = await service.UpdateAsync(motivo, cancellationToken);
            if (result)
                return Ok(new SuccessMessage("Edição efetuada com sucesso.", motivo));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao atualizar motivo de cancelamento"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar motivo de cancelamento.");
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
                return NotFound(new ErrorMessage("Motivos de cancelamento não encontrados."));

            return Ok(new SuccessMessage("Motivos de cancelamento retornados com sucesso.", result));
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
                return NotFound(new ErrorMessage("Motivo de cancelamento não encontrado.", id));

            return Ok(new SuccessMessage("Motivo de cancelamento retornado com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista")]
    public async Task<IActionResult> PaginatedGetAsync([FromBody] MotivoCancelamentoFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PaginatedGetAsync(filter, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao listar registros.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPatch("status")]
    public async Task<IActionResult> UpdateStatusAsync([FromQuery] Guid id, [FromQuery] bool ativo, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.UpdateStatusAsync(id, ativo, cancellationToken);
            if (result)
                return Ok(new SuccessMessage("Status atualizado com sucesso.", id));

            return BadRequest(new ErrorMessage("Erro ao atualizar status."));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar status.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }
}