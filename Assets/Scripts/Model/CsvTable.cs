using System;
using System.Collections.Generic;
using System.Text;

public sealed class CsvTable
{
    public CsvTable(IReadOnlyList<string> headers, IReadOnlyList<CsvRow> rows)
    {
        Headers = headers;
        Rows = rows;
    }

    public IReadOnlyList<string> Headers { get; }
    public IReadOnlyList<CsvRow> Rows { get; }

    public static CsvTable Parse(string csvText)
    {
        var rawRows = ParseRows(csvText ?? string.Empty);
        if (rawRows.Count == 0)
        {
            return new CsvTable(Array.Empty<string>(), Array.Empty<CsvRow>());
        }

        var headers = rawRows[0].Values;
        var rows = new List<CsvRow>(Math.Max(0, rawRows.Count - 1));
        for (var i = 1; i < rawRows.Count; i++)
        {
            if (IsEmptyRow(rawRows[i].Values))
            {
                continue;
            }

            rows.Add(new CsvRow(headers, rawRows[i].Values, rawRows[i].LineNumber));
        }

        return new CsvTable(headers, rows);
    }

    private static List<RawCsvRow> ParseRows(string csvText)
    {
        var rows = new List<RawCsvRow>();
        var currentRow = new List<string>();
        var currentField = new StringBuilder();
        var isQuoted = false;
        var lineNumber = 1;
        var rowLineNumber = 1;

        for (var i = 0; i < csvText.Length; i++)
        {
            var c = csvText[i];
            if (isQuoted)
            {
                if (c == '"')
                {
                    if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        isQuoted = false;
                    }
                }
                else
                {
                    if (c == '\n')
                    {
                        lineNumber++;
                    }

                    currentField.Append(c);
                }

                continue;
            }

            if (c == '"')
            {
                isQuoted = true;
                continue;
            }

            if (c == ',')
            {
                currentRow.Add(currentField.ToString());
                currentField.Clear();
                continue;
            }

            if (c == '\r' || c == '\n')
            {
                currentRow.Add(currentField.ToString());
                currentField.Clear();
                rows.Add(new RawCsvRow(currentRow, rowLineNumber));
                currentRow = new List<string>();

                if (c == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                {
                    i++;
                }

                lineNumber++;
                rowLineNumber = lineNumber;
                continue;
            }

            currentField.Append(c);
        }

        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField.ToString());
            rows.Add(new RawCsvRow(currentRow, rowLineNumber));
        }

        return rows;
    }

    private static bool IsEmptyRow(IReadOnlyList<string> values)
    {
        for (var i = 0; i < values.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(values[i]))
            {
                return false;
            }
        }

        return true;
    }

    private readonly struct RawCsvRow
    {
        public RawCsvRow(IReadOnlyList<string> values, int lineNumber)
        {
            Values = values;
            LineNumber = lineNumber;
        }

        public IReadOnlyList<string> Values { get; }
        public int LineNumber { get; }
    }
}

public sealed class CsvRow
{
    private readonly Dictionary<string, string> _valuesByHeader;

    public CsvRow(IReadOnlyList<string> headers, IReadOnlyList<string> values, int lineNumber)
    {
        Values = values;
        LineNumber = lineNumber;
        _valuesByHeader = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Count; i++)
        {
            var header = headers[i]?.Trim();
            if (string.IsNullOrEmpty(header))
            {
                continue;
            }

            _valuesByHeader[header] = i < values.Count ? values[i] : string.Empty;
        }
    }

    public IReadOnlyList<string> Values { get; }
    public int LineNumber { get; }

    public bool TryGetString(string header, out string value)
    {
        if (_valuesByHeader.TryGetValue(header, out value))
        {
            value = value?.Trim() ?? string.Empty;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public bool TryGetInt(string header, out int value)
    {
        value = 0;
        return TryGetString(header, out var text) && int.TryParse(text, out value);
    }
}
