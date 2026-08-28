using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Tests;

public sealed class JobSourceReaderTests
{
    [Fact]
    public async Task JsonReader_ReadsVersionedBatch()
    {
        var path = Path.Combine(Path.GetTempPath(), $"job-source-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(path,
                """
                {
                  "schemaVersion": 1,
                  "sourceSystem": "Example Portal",
                  "searchProfileId": null,
                  "capturedAtUtc": "2026-08-27T08:00:00Z",
                  "items": [
                    {
                      "externalJobId": "job-42",
                      "title": "Linux Engineer",
                      "organizationName": "Example GmbH",
                      "location": "Example City",
                      "remoteText": "Hybrid",
                      "salaryText": null,
                      "url": "https://jobs.example.invalid/42",
                      "descriptionText": "Synthetic role",
                      "publishedAtUtc": null
                    }
                  ]
                }
                """);

            var batch = await new JsonJobSourceReader().ReadAsync(path);

            Assert.Equal(1, batch.SchemaVersion);
            Assert.Equal("Example Portal", batch.SourceSystem);
            var item = Assert.Single(batch.Items);
            Assert.Equal("Linux Engineer", item.Title);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task CsvReader_SupportsQuotedMultilineDescription()
    {
        var path = Path.Combine(Path.GetTempPath(), $"job-source-{Guid.NewGuid():N}.csv");
        try
        {
            var csv = "sourceSystem;searchProfileId;capturedAtUtc;externalJobId;title;organizationName;location;remoteText;salaryText;url;descriptionText;publishedAtUtc\r\n" +
                      "Example Portal;;2026-08-27T08:00:00Z;job-7;Platform Engineer;Example GmbH;Example City;Remote;;https://jobs.example.invalid/7;\"First line\r\nSecond line with \"\"quote\"\"\";2026-08-26T12:00:00Z\r\n";
            await File.WriteAllTextAsync(path, csv);

            var batch = await new CsvJobSourceReader().ReadAsync(path);

            Assert.Equal("Example Portal", batch.SourceSystem);
            var item = Assert.Single(batch.Items);
            Assert.Equal("Platform Engineer", item.Title);
            Assert.Contains("Second line with \"quote\"", item.DescriptionText ?? string.Empty, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
