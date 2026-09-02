using SASD.LearningManager.Application.ImportExport;

namespace SASD.LearningManager.Application.Tests;

/// <summary>Regression tests for the dependency-free CSV codec used by portable resource transfer.</summary>
public sealed class CsvDocumentTests
{
    [Fact]
    public void WriteAndRead_RoundTripsQuotedFieldsAndLineBreaks()
    {
        var csv = CsvDocument.Write(
            ["Title", "Notes"],
            [
                ["A, B", "Line 1\r\nLine 2"],
                ["Quote \"inside\"", "plain"]
            ]);

        var parsed = CsvDocument.Read(csv);

        Assert.Equal(["Title", "Notes"], parsed.Header);
        Assert.Equal(2, parsed.Rows.Count);
        Assert.Equal("A, B", parsed.Rows[0][0]);
        Assert.Equal("Line 1\r\nLine 2", parsed.Rows[0][1]);
        Assert.Equal("Quote \"inside\"", parsed.Rows[1][0]);
    }

    [Fact]
    public void Read_RemovesUtf8BomFromFirstHeaderCell()
    {
        var parsed = CsvDocument.Read("\uFEFFTitle,Type\r\nExample,Course\r\n");

        Assert.Equal("Title", parsed.Header[0]);
        Assert.Equal("Type", parsed.Header[1]);
    }

    [Fact]
    public void Read_RejectsRowsWithDifferentColumnCount()
    {
        var exception = Assert.Throws<FormatException>(() => CsvDocument.Read("A,B\r\n1,2,3\r\n"));

        Assert.Contains("different number of columns", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Read_RejectsUnterminatedQuotedField()
    {
        Assert.Throws<FormatException>(() => CsvDocument.Read("A,B\r\n\"open,field\r\n"));
    }
}
