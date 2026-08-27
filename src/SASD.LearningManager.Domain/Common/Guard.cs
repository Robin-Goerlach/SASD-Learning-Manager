namespace SASD.LearningManager.Domain.Common;

/// <summary>
/// Contains small domain-oriented guard clauses. Keeping these rules in the Domain project
/// prevents the UI and persistence layers from becoming the only line of defence against
/// invalid business data.
/// </summary>
internal static class Guard
{
    public static string RequiredText(string? value, string fieldName, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainValidationException($"{fieldName} must not be empty.");
        }

        if (normalized.Length > maxLength)
        {
            throw new DomainValidationException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    public static string? OptionalText(string? value, string fieldName, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is not null && normalized.Length > maxLength)
        {
            throw new DomainValidationException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    public static string? OptionalHttpUrl(string? value)
    {
        var normalized = OptionalText(value, "URL", 4096);
        if (normalized is null)
        {
            return null;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainValidationException("URL must be an absolute HTTP or HTTPS address.");
        }

        return normalized;
    }
}
