using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SASD.Bewerbungsmanager.Domain.Entities;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;
using TrackerActivity = SASD.Bewerbungsmanager.Domain.Entities.Activity;
using TrackerDocument = SASD.Bewerbungsmanager.Domain.Entities.Document;

#nullable disable

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence.Migrations;

/// <summary>
/// EF Core model snapshot for the current SQLite schema.
/// </summary>
/// <remarks>
/// Migration snapshots deliberately describe SQLite column types explicitly. Reusing the runtime
/// <see cref="ApplicationTrackerDbContext"/> configuration here looks attractive, but EF builds a
/// snapshot with a different convention set than the SQLite runtime model. In that case inferred
/// provider types such as TEXT and INTEGER are missing and EF reports false pending model changes.
/// </remarks>
[DbContext(typeof(ApplicationTrackerDbContext))]
partial class ApplicationTrackerModelSnapshot : ModelSnapshot
{
    /// <inheritdoc />
    protected override void BuildModel(ModelBuilder modelBuilder)
        => BuildAssistantWorkspaceModel(modelBuilder);

    /// <summary>
    /// Builds the frozen target model of migration 202608260001_InitialMilestone1.
    /// Historical migration metadata must not start describing newer schema versions merely because
    /// the current application model evolves.
    /// </summary>
    /// <param name="modelBuilder">Model builder supplied by EF Core.</param>
    internal static void BuildMilestone1Model(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT");
            entity.Property(item => item.Name).HasColumnType("TEXT").HasMaxLength(200).IsRequired();
            entity.Property(item => item.Type).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Website).HasColumnType("TEXT").HasMaxLength(2048);
            entity.Property(item => item.Notes).HasColumnType("TEXT").HasMaxLength(4000);
            entity.Property(item => item.IsArchived).HasColumnType("INTEGER");
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.Name);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("contacts");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT");
            entity.Property(item => item.OrganizationId).HasColumnType("TEXT");
            entity.Property(item => item.FullName).HasColumnType("TEXT").HasMaxLength(200).IsRequired();
            entity.Property(item => item.Role).HasColumnType("TEXT").HasMaxLength(200);
            entity.Property(item => item.Email).HasColumnType("TEXT").HasMaxLength(320);
            entity.Property(item => item.Phone).HasColumnType("TEXT").HasMaxLength(100);
            entity.Property(item => item.LinkedInUrl).HasColumnType("TEXT").HasMaxLength(2048);
            entity.Property(item => item.Notes).HasColumnType("TEXT").HasMaxLength(4000);
            entity.Property(item => item.IsArchived).HasColumnType("INTEGER");
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.FullName);
            entity.HasIndex(item => item.OrganizationId);
            entity.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(item => item.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.ToTable("opportunities");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT");
            entity.Property(item => item.EmployerOrganizationId).HasColumnType("TEXT");
            entity.Property(item => item.IntermediaryOrganizationId).HasColumnType("TEXT");
            entity.Property(item => item.Title).HasColumnType("TEXT").HasMaxLength(250).IsRequired();
            entity.Property(item => item.DescriptionSnapshot).HasColumnType("TEXT").HasMaxLength(100000).IsRequired();
            entity.Property(item => item.Location).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.RemoteText).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.SalaryText).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.Status).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.FoundAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.PublishedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.DeadlineAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.EmployerOrganizationId);
            entity.HasIndex(item => item.IntermediaryOrganizationId);
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
        });

        modelBuilder.Entity<SourceLink>(entity =>
        {
            entity.ToTable("source_links");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT");
            entity.Property(item => item.OpportunityId).HasColumnType("TEXT");
            entity.Property(item => item.Source).HasColumnType("TEXT").HasMaxLength(100).IsRequired();
            entity.Property(item => item.Url).HasColumnType("TEXT").HasMaxLength(2048).IsRequired();
            entity.Property(item => item.ExternalId).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.CapturedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.OpportunityId);
            entity.HasOne<Opportunity>()
                .WithMany()
                .HasForeignKey(item => item.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.ToTable("applications");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT");
            entity.Property(item => item.OpportunityId).HasColumnType("TEXT");
            entity.Property(item => item.StartedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.SubmittedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.Stage).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Channel).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.SalaryExpectation).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.ClosedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.Outcome).HasColumnType("TEXT").HasMaxLength(2000);
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.Ignore(item => item.StatusHistory);
            entity.HasIndex(item => item.OpportunityId);
            entity.HasIndex(item => item.Stage);
            entity.HasOne<Opportunity>()
                .WithMany()
                .HasForeignKey(item => item.OpportunityId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany<ApplicationStatusHistory>("_statusHistory")
                .WithOne()
                .HasForeignKey(item => item.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation("_statusHistory")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ApplicationStatusHistory>(entity =>
        {
            entity.ToTable("application_status_history");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT").ValueGeneratedNever();
            entity.Property(item => item.ApplicationId).HasColumnType("TEXT");
            entity.Property(item => item.Stage).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.ChangedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.Note).HasColumnType("TEXT").HasMaxLength(2000);
            entity.HasIndex(item => new { item.ApplicationId, item.ChangedAtUtc });
        });
    }

    /// <summary>
    /// Builds the frozen target model of migration 202608260002_OperationalMvp and therefore the
    /// current v0.1.0 database schema.
    /// </summary>
    /// <param name="modelBuilder">Model builder supplied by EF Core.</param>
    internal static void BuildOperationalMvpModel(ModelBuilder modelBuilder)
    {
        BuildMilestone1Model(modelBuilder);
        modelBuilder.HasAnnotation("ProductVersion", "10.0.11");

        modelBuilder.Entity<TrackerActivity>(entity =>
        {
            entity.ToTable("activities");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT").ValueGeneratedNever();
            entity.Property(item => item.OpportunityId).HasColumnType("TEXT");
            entity.Property(item => item.ApplicationId).HasColumnType("TEXT");
            entity.Property(item => item.ContactId).HasColumnType("TEXT");
            entity.Property(item => item.OrganizationId).HasColumnType("TEXT");
            entity.Property(item => item.Kind).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Status).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Subject).HasColumnType("TEXT").HasMaxLength(250).IsRequired();
            entity.Property(item => item.Notes).HasColumnType("TEXT").HasMaxLength(8000);
            entity.Property(item => item.OccurredAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.ScheduledAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.CompletedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.ApplicationId);
            entity.HasIndex(item => item.ContactId);
            entity.HasIndex(item => item.OpportunityId);
            entity.HasIndex(item => item.OrganizationId);
            entity.HasIndex(item => item.ScheduledAtUtc);
            entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<JobApplication>().WithMany().HasForeignKey(item => item.ApplicationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Contact>().WithMany().HasForeignKey(item => item.ContactId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Organization>().WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TrackerTask>(entity =>
        {
            entity.ToTable("work_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT").ValueGeneratedNever();
            entity.Property(item => item.OpportunityId).HasColumnType("TEXT");
            entity.Property(item => item.ApplicationId).HasColumnType("TEXT");
            entity.Property(item => item.ContactId).HasColumnType("TEXT");
            entity.Property(item => item.OrganizationId).HasColumnType("TEXT");
            entity.Property(item => item.Kind).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Status).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Title).HasColumnType("TEXT").HasMaxLength(250).IsRequired();
            entity.Property(item => item.Notes).HasColumnType("TEXT").HasMaxLength(8000);
            entity.Property(item => item.DueAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.CompletedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.ApplicationId);
            entity.HasIndex(item => item.ContactId);
            entity.HasIndex(item => item.DueAtUtc);
            entity.HasIndex(item => new { item.Kind, item.Status });
            entity.HasIndex(item => item.OpportunityId);
            entity.HasIndex(item => item.OrganizationId);
            entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<JobApplication>().WithMany().HasForeignKey(item => item.ApplicationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Contact>().WithMany().HasForeignKey(item => item.ContactId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Organization>().WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SearchProfile>(entity =>
        {
            entity.ToTable("search_profiles");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT").ValueGeneratedNever();
            entity.Property(item => item.Name).HasColumnType("TEXT").HasMaxLength(200).IsRequired();
            entity.Property(item => item.Source).HasColumnType("TEXT").HasMaxLength(100).IsRequired();
            entity.Property(item => item.Url).HasColumnType("TEXT").HasMaxLength(2048).IsRequired();
            entity.Property(item => item.CheckIntervalDays).HasColumnType("INTEGER");
            entity.Property(item => item.LastCheckedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.NextCheckAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.IsActive).HasColumnType("INTEGER");
            entity.Property(item => item.Notes).HasColumnType("TEXT").HasMaxLength(4000);
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.NextCheckAtUtc);
            entity.HasIndex(item => item.IsActive);
        });

        modelBuilder.Entity<TrackerDocument>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT").ValueGeneratedNever();
            entity.Property(item => item.Type).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Label).HasColumnType("TEXT").HasMaxLength(200).IsRequired();
            entity.Property(item => item.Version).HasColumnType("TEXT").HasMaxLength(100).IsRequired();
            entity.Property(item => item.Language).HasColumnType("TEXT").HasMaxLength(20).IsRequired();
            entity.Property(item => item.Tags).HasColumnType("TEXT").HasMaxLength(1000);
            entity.Property(item => item.OriginalPath).HasColumnType("TEXT").HasMaxLength(4096).IsRequired();
            entity.Property(item => item.Sha256).HasColumnType("TEXT").HasMaxLength(64).IsRequired();
            entity.Property(item => item.SizeBytes).HasColumnType("INTEGER");
            entity.Property(item => item.IsArchived).HasColumnType("INTEGER");
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.Sha256);
            entity.HasIndex(item => new { item.Type, item.IsArchived });
        });

        modelBuilder.Entity<ApplicationDocumentSnapshot>(entity =>
        {
            entity.ToTable("application_document_snapshots");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT").ValueGeneratedNever();
            entity.Property(item => item.ApplicationId).HasColumnType("TEXT");
            entity.Property(item => item.DocumentId).HasColumnType("TEXT");
            entity.Property(item => item.Type).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Label).HasColumnType("TEXT").HasMaxLength(200).IsRequired();
            entity.Property(item => item.Version).HasColumnType("TEXT").HasMaxLength(100).IsRequired();
            entity.Property(item => item.Language).HasColumnType("TEXT").HasMaxLength(20).IsRequired();
            entity.Property(item => item.OriginalPath).HasColumnType("TEXT").HasMaxLength(4096).IsRequired();
            entity.Property(item => item.StoredPath).HasColumnType("TEXT").HasMaxLength(4096).IsRequired();
            entity.Property(item => item.Sha256).HasColumnType("TEXT").HasMaxLength(64).IsRequired();
            entity.Property(item => item.CapturedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.ApplicationId);
            entity.HasIndex(item => item.DocumentId);
            entity.HasOne<JobApplication>()
                .WithMany()
                .HasForeignKey(item => item.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<TrackerDocument>()
                .WithMany()
                .HasForeignKey(item => item.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Builds the frozen target model of migration 202608270003_CommunicationIntegration and therefore
    /// the current v0.3.0 communication-integration schema.
    /// </summary>
    /// <param name="modelBuilder">Model builder supplied by EF Core.</param>
    internal static void BuildCommunicationIntegrationModel(ModelBuilder modelBuilder)
    {
        BuildOperationalMvpModel(modelBuilder);
        modelBuilder.HasAnnotation("ProductVersion", "10.0.11");

        modelBuilder.Entity<CommunicationMessage>(entity =>
        {
            entity.ToTable("communication_messages");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT").ValueGeneratedNever();
            entity.Property(item => item.SourceSystem).HasColumnType("TEXT").HasMaxLength(100).IsRequired();
            entity.Property(item => item.ExternalMessageId).HasColumnType("TEXT").HasMaxLength(512);
            entity.Property(item => item.FingerprintSha256).HasColumnType("TEXT").HasMaxLength(64).IsRequired();
            entity.Property(item => item.Direction).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Kind).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Status).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.FromName).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.FromAddress).HasColumnType("TEXT").HasMaxLength(320);
            entity.Property(item => item.ToAddresses).HasColumnType("TEXT").HasMaxLength(2000);
            entity.Property(item => item.Subject).HasColumnType("TEXT").HasMaxLength(500).IsRequired();
            entity.Property(item => item.BodyText).HasColumnType("TEXT").HasMaxLength(100000).IsRequired();
            entity.Property(item => item.MessageAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.SourceReference).HasColumnType("TEXT").HasMaxLength(2048);
            entity.Property(item => item.OpportunityId).HasColumnType("TEXT");
            entity.Property(item => item.ApplicationId).HasColumnType("TEXT");
            entity.Property(item => item.ContactId).HasColumnType("TEXT");
            entity.Property(item => item.OrganizationId).HasColumnType("TEXT");
            entity.Property(item => item.ActivityId).HasColumnType("TEXT");
            entity.Property(item => item.ImportedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.ActivityId);
            entity.HasIndex(item => item.ApplicationId);
            entity.HasIndex(item => item.ContactId);
            entity.HasIndex(item => item.FingerprintSha256).IsUnique();
            entity.HasIndex(item => item.MessageAtUtc);
            entity.HasIndex(item => item.OpportunityId);
            entity.HasIndex(item => item.OrganizationId);
            entity.HasIndex(item => new { item.SourceSystem, item.ExternalMessageId });
            entity.HasIndex(item => item.Status);
            entity.HasOne<TrackerActivity>().WithMany().HasForeignKey(item => item.ActivityId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<JobApplication>().WithMany().HasForeignKey(item => item.ApplicationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Contact>().WithMany().HasForeignKey(item => item.ContactId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Organization>().WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Builds the frozen target model of migration 202608270004_JobSearchAdapters and therefore
    /// the current v0.4.0 job-search schema.
    /// </summary>
    /// <param name="modelBuilder">Model builder supplied by EF Core.</param>
    internal static void BuildJobSearchModel(ModelBuilder modelBuilder)
    {
        BuildCommunicationIntegrationModel(modelBuilder);
        modelBuilder.HasAnnotation("ProductVersion", "10.0.11");

        modelBuilder.Entity<JobLead>(entity =>
        {
            entity.ToTable("job_leads");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT").ValueGeneratedNever();
            entity.Property(item => item.SearchProfileId).HasColumnType("TEXT");
            entity.Property(item => item.SourceSystem).HasColumnType("TEXT").HasMaxLength(100).IsRequired();
            entity.Property(item => item.ExternalJobId).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.FingerprintSha256).HasColumnType("TEXT").HasMaxLength(64).IsRequired();
            entity.Property(item => item.Title).HasColumnType("TEXT").HasMaxLength(250).IsRequired();
            entity.Property(item => item.OrganizationName).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.Location).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.RemoteText).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.SalaryText).HasColumnType("TEXT").HasMaxLength(250);
            entity.Property(item => item.SourceUrl).HasColumnType("TEXT").HasMaxLength(2048);
            entity.Property(item => item.DescriptionText).HasColumnType("TEXT").HasMaxLength(100000);
            entity.Property(item => item.PublishedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.FoundAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.Status).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.OpportunityId).HasColumnType("TEXT");
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.FingerprintSha256).IsUnique();
            entity.HasIndex(item => item.FoundAtUtc);
            entity.HasIndex(item => item.OpportunityId);
            entity.HasIndex(item => item.SearchProfileId);
            entity.HasIndex(item => new { item.SourceSystem, item.ExternalJobId });
            entity.HasIndex(item => item.SourceUrl);
            entity.HasIndex(item => item.Status);
            entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<SearchProfile>().WithMany().HasForeignKey(item => item.SearchProfileId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Builds the frozen target model of migration 202608270005_AssistantWorkspace and therefore
    /// the current v0.5.0 optional-assistant schema.
    /// </summary>
    /// <param name="modelBuilder">Model builder supplied by EF Core.</param>
    internal static void BuildAssistantWorkspaceModel(ModelBuilder modelBuilder)
    {
        BuildJobSearchModel(modelBuilder);
        modelBuilder.HasAnnotation("ProductVersion", "10.0.11");

        modelBuilder.Entity<AssistantSession>(entity =>
        {
            entity.ToTable("assistant_sessions");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnType("TEXT").ValueGeneratedNever();
            entity.Property(item => item.OpportunityId).HasColumnType("TEXT");
            entity.Property(item => item.ApplicationId).HasColumnType("TEXT");
            entity.Property(item => item.TaskKind).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Status).HasColumnType("TEXT").HasMaxLength(50).HasConversion<string>().IsRequired();
            entity.Property(item => item.Title).HasColumnType("TEXT").HasMaxLength(250).IsRequired();
            entity.Property(item => item.ContextSha256).HasColumnType("TEXT").HasMaxLength(64).IsRequired();
            entity.Property(item => item.PromptText).HasColumnType("TEXT").HasMaxLength(250000).IsRequired();
            entity.Property(item => item.ResponseText).HasColumnType("TEXT").HasMaxLength(250000);
            entity.Property(item => item.ProviderLabel).HasColumnType("TEXT").HasMaxLength(100);
            entity.Property(item => item.AdditionalInstructions).HasColumnType("TEXT").HasMaxLength(4000);
            entity.Property(item => item.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.CompletedAtUtc).HasColumnType("TEXT");
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("TEXT");
            entity.HasIndex(item => item.ApplicationId);
            entity.HasIndex(item => item.CreatedAtUtc);
            entity.HasIndex(item => item.OpportunityId);
            entity.HasIndex(item => item.Status);
            entity.HasOne<JobApplication>().WithMany().HasForeignKey(item => item.ApplicationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Opportunity>().WithMany().HasForeignKey(item => item.OpportunityId).OnDelete(DeleteBehavior.SetNull);
        });
    }

}
