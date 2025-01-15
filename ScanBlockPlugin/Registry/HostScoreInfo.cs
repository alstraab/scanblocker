using System.Collections.Generic;

namespace Alstra.ScanBlockPlugin.Registry
{
    internal class HostScoreInfo
    {
        public string Date { get; set; }
        public ushort Score { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
    }
}
