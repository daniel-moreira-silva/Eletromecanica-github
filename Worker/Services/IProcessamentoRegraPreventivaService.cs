namespace Worker.Services;

public interface IProcessamentoRegraPreventivaService
{
    Task ProcessarRegraAsync(RegraPreventiva regra, CancellationToken cancellationToken);
}
