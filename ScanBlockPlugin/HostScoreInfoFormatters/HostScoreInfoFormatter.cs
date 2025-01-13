using Alstra.ScanBlockPlugin.Config;
using Alstra.ScanBlockPlugin.Registry;

namespace Alstra.ScanBlockPlugin.HostScoreInfoFormatters
{
    /// <summary>
    /// Base class for host score info formatters
    /// </summary>
    public abstract class HostScoreInfoFormatter
    {
        internal abstract string Format(Dictionary<string, List<HostScoreInfo>> hostScores, ScanBlockFeatureConfig config);
        internal abstract string MimeType { get; }
    }
}
