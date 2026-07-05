/**
 * CsvParser — minimal RFC 4180-style CSV tokenizer.
 *
 * Handles quoted fields (including embedded commas, quotes and newlines)
 * and both \r\n and \n line endings. Does not attempt delimiter sniffing —
 * always splits on comma, which matches Excel's "CSV UTF-8" export.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using System.Text;

namespace OSPTracker.Utils
{
    public static class CsvParser
    {
        /// <summary>Parses CSV text into rows of raw string fields. Blank trailing lines are omitted.</summary>
        public static List<string[]> Parse(string text)
        {
            var rows  = new List<string[]>();
            var row   = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;
            int i = 0, len = text.Length;

            while (i < len)
            {
                char c = text[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < len && text[i + 1] == '"') { field.Append('"'); i += 2; continue; }
                        inQuotes = false; i++; continue;
                    }
                    field.Append(c); i++; continue;
                }

                switch (c)
                {
                    case '"':
                        inQuotes = true; i++; break;
                    case ',':
                        row.Add(field.ToString()); field.Clear(); i++; break;
                    case '\r':
                        i++; break;
                    case '\n':
                        row.Add(field.ToString()); field.Clear();
                        rows.Add(row.ToArray()); row = new List<string>();
                        i++; break;
                    default:
                        field.Append(c); i++; break;
                }
            }

            // Flush the final field/row if the file didn't end with a newline
            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                rows.Add(row.ToArray());
            }

            return rows;
        }
    }
}
