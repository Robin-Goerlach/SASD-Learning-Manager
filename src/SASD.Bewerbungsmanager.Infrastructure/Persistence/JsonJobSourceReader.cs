using System.Text.Json;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>Reads the versioned v0.4 JSON job-source handoff format from a local file.</summary>
public sealed class JsonJobSourceReader : IJobSourceReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc />
    public bool CanRead(string path)
        => Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<JobSourceBatch> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = ValidateExistingFile(path);
        await using var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 16 * 1024,
            useAsync: true);
        try
        {
            var batch = await JsonSerializer.DeserializeAsync<JobSourceBatch>(
                stream,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);
            return batch ?? throw new ValidationException("Die Job-Quellen-Datei enthält kein gültiges Paket.");
        }
        catch (JsonException ex)
        {
            throw new ValidationException($"Die Job-Quellen-Datei enthält ungültiges JSON: {ex.Message}");
        }
    }

    private static string ValidateExistingFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException("Eine Job-Quellen-Datei ist erforderlich.");
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Die Job-Quellen-Datei wurde nicht gefunden.", fullPath);
        }

        return fullPath;
    }
}
