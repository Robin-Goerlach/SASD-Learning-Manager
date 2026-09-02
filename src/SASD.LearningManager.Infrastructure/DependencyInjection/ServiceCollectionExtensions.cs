using Microsoft.Extensions.DependencyInjection;
using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Application.Evidence;
using SASD.LearningManager.Application.Knowledge;
using SASD.LearningManager.Application.LearningPaths;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Application.Skills;
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
        services.AddTransient<ICompetencyCatalogRepository, CompetencyCatalogRepository>();
        services.AddTransient<ISkillRepository, SkillRepository>();
        services.AddTransient<IGoalRepository, GoalRepository>();
        services.AddTransient<ILearningPathRepository, LearningPathRepository>();
        services.AddTransient<IResourceRepository, ResourceRepository>();
        services.AddTransient<IKnowledgeArtifactRepository, KnowledgeArtifactRepository>();
        services.AddTransient<IEvidenceRepository, EvidenceRepository>();
        services.AddTransient<ProviderService>();
        services.AddTransient<CompetencyCatalogService>();
        services.AddTransient<SkillService>();
        services.AddTransient<GoalService>();
        services.AddTransient<LearningPathService>();
        services.AddTransient<ResourceService>();
        services.AddTransient<KnowledgeArtifactService>();
        services.AddTransient<EvidenceService>();
        return services;
    }
}
