# Json2md

A .NET library that converts JSON objects into Markdown strings. Ported from the original [json2md](https://github.com/IonicaBizau/json2md) JavaScript library by [Ionică Bizău](https://github.com/IonicaBizau).

## Installation

```bash
dotnet add package BlueCurve.Json2md
```

## Quick Start

```csharp
using Json2md;

var markdown = Json2Md.Convert(new Dictionary<string, object?>
{
    ["h1"] = "Hello World",
    ["p"] = "This is a paragraph.",
});

// # Hello World
//
// This is a paragraph.
```

## Supported Converters

| Key           | Description              | Input Type                                    |
|---------------|--------------------------|-----------------------------------------------|
| `h1` - `h6`  | Headings                 | `string`                                      |
| `blockquote`  | Blockquote               | `string` or `number`                          |
| `img`         | Image                    | `{ source, title, alt }` or `List` of those   |
| `link`        | Link                     | `{ source, title }`                           |
| `ul`          | Unordered list           | `List<string>`                                |
| `ol`          | Ordered list             | `List<string>`                                |
| `taskLists`   | Task list                | `List` of `{ title, isDone? }` or `string`    |
| `code`        | Code block               | `{ language, content }`                       |
| `p`           | Paragraphs               | `List<string>`                                |
| `table`       | Table                    | `{ headers, rows, aligns?, pretty? }`         |
| `hr`          | Horizontal rule          | `string` (ignored)                            |

## Usage Examples

### Headings

```csharp
Json2Md.Convert(new Dictionary<string, object?> { ["h1"] = "Title" });
// # Title

Json2Md.Convert(new Dictionary<string, object?> { ["h3"] = "Subtitle" });
// ### Subtitle
```

### Lists

```csharp
Json2Md.Convert(new Dictionary<string, object?>
{
    ["ul"] = new List<object?> { "Item 1", "Item 2" },
});
//
//  - Item 1
//  - Item 2

Json2Md.Convert(new Dictionary<string, object?>
{
    ["ol"] = new List<object?> { "First", "Second" },
});
//
//  1. First
//  2. Second
```

### Task Lists

```csharp
Json2Md.Convert(new Dictionary<string, object?>
{
    ["taskLists"] = new List<object?>
    {
        new Dictionary<string, object?> { ["title"] = "Buy milk" },
        new Dictionary<string, object?> { ["title"] = "Ship it", ["isDone"] = true },
    },
});
//
//  - [ ] Buy milk
//  - [x] Ship it
```

### Code Blocks

```csharp
Json2Md.Convert(new Dictionary<string, object?>
{
    ["code"] = new Dictionary<string, object?>
    {
        ["language"] = "csharp",
        ["content"] = new List<object?> { "var x = 42;", "Console.WriteLine(x);" },
    },
});
// ```csharp
// var x = 42;
// Console.WriteLine(x);
// ```
```

### Tables

```csharp
Json2Md.Convert(new Dictionary<string, object?>
{
    ["table"] = new Dictionary<string, object?>
    {
        ["headers"] = new List<object?> { "Name", "Age" },
        ["rows"] = new List<object?>
        {
            new List<object?> { "Alice", 30 },
            new List<object?> { "Bob", 25 },
        },
    },
});
// | Name  | Age |
// | ----- | --- |
// | Alice | 30  |
// | Bob   | 25  |
```

Tables also support object-based rows, column alignment, and pretty-printing:

```csharp
Json2Md.Convert(new Dictionary<string, object?>
{
    ["table"] = new Dictionary<string, object?>
    {
        ["pretty"] = true,
        ["headers"] = new List<object?> { "Name", "Score" },
        ["rows"] = new List<object?>
        {
            new Dictionary<string, object?> { ["Name"] = "Alice", ["Score"] = 100 },
        },
        ["aligns"] = new List<object?> { "left", "right" },
    },
});
// | Name  | Score |
// | :---- | ----: |
// | Alice |   100 |
```

### Images and Links

```csharp
Json2Md.Convert(new Dictionary<string, object?>
{
    ["img"] = new Dictionary<string, object?>
    {
        ["source"] = "https://example.com/logo.png",
        ["title"] = "Logo",
        ["alt"] = "Company Logo",
    },
});
// ![Company Logo](https://example.com/logo.png "Logo")

Json2Md.Convert(new Dictionary<string, object?>
{
    ["link"] = new Dictionary<string, object?>
    {
        ["source"] = "https://example.com",
        ["title"] = "Example",
    },
});
// [Example](https://example.com)
```

### Text Formatting

Strings support inline HTML-like tags that are converted to Markdown formatting:

| Tag                  | Markdown Output |
|----------------------|-----------------|
| `<bold>text</bold>`  | `**text**`      |
| `<strong>text</strong>` | `**text**`   |
| `<em>text</em>`      | `*text*`        |
| `<italic>text</italic>` | `*text*`     |
| `<u>text</u>`        | `_text_`        |
| `<strike>text</strike>` | `~~text~~`   |

```csharp
Json2Md.Convert(new Dictionary<string, object?>
{
    ["p"] = new List<object?> { "This is <bold>important</bold> and <em>emphasized</em>." },
});
//
// This is **important** and *emphasized*.
```

### Multiple Elements

```csharp
Json2Md.Convert(new List<object?>
{
    new Dictionary<string, object?> { ["h1"] = "Title" },
    new Dictionary<string, object?> { ["p"] = "First paragraph." },
    new Dictionary<string, object?> { ["p"] = "Second paragraph." },
});
// # Title
//
// First paragraph.
//
// Second paragraph.
```

### Async API

```csharp
var result = await Json2Md.ConvertAsync(new Dictionary<string, object?>
{
    ["h1"] = "Async Heading",
});
// # Async Heading
```

### Custom Converters

```csharp
Json2Md.Converters["alert"] = (input, convert) => $"> [!WARNING]\n> {input}\n";

Json2Md.Convert(new Dictionary<string, object?> { ["alert"] = "Disk space low" });
// > [!WARNING]
// > Disk space low
```

Async custom converters are also supported:

```csharp
Json2Md.AsyncConverters["fetchContent"] = async (input, convert) =>
{
    var content = await FetchFromSomewhere(input?.ToString() ?? "");
    return content;
};
```

### System.Text.Json Support

The library accepts `JsonDocument` and `JsonElement` directly, automatically converting JSON types to .NET types:

```csharp
using System.Text.Json;

var json = """{"name": "Alice", "age": 30, "active": true, "tags": ["admin", "user"]}""";
using var doc = JsonDocument.Parse(json);

Json2Md.Convert(doc);
// or
Json2Md.Convert(doc.RootElement);

// **Name:** Alice
// **Age:** 30
// **Active:** true
// **Tags:**
//
//  - admin
//  - user
```

### Auto-Render for Arbitrary JSON

When dictionary keys don't match registered converters, they are automatically rendered as readable markdown with smart type detection:

```csharp
Json2Md.Convert(new Dictionary<string, object?>
{
    ["confidence"] = "High",
    ["status"] = "Evaluating",
    ["estimatedValue"] = null,  // null values are skipped
    ["types"] = new List<object?> { "marketcap up 15%" },
});
// **Confidence:** High
// **Status:** Evaluating
// **Types:**
//
//  - marketcap up 15%
```

Keys are automatically converted from camelCase to Title Case (e.g., `estimatedValue` → `Estimated Value`).

## Original Library

This is a .NET port of [json2md](https://github.com/IonicaBizau/json2md) (v2.0.3) by [Ionică Bizău](https://github.com/IonicaBizau). The API and behavior are preserved as closely as possible, including edge cases.

## License

MIT License

Copyright (c) 2026 BlueCurve Corp

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
