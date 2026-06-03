using TUnit;

namespace Json2md.Tests;

public sealed class Json2MdAsyncTests
{
    [Test]
    public async Task ShouldReturnTaskInstance()
    {
        var task = Json2Md.ConvertAsync(new Dictionary<string, object?> { ["h1"] = "Heading 1" });
        await Assert.That(task is not null).IsTrue();
        var result = await task!;
        await Assert.That(result).IsEqualTo("# Heading 1\n");
    }

    [Test]
    public async Task ShouldAcceptAnArray()
    {
        var result = await Json2Md.ConvertAsync(new List<object?>
        {
            new Dictionary<string, object?> { ["h1"] = "Heading 1" },
            new Dictionary<string, object?> { ["h2"] = "Heading 2" },
        });

        await Assert.That(result).IsEqualTo("# Heading 1\n\n## Heading 2\n");
    }

    [Test]
    public async Task ShouldHaveSameBehaviorsToOriginalJson2Md()
    {
        var h1Result = await Json2Md.ConvertAsync(new Dictionary<string, object?> { ["h1"] = "Heading 1" });
        await Assert.That(h1Result).IsEqualTo("# Heading 1\n");

        var h2Result = await Json2Md.ConvertAsync(new Dictionary<string, object?> { ["h2"] = "Heading 2" });
        await Assert.That(h2Result).IsEqualTo("## Heading 2\n");

        var h3Result = await Json2Md.ConvertAsync(new Dictionary<string, object?> { ["h3"] = "Heading 3" });
        await Assert.That(h3Result).IsEqualTo("### Heading 3\n");

        var bqResult = await Json2Md.ConvertAsync(new Dictionary<string, object?> { ["blockquote"] = "Some content" });
        await Assert.That(bqResult).IsEqualTo("> Some content\n");
    }

    [Test]
    public async Task ShouldSupportCustomTypesAsync()
    {
        Json2Md.Converters["sayHello"] = (input, convert) => "Hello " + input + "!";

        var result = await Json2Md.ConvertAsync(new Dictionary<string, object?> { ["sayHello"] = "World" });
        await Assert.That(result).IsEqualTo("Hello World!\n");
    }

    [Test]
    public async Task ShouldSupportAsyncConverter()
    {
        Json2Md.AsyncConverters["asyncConvert"] = async (input, convert) =>
        {
            await Task.Delay(100);
            return "Hello " + input + "!";
        };

        var result = await Json2Md.ConvertAsync(new Dictionary<string, object?> { ["asyncConvert"] = "World" });
        await Assert.That(result).IsEqualTo("Hello World!\n");
    }

    [Test]
    public async Task ShouldKeepOrderWhenAsyncConvertersFinishedAtDifferentTimes()
    {
        Json2Md.AsyncConverters["asyncConvert2"] = async (input, convert) =>
        {
            if (input is System.Collections.IDictionary dict)
            {
                var text = dict["text"]?.ToString() ?? "";
                var timeout = System.Convert.ToInt32(dict["timeout"]);
                await Task.Delay(timeout);
                return "Hello " + text + "!";
            }

            return "Hello!";
        };

        var result = await Json2Md.ConvertAsync(new List<object?>
        {
            new Dictionary<string, object?> { ["h1"] = "Heading 1" },
            new Dictionary<string, object?> { ["asyncConvert2"] = new Dictionary<string, object?> { ["text"] = "World", ["timeout"] = 200 } },
            new Dictionary<string, object?> { ["h2"] = "Heading 2" },
            new Dictionary<string, object?> { ["asyncConvert2"] = new Dictionary<string, object?> { ["text"] = "hello", ["timeout"] = 100 } },
        });

        await Assert.That(result).IsEqualTo("# Heading 1\n\nHello World!\n\n## Heading 2\n\nHello hello!\n");
    }
}
