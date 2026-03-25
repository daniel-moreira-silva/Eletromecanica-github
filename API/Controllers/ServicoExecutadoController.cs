namespace API.Controllers
{
    [ApiController]
    [Route("servicos-executados")]
    public class ServicoExecutadoController(ILogger<ServicoExecutadoController> logger, IServicoExecutadoService service) : BaseController(logger)
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ServicoExecutado servicoExecutado, CancellationToken cancellationToken)
        {
            try
            {
                if (await service.ValidateCodeIsDuplicatedAsync(servicoExecutado.Codigo, null, cancellationToken))
                    return BadRequest(new ErrorMessage("Já existe um serviço executado cadastrado com este código."));

                var result = await service.AddAsync(servicoExecutado, cancellationToken);

                if (result != Guid.Empty)
                    return Ok(new SuccessMessage("Cadastro efetuado com sucesso.", servicoExecutado));

                return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + "Erro ao adicionar serviço executado"));
            }
            catch (Exception ex)
            {
                LogError(ex, "Erro ao adicionar registro.");
                return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] ServicoExecutado servicoExecutado, CancellationToken cancellationToken)
        {
            try
            {
                if (await service.ValidateCodeIsDuplicatedAsync(servicoExecutado.Codigo, servicoExecutado.Id, cancellationToken))
                    return BadRequest(new ErrorMessage("Já existe um serviço executado cadastrado com este código"));

                var result = await service.UpdateAsync(servicoExecutado, cancellationToken);
                if (result)
                    return Ok(new SuccessMessage("Edição efetuada com sucesso.", servicoExecutado));

                return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + "Erro ao atualizar serviço executado"));
            }
            catch (Exception ex)
            {
                LogError(ex, "Erro ao atualizar serviço executado.");
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
                    return NotFound(new ErrorMessage("Serviços solicitados não encontrados."));

                return Ok(new SuccessMessage("Serviços solicitados retornados com sucesso.", result));
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
                    return NotFound(new ErrorMessage("Serviço executado não encontrado.", id));

                return Ok(new SuccessMessage("Serviço executado retornado com sucesso.", result));
            }
            catch (Exception ex)
            {
                LogError(ex, "Erro ao buscar registro.");
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
                    return NotFound(new ErrorMessage("Serviço executado não encontrado.", id));

                return Ok(new SuccessMessage("Status atualizado com sucesso.", id));
            }
            catch (Exception ex)
            {
                LogError(ex, "Erro ao atualizar status.");
                return BadRequest(new ErrorMessage(ConstantResources.ERRO_EXEC_METODO + ex.Message));
            }
        }

        [HttpPost("lista")]
        public async Task<IActionResult> GetPaginatedAsync([FromBody] ServicoExecutadoFilter filter, CancellationToken cancellationToken)
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
}
