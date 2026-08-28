using System.Net.Mail;
using SASD.Bewerbungsmanager.Application.Exceptions;

namespace SASD.Bewerbungsmanager.Application.Services;

internal static class Validation
{
    public static string Required(string? value, string fieldName, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ValidationException($"{fieldName} ist erforderlich.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new ValidationException($"{fieldName} darf höchstens {maxLength} Zeichen enthalten.");
        }

        return trimmed;
    }

    public static string? Optional(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ValidationException($"{fieldName} darf höchstens {maxLength} Zeichen enthalten.");
        }

        return trimmed;
    }

    public static string? Email(string? value)
    {
        var normalized = Optional(value, "E-Mail", 320);
        if (normalized is null)
        {
            return null;
        }

        try
        {
            _ = new MailAddress(normalized);
        }
        catch (FormatException)
        {
            throw new ValidationException("Die E-Mail-Adresse hat kein gültiges Format.");
        }

        return normalized;
    }

    public static string? Url(string? value, string fieldName)
    {
        var normalized = Optional(value, fieldName, 2048);
        if (normalized is null)
        {
            return null;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ValidationException($"{fieldName} muss eine absolute HTTP- oder HTTPS-Adresse sein.");
        }

        return normalized;
    }
}
