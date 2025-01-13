using ScanBlockPluginTest.Base;

namespace ScanBlockPluginTest;

public class RegistrationTests : BaseIntegrationTest
{
    public RegistrationTests() => Plugin = new(new()
    {
        AllowHostScoreListing = req => true,
        SkipHostScoringForRequest = req => false
    });

    [Test]
    public void RegistersHandlersCorrectly()
    {
        Assert.That(AppHost.GlobalRequestFilters, Has.Count.EqualTo(1));
        Assert.That(AppHost.CatchAllHandlers, Has.Count.EqualTo(3));
    }
}