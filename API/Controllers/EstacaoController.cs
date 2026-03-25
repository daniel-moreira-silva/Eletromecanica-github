namespace API.Controllers;

[ApiController]
[Route("estacoes")]
public class EstacaoController(ILogger<EstacaoController> logger, IEstacaoService service) : BaseController(logger)
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Estacao estacao, CancellationToken cancellationToken)
    {
        try
        {
            if(await service.ValidateNameIsDuplicatedAsync(estacao.Nome, null, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe uma estação cadastrada com este nome."));

            var result = await service.AddAsync(estacao, cancellationToken);

            if (result != Guid.Empty)
                return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", estacao));
            else
                return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + "Erro ao adicionar estação"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao adicionar registro.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] Estacao estacao, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateNameIsDuplicatedAsync(estacao.Nome, estacao.Id, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe uma estação cadastrada com este nome."));

            var result = await service.UpdateAsync(estacao, cancellationToken);
            if (result)
                return Ok(new SuccessMessage("Edição efetuada com sucesso.", estacao));

            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + "Erro ao atualizar estação"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar estação.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetAllAsync(cancellationToken);

            if (result is null)
                return NotFound(new ErrorMessage("Estações não encontradas."));

            return Ok(new SuccessMessage("Estações retornadas com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar registro.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(new ErrorMessage("Estação não encontrada.", id));

            return Ok(new SuccessMessage("Estação retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar registro.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("tiposEstacao")]
    public async Task<IActionResult> TipoEstacaoGetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.TipoEstacaoGetAllAsync(cancellationToken);

            if (result is null)
                return NotFound(new ErrorMessage("Tipos de estação não encontrados."));

            return Ok(new SuccessMessage("Tipos de estação retornado com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar tipos de estação.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPatch("status")]
    public async Task<IActionResult> AtualizaStatusAsync([FromQuery] Guid id, [FromQuery] bool ativo, CancellationToken cancellationToken)
    {
        try
        {
            bool resultado = await service.UpdateStatusAsync(id, ativo, cancellationToken);

            if (!resultado)
                return NotFound(new ErrorMessage("Estação não encontrada.", id));

            return Ok(new SuccessMessage("Status atualizado com sucesso.", id));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar status.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista")]
    public async Task<IActionResult> GetPaginatedAsync([FromBody] EstacaoFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PaginatedGetAsync(filter, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar estações.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }
}
