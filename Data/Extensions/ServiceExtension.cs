namespace Data.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<DbConnection>((sp) => new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBombaRepository, BombaRepository>();
        services.AddScoped<ICaracteristicaEquipamentoRepository, CaracteristicaEquipamentoRepository>();
        services.AddScoped<ICLPRepository, CLPRepository>();
        services.AddScoped<IEquipamentoRepository, EquipamentoRepository>();
        services.AddScoped<IEstacaoRepository, EstacaoRepository>();
        services.AddScoped<IMotorRepository, MotorRepository>();
        services.AddScoped<INobreakRepository, NobreakRepository>();
        services.AddScoped<IMedidorVazaoRepository, MedidorVazaoRepository>();
        services.AddScoped<IDocumentoRepository, DocumentoRepository>();
        services.AddScoped<IServicoSolicitadoRepository, ServicoSolicitadoRepository>();
        services.AddScoped<IServicoExecutadoRepository, ServicoExecutadoRepository>();
        services.AddScoped<IGoogleMapRepository, GoogleMapRepository>();
        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IRegraPreventivaRepository, RegraPreventivaRepository>();

        services.AddRefitClient<IGoogleMapClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(configuration["GoogleMaps:BaseAddress"] ?? string.Empty);
            });

        return services;
    }
}
