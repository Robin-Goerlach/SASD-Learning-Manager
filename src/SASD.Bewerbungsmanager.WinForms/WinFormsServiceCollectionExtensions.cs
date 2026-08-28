using Microsoft.Extensions.DependencyInjection;
using SASD.Bewerbungsmanager.WinForms.Controls;
using SASD.Bewerbungsmanager.WinForms.Presentation;
using OperationalMainForm = SASD.Bewerbungsmanager.WinForms.Forms.MainForm;

namespace SASD.Bewerbungsmanager.WinForms;

/// <summary>
/// Registers the Windows Forms composition root. Views are transient because each navigation
/// change creates a fresh control and immediately disposes the previous one.
/// </summary>
public static class WinFormsServiceCollectionExtensions
{
    /// <summary>Adds the current desktop shell, views, and presentation helpers.</summary>
    public static IServiceCollection AddWinFormsPresentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<UiExceptionPresenter>();
        services.AddSingleton<OperationalMainForm>();

        services.AddTransient<DashboardControl>();
        services.AddTransient<TasksControl>();
        services.AddTransient<AppointmentsControl>();
        services.AddTransient<ActivitiesControl>();
        services.AddTransient<JobLeadsControl>();
        services.AddTransient<AssistantControl>();
        services.AddTransient<BackupDiagnosticsControl>();
        services.AddTransient<SearchProfilesControl>();
        services.AddTransient<EvidenceExportControl>();
        services.AddTransient<CommunicationsControl>();
        services.AddTransient<ApplicationsControl>();
        services.AddTransient<OpportunitiesControl>();
        services.AddTransient<ContactsControl>();
        services.AddTransient<OrganizationsControl>();
        services.AddTransient<DocumentsControl>();

        return services;
    }
}
