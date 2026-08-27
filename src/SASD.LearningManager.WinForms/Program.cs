using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SASD.LearningManager.Infrastructure.Configuration;
using SASD.LearningManager.Infrastructure.DependencyInjection;
using SASD.LearningManager.Infrastructure.Logging;
using SASD.LearningManager.Infrastructure.Persistence;
using SASD.LearningManager.WinForms.Forms;
using SASD.LearningManager.WinForms.Views;

namespace SASD.LearningManager.WinForms;

internal static class Program
{
    private const string SingleInstanceMutexName = @"Local\SASD.LearningManager";

    /// <summary>The main entry point for the application.</summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var mutex = new Mutex(true, SingleInstanceMutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show(
                "SASD Learning Manager läuft bereits in einer anderen Instanz.",
                "SASD Learning Manager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        IHost? host = null;
        try
        {
            var paths = ApplicationPaths.CreateDefault();
            var builder = Host.CreateApplicationBuilder();
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new FileLoggerProvider(paths.LogDirectory));

            builder.Services.AddSingleton(paths);
            builder.Services.AddLearningManagerInfrastructure(paths.DatabasePath);
            builder.Services.AddTransient<ResourcesView>();
            builder.Services.AddTransient<InboxView>();
            builder.Services.AddTransient<MainForm>();

            host = builder.Build();
            host.StartAsync().GetAwaiter().GetResult();
            host.Services.GetRequiredService<DatabaseInitializer>()
                .InitializeAsync()
                .GetAwaiter()
                .GetResult();

            System.Windows.Forms.Application.Run(host.Services.GetRequiredService<MainForm>());
        }
        catch (Exception exception)
        {
            var errorId = $"ERR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
            try
            {
                host?.Services.GetService<ILoggerFactory>()?.CreateLogger("Startup")
                    .LogCritical(exception, "Fatal startup failure. ErrorId={ErrorId}", errorId);
            }
            catch
            {
                // Startup error reporting must never hide the original failure.
            }

            MessageBox.Show(
                $"Die Anwendung konnte nicht sicher gestartet werden.\n\nFehler-ID: {errorId}\n\n{exception.Message}",
                "SASD Learning Manager – Startfehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            if (host is not null)
            {
                try
                {
                    host.StopAsync().GetAwaiter().GetResult();
                }
                finally
                {
                    host.Dispose();
                }
            }

            if (isFirstInstance)
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
