namespace API.Controllers;

[ApiController]
[Route("equipamentos")]
public class EquipamentoController(IEquipamentoService service, ILogger<EquipamentoController> logger) : BaseController(logger)
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] EquipamentoCompletoDto equipamento, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.AddAsync(equipamento, cancellationToken);

            if (result != Guid.Empty)
                return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", equipamento));

            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + "Erro ao adicionar equipamento"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao adicionar registro.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] EquipamentoCompletoDto equipamento, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.UpdateAsync(equipamento, cancellationToken);
            if (result)
                return Ok(new SuccessMessage("Edição efetuada com sucesso.", equipamento));

            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + "Erro ao atualizar equipamento"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar equipamento.");
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
                return NotFound(new ErrorMessage("Equipamento não encontrado.", id));

            return Ok(new SuccessMessage("Equipamento retornado com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar registro.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("completo/{id}")]
    public async Task<IActionResult> GetCopleteEquipmentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetCompleteEquipmentByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(new ErrorMessage("Equipamento não encontrado.", id));

            return Ok(new SuccessMessage("Equipamento completo retornado com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar registro.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("estacoes")]
    public async Task<IActionResult> GetAllEquipmentsByEstacaoIdAsync(Guid estacaoId, bool? principal, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetAllEquipmentsByEstacaoIdAsync(estacaoId, principal, cancellationToken);

            if (result is null)
                return NotFound(new ErrorMessage("Lista de equipamentos principais não encontrada.", estacaoId));

            return Ok(new SuccessMessage("Lista de equipamentos principais retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar registros.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("tiposEquipamento")]
    public async Task<IActionResult> TipoEquipamentoGetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetAllTiposEquipamentoAsync(cancellationToken);

            if (result is null)
                return NotFound(new ErrorMessage("Tipos de equipamento não encontrados."));

            return Ok(new SuccessMessage("Tipos de equipamento retornado com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar tipos de equipamento.");
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
                return NotFound(new ErrorMessage("Equipamento não encontrado.", id));

            return Ok(new SuccessMessage("Status atualizado com sucesso.", id));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar status.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista")]
    public async Task<IActionResult> GetPaginatedAsync([FromBody] EquipamentoFilter filter, CancellationToken cancellationToken)
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

    [HttpGet("regras-preventivas")]
    public async Task<IActionResult> GetAllByEquipamentoIdAsync(
        [FromServices] IRegraPreventivaService regraPreventivaService,
        [FromQuery] Guid equipamentoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await regraPreventivaService.GetAllPreventivasByEquipamentoIdAsync(equipamentoId, cancellationToken);
            return Ok(new SuccessMessage("Regras preventivas retornadas com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar regras preventivas.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("regras-preventivas")]
    public async Task<IActionResult> Post(
        [FromServices] IRegraPreventivaService regraPreventivaService,
        [FromBody] RegraPreventiva regra,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await regraPreventivaService.AddAsync(regra, cancellationToken);

            if (result.HasValue && result.Value != Guid.Empty)
                return Ok(new SuccessMessage("Regra preventiva cadastrada com sucesso.", result));

            return BadRequest(new ErrorMessage("Erro ao cadastrar regra preventiva."));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao cadastrar regra preventiva.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut("regras-preventivas")]
    public async Task<IActionResult> Put(
        [FromServices] IRegraPreventivaService regraPreventivaService,
        [FromBody] RegraPreventiva regra,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await regraPreventivaService.UpdateAsync(regra, cancellationToken);

            if (result)
                return Ok(new SuccessMessage("Regra preventiva atualizada com sucesso.", regra));

            return BadRequest(new ErrorMessage("Erro ao atualizar regra preventiva."));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar regra preventiva.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpDelete("regras-preventivas/{id}")]
    public async Task<IActionResult> Delete(
        [FromServices] IRegraPreventivaService regraPreventivaService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await regraPreventivaService.DeleteAsync(id, cancellationToken);

            if (result)
                return Ok(new SuccessMessage("Regra preventiva removida com sucesso.", id));

            return NotFound(new ErrorMessage("Regra preventiva não encontrada.", id));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao deletar regra preventiva.");
            return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
        }
    }
}
