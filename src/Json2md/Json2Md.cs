namespace Json2md;

/// <summary>
/// Callback interface for synchronous markdown conversion.
/// </summary>
public interface IConvertCallback
{
    /// <summary>
    /// Converts the specified data to markdown.
    /// </summary>
    /// <param name="data">The data to convert.</param>
    /// <returns>The generated markdown string.</returns>
    string Convert(object? data);

    /// <summary>
    /// Converts the specified data to markdown with a prefix applied to each line.
    /// </summary>
    /// <param name="data">The data to convert.</param>
    /// <param name="prefix">A snippet to add before each line.</param>
    /// <returns>The generated markdown string.</returns>
    string Convert(object? data, string prefix);

    /// <summary>
    /// Converts the specified data to markdown using a specific converter type.
    /// </summary>
    /// <param name="data">The data to convert.</param>
    /// <param name="prefix">A snippet to add before each line.</param>
    /// <param name="type">The converter type to use.</param>
    /// <returns>The generated markdown string.</returns>
    string Convert(object? data, string prefix, string? type);
}

/// <summary>
/// Callback interface for asynchronous markdown conversion.
/// </summary>
public interface IConvertAsyncCallback
{
    /// <summary>
    /// Asynchronously converts the specified data to markdown.
    /// </summary>
    /// <param name="data">The data to convert.</param>
    /// <returns>A task representing the generated markdown string.</returns>
    Task<string> ConvertAsync(object? data);

    /// <summary>
    /// Asynchronously converts the specified data to markdown with a prefix applied to each line.
    /// </summary>
    /// <param name="data">The data to convert.</param>
    /// <param name="prefix">A snippet to add before each line.</param>
    /// <returns>A task representing the generated markdown string.</returns>
    Task<string> ConvertAsync(object? data, string prefix);

    /// <summary>
    /// Asynchronously converts the specified data to markdown using a specific converter type.
    /// </summary>
    /// <param name="data">The data to convert.</param>
    /// <param name="prefix">A snippet to add before each line.</param>
    /// <param name="type">The converter type to use.</param>
    /// <returns>A task representing the generated markdown string.</returns>
    Task<string> ConvertAsync(object? data, string prefix, string? type);
}

/// <summary>
/// Converts JSON-like data structures to Markdown output.
/// </summary>
public static class Json2Md
{
    /// <summary>
    /// Delegate for synchronous converter functions.
    /// </summary>
    /// <param name="input">The converter input data.</param>
    /// <param name="convert">Callback to recursively convert nested data.</param>
    /// <returns>The generated markdown string.</returns>
    public delegate string Converter(object? input, IConvertCallback convert);

    /// <summary>
    /// Delegate for asynchronous converter functions.
    /// </summary>
    /// <param name="input">The converter input data.</param>
    /// <param name="convert">Callback to recursively convert nested data asynchronously.</param>
    /// <returns>A task representing the generated markdown string.</returns>
    public delegate Task<string> AsyncConverter(object? input, IConvertAsyncCallback convert);

    /// <summary>
    /// Gets the dictionary of registered synchronous converters.
    /// Add or replace entries to extend conversion capabilities.
    /// </summary>
    public static Dictionary<string, Converter> Converters { get; } = new()
    {
        ["h1"] = Json2MdConverters.H1,
        ["h2"] = Json2MdConverters.H2,
        ["h3"] = Json2MdConverters.H3,
        ["h4"] = Json2MdConverters.H4,
        ["h5"] = Json2MdConverters.H5,
        ["h6"] = Json2MdConverters.H6,
        ["blockquote"] = Json2MdConverters.Blockquote,
        ["img"] = Json2MdConverters.Img,
        ["ul"] = Json2MdConverters.Ul,
        ["ol"] = Json2MdConverters.Ol,
        ["taskLists"] = Json2MdConverters.TaskLists,
        ["code"] = Json2MdConverters.Code,
        ["p"] = Json2MdConverters.P,
        ["table"] = Json2MdConverters.Table,
        ["link"] = Json2MdConverters.Link,
        ["hr"] = Json2MdConverters.Hr,
    };

