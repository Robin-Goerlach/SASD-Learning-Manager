using Microsoft.Data.Sqlite;

namespace SASD.LearningManager.Infrastructure.Persistence;

/// <summary>Small conversion helpers shared by explicit repository mappers.</summary>
internal static class SqliteValue
{
    public static string? NullableString(SqliteDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    public static int? NullableInt32(SqliteDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);

    public static Guid? NullableGuid(SqliteDataReader reader, int ordinal)
    {
        var value = NullableString(reader, ordinal);
        return value is null ? null : Guid.Parse(value);
    }

    public static DateTimeOffset DateTimeOffset(SqliteDataReader reader, int ordinal)
        => System.DateTimeOffset.Parse(reader.GetString(ordinal), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);

    public static DateOnly? NullableDateOnly(SqliteDataReader reader, int ordinal)
    {
        var value = NullableString(reader, ordinal);
        return value is null
            ? null
            : DateOnly.ParseExact(value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static DateTimeOffset? NullableDateTimeOffset(SqliteDataReader reader, int ordinal)
    {
        var value = NullableString(reader, ordinal);
        return value is null
            ? null
            : System.DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    public static string ToDb(DateTimeOffset value) => value.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture);
    public static object ToDb(DateOnly? value) => value is null ? DBNull.Value : value.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    public static object ToDb(DateTimeOffset? value) => value is null ? DBNull.Value : ToDb(value.Value);
    public static object ToDb(Guid? value) => value is null ? DBNull.Value : value.Value.ToString("D");
    public static object ToDb(string? value) => value is null ? DBNull.Value : value;
    public static object ToDb(int? value) => value is null ? DBNull.Value : value.Value;
}
