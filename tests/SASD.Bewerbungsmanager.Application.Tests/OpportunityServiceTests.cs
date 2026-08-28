using Xunit;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Tests;

public sealed class OpportunityServiceTests
{
    [Fact]
    public async Task CreateAsync_PreservesRoleDescriptionAsSnapshot()
    {
        var store = new MemoryTrackerDataStore();
        var employer = new Organization { Id = Guid.NewGuid(), Name = "Example Health IT GmbH" };
        store.Organizations.Add(employer);
        var clock = new FixedClock(new DateTimeOffset(2026, 8, 26, 7, 0, 0, TimeSpan.Zero));
        var service = new OpportunityService(store, clock);

        var created = await service.CreateAsync(new OpportunityInput(
            employer.Id,
            null,
            "System Engineer Linux",
            "Betrieb und Weiterentwicklung einer Linux-Plattform.",
            "Beispielstadt",
            "Hybrid",
            "60–70 k€",
            OpportunityStatus.Identified,
            clock.UtcNow,
            null,
            null));

        Assert.Equal("Betrieb und Weiterentwicklung einer Linux-Plattform.", created.DescriptionSnapshot);
        Assert.Single(store.Opportunities);
    }

    [Fact]
    public async Task CreateAsync_WhenEmployerAndIntermediaryAreSame_RejectsInput()
    {
        var store = new MemoryTrackerDataStore();
        var organization = new Organization { Id = Guid.NewGuid(), Name = "Example Recruiting GmbH" };
        store.Organizations.Add(organization);
        var service = new OpportunityService(store, new FixedClock(DateTimeOffset.UtcNow));

        var input = new OpportunityInput(
            organization.Id,
            organization.Id,
            "Platform Engineer",
            "Synthetische Rollenbeschreibung",
            null,
            null,
            null,
            OpportunityStatus.Identified,
            DateTimeOffset.UtcNow,
            null,
            null);

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(input));
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