    /// <summary>
    /// Gets the dictionary of registered asynchronous converters.
    /// Add entries to support async conversion operations.
    /// </summary>
    public static Dictionary<string, AsyncConverter> AsyncConverters { get; } = [];

    /// <summary>
    /// Converts the specified data to Markdown.
    /// </summary>
    /// <param name="data">The input data: string, number, list, or dictionary.</param>
    /// <param name="prefix">A snippet to add before each line of output.</param>
    /// <param name="type">An optional converter type to force for all elements.</param>
    /// <returns>The generated markdown string.</returns>
    public static string Convert(object? data, string prefix = "", string? type = null)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        if (data is null)
        {
            return string.Empty;
        }

        if (data is string str)
        {
            return IndentWithPrefix(str, prefix);
        }

        if (data is int or long or float or double or decimal or short or byte or uint or ulong or ushort or sbyte)
        {
            return IndentWithPrefix(System.Convert.ToString(data, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty, prefix);
        }

        if (data is System.Collections.IList list)
        {
            var content = new List<string>();
            for (var i = 0; i < list.Count; i++)
            {
                content.Add(IndentWithPrefix(Convert(list[i], "", type), prefix));
            }

            return string.Join("\n", content);
        }

        if (data is System.Collections.IDictionary dict)
        {
            var mdText = "";
            var callback = new SyncCallback();

            if (type is not null)
            {
                if (!Converters.TryGetValue(type, out var func))
                {
                    throw new ArgumentException($"There is no such converter: {type}", nameof(type));
                }

                mdText += IndentWithPrefix(func(data, callback), prefix) + "\n";
                return mdText;
            }

            foreach (System.Collections.DictionaryEntry entry in dict)
            {
                var entryType = (string)entry.Key;
                if (Converters.TryGetValue(entryType, out var func))
                {
                    mdText += IndentWithPrefix(func(entry.Value, callback), prefix) + "\n";
                }
                else
                {
                    mdText += IndentWithPrefix(AutoRenderKeyValue(entryType, entry.Value, callback), prefix);
                }
            }

            return mdText;
        }

        return IndentWithPrefix(data.ToString() ?? string.Empty, prefix);
    }

    /// <summary>
    /// Asynchronously converts the specified data to Markdown.
    /// Supports both synchronous and asynchronous converters.
    /// </summary>
    /// <param name="data">The input data: string, number, list, or dictionary.</param>
    /// <param name="prefix">A snippet to add before each line of output.</param>
    /// <param name="type">An optional converter type to force for all elements.</param>
    /// <returns>A task representing the generated markdown string.</returns>
    public static async Task<string> ConvertAsync(object? data, string prefix = "", string? type = null)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        if (data is null)
        {
            return string.Empty;
        }

        if (data is string str)
        {
            return IndentWithPrefix(str, prefix);
        }

        if (data is int or long or float or double or decimal or short or byte or uint or ulong or ushort or sbyte)
        {
            return IndentWithPrefix(System.Convert.ToString(data, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty, prefix);
        }

        if (data is System.Collections.IList list)
        {
            var tasks = new Task<string>[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                var index = i;
                tasks[index] = ProcessAsyncItem(list[index], prefix, type);
            }

            var results = await Task.WhenAll(tasks);
            return string.Join("\n", results);
        }

        if (data is System.Collections.IDictionary dict)
        {
            var keys = new List<string>();
            foreach (System.Collections.DictionaryEntry entry in dict)
            {
                keys.Add((string)entry.Key);
            }

            if (keys.Count == 0)
            {
                return string.Empty;
            }

            var firstKey = type ?? keys[0];
            var asyncCallback = new AsyncCallback();

            if (AsyncConverters.TryGetValue(firstKey, out var asyncFunc))
            {
                var result = await asyncFunc(dict[firstKey], asyncCallback);
                return IndentWithPrefix(result, prefix) + "\n";
            }

            if (Converters.TryGetValue(firstKey, out var syncFunc))
            {
                var syncCallback = new SyncCallback();
                var result = syncFunc(dict[firstKey], syncCallback);
                return IndentWithPrefix(result, prefix) + "\n";
            }

            var autoResult = AutoRenderKeyValue(firstKey, dict[firstKey], new SyncCallback());
            return IndentWithPrefix(autoResult, prefix);
        }

        return IndentWithPrefix(data.ToString() ?? string.Empty, prefix);
    }

