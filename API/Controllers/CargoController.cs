using Core.Models.FuncionarioAggregate;

namespace API.Controllers;

[ApiController]
[Route("cargos")]
public class CargoController(ILogger<CargoController> logger, ICargoService service) : BaseController(logger)
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Cargo cargo, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateDescriptionIsDuplicatedAsync(cargo.Descricao, null, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um cargo cadastrado com esta descrição."));

            var result = await service.AddAsync(cargo, cancellationToken);

            if (result != Guid.Empty)
                return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", cargo));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao adicionar cargo"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao adicionar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] Cargo cargo, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateDescriptionIsDuplicatedAsync(cargo.Descricao, cargo.Id, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um cargo cadastrado com esta descrição."));

            var result = await service.UpdateAsync(cargo, cancellationToken);
            if (result)
                return Ok(new SuccessMessage("Edição efetuada com sucesso.", cargo));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao atualizar cargo"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar cargo.");
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
                return NotFound(new ErrorMessage("Cargos não encontrados."));

            return Ok(new SuccessMessage("Cargos retornados com sucesso.", result));
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
                return NotFound(new ErrorMessage("Cargo não encontrado.", id));

            return Ok(new SuccessMessage("Cargo retornado com sucesso.", result));
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
                return NotFound(new ErrorMessage("Cargo não encontrado.", id));

            return Ok(new SuccessMessage("Status atualizado com sucesso.", id));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar status.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista")]
    public async Task<IActionResult> GetPaginatedAsync([FromBody] CargoFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PaginatedGetAsync(filter, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar cargos.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }
}