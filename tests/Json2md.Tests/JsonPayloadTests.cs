using TUnit;

namespace Json2md.Tests;

public sealed class JsonPayloadTests
{
    [Test]
    public async Task TestComplexJsonPayload()
    {
        var payload = new Dictionary<string, object?>
        {
            ["confidence"] = "High",
            ["currency"] = null,
            ["details"] = null,
            ["estimatedValue"] = null,
            ["status"] = "Evaluating",
            ["timePeriod"] = null,
            ["types"] = new List<object?> { "marketcap up 15%" }
        };

        var result = Json2Md.Convert(payload);
        await Assert.That(result).Contains("**Confidence:** High");
        await Assert.That(result).Contains("**Status:** Evaluating");
        await Assert.That(result).Contains("**Types:**");
        await Assert.That(result).Contains(" - marketcap up 15%");
    }

    [Test]
    public async Task TestArbitraryJsonWithNumbers()
    {
        var payload = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["count"] = 42,
            ["price"] = 19.99,
            ["active"] = true
        };

        var result = Json2Md.Convert(payload);
        await Assert.That(result).Contains("**Name:** Test");
        await Assert.That(result).Contains("**Count:** 42");
        await Assert.That(result).Contains("**Price:** 19.99");
        await Assert.That(result).Contains("**Active:** true");
    }

    [Test]
    public async Task TestArbitraryJsonWithNestedDict()
    {
        var payload = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = "Alice",
                ["age"] = 30
            }
        };

        var result = Json2Md.Convert(payload);
        await Assert.That(result).Contains("**User:**");
        await Assert.That(result).Contains("**Name:** Alice");
        await Assert.That(result).Contains("**Age:** 30");
    }

    [Test]
    public async Task TestArbitraryJsonWithEmptyList()
    {
        var payload = new Dictionary<string, object?>
        {
            ["tags"] = new List<object?>()
        };

        var result = Json2Md.Convert(payload);
        await Assert.That(result).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task TestMixedConvertersAndArbitraryKeys()
    {
        var payload = new Dictionary<string, object?>
        {
            ["h1"] = "Report",
            ["confidence"] = "High",
            ["p"] = "Analysis complete"
        };

        var result = Json2Md.Convert(payload);
        await Assert.That(result).Contains("# Report");
        await Assert.That(result).Contains("**Confidence:** High");
        await Assert.That(result).Contains("Analysis complete");
    }
}
