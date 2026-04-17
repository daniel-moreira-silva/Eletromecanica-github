using Core.Models.FuncionarioAggregate;

namespace API.Controllers;

[ApiController]
[Route("funcionarios")]
public class FuncionarioController(ILogger<FuncionarioController> logger, IFuncionarioService service) : BaseController(logger)
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Funcionario funcionario, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateCodeIsDuplicatedAsync(funcionario.Codigo, null, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um funcionário cadastrado com este código."));

            var result = await service.AddAsync(funcionario, cancellationToken);

            if (result != Guid.Empty)
                return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", funcionario));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao adicionar funcionário"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao adicionar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] Funcionario funcionario, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateCodeIsDuplicatedAsync(funcionario.Codigo, funcionario.Id, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um funcionário cadastrado com este código."));

            var result = await service.UpdateAsync(funcionario, cancellationToken);
            if (result)
                return Ok(new SuccessMessage("Edição efetuada com sucesso.", funcionario));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao atualizar funcionário"));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar funcionário.");
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
                return NotFound(new ErrorMessage("Funcionários não encontrados."));

            return Ok(new SuccessMessage("Funcionários retornados com sucesso.", result));
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
                return NotFound(new ErrorMessage("Funcionário não encontrado.", id));

            return Ok(new SuccessMessage("Funcionário retornado com sucesso.", result));
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
                return NotFound(new ErrorMessage("Funcionário não encontrado.", id));

            return Ok(new SuccessMessage("Status atualizado com sucesso.", id));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar status.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista")]
    public async Task<IActionResult> GetPaginatedAsync([FromBody] FuncionarioFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PaginatedGetAsync(filter, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar funcionários.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }
}