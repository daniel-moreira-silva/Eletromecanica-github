namespace API.Controllers;

[ApiController]
[Route("material")]
public class MaterialController(ILogger<MaterialController> logger, IMaterialService service) : BaseController(logger)
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Material material, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateDescriptionIsDuplicatedAsync(material.Descricao, null, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um material cadastrado com esta descrição."));

            var result = await service.AddAsync(material, cancellationToken);

            if (result != Guid.Empty)
                return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", material));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao adicionar material."));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao adicionar registro.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] Material material, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ValidateDescriptionIsDuplicatedAsync(material.Descricao, material.Id, cancellationToken))
                return BadRequest(new ErrorMessage("Já existe um material cadastrado com esta descrição."));

            var result = await service.UpdateAsync(material, cancellationToken);
            if (result)
                return Ok(new SuccessMessage("Edição efetuada com sucesso.", material));

            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + "Erro ao atualizar material."));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar material.");
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
                return NotFound(new ErrorMessage("Materiais não encontrados."));

            return Ok(new SuccessMessage("Materiais retornados com sucesso.", result));
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
                return NotFound(new ErrorMessage("Material não encontrado.", id));

            return Ok(new SuccessMessage("Material retornado com sucesso.", result));
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
                return NotFound(new ErrorMessage("Material não encontrado.", id));

            return Ok(new SuccessMessage("Status atualizado com sucesso.", id));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao atualizar status.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }

    [HttpPost("lista")]
    public async Task<IActionResult> GetPaginatedAsync([FromBody] MaterialFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PaginatedGetAsync(filter, cancellationToken);
            return Ok(new SuccessMessage("Lista retornada com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar materiais.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }
}
