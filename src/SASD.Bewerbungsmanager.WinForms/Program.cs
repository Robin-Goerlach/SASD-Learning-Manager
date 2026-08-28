using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SASD.Bewerbungsmanager.Application;
using SASD.Bewerbungsmanager.Infrastructure;
using SASD.Bewerbungsmanager.Infrastructure.Operations;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;
using SASD.Bewerbungsmanager.WinForms.Presentation;
using WinFormsApplication = System.Windows.Forms.Application;
using OperationalMainForm = SASD.Bewerbungsmanager.WinForms.Forms.MainForm;

namespace SASD.Bewerbungsmanager.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        // The desktop tracker intentionally has one writer process per interactive Windows session.
        // This also gives staged restore a clean startup boundary without another process holding SQLite files.
        using var singleInstance = SingleInstanceGuard.TryAcquire();
        if (singleInstance is null)
        {
            MessageBox.Show(
                "SASD Bewerbungsmanager läuft bereits in dieser Windows-Sitzung.",
                "SASD Bewerbungsmanager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddApplicationServices();
        builder.Services.AddTrackerInfrastructure(builder.Configuration);
        builder.Services.AddWinFormsPresentation();

        using var host = builder.Build();

        // Apply a staged restore before Host.Start and before DatabaseInitializer opens the tracker DB.
        host.Services.GetRequiredService<PendingRestoreCoordinator>()
            .ApplyPendingRestoreAsync()
            .GetAwaiter()
            .GetResult();

        host.Start();
        try
        {
            host.Services.GetRequiredService<DatabaseInitializer>()
                .InitializeAsync()
                .GetAwaiter()
                .GetResult();

            // Restored snapshot paths may point to another Windows profile. Rebind only paths that
            // can be resolved unambiguously from application-id and SHA-256 in the restored archive.
            host.Services.GetRequiredService<SnapshotPathRelocator>()
                .RelocateAsync()
                .GetAwaiter()
                .GetResult();

            InstallGlobalExceptionHandling(host.Services.GetRequiredService<UiExceptionPresenter>());
            WinFormsApplication.Run(host.Services.GetRequiredService<OperationalMainForm>());
        }
        finally
        {
            host.StopAsync().GetAwaiter().GetResult();
        }
    }

    private static void InstallGlobalExceptionHandling(UiExceptionPresenter presenter)
    {
        WinFormsApplication.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        WinFormsApplication.ThreadException += (_, eventArgs) => presenter.Show(eventArgs.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            if (eventArgs.ExceptionObject is Exception exception)
            {
                presenter.Show(exception);
            }
        };
    }
}
