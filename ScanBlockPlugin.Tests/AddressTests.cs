using System.Net;
using Alstra.ScanBlockPlugin.Config;
using ScanBlockPlugin.Tests.Base;
using ServiceStack;

namespace ScanBlockPlugin.Tests;

public class AddressTests : BaseIntegrationTest
{
    public AddressTests() => Plugin = new(new()
    {
        AllowHostScoreListing = req => true,
        SkipHostScoringForRequest = req => false,
        OnBlockedRequest = (req, reason) => BlockCount++,
        BadFileEndingsScore = 10,
        BlockScoreThreshold = 20,
        PermanentlyAllowedHosts = [],
    });

    private uint BlockCount { get; set; }
    private Bogus.Faker Faker { get; } = new();

    [SetUp]
    public void SetUp()
    {
        BlockCount = 0;
        Plugin.ResetScores();
    }

    [Test]
    public void XForwardedFor_Is_Blocked()
    {
        // Arrange
        var path = $"/my-file{Faker.PickRandom(ScanBlockFeatureConfigDefaults.BadFileEndings)}";
        var headers = new Dictionary<string, string>
        {
            { "X-Forwarded-For", Faker.Internet.Ip() }
        };

        // Act
        MakeAnonymousRequest(path, headers);
        MakeAnonymousRequest(path, headers);
        MakeAnonymousRequest(path);

        // Assert
        Assert.That(BlockCount, Is.EqualTo(1));
    }

    [Test]
    public void XRealIp_Is_Blocked()
    {
        // Arrange
        var path = $"/my-file{Faker.PickRandom(ScanBlockFeatureConfigDefaults.BadFileEndings)}";
        var headers = new Dictionary<string, string>
        {
            { "X-Real-IP", Faker.Internet.Ip() }
        };

        // Act
        MakeAnonymousRequest(path, headers);
        MakeAnonymousRequest(path, headers);
        MakeAnonymousRequest(path, headers);
        MakeAnonymousRequest(path);

        // Assert
        Assert.That(BlockCount, Is.EqualTo(2));
    }

    private static void MakeAnonymousRequest(string path, Dictionary<string, string>? headers = null)
    {
        var anonClient = new JsonServiceClient(BaseUri);

        headers ??= [];
        foreach (var (key, value) in headers)
        {
            anonClient.Headers.Add(key, value);
        }

        var exception = Assert.Throws<WebServiceException>(() => anonClient.CustomMethod("GET", path, ""));
        Assert.That(exception.StatusCode, Is.Not.EqualTo((int)HttpStatusCode.OK));
    }
}

