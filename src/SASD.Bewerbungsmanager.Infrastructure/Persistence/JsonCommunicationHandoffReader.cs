using System.Text.Json;
using System.Text.Json.Serialization;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>
/// Reads the versioned JSON handoff format used to transfer normalized messages from the SASD Mail
/// Workbench into the application tracker. The reader deliberately accepts local files only.
/// </summary>
public sealed class JsonCommunicationHandoffReader : ICommunicationHandoffReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <inheritdoc />
    public async Task<CommunicationHandoffBatch> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException("Eine Kommunikations-Handoff-Datei ist erforderlich.");
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Die Kommunikations-Handoff-Datei wurde nicht gefunden.", fullPath);
        }

        await using var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 16 * 1024,
            useAsync: true);
        try
        {
            var batch = await JsonSerializer.DeserializeAsync<CommunicationHandoffBatch>(
                stream,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);
            return batch ?? throw new ValidationException("Die Handoff-Datei enthält kein gültiges Kommunikationspaket.");
        }
        catch (JsonException ex)
        {
            throw new ValidationException($"Die Handoff-Datei enthält ungültiges JSON: {ex.Message}");
        }
    }
}
