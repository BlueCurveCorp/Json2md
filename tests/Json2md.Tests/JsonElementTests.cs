using System.Text.Json;
using TUnit;

namespace Json2md.Tests;

public sealed class JsonElementTests
{
    private const string TestJson = """{"confidence": "High", "currency": null, "details": null, "estimatedValue": null, "status": "Evaluating", "timePeriod": null, "types": ["marketcap up 15%"]}""";

    [Test]
    public async Task ShouldConvertJsonDocument()
    {
        using var doc = JsonDocument.Parse(TestJson);
        var result = Json2Md.Convert(doc);

        await Assert.That(result).Contains("**Confidence:** High");
        await Assert.That(result).Contains("**Status:** Evaluating");
        await Assert.That(result).Contains("**Types:**");
        await Assert.That(result).Contains(" - marketcap up 15%");
    }

    [Test]
    public async Task ShouldConvertJsonElement()
    {
        using var doc = JsonDocument.Parse(TestJson);
        var result = Json2Md.Convert(doc.RootElement);

        await Assert.That(result).Contains("**Confidence:** High");
        await Assert.That(result).Contains("**Status:** Evaluating");
        await Assert.That(result).Contains("**Types:**");
        await Assert.That(result).Contains(" - marketcap up 15%");
    }

    [Test]
    public async Task ShouldConvertJsonDocumentAsync()
    {
        using var doc = JsonDocument.Parse(TestJson);
        var result = await Json2Md.ConvertAsync(doc);

        await Assert.That(result).Contains("**Confidence:** High");
    }

    [Test]
    public async Task ShouldConvertJsonElementAsync()
    {
        using var doc = JsonDocument.Parse(TestJson);
        var result = await Json2Md.ConvertAsync(doc.RootElement);

        await Assert.That(result).Contains("**Confidence:** High");
    }

    [Test]
    public async Task ShouldHandleNestedJsonObjects()
    {
        var json = """{"user": {"name": "Alice", "age": 30}, "active": true}""";
        using var doc = JsonDocument.Parse(json);
        var result = Json2Md.Convert(doc);

        await Assert.That(result).Contains("**User:**");
        await Assert.That(result).Contains("**Name:** Alice");
        await Assert.That(result).Contains("**Age:** 30");
        await Assert.That(result).Contains("**Active:** true");
    }

    [Test]
    public async Task ShouldHandleJsonArrays()
    {
        var json = """["item1", "item2", "item3"]""";
        using var doc = JsonDocument.Parse(json);
        var result = Json2Md.Convert(doc);

        await Assert.That(result).Contains("item1");
        await Assert.That(result).Contains("item2");
        await Assert.That(result).Contains("item3");
    }

    [Test]
    public async Task ShouldHandleJsonNumbers()
    {
        var json = """{"integer": 42, "decimal": 3.14}""";
        using var doc = JsonDocument.Parse(json);
        var result = Json2Md.Convert(doc);

        await Assert.That(result).Contains("**Integer:** 42");
        await Assert.That(result).Contains("**Decimal:** 3.14");
    }

    [Test]
    public async Task ShouldHandleJsonBooleans()
    {
        var json = """{"enabled": true, "disabled": false}""";
        using var doc = JsonDocument.Parse(json);
        var result = Json2Md.Convert(doc);

        await Assert.That(result).Contains("**Enabled:** true");
        await Assert.That(result).Contains("**Disabled:** false");
    }

    [Test]
    public async Task ShouldSkipNullValuesInJson()
    {
        using var doc = JsonDocument.Parse(TestJson);
        var result = Json2Md.Convert(doc);

        await Assert.That(result).DoesNotContain("Currency");
        await Assert.That(result).DoesNotContain("Details");
        await Assert.That(result).DoesNotContain("Estimated Value");
        await Assert.That(result).DoesNotContain("Time Period");
    }
}
