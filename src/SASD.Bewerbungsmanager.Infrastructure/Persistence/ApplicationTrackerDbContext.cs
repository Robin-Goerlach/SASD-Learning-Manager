using Microsoft.EntityFrameworkCore;
using SASD.Bewerbungsmanager.Domain.Entities;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;
using TrackerActivity = SASD.Bewerbungsmanager.Domain.Entities.Activity;
using TrackerDocument = SASD.Bewerbungsmanager.Domain.Entities.Document;

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the local application-tracker database. The desktop application obtains
/// contexts through <see cref="IDbContextFactory{TContext}"/> so no context is retained for the
/// lifetime of the main form or shared across UI/background operations.
/// </summary>
public sealed class ApplicationTrackerDbContext(DbContextOptions<ApplicationTrackerDbContext> options) : DbContext(options)
{
    /// <summary>Gets the organizations table.</summary>
    public DbSet<Organization> Organizations => Set<Organization>();

    /// <summary>Gets the contacts table.</summary>
    public DbSet<Contact> Contacts => Set<Contact>();

    /// <summary>Gets the opportunities table.</summary>
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();

    /// <summary>Gets the source-links table.</summary>
    public DbSet<SourceLink> SourceLinks => Set<SourceLink>();

    /// <summary>Gets the applications table.</summary>
    public DbSet<JobApplication> Applications => Set<JobApplication>();

    /// <summary>Gets the application-status-history table.</summary>
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistory => Set<ApplicationStatusHistory>();

    /// <summary>Gets the timeline activities and appointments table.</summary>
    public DbSet<TrackerActivity> Activities => Set<TrackerActivity>();

    /// <summary>Gets ACTION and WAITING_FOR work items.</summary>
    public DbSet<TrackerTask> Tasks => Set<TrackerTask>();

    /// <summary>Gets manually checked search routines.</summary>
    public DbSet<SearchProfile> SearchProfiles => Set<SearchProfile>();

    /// <summary>Gets the document-version catalog.</summary>
    public DbSet<TrackerDocument> Documents => Set<TrackerDocument>();

    /// <summary>Gets immutable document snapshots assigned to applications.</summary>
    public DbSet<ApplicationDocumentSnapshot> ApplicationDocumentSnapshots => Set<ApplicationDocumentSnapshot>();

    /// <summary>Gets normalized communication messages imported from local handoff sources.</summary>
    public DbSet<CommunicationMessage> CommunicationMessages => Set<CommunicationMessage>();

    /// <summary>Gets discovered job-source results awaiting review or promotion.</summary>
    public DbSet<JobLead> JobLeads => Set<JobLead>();

    /// <summary>Gets optional assistant handoff sessions and their locally stored responses.</summary>
    public DbSet<AssistantSession> AssistantSessions => Set<AssistantSession>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => ConfigureCurrentModel(modelBuilder);

    /// <summary>
    /// Applies the complete current runtime persistence model. Migration snapshots intentionally keep
    /// their own frozen, provider-specific model descriptions so historical migrations do not drift
    /// when the runtime model evolves.
    /// </summary>
    /// <param name="modelBuilder">EF Core model builder.</param>
    internal static void ConfigureCurrentModel(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        ConfigureOrganization(modelBuilder);
        ConfigureContact(modelBuilder);
        ConfigureOpportunity(modelBuilder);
        ConfigureSourceLink(modelBuilder);
        ConfigureApplication(modelBuilder);
        ConfigureApplicationStatusHistory(modelBuilder);
        ConfigureActivity(modelBuilder);
        ConfigureTask(modelBuilder);
        ConfigureSearchProfile(modelBuilder);
        ConfigureDocument(modelBuilder);
        ConfigureApplicationDocumentSnapshot(modelBuilder);
        ConfigureCommunicationMessage(modelBuilder);
        ConfigureJobLead(modelBuilder);
        ConfigureAssistantSession(modelBuilder);
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Organization>();
        entity.ToTable("organizations");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
        entity.Property(item => item.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Website).HasMaxLength(2048);
        entity.Property(item => item.Notes).HasMaxLength(4000);
        entity.HasIndex(item => item.Name);
    }

