using Microsoft.Extensions.DependencyInjection;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Services;

namespace SASD.Bewerbungsmanager.Application;

/// <summary>Registers application-layer services.</summary>
public static class DependencyInjection
{
    /// <summary>Adds the use-case services required by the desktop application.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IClock, SystemClock>();
        services.AddTransient<OrganizationService>();
        services.AddTransient<ContactService>();
        services.AddTransient<OpportunityService>();
        services.AddTransient<ApplicationService>();
        services.AddTransient<DashboardService>();
        services.AddTransient<ActivityService>();
        services.AddTransient<WorkItemService>();
        services.AddTransient<SearchProfileService>();
        services.AddTransient<DocumentService>();
        services.AddTransient<TodayService>();
        services.AddTransient<ApplicationContextService>();
        services.AddTransient<ApplicationEvidenceService>();
        services.AddTransient<ApplicationDossierService>();
        services.AddTransient<CommunicationImportService>();
        services.AddTransient<JobLeadService>();
        services.AddTransient<AssistantWorkspaceService>();
        return services;
    }
}
