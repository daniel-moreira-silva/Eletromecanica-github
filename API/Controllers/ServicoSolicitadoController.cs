namespace API.Controllers;

[ApiController]
[Route("servicos-solicitados")]
public class ServicoSolicitadoController(ILogger<ServicoSolicitadoController> logger, IServicoSolicitadoService service) : BaseController(logger)
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ServicoSolicitado servicoSolicitado, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateCodeIsDuplicatedAsync(servicoSolicitado.Codigo, null, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um servico solicitado cadastrado com este código."));

            var result = await service.AddAsync(servicoSolicitado, cancellationToken);

            if (result != Guid.Empty)
                return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", servicoSolicitado));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao adicionar serviço solicitado"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao adicionar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] ServicoSolicitado servicoSolicitado, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateCodeIsDuplicatedAsync(servicoSolicitado.Codigo, servicoSolicitado.Id, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um serviço solicitado cadastrado com este código"));

            var result = await service.UpdateAsync(servicoSolicitado, cancellationToken);
            if (result)
                return Ok(new SuccessMessage("Edição efetuada com sucesso.", servicoSolicitado));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao atualizar serviço solicitado"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar serviço solicitado.");
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
                return NotFound(new ErrorMessage("Serviços solicitados não encontrados."));

            return Ok(new SuccessMessage("Serviços solicitados retornados com sucesso.", result));
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
                return NotFound(new ErrorMessage("Serviço solicitado não encontrado.", id));

            return Ok(new SuccessMessage("Serviço solicitado retornado com sucesso.", result));
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
                return NotFound(new ErrorMessage("Serviço solicitado não encontrado.", id));

            return Ok(new SuccessMessage("Status atualizado com sucesso.", id));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar status.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista")]
    public async Task<IActionResult> GetPaginatedAsync([FromBody] ServicoSolicitadoFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PaginatedGetAsync(filter, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar estações.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }
}
