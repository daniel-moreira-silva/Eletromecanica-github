using System.Resources;

namespace API.Controllers;

[ApiController]
[Route("ordem-servico")]
public class OrdemServicoController(ILogger<EstacaoController> logger, IOrdemServicoService service) : BaseController(logger)
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] OrdemServico ordemServico, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.AddAsync(ordemServico, cancellationToken);

            if (result is not null)
                return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", result));
            else
                return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao adicionar ordem de serviço"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao adicionar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] OrdemServico ordemServico, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.UpdateAsync(ordemServico, cancellationToken);

            if (result)
                return Ok(new SuccessMessage("Edição efetuada com sucesso."));
            else
                return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao atualizar ordem de serviço"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista")]
    public async Task<IActionResult> PaginatedGetAsync([FromBody] OrdemServicoFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PaginatedGetAsync(filter, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar lista.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista-count")]
    public async Task<IActionResult> ListaCountOrdemServicoAsync([FromBody] OrdemServicoFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ListaCountOrdemServicoAsync(filter, cancellationToken);
            return Ok(new SuccessMessage("Lista count retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar lista count.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("buscar-por-endereco")]
    public async Task<IActionResult> GetByAddressAsync([FromQuery] string search, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetByAddressAsync(search, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar lista.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet("buscar-orderns-servico-proximas")]
    public async Task<IActionResult> GetOrdemServicoNearByAsync([FromQuery] string lat, [FromQuery] string lon, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetOrdemServicoNearByAsync(lat, lon, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar lista.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> BuscarOcorrenciaPorId([FromQuery] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Ok(new SuccessMessage("Ordem de serviço obtida com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar ordem de serviço.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPatch("iniciar")]
    public async Task<IActionResult> IniciarOrdemServicoAsync(IniciarDto iniciar, CancellationToken cancellationToken)
    {
        try
        {
            bool result = await service.IniciarOrdemServicoAsync(iniciar.OrdemServicoId, iniciar.FuncionarioId, cancellationToken);

            if (!result)
                return NotFound(new ErrorMessage("Registro não encontrado.", iniciar.OrdemServicoId));

            return Ok(new ErrorMessage("Ordem de serviço iniciada com sucesso.", iniciar.OrdemServicoId));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao iniciar a ordem de serviço.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPatch("devolver")]
    public async Task<IActionResult> DevolverOrdemServicoAsync(DevolverDto devolver, CancellationToken cancellationToken)
    {
        try
        {
            bool result = await service.DevolverOrdemServicoAsync(devolver.OrdemServicoId, devolver.ObservacaoDevolucao, cancellationToken);

            if (!result)
                return NotFound(new ErrorMessage("Registro não encontrado.", devolver.OrdemServicoId));

            return Ok(new ErrorMessage("Ordem de serviço devolvida com sucesso.", devolver.OrdemServicoId));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao devolver a ordem de serviço.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPatch("despachar")]
    public async Task<IActionResult> DespacharOrdemServicoAsync(DespacharDto despachar, CancellationToken cancellationToken)
    {
        try
        {
            bool result = await service.DespacharOrdemServicoAsync(despachar.OrdemServicoId, despachar.FuncionarioId, despachar.DataDespachoProgramado, cancellationToken);

            if (!result)
                return NotFound(new ErrorMessage("Registro não encontrado.", despachar.OrdemServicoId));

            return Ok(new ErrorMessage("Ordem de serviço despachada com sucesso.", despachar.OrdemServicoId));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao despachar a ordem de serviço.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPatch("cancelar")]
    public async Task<IActionResult> CancelarOrdemServicoAsync(CancelamentoDto cancelamento, CancellationToken cancellationToken)
    {
        try
        {
            bool result = await service.CancelarOrdemServicoAsync(cancelamento.OrdemServicoId, cancelamento.MotivoCancelamentoId, cancelamento.Observacao, cancellationToken);

            if (!result)
                return NotFound(new ErrorMessage("Registro não encontrado.", cancelamento.OrdemServicoId));

            return Ok(new ErrorMessage("Ordem de serviço cancelada com sucesso.", cancelamento.OrdemServicoId));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao cancelar a ordem de serviço.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    public sealed record CancelamentoDto(Guid OrdemServicoId, Guid MotivoCancelamentoId, string Observacao);
    public sealed record IniciarDto(Guid OrdemServicoId, Guid FuncionarioId);
    public sealed record DespacharDto(Guid OrdemServicoId, Guid FuncionarioId, DateTime DataDespachoProgramado);
    public sealed record DevolverDto(Guid OrdemServicoId, string ObservacaoDevolucao);
}
