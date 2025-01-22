using Alstra.ScanBlockPlugin.Config;

namespace ScanBlockPlugin.Tests;

public class ConfigDefaultTests
{
    [Test]
    public void Config_Has_Default_Values()
    {
        var config = new ScanBlockFeatureConfig()
        {
            AllowHostScoreListing = req => true,
            SkipHostScoringForRequest = req => false
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.HostScoreListingPath, Is.EqualTo(ScanBlockFeatureConfigDefaults.HostScoreListingPath));
            Assert.That(config.BlockScoreThreshold, Is.EqualTo(ScanBlockFeatureConfigDefaults.BlockScoreThreshold));
            Assert.That(config.ForbiddenFullPathsScore, Is.EqualTo(ScanBlockFeatureConfigDefaults.ForbiddenFullPathsScore));
            Assert.That(config.ForbiddenPartialPathsScore, Is.EqualTo(ScanBlockFeatureConfigDefaults.ForbiddenPartialPathsScore));
            Assert.That(config.BadFileEndingsScore, Is.EqualTo(ScanBlockFeatureConfigDefaults.BadFileEndingsScore));
            Assert.That(config.BadPartialPathsScore, Is.EqualTo(ScanBlockFeatureConfigDefaults.BadPartialPathsScore));
            Assert.That(config.PermanentlyAllowedHosts, Is.EqualTo(ScanBlockFeatureConfigDefaults.PermanentlyAllowedHosts));
            Assert.That(config.ForbiddenFullPaths, Is.EqualTo(ScanBlockFeatureConfigDefaults.ForbiddenFullPaths));
            Assert.That(config.ForbiddenPartialPaths, Is.EqualTo(ScanBlockFeatureConfigDefaults.ForbiddenPartialPaths));
            Assert.That(config.BadFileEndings, Is.EqualTo(ScanBlockFeatureConfigDefaults.BadFileEndings));
            Assert.That(config.BadPartialPaths, Is.EqualTo(ScanBlockFeatureConfigDefaults.BadPartialPaths));
        }
    }
}