    private static async Task<string> ProcessAsyncItem(object? item, string prefix, string? type)
    {
        var result = await ConvertAsync(item, "", type);
        return IndentWithPrefix(result, prefix);
    }

    private static string AutoRenderKeyValue(string key, object? value, IConvertCallback convert)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var label = FormatKey(key);

        if (value is string str)
        {
            return $"**{label}:** {str}\n";
        }

        if (value is bool b)
        {
            return $"**{label}:** {(b ? "true" : "false")}\n";
        }

        if (value is int or long or float or double or decimal or short or byte or uint or ulong or ushort or sbyte)
        {
            return $"**{label}:** {System.Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)}\n";
        }

        if (value is System.Collections.IList list)
        {
            if (list.Count == 0)
            {
                return string.Empty;
            }

            var items = new List<string>();
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item is null)
                {
                    continue;
                }

                if (item is System.Collections.IDictionary or System.Collections.IList)
                {
                    items.Add($" - {convert.Convert(item)}");
                }
                else
                {
                    items.Add($" - {item}");
                }
            }

            if (items.Count == 0)
            {
                return string.Empty;
            }

            return $"**{label}:**\n\n{string.Join("\n", items)}\n";
        }

        if (value is System.Collections.IDictionary nestedDict)
        {
            var nested = convert.Convert(nestedDict);
            if (string.IsNullOrWhiteSpace(nested))
            {
                return string.Empty;
            }

            return $"**{label}:**\n\n{nested}\n";
        }

        return $"**{label}:** {value}\n";
    }

    private static string FormatKey(string key)
    {
        if (key.Length == 0)
        {
            return key;
        }

        var result = new System.Text.StringBuilder();
        for (var i = 0; i < key.Length; i++)
        {
            var c = key[i];
            if (i == 0)
            {
                result.Append(char.ToUpperInvariant(c));
            }
            else if (char.IsUpper(c))
            {
                result.Append(' ');
                result.Append(c);
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    internal static string IndentWithPrefix(string content, string prefix)
    {
        if (prefix.Length == 0)
        {
            return content;
        }

        var lines = content.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = prefix + lines[i];
        }

        return string.Join("\n", lines);
    }

    internal static string IndentWithSpaces(string content, int spaces, bool ignoreFirst)
    {
        var lines = content.Split('\n');

        if (ignoreFirst)
        {
            if (lines.Length <= 1)
            {
                return string.Join("\n", lines);
            }

            var rest = string.Join("\n", lines.AsSpan(1).ToArray());
            return lines[0] + "\n" + IndentWithSpaces(rest, spaces, false);
        }

        var spaceStr = new string(' ', spaces);
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = spaceStr + lines[i];
        }

        return string.Join("\n", lines);
    }

    private sealed class SyncCallback : IConvertCallback
    {
        public string Convert(object? data) => Json2Md.Convert(data);

        public string Convert(object? data, string prefix) => Json2Md.Convert(data, prefix);

        public string Convert(object? data, string prefix, string? type) => Json2Md.Convert(data, prefix, type);
    }

    private sealed class AsyncCallback : IConvertAsyncCallback
    {
        public Task<string> ConvertAsync(object? data) => Json2Md.ConvertAsync(data);

        public Task<string> ConvertAsync(object? data, string prefix) => Json2Md.ConvertAsync(data, prefix);

        public Task<string> ConvertAsync(object? data, string prefix, string? type) => Json2Md.ConvertAsync(data, prefix, type);
    }
}
