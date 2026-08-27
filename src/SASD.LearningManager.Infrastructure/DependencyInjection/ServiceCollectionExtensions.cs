using Microsoft.Extensions.DependencyInjection;
using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Infrastructure.Files;
using SASD.LearningManager.Infrastructure.Persistence;
using SASD.LearningManager.Infrastructure.Persistence.Repositories;
using SASD.LearningManager.Infrastructure.Time;

namespace SASD.LearningManager.Infrastructure.DependencyInjection;

/// <summary>Registers the V1 local infrastructure adapters and application services.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLearningManagerInfrastructure(this IServiceCollection services, string databasePath)
    {
        services.AddSingleton(new SqliteConnectionFactory(databasePath));
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IUrlNormalizer, UrlNormalizer>();
        services.AddSingleton<IExternalLinkLauncher, ExternalLinkLauncher>();

        services.AddTransient<IProviderRepository, ProviderRepository>();
        services.AddTransient<IResourceRepository, ResourceRepository>();
        services.AddTransient<ProviderService>();
        services.AddTransient<ResourceService>();
        return services;
    }
}
