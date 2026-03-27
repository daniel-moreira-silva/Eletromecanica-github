namespace Business.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatusOsDto> ObterStatusOsAsync(Guid? estacaoId, CancellationToken ct = default);
    Task<DashboardIndicadoresDto> ObterIndicadoresAsync(Guid? estacaoId, CancellationToken ct = default);
    Task<DashboardDisponibilidadeDto> ObterDisponibilidadeAsync(Guid? estacaoId, CancellationToken ct = default);
    Task<DashboardMotivacaoDto> ObterMotivacaoAsync(Guid? estacaoId, CancellationToken ct = default);
    Task<DashboardCustosDto> ObterCustosAsync(Guid? estacaoId, CancellationToken ct = default);
    Task<DashboardEstoqueDto> ObterEstoqueAsync(CancellationToken ct = default);
    Task<List<DashboardOsAtrasadaDto>> ObterOsAtrasadasAsync(Guid? estacaoId, CancellationToken ct = default);
}