    private static void ConfigureContact(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Contact>();
        entity.ToTable("contacts");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.FullName).HasMaxLength(200).IsRequired();
        entity.Property(item => item.Role).HasMaxLength(200);
        entity.Property(item => item.Email).HasMaxLength(320);
        entity.Property(item => item.Phone).HasMaxLength(100);
        entity.Property(item => item.LinkedInUrl).HasMaxLength(2048);
        entity.Property(item => item.Notes).HasMaxLength(4000);
        entity.HasIndex(item => item.FullName);
        entity.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(item => item.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureOpportunity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Opportunity>();
        entity.ToTable("opportunities");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Title).HasMaxLength(250).IsRequired();
        entity.Property(item => item.DescriptionSnapshot).HasMaxLength(100_000).IsRequired();
        entity.Property(item => item.Location).HasMaxLength(250);
        entity.Property(item => item.RemoteText).HasMaxLength(250);
        entity.Property(item => item.SalaryText).HasMaxLength(250);
        entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.HasIndex(item => item.Status);
        entity.HasIndex(item => item.FoundAtUtc);
        entity.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(item => item.EmployerOrganizationId)
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(item => item.IntermediaryOrganizationId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureSourceLink(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<SourceLink>();
        entity.ToTable("source_links");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Source).HasMaxLength(100).IsRequired();
        entity.Property(item => item.Url).HasMaxLength(2048).IsRequired();
        entity.Property(item => item.ExternalId).HasMaxLength(250);
        entity.HasIndex(item => item.OpportunityId);
        entity.HasOne<Opportunity>()
            .WithMany()
            .HasForeignKey(item => item.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureApplication(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<JobApplication>();
        entity.ToTable("applications");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Stage).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Channel).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.SalaryExpectation).HasMaxLength(250);
        entity.Property(item => item.Outcome).HasMaxLength(2000);
        entity.HasIndex(item => item.Stage);
        entity.HasIndex(item => item.OpportunityId);
        entity.HasOne<Opportunity>()
            .WithMany()
            .HasForeignKey(item => item.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        // The public StatusHistory collection is intentionally read-only. EF writes directly to the
        // backing field when materializing a graph, preserving the domain API while keeping persistence simple.
        entity.Ignore(item => item.StatusHistory);
        entity.HasMany<ApplicationStatusHistory>("_statusHistory")
            .WithOne()
            .HasForeignKey(item => item.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.Navigation("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureApplicationStatusHistory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ApplicationStatusHistory>();
        entity.ToTable("application_status_history");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).ValueGeneratedNever();
        entity.Property(item => item.Stage).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Note).HasMaxLength(2000);
        entity.HasIndex(item => new { item.ApplicationId, item.ChangedAtUtc });
    }

