namespace Alstra.ScanBlockPlugin.Registry;

internal class HostScoreInfo
{
    public DateOnly Date { get; set; }
    public ushort Score { get; set; }
    public List<string> Reasons { get; set; } = [];
}
