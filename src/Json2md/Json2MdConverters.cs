using System.Collections;
using System.Text.RegularExpressions;

namespace Json2md;

internal static partial class Json2MdConverters
{
    private static string GenerateHeader(int repeat, object? input, IConvertCallback convert)
    {
        return new string('#', repeat) + " " + convert.Convert(input);
    }

    public static string H1(object? input, IConvertCallback convert) => GenerateHeader(1, input, convert);

    public static string H2(object? input, IConvertCallback convert) => GenerateHeader(2, input, convert);

    public static string H3(object? input, IConvertCallback convert) => GenerateHeader(3, input, convert);

    public static string H4(object? input, IConvertCallback convert) => GenerateHeader(4, input, convert);

    public static string H5(object? input, IConvertCallback convert) => GenerateHeader(5, input, convert);

    public static string H6(object? input, IConvertCallback convert) => GenerateHeader(6, input, convert);

    public static string Blockquote(object? input, IConvertCallback convert)
    {
        return convert.Convert(input, "> ");
    }

    public static string Img(object? input, IConvertCallback convert)
    {
        if (input is IList list)
        {
            return convert.Convert(list, "", "img");
        }

        if (input is string str)
        {
            return ImgObject("", str, "");
        }

        if (input is System.Collections.IDictionary dict)
        {
            var source = GetString(dict, "source");
            var title = GetString(dict, "title");
            var alt = GetString(dict, "alt");
            return ImgObject(title, source, alt);
        }

        return string.Empty;
    }

    private static string ImgObject(string title, string source, string alt)
    {
        return $"![{alt}]({source} \"{title}\")";
    }

    public static string Ul(object? input, IConvertCallback convert)
    {
        var c = "";
        if (input is not IList list)
        {
            return c;
        }

        for (var i = 0; i < list.Count; i++)
        {
            var marker = "";
            var type = GetFirstKey(list[i]);

            if (type is not "ul" and not "ol" and not "taskLists")
            {
                marker += "\n - ";
            }

            c += marker + ParseTextFormat(Json2Md.IndentWithSpaces(convert.Convert(list[i]), 4, true));
        }

        return c;
    }

    public static string Ol(object? input, IConvertCallback convert)
    {
        var c = "";
        var jumpCount = 0;

        if (input is not IList list)
        {
            return c;
        }

        for (var i = 0; i < list.Count; i++)
        {
            var marker = "";
            var type = GetFirstKey(list[i]);

            if (type is not "ul" and not "ol" and not "taskLists")
            {
                marker = "\n " + (i + 1 - jumpCount) + ". ";
            }
            else
            {
                jumpCount++;
            }

            c += marker + ParseTextFormat(Json2Md.IndentWithSpaces(convert.Convert(list[i]), 4, true));
        }

        return c;
    }

    public static string TaskLists(object? input, IConvertCallback convert)
    {
        var c = "";
        if (input is not IList list)
        {
            return c;
        }

        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            var marker = "";

            if (item is IDictionary dict)
            {
                var type = GetFirstKey(item);

                if (type is not "ul" and not "ol" and not "taskLists")
                {
                    var isDone = dict.Contains("isDone") && dict["isDone"] is true;
                    marker += isDone ? "\n - [x] " : "\n - [ ] ";
                }

                var title = dict.Contains("title") ? dict["title"] : item;

                c += marker + ParseTextFormat(Json2Md.IndentWithSpaces(convert.Convert(title), 4, true));
            }
            else
            {
                marker += "\n - [ ] ";

                c += marker + ParseTextFormat(Json2Md.IndentWithSpaces(convert.Convert(item), 4, true));
            }
        }

