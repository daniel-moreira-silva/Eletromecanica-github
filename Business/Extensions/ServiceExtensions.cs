namespace Business.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataServices(configuration);

        services.AddScoped<IEstacaoService, EstacaoService>();
        services.AddScoped<IEquipamentoService, EquipamentoService>();
        services.AddScoped<IDocumentoService, DocumentoService>();
        services.AddScoped<IServicoSolicitadoService, ServicoSolicitadoService>();
        services.AddScoped<IServicoExecutadoService, ServicoExecutadoService>();
        services.AddScoped<IGoogleMapService, GoogleMapService>();
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();
        services.AddScoped<IRegraPreventivaService, RegraPreventivaService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IMotivoCancelamentoService, MotivoCancelamentoService>();
        services.AddScoped<IFuncionarioService, FuncionarioService>();
        services.AddScoped<ICargoService, CargoService>();
        services.AddScoped<ISetorService, SetorService>();
        services.AddScoped<ITipoFuncionarioService, TipoFuncionarioService>();
        services.AddScoped<IFuncionarioService, FuncionarioService>();

        return services;
    }
}
