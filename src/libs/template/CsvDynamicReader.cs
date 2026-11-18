using System.Dynamic;
using CsvHelper;

namespace TNO.TemplateEngine;

/// <summary>
/// CsvDynamicReader class, parses CSV report data into dynamic records for report templates.
/// Each record is an <see cref="ExpandoObject"/> keyed by header name with string values, which is the shape
/// templates (record.ColumnName) and JSON serialization have always received:
/// - a row shorter than the header yields null for the missing columns;
/// - fields beyond the header are ignored;
/// - a repeated header name keeps its first position and the last value;
/// - blank lines are skipped and quoted fields may contain separators, quotes and line breaks.
/// </summary>
public static class CsvDynamicReader
{
    #region Methods
    /// <summary>
    /// Read every record from the CSV text. The first row is the header.
    /// </summary>
    /// <param name="reader">The CSV text. It is left open.</param>
    /// <param name="onMalformedRecord">Called with the raw text of any record that is not valid CSV; the record is still read.</param>
    /// <returns></returns>
    public static List<dynamic> ReadRecords(TextReader reader, Action<string>? onMalformedRecord = null)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var options = new CsvReaderOptions()
        {
            Separator = ',',
            HasHeader = true,
            AllowLineBreaksInQuotes = true,
            SkipEmptyLines = true,
            TrimWhitespace = false,
            MalformedData = CsvMalformedDataHandling.Recover,
            FieldCountMismatch = CsvFieldCountHandling.Allow,
            OnMalformedRecord = record => onMalformedRecord?.Invoke(record.RawRecord),
        };

        using var csv = CsvReader.Create(reader, options, true);
        var records = new List<dynamic>();
        while (csv.Read())
        {
            var headers = csv.Headers;
            IDictionary<string, object?> record = new ExpandoObject();
            for (var i = 0; i < headers.Count; i++)
            {
                record[headers[i]] = i < csv.FieldCount ? csv.GetString(i) : null;
            }
            records.Add(record);
        }
        return records;
    }
    #endregion
}