        return c;
    }

    public static string Code(object? input, IConvertCallback convert)
    {
        if (input is not System.Collections.IDictionary dict)
        {
            return string.Empty;
        }

        var language = GetString(dict, "language");
        var c = "```" + language + "\n";
        var content = dict["content"];

        if (content is System.Collections.IList contentList)
        {
            var lines = new List<string>();
            for (var i = 0; i < contentList.Count; i++)
            {
                lines.Add(contentList[i]?.ToString() ?? string.Empty);
            }

            c += string.Join("\n", lines);
        }
        else
        {
            c += content?.ToString() ?? string.Empty;
        }

        c += "\n```";

        return c;
    }

    public static string P(object? input, IConvertCallback convert)
    {
        return ParseTextFormat(convert.Convert(input, "\n"));
    }

    public static string Table(object? input, IConvertCallback convert)
    {
        if (input is not System.Collections.IDictionary dict)
        {
            return "";
        }

        if (!dict.Contains("headers") || !dict.Contains("rows"))
        {
            return "";
        }

        var headersObj = dict["headers"];
        var rowsObj = dict["rows"];
        var pretty = dict.Contains("pretty") && dict["pretty"] is true;

        if (headersObj is not IList headersList || rowsObj is not IList rowsList)
        {
            return "";
        }

        var headers = new string[headersList.Count];

        for (var i = 0; i < headersList.Count; i++)
        {
            headers[i] = headersList[i]?.ToString() ?? string.Empty;
        }

        var alignsObj = dict.Contains("aligns") ? dict["aligns"] : null;

        var aligns = new string[headers.Length];

        for (var i = 0; i < headers.Length; i++)
        {
            if (alignsObj is IList alignsList && i < alignsList.Count)
            {
                var alignValue = alignsList[i]?.ToString() ?? "";
                aligns[i] = string.IsNullOrEmpty(alignValue) ? "none" : alignValue;
            }
            else
            {
                aligns[i] = "none";
            }
        }

        var preferredLengthPerAlignment = new Dictionary<string, int>
        {
            ["center"] = 3,
            ["right"] = 2,
            ["left"] = 2,
            ["none"] = 1,
        };

        var preferredLengths = new int[headers.Length];

        for (var i = 0; i < headers.Length; i++)
        {
            preferredLengths[i] = Math.Max(
                preferredLengthPerAlignment.GetValueOrDefault(aligns[i], 1),
                headers[i].Length - 2);
        }

        var rows = new List<System.Collections.IList>();

        for (var r = 0; r < rowsList.Count; r++)
        {
            var row = rowsList[r];

            if (row is IList rowList)
            {
                rows.Add(rowList);
            }
            else if (row is IDictionary rowDict)
            {
                var arrayRow = new object?[headers.Length];

                for (var h = 0; h < headers.Length; h++)
                {
                    arrayRow[h] = rowDict.Contains(headers[h]) ? rowDict[headers[h]] : "";
                }

                rows.Add(arrayRow);
            }
        }

        if (pretty)
        {
            foreach (var row in rows)
            {
                for (var j = 0; j < row.Count && j < preferredLengths.Length; j++)
                {
                    var cellStr = row[j]?.ToString() ?? "";

                    preferredLengths[j] = Math.Max(preferredLengths[j], cellStr.Length - 2);
                }
            }
        }

        var columnNames = new string[headers.Length];

        for (var i = 0; i < headers.Length; i++)
        {
            columnNames[i] = FillTh(headers[i], i, aligns, preferredLengths);
        }

        var headerLine = "| " + string.Join(" | ", columnNames) + " |";

        var separatorParts = new string[headers.Length];

        for (var i = 0; i < headers.Length; i++)
        {
            var inner = new string('-', preferredLengths[i]);

            separatorParts[i] = aligns[i] switch
            {
                "center" => ":" + inner + ":",
                "right" => "-" + inner + ":",
                "left" => ":" + inner + "-",
                _ => "-" + inner + "-",
            };
        }

        var separatorLine = "| " + string.Join(" | ", separatorParts) + " |";

        var dataLines = new List<string>();

        foreach (var row in rows)
        {
            var cells = new string[row.Count];
            for (var j = 0; j < row.Count; j++)
            {
                var cell = convert.Convert(row[j]);
                cell = ParseTextFormat(cell);
                cell = EscapePipeRegex().Replace(cell, "$1\\|");
                cell = cell.Trim();

                if (pretty)
                {
                    cell = FillTd(cell, j, aligns, preferredLengths);
                }

                cells[j] = cell;
            }

            dataLines.Add("| " + string.Join(" | ", cells) + " |");
        }

        var data = string.Join("\n", dataLines);

        return headerLine + "\n" + separatorLine + "\n" + data;
    }

    public static string Link(object? input, IConvertCallback convert)
    {
        switch (input)
        {
            case IList list:
                return convert.Convert(list, "", "link");
            case string str:
                return $"[]({str})";
            case IDictionary dict:
                {
                    var title = GetString(dict, "title");
                    var source = GetString(dict, "source");

                    return $"[{title}]({source})";
                }
            default:
                return string.Empty;
        }
    }

    public static string Hr(object? input, IConvertCallback convert)
    {
        return "---";
    }

    internal static string ParseTextFormat(string text)
    {
        text = StrongRegex().Replace(text, "**");
        text = BoldRegex().Replace(text, "**");
        text = EmRegex().Replace(text, "*");
        text = ItalicRegex().Replace(text, "*");
        text = UnderlineRegex().Replace(text, "_");
        text = StrikeRegex().Replace(text, "~~");

        return text;
    }

    private static string FillTh(string header, int index, string[] aligns, int[] preferredLengths)
    {
        var diff = preferredLengths[index] + 2 - header.Length;

        return aligns[index] switch
        {
            "right" => FillRight(diff, header),
            "left" => FillLeft(diff, header),
            _ => FillCenter(diff, header),
        };
    }

    private static string FillTd(string header, int index, string[] aligns, int[] preferredLengths)
    {
        var diff = preferredLengths[index] + 2 - header.Length;

        return aligns[index] switch
        {
            "right" => FillRight(diff, header),
            "center" => FillCenter(diff, header),
            _ => FillLeft(diff, header),
        };
    }

    private static string FillRight(int diff, string header)
    {
        return new string(' ', diff) + header;
    }

    private static string FillLeft(int diff, string header)
    {
        return header + new string(' ', diff);
    }

    private static string FillCenter(int diff, string header)
    {
        return new string(' ', diff / 2) + header + new string(' ', (diff + 1) / 2);
    }

    private static string GetFirstKey(object? item)
    {
        if (item is IDictionary dict)
        {
            foreach (DictionaryEntry entry in dict)
            {
                return (string)entry.Key;
            }
        }

        return "";
    }

    private static string GetString(IDictionary dict, string key)
    {
        if (!dict.Contains(key))
        {
            return "";
        }

        return dict[key]?.ToString() ?? "";
    }

    [GeneratedRegex(@"</?strong\>", RegexOptions.IgnoreCase)]
    private static partial Regex StrongRegex();

    [GeneratedRegex(@"</?bold\>", RegexOptions.IgnoreCase)]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"</?em\>", RegexOptions.IgnoreCase)]
    private static partial Regex EmRegex();

    [GeneratedRegex(@"</?italic\>", RegexOptions.IgnoreCase)]
    private static partial Regex ItalicRegex();

    [GeneratedRegex(@"</?u\>", RegexOptions.IgnoreCase)]
    private static partial Regex UnderlineRegex();

    [GeneratedRegex(@"</?strike\>", RegexOptions.IgnoreCase)]
    private static partial Regex StrikeRegex();

    [GeneratedRegex(@"([^\\])\|")]
    private static partial Regex EscapePipeRegex();
}
