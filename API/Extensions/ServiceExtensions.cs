namespace API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDependencyInjections(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBusinessServices(configuration);
        return services;
    }
}