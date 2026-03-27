namespace API.Controllers;

[ApiController]
[Route("dashboard")]
public class DashboardController(
    ILogger<DashboardController> logger,
    IDashboardService service
) : BaseController(logger)
{
    [HttpGet("status-os")]
    public async Task<IActionResult> ObterStatusOs(
        [FromQuery] Guid? estacaoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ObterStatusOsAsync(estacaoId, cancellationToken);
            return Ok(new SuccessMessage("Status retornado com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao obter status das OS.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("indicadores")]
    public async Task<IActionResult> ObterIndicadores(
        [FromQuery] Guid? estacaoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ObterIndicadoresAsync(estacaoId, cancellationToken);
            return Ok(new SuccessMessage("Indicadores retornados com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao obter indicadores.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("disponibilidade")]
    public async Task<IActionResult> ObterDisponibilidade(
        [FromQuery] Guid? estacaoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ObterDisponibilidadeAsync(estacaoId, cancellationToken);
            return Ok(new SuccessMessage("Disponibilidade retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao obter disponibilidade.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("motivacao")]
    public async Task<IActionResult> ObterMotivacao(
        [FromQuery] Guid? estacaoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ObterMotivacaoAsync(estacaoId, cancellationToken);
            return Ok(new SuccessMessage("Motivação retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao obter motivação.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("custos")]
    public async Task<IActionResult> ObterCustos(
        [FromQuery] Guid? estacaoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ObterCustosAsync(estacaoId, cancellationToken);
            return Ok(new SuccessMessage("Custos retornados com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao obter custos.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("estoque")]
    public async Task<IActionResult> ObterEstoque(CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ObterEstoqueAsync(cancellationToken);
            return Ok(new SuccessMessage("Estoque retornado com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao obter estoque.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("os-atrasadas")]
    public async Task<IActionResult> ObterOsAtrasadas(
        [FromQuery] Guid? estacaoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ObterOsAtrasadasAsync(estacaoId, cancellationToken);
            return Ok(new SuccessMessage("OS atrasadas retornadas com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao obter OS atrasadas.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }
}