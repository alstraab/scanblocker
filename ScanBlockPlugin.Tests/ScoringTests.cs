using System.Net;
using ServiceStack;
using Alstra.ScanBlockPlugin.Config;
using ScanBlockPlugin.Tests.Base;

namespace ScanBlockPlugin.Tests;

public class ScoringTests : BaseIntegrationTest
{
    public ScoringTests() => Plugin = new(new()
    {
        AllowHostScoreListing = req => req.PathInfo == ScanBlockFeatureConfigDefaults.HostScoreListingPath,
        SkipHostScoringForRequest = req => false,
        PermanentlyAllowedHosts = [],
    });

    private Bogus.Faker Faker { get; } = new();

    [SetUp]
    public void SetUp() => Plugin.ResetScores();

    [Test]
    public void HostReceives_Score_On_ForbiddenFullPath()
    {
        // Arrange
        var path = Faker.PickRandom(ScanBlockFeatureConfigDefaults.ForbiddenFullPaths);

        // Act
        MakeAnonymousRequest(path);

        // Assert
        var content = GetHostScore();

        Assert.That(content, Has.Length.GreaterThan(0));
        Assert.That(content, Does.Contain(path));
        Assert.That(content, Does.Contain($" {ScanBlockFeatureConfigDefaults.ForbiddenFullPathsScore} "));
    }

    [Test]
    public void HostReceives_Score_On_ForbiddenPartialPath()
    {
        // Arrange
        var forbiddenPath = Faker.PickRandom(ScanBlockFeatureConfigDefaults.ForbiddenPartialPaths);
        var path = $"/index?abc={forbiddenPath}";

        // Act
        MakeAnonymousRequest(path);

        // Assert
        var content = GetHostScore();

        Assert.That(content, Has.Length.GreaterThan(0));
        Assert.That(content, Does.Contain(forbiddenPath));
        Assert.That(content, Does.Contain($" {ScanBlockFeatureConfigDefaults.ForbiddenPartialPathsScore} "));
    }

    [Test]
    public void HostReceives_Score_On_BadFileEnding()
    {
        // Arrange
        var path = $"/my-file{Faker.PickRandom(ScanBlockFeatureConfigDefaults.BadFileEndings)}";

        // Act
        MakeAnonymousRequest(path);

        // Assert
        var content = GetHostScore();

        Assert.That(content, Has.Length.GreaterThan(0));
        Assert.That(content, Does.Contain(path));
        Assert.That(content, Does.Contain($" {ScanBlockFeatureConfigDefaults.BadFileEndingsScore} "));
    }

    [Test]
    public void HostReceives_Score_On_BadPartialPath()
    {
        // Arrange
        var path = Faker.PickRandom(ScanBlockFeatureConfigDefaults.BadPartialPaths);

        // Act
        MakeAnonymousRequest(path);

        // Assert
        var content = GetHostScore();

        Assert.That(content, Has.Length.GreaterThan(0));
        Assert.That(content, Does.Contain(path));
        Assert.That(content, Does.Contain($" {ScanBlockFeatureConfigDefaults.BadPartialPathsScore} "));
    }

    [Test]
    public void Host_Is_Blocked_After_Reaching_Threshold()
    {
        // Arrange
        var path = Faker.PickRandom(ScanBlockFeatureConfigDefaults.ForbiddenFullPaths);
        var requestCount = ScanBlockFeatureConfigDefaults.BlockScoreThreshold / ScanBlockFeatureConfigDefaults.ForbiddenFullPathsScore;

        // Act
        for (var i = 0; i < requestCount; i += 1)
        {
            MakeAnonymousRequest(path);
        }

        // Assert
        var content = GetHostScore();
        Assert.That(content, Has.Length.GreaterThan(0));
        Assert.That(content, Does.Contain(path));
        Assert.That(content, Does.Contain($" {ScanBlockFeatureConfigDefaults.BlockScoreThreshold} "));
        Assert.That(content, Does.Contain("Blocked"));
    }

    [Test]
    public void Host_Score_Cannot_Go_Past_100()
    {
        // Arrange
        var requestCount = (100 / ScanBlockFeatureConfigDefaults.ForbiddenFullPathsScore) + 1;

        // Act
        for (var i = 0; i < requestCount; i += 1)
        {
            var path = Faker.PickRandom(ScanBlockFeatureConfigDefaults.ForbiddenFullPaths);
            MakeAnonymousRequest(path);
        }

        // Assert
        var content = GetHostScore();
        Assert.That(content, Has.Length.GreaterThan(0));
        Assert.That(content, Does.Contain(" 100 "));
        Assert.That(content, Does.Contain("Blocked"));
    }

    private static void MakeAnonymousRequest(string path)
    {
        var anonClient = new JsonServiceClient(BaseUri);
        var exception = Assert.Throws<WebServiceException>(() => anonClient.CustomMethod("GET", path, ""));
        Assert.That(exception.StatusCode, Is.Not.EqualTo((int)HttpStatusCode.OK));
    }

    private static string GetHostScore()
    {
        var client = new JsonServiceClient(BaseUri);
        var response = client.CustomMethod("GET", ScanBlockFeatureConfigDefaults.HostScoreListingPath, "");
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        return response.ReadToEnd();
    }
}
