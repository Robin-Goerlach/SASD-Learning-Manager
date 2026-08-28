using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;
using SASD.Bewerbungsmanager.Application;
using SASD.Bewerbungsmanager.Infrastructure;
using SASD.Bewerbungsmanager.Infrastructure.Operations;
using SASD.Bewerbungsmanager.WinForms;
using SASD.Bewerbungsmanager.WinForms.Controls;
using OperationalMainForm = SASD.Bewerbungsmanager.WinForms.Forms.MainForm;

namespace SASD.Bewerbungsmanager.Presentation.Tests;

/// <summary>Guards the real WinForms composition root and current view dependencies.</summary>
public sealed class MainShellTests
{
    [Fact]
    public void Operational_main_form_is_a_windows_form()
    {
        Assert.True(typeof(Form).IsAssignableFrom(typeof(OperationalMainForm)));
    }

    [Fact]
    public void Full_composition_root_validates_all_registered_views()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationManager
        {
            ["Database:Path"] = Path.Combine(Path.GetTempPath(), $"sasd-composition-{Guid.NewGuid():N}.db"),
        };
        services.AddLogging();
        services.AddApplicationServices();
        services.AddTrackerInfrastructure(configuration);
        services.AddWinFormsPresentation();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(OperationalMainForm));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(DashboardControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TasksControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(AppointmentsControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ActivitiesControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(SearchProfilesControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(DocumentsControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(EvidenceExportControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(CommunicationsControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(JobLeadsControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(AssistantControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(BackupDiagnosticsControl));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(PasswordProtectedBackupService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TrackerBackupCoordinator));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ReleaseReadinessService));

        // This validates constructor dependencies for every registered service without opening a
        // window on the xUnit worker thread. It protects against the class of startup/DI errors
        // that escaped the first Milestone-1 test suite.
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }
}
