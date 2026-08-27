using System.Reflection;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Domain.Resources;
using SASD.LearningManager.Infrastructure.Persistence;
using SASD.LearningManager.WinForms.Forms;

namespace SASD.LearningManager.Architecture.Tests;

/// <summary>Fitness functions protecting the four-project architecture from gradual dependency erosion.</summary>
public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_DoesNotReferenceInfrastructureOrWinForms()
    {
        var references = Names(typeof(Resource).Assembly);
        Assert.DoesNotContain("SASD.LearningManager.Infrastructure", references);
        Assert.DoesNotContain(WinFormsAssemblyName, references);
        Assert.DoesNotContain("System.Windows.Forms", references);
        Assert.DoesNotContain("Microsoft.Data.Sqlite", references);
    }

    [Fact]
    public void Application_DoesNotReferenceInfrastructureOrWinForms()
    {
        var references = Names(typeof(ResourceService).Assembly);
        Assert.DoesNotContain("SASD.LearningManager.Infrastructure", references);
        Assert.DoesNotContain(WinFormsAssemblyName, references);
        Assert.DoesNotContain("System.Windows.Forms", references);
        Assert.DoesNotContain("Microsoft.Data.Sqlite", references);
    }

    [Fact]
    public void Infrastructure_DoesNotReferenceWinForms()
    {
        var references = Names(typeof(DatabaseInitializer).Assembly);
        Assert.DoesNotContain(WinFormsAssemblyName, references);
        Assert.DoesNotContain("System.Windows.Forms", references);
    }

    [Fact]
    public void WinForms_IsTheOnlyLayerReferencingWindowsForms()
    {
        var references = Names(typeof(MainForm).Assembly);
        Assert.Contains("System.Windows.Forms", references);
    }

    private static string WinFormsAssemblyName => typeof(MainForm).Assembly.GetName().Name
        ?? throw new InvalidOperationException("WinForms assembly name is unavailable.");

    private static HashSet<string> Names(Assembly assembly)
        => assembly.GetReferencedAssemblies().Select(static name => name.Name ?? string.Empty).ToHashSet(StringComparer.Ordinal);
}