    private static void ConfigureActivity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TrackerActivity>();
        entity.ToTable("activities");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).ValueGeneratedNever();
        entity.Property(item => item.Kind).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Subject).HasMaxLength(250).IsRequired();
        entity.Property(item => item.Notes).HasMaxLength(8000);
        entity.HasIndex(item => item.ApplicationId);
        entity.HasIndex(item => item.OpportunityId);
        entity.HasIndex(item => item.ScheduledAtUtc);
        entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<JobApplication>().WithMany().HasForeignKey(item => item.ApplicationId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Contact>().WithMany().HasForeignKey(item => item.ContactId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Organization>().WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureTask(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TrackerTask>();
        entity.ToTable("work_items");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).ValueGeneratedNever();
        entity.Property(item => item.Kind).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Title).HasMaxLength(250).IsRequired();
        entity.Property(item => item.Notes).HasMaxLength(8000);
        entity.HasIndex(item => new { item.Kind, item.Status });
        entity.HasIndex(item => item.DueAtUtc);
        entity.HasIndex(item => item.ApplicationId);
        entity.HasIndex(item => item.OpportunityId);
        entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<JobApplication>().WithMany().HasForeignKey(item => item.ApplicationId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Contact>().WithMany().HasForeignKey(item => item.ContactId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Organization>().WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureSearchProfile(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<SearchProfile>();
        entity.ToTable("search_profiles");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).ValueGeneratedNever();
        entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
        entity.Property(item => item.Source).HasMaxLength(100).IsRequired();
        entity.Property(item => item.Url).HasMaxLength(2048).IsRequired();
        entity.Property(item => item.Notes).HasMaxLength(4000);
        entity.HasIndex(item => item.NextCheckAtUtc);
        entity.HasIndex(item => item.IsActive);
    }

    private static void ConfigureDocument(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TrackerDocument>();
        entity.ToTable("documents");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).ValueGeneratedNever();
        entity.Property(item => item.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Label).HasMaxLength(200).IsRequired();
        entity.Property(item => item.Version).HasMaxLength(100).IsRequired();
        entity.Property(item => item.Language).HasMaxLength(20).IsRequired();
        entity.Property(item => item.Tags).HasMaxLength(1000);
        entity.Property(item => item.OriginalPath).HasMaxLength(4096).IsRequired();
        entity.Property(item => item.Sha256).HasMaxLength(64).IsRequired();
        entity.HasIndex(item => item.Sha256);
        entity.HasIndex(item => new { item.Type, item.IsArchived });
    }

    private static void ConfigureApplicationDocumentSnapshot(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ApplicationDocumentSnapshot>();
        entity.ToTable("application_document_snapshots");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).ValueGeneratedNever();
        entity.Property(item => item.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Label).HasMaxLength(200).IsRequired();
        entity.Property(item => item.Version).HasMaxLength(100).IsRequired();
        entity.Property(item => item.Language).HasMaxLength(20).IsRequired();
        entity.Property(item => item.OriginalPath).HasMaxLength(4096).IsRequired();
        entity.Property(item => item.StoredPath).HasMaxLength(4096).IsRequired();
        entity.Property(item => item.Sha256).HasMaxLength(64).IsRequired();
        entity.HasIndex(item => item.ApplicationId);
        entity.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(item => item.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<TrackerDocument>()
            .WithMany()
            .HasForeignKey(item => item.DocumentId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureCommunicationMessage(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CommunicationMessage>();
        entity.ToTable("communication_messages");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).ValueGeneratedNever();
        entity.Property(item => item.SourceSystem).HasMaxLength(100).IsRequired();
        entity.Property(item => item.ExternalMessageId).HasMaxLength(512);
        entity.Property(item => item.FingerprintSha256).HasMaxLength(64).IsRequired();
        entity.Property(item => item.Direction).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Kind).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.FromName).HasMaxLength(250);
        entity.Property(item => item.FromAddress).HasMaxLength(320);
        entity.Property(item => item.ToAddresses).HasMaxLength(2000);
        entity.Property(item => item.Subject).HasMaxLength(500).IsRequired();
        entity.Property(item => item.BodyText).HasMaxLength(100_000).IsRequired();
        entity.Property(item => item.SourceReference).HasMaxLength(2048);
        entity.HasIndex(item => item.FingerprintSha256).IsUnique();
        entity.HasIndex(item => new { item.SourceSystem, item.ExternalMessageId });
        entity.HasIndex(item => item.MessageAtUtc);
        entity.HasIndex(item => item.Status);
        entity.HasIndex(item => item.ApplicationId);
        entity.HasIndex(item => item.OpportunityId);
        entity.HasIndex(item => item.ContactId);
        entity.HasIndex(item => item.OrganizationId);
        entity.HasIndex(item => item.ActivityId);
        entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<JobApplication>().WithMany().HasForeignKey(item => item.ApplicationId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Contact>().WithMany().HasForeignKey(item => item.ContactId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Organization>().WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<TrackerActivity>().WithMany().HasForeignKey(item => item.ActivityId).OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureJobLead(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<JobLead>();
        entity.ToTable("job_leads");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).ValueGeneratedNever();
        entity.Property(item => item.SourceSystem).HasMaxLength(100).IsRequired();
        entity.Property(item => item.ExternalJobId).HasMaxLength(250);
        entity.Property(item => item.FingerprintSha256).HasMaxLength(64).IsRequired();
        entity.Property(item => item.Title).HasMaxLength(250).IsRequired();
        entity.Property(item => item.OrganizationName).HasMaxLength(250);
        entity.Property(item => item.Location).HasMaxLength(250);
        entity.Property(item => item.RemoteText).HasMaxLength(250);
        entity.Property(item => item.SalaryText).HasMaxLength(250);
        entity.Property(item => item.SourceUrl).HasMaxLength(2048);
        entity.Property(item => item.DescriptionText).HasMaxLength(100_000);
        entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.HasIndex(item => item.FingerprintSha256).IsUnique();
        entity.HasIndex(item => new { item.SourceSystem, item.ExternalJobId });
        entity.HasIndex(item => item.SourceUrl);
        entity.HasIndex(item => item.SearchProfileId);
        entity.HasIndex(item => item.OpportunityId);
        entity.HasIndex(item => item.Status);
        entity.HasIndex(item => item.FoundAtUtc);
        entity.HasOne<SearchProfile>().WithMany().HasForeignKey(item => item.SearchProfileId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
    }


    private static void ConfigureAssistantSession(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AssistantSession>();
        entity.ToTable("assistant_sessions");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).ValueGeneratedNever();
        entity.Property(item => item.TaskKind).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(item => item.Title).HasMaxLength(250).IsRequired();
        entity.Property(item => item.ContextSha256).HasMaxLength(64).IsRequired();
        entity.Property(item => item.PromptText).HasMaxLength(250_000).IsRequired();
        entity.Property(item => item.ResponseText).HasMaxLength(250_000);
        entity.Property(item => item.ProviderLabel).HasMaxLength(100);
        entity.Property(item => item.AdditionalInstructions).HasMaxLength(4_000);
        entity.HasIndex(item => item.ApplicationId);
        entity.HasIndex(item => item.OpportunityId);
        entity.HasIndex(item => item.Status);
        entity.HasIndex(item => item.CreatedAtUtc);
        entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<JobApplication>().WithMany().HasForeignKey(item => item.ApplicationId).OnDelete(DeleteBehavior.SetNull);
    }


}
