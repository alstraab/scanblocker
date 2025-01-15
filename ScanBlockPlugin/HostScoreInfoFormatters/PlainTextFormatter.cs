using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Alstra.ScanBlockPlugin.Config;
using Alstra.ScanBlockPlugin.Registry;

namespace Alstra.ScanBlockPlugin.HostScoreInfoFormatters
{
    /// <summary>
    /// Formatter for host score info in plain text format
    /// </summary>
    public class PlainTextFormatter : HostScoreInfoFormatter
    {
        internal override string MimeType { get; } = "text/plain";

        internal override string Format(Dictionary<string, List<HostScoreInfo>> hostScores, ScanBlockFeatureConfig config)
        {
            var hosts = hostScores.Keys.ToList();
            hosts.Sort();

            var columns = new Dictionary<string, List<string>>()
            {
                { "Blocked", new List<string> { "Blocked" } },
                { "Host", new List<string> { "Host"} },
                { "Date", new List<string> { "Date"} },
                { "Score", new List<string> { "Score"} },
                { "Reasons", new List<string> { "Reasons"} },
            };

            foreach (var host in hosts)
            {
                var scores = hostScores[host];
                var totalScore = scores.Sum(score => score.Score);

                scores.Sort((a, b) => a.Date.CompareTo(b.Date));

                foreach (var score in scores)
                {
                    if (score == scores.First())
                    {
                        columns["Blocked"].Add(totalScore >= config.BlockScoreThreshold ? "X" : "");
                        columns["Host"].Add(host);
                    }
                    else
                    {
                        columns["Blocked"].Add("");
                        columns["Host"].Add("");
                    }
                    columns["Date"].Add(score.Date);
                    columns["Score"].Add(score.Score.ToString(CultureInfo.InvariantCulture));

                    foreach (var reason in score.Reasons)
                    {
                        if (reason != score.Reasons.First())
                        {
                            columns["Blocked"].Add("");
                            columns["Host"].Add("");
                            columns["Date"].Add("");
                            columns["Score"].Add("");
                        }
                        columns["Reasons"].Add(reason);
                    }
                }
            }

            // Pad all columns to the same length
            foreach (var (key, value) in columns)
            {
                var maxLength = value.Max(s => s.Length) + 1;
                columns[key] = value.Select(s => s.PadRight(maxLength)).ToList();
            }

            var rowCount = columns["Blocked"].Count;
            var sb = new StringBuilder();

            for (var i = 0; i < rowCount; i += 1)
            {
                sb.Append(columns["Blocked"][i])
                    .Append(columns["Host"][i])
                    .Append(columns["Date"][i])
                    .Append(columns["Score"][i])
                    .Append(columns["Reasons"][i])
                    .AppendLine();
            }

            return sb.ToString();
        }
    }
}
