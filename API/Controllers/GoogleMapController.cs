namespace API.Controllers;

[ApiController]
[Route("google-maps")]
public class GoogleMapController(ILogger<GoogleMapController> logger, IGoogleMapService service) : BaseController(logger)
{
    [HttpGet("buscar-endereco")]
    public async Task<IActionResult> BuscarEnderecoAsync([FromQuery] string? latLong, [FromQuery] string? textoLivre, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetAsync(latLong, textoLivre, cancellationToken: cancellationToken);
            return Ok(new SuccessMessage("Endereço encontrado com sucesso.", result));
        }
        catch (Exception ex)
        {
            LogError(ex, "Erro ao buscar endereço.");
            return BadRequest(new ErrorMessage(Constantes.ERRO_EXEC_METODO + ex.Message));
        }
    }
}
