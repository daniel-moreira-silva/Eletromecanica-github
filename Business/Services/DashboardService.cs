namespace Business.Services;

public class DashboardService(IDashboardRepository repository) : IDashboardService
{
    public Task<DashboardStatusOsDto> ObterStatusOsAsync(Guid? estacaoId, CancellationToken ct = default)
        => repository.ObterStatusOsAsync(estacaoId, ct);

    public Task<DashboardIndicadoresDto> ObterIndicadoresAsync(Guid? estacaoId, CancellationToken ct = default)
        => repository.ObterIndicadoresAsync(estacaoId, ct);

    public Task<DashboardDisponibilidadeDto> ObterDisponibilidadeAsync(Guid? estacaoId, CancellationToken ct = default)
        => repository.ObterDisponibilidadeAsync(estacaoId, ct);

    public Task<DashboardMotivacaoDto> ObterMotivacaoAsync(Guid? estacaoId, CancellationToken ct = default)
        => repository.ObterMotivacaoAsync(estacaoId, ct);

    public Task<DashboardCustosDto> ObterCustosAsync(Guid? estacaoId, CancellationToken ct = default)
        => repository.ObterCustosAsync(estacaoId, ct);

    public Task<DashboardEstoqueDto> ObterEstoqueAsync(CancellationToken ct = default)
        => repository.ObterEstoqueAsync(ct);

    public Task<List<DashboardOsAtrasadaDto>> ObterOsAtrasadasAsync(Guid? estacaoId, CancellationToken ct = default)
        => repository.ObterOsAtrasadasAsync(estacaoId, ct);
}