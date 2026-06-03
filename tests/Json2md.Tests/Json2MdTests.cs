using TUnit;

namespace Json2md.Tests;

public sealed class Json2MdTests
{
    [Test]
    public async Task ShouldSupportHeadings()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["h1"] = "Heading 1" }))
            .IsEqualTo("# Heading 1\n");
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["h2"] = "Heading 2" }))
            .IsEqualTo("## Heading 2\n");
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["h3"] = "Heading 3" }))
            .IsEqualTo("### Heading 3\n");
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["h4"] = "Heading 4" }))
            .IsEqualTo("#### Heading 4\n");
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["h5"] = "Heading 5" }))
            .IsEqualTo("##### Heading 5\n");
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["h6"] = "Heading 6" }))
            .IsEqualTo("###### Heading 6\n");
    }

    [Test]
    public async Task ShouldSupportBlockquotes()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["blockquote"] = "Some content" }))
            .IsEqualTo("> Some content\n");
    }

    [Test]
    public async Task ShouldSupportImages()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["img"] = new Dictionary<string, object?>
            {
                ["source"] = "source",
                ["title"] = "title",
                ["alt"] = "alt",
            },
        })).IsEqualTo("![alt](source \"title\")\n");
    }

    [Test]
    public async Task ShouldSupportArrayOfImages()
    {
        var expected = string.Join("\n",
            "![alt](source \"title\")",
            "",
            "![salt](sauce \"heading\")",
            "",
            "");

        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["img"] = new List<object?>
            {
                new Dictionary<string, object?>
                {
                    ["source"] = "source",
                    ["title"] = "title",
                    ["alt"] = "alt",
                },
                new Dictionary<string, object?>
                {
                    ["source"] = "sauce",
                    ["title"] = "heading",
                    ["alt"] = "salt",
                },
            },
        })).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldSupportLinks()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["link"] = new Dictionary<string, object?>
            {
                ["source"] = "source",
                ["title"] = "title",
            },
        })).IsEqualTo("[title](source)\n");
    }

    [Test]
    public async Task ShouldSupportHorizontalRule()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["hr"] = "" }))
            .IsEqualTo("---\n");
    }

    [Test]
    public async Task ShouldSupportUnorderedLists()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["ul"] = new List<object?> { "item 1", "item 2" },
        })).IsEqualTo("\n - item 1\n - item 2\n");
    }

    [Test]
    public async Task ShouldSupportUnorderedListsWithEmphasisFormat()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["ul"] = new List<object?> { "<em>item 1</em>", "<bold>item 2</bold>" },
        })).IsEqualTo("\n - *item 1*\n - **item 2**\n");
    }

    [Test]
    public async Task ShouldSupportOrderedLists()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["ol"] = new List<object?> { "item 1", "item 2" },
        })).IsEqualTo("\n 1. item 1\n 2. item 2\n");
    }

    [Test]
    public async Task ShouldSupportTaskLists()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["taskLists"] = new List<object?>
            {
                new Dictionary<string, object?> { ["title"] = "item 1" },
                new Dictionary<string, object?> { ["title"] = "item 2", ["isDone"] = true },
                "item 3",
            },
        })).IsEqualTo("\n - [ ] item 1\n - [x] item 2\n - [ ] item 3\n");
    }

    [Test]
    public async Task ShouldSupportCodeBlocks()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["code"] = new Dictionary<string, object?>
            {
                ["language"] = "js",
                ["content"] = new List<object?>
                {
                    "function sum (a, b) {",
                    "   return a + b;",
                    "}",
                    "sum(1, 2);",
                },
            },
        })).IsEqualTo("```js\nfunction sum (a, b) {\n   return a + b;\n}\nsum(1, 2);\n```\n");
    }

    [Test]
    public async Task ShouldSupportParagraphs()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["p"] = new List<object?> { "Two", "Paragraphs" },
        })).IsEqualTo("\nTwo\n\nParagraphs\n");
    }

    [Test]
    public async Task ShouldSupportParagraphsWithBoldText()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["p"] = new List<object?>
            {
                "Two <bold>more words</bold>",
                "in this paragraph, <strong>right?</strong>",
            },
        })).IsEqualTo("\nTwo **more words**\n\nin this paragraph, **right?**\n");
    }

    [Test]
    public async Task ShouldSupportParagraphsWithUnderline()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["p"] = new List<object?>
            {
                "Two <u>more words</u>",
                "in this paragraph, <u>right?</u>",
            },
        })).IsEqualTo("\nTwo _more words_\n\nin this paragraph, _right?_\n");
    }

    [Test]
    public async Task ShouldSupportParagraphsWithStrikethrough()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["p"] = new List<object?>
            {
                "Two <strike>more words</strike>",
                "in this paragraph, <strike>right?</strike>",
            },
        })).IsEqualTo("\nTwo ~~more words~~\n\nin this paragraph, ~~right?~~\n");
    }

    [Test]
    [NotInParallel]
    public async Task ShouldSupportCustomTypes()
    {
        Json2Md.Converters["sayHello"] = (input, convert) => "Hello " + input + "!";

        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["sayHello"] = "World" }))
            .IsEqualTo("Hello World!\n");
    }

    [Test]
    public async Task ShouldCorrectlyIndentCodeBlocksInOrderedLists()
    {
        var expected = "\n 1. Copy the code below:\n    ```js\n    function sum (a, b) {\n       return a + b;\n    }\n    sum(1, 2);\n    ```\n    \n";

        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["ol"] = new List<object?>
            {
                new List<object?>
                {
                    "Copy the code below:",
                    new Dictionary<string, object?>
                    {
                        ["code"] = new Dictionary<string, object?>
                        {
                            ["language"] = "js",
                            ["content"] = new List<object?>
                            {
                                "function sum (a, b) {",
                                "   return a + b;",
                                "}",
                                "sum(1, 2);",
                            },
                        },
                    },
                },
            },
        })).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldCorrectlyIndentCodeBlocksInUnorderedLists()
    {
        var expected = "\n - Copy the code below:\n    ```js\n    function sum (a, b) {\n       return a + b;\n    }\n    sum(1, 2);\n    ```\n    \n";

        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["ul"] = new List<object?>
            {
                new List<object?>
                {
                    "Copy the code below:",
                    new Dictionary<string, object?>
                    {
                        ["code"] = new Dictionary<string, object?>
                        {
                            ["language"] = "js",
                            ["content"] = new List<object?>
                            {
                                "function sum (a, b) {",
                                "   return a + b;",
                                "}",
                                "sum(1, 2);",
                            },
                        },
                    },
                },
            },
        })).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldWorkWhenInputIsNumber()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?> { ["blockquote"] = 123 }))
            .IsEqualTo("> 123\n");
    }

    [Test]
    public async Task ShouldSupportTablesRowsIsObjects()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["table"] = new Dictionary<string, object?>
            {
                ["headers"] = new List<object?> { "a", "b" },
                ["rows"] = new List<object?>
                {
                    new Dictionary<string, object?> { ["a"] = "col1", ["b"] = "col2" },
                    new Dictionary<string, object?> { ["a"] = "col1", ["b"] = "col2 very long" },
                },
            },
        })).IsEqualTo("|  a  |  b  |\n| --- | --- |\n| col1 | col2 |\n| col1 | col2 very long |\n");
    }

    [Test]
    public async Task ShouldSupportTablesRowsIsArrays()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["table"] = new Dictionary<string, object?>
            {
                ["headers"] = new List<object?> { "a", "b" },
                ["rows"] = new List<object?>
                {
                    new List<object?> { "col1", "col2" },
                    new List<object?> { "col1", "col2" },
                },
            },
        })).IsEqualTo("|  a  |  b  |\n| --- | --- |\n| col1 | col2 |\n| col1 | col2 |\n");
    }

    [Test]
    public async Task ShouldSupportTablesAligns()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["table"] = new Dictionary<string, object?>
            {
                ["headers"] = new List<object?> { "a", "b", "c", "d" },
                ["rows"] = new List<object?>
                {
                    new List<object?> { "col1", "col2", "col3", "col4" },
                },
                ["aligns"] = new List<object?> { "", "center", "left", "right" },
            },
        })).IsEqualTo("|  a  |   b   | c    |    d |\n| --- | :---: | :--- | ---: |\n| col1 | col2 | col3 | col4 |\n");
    }

    [Test]
    public async Task ShouldSupportTablesAndMatchColumnNameLengthWithDashes()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["table"] = new Dictionary<string, object?>
            {
                ["headers"] = new List<object?> { "name", "amount", "somesuperlongword", "a" },
                ["rows"] = new List<object?>
                {
                    new List<object?> { "col1", "col2", "col3", "col4" },
                },
                ["aligns"] = new List<object?> { "", "center", "left", "right" },
            },
        })).IsEqualTo("| name | amount | somesuperlongword |    a |\n| ---- | :----: | :---------------- | ---: |\n| col1 | col2 | col3 | col4 |\n");
    }

    [Test]
    public async Task ShouldSupportTablesAndEscapePipes()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["table"] = new Dictionary<string, object?>
            {
                ["headers"] = new List<object?> { "a", "b" },
                ["rows"] = new List<object?>
                {
                    new List<object?> { "col|1|2", "col\\|3\\|4" },
                },
            },
        })).IsEqualTo("|  a  |  b  |\n| --- | --- |\n| col\\|1\\|2 | col\\|3\\|4 |\n");
    }

    [Test]
    public async Task ShouldSupportPrettyTablesRowsIsObjects()
    {
        var expected = "|  a   |       b        |\n| ---- | -------------- |\n| 1000 | col2           |\n| 1000 | col2 very long |\n";

        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["table"] = new Dictionary<string, object?>
            {
                ["pretty"] = true,
                ["headers"] = new List<object?> { "a", "b" },
                ["rows"] = new List<object?>
                {
                    new Dictionary<string, object?> { ["a"] = 1000, ["b"] = "col2" },
                    new Dictionary<string, object?> { ["a"] = 1000, ["b"] = "col2 very long" },
                },
            },
        })).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldSupportPrettyTablesRowsIsArray()
    {
        var expected = "|  a   |       b        |\n| ---- | -------------- |\n| col1 | col2           |\n| col1 | col2 very long |\n";

        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["table"] = new Dictionary<string, object?>
            {
                ["pretty"] = true,
                ["headers"] = new List<object?> { "a", "b" },
                ["rows"] = new List<object?>
                {
                    new List<object?> { "col1", "col2" },
                    new List<object?> { "col1", "col2 very long" },
                },
            },
        })).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldSupportPrettyTablesAligns()
    {
        var expected = "| a    |              b |\n| :--- | -------------: |\n| col1 |           col2 |\n| col1 | col2 very long |\n";

        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["table"] = new Dictionary<string, object?>
            {
                ["pretty"] = true,
                ["aligns"] = new List<object?> { "left", "right" },
                ["headers"] = new List<object?> { "a", "b" },
                ["rows"] = new List<object?>
                {
                    new List<object?> { "col1", "col2" },
                    new List<object?> { "col1", "col2 very long" },
                },
            },
        })).IsEqualTo(expected);
    }

    [Test]
    [NotInParallel]
    public async Task ShouldSupportSeveralTopLevelObjectKeys()
    {
        Json2Md.Converters["sayHello"] = (input, convert) => "Hello " + input + "!";

        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["sayHello"] = "World",
            ["h1"] = "Hello Friends!",
        })).IsEqualTo("Hello World!\n# Hello Friends!\n");
    }

    [Test]
    public async Task ShouldSupportTablesWithLinks()
    {
        await Assert.That(Json2Md.Convert(new Dictionary<string, object?>
        {
            ["table"] = new Dictionary<string, object?>
            {
                ["headers"] = new List<object?> { "a", "b" },
                ["rows"] = new List<object?>
                {
                    new Dictionary<string, object?>
                    {
                        ["a"] = new Dictionary<string, object?>
                        {
                            ["link"] = new Dictionary<string, object?>
                            {
                                ["title"] = "aTitle",
                                ["source"] = "http://www.example.com",
                            },
                        },
                        ["b"] = "col2",
                    },
                },
            },
        })).IsEqualTo("|  a  |  b  |\n| --- | --- |\n| [aTitle](http://www.example.com) | col2 |\n");
    }
}
