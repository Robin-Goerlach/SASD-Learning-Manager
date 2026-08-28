using SASD.Bewerbungsmanager.Domain.Entities;
using Xunit;

namespace SASD.Bewerbungsmanager.Domain.Tests;

/// <summary>
/// Protects the innermost domain project from accidental dependencies on UI or persistence
/// technologies. The domain model must remain usable without WinForms, EF Core or SQLite.
/// </summary>
public sealed class ArchitectureTests
{
    [Fact]
    public void DomainAssembly_DoesNotReferenceInfrastructureOrPresentationFrameworks()
    {
        var referencedAssemblies = typeof(Organization).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        var forbiddenAssemblyPrefixes = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.Data.Sqlite",
            "System.Windows.Forms",
            "SASD.Bewerbungsmanager.Infrastructure",
            "SASD.Bewerbungsmanager.WinForms",
        };

        // Assert.DoesNotContain is intentionally used instead of Assert.False(collection.Any(...)).
        // Besides producing a clearer failure message, this satisfies xUnit analyzer rule xUnit2012.
        Assert.DoesNotContain(
            referencedAssemblies,
            referenced => forbiddenAssemblyPrefixes.Any(
                prefix => referenced.StartsWith(prefix, StringComparison.Ordinal)));
    }
}
