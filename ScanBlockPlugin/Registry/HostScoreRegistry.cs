using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Alstra.ScanBlockPlugin.Registry
{
    internal class HostScoreRegistry
    {
        private const int DaysToKeepScores = 7;
        private const ushort MaxScore = 100;

        private ConcurrentDictionary<string, List<HostScoreInfo>> HostScores { get; }
            = new ConcurrentDictionary<string, List<HostScoreInfo>>();

        private static string CurrentDate => DateTime.Now.ToString("yyyy-MM-dd");

        /// <summary>
        /// Add score to a host where the total score is less than <see cref="MaxScore"/>
        /// </summary>
        /// <param name="host">RemoteIp</param>
        /// <param name="score">Score to add</param>
        /// <param name="reason">Reason for adding score</param>
        /// <remarks>
        /// If the host has a score lower than <see cref="MaxScore"/>,
        /// any history older than <see cref="DaysToKeepScores"/> will be purged.
        /// </remarks>
        public void AddScore(string host, ushort score, string reason)
        {
            if (GetScore(host) >= MaxScore)
            {
                return;
            }

            PurgeOldScores(host);

            _ = HostScores.AddOrUpdate(
                host,
                new List<HostScoreInfo>()
                {
                    new HostScoreInfo()
                    {
                        Score = score,
                        Date = CurrentDate,
                        Reasons = new List<string>() { reason },
                    },
                },
                (_, list) => UpdateScore(list, score, reason));
        }

        /// <summary>
        /// Get the total score of a host
        /// </summary>
        /// <param name="host">RemoteIp</param>
        /// <returns>uint</returns>
        public uint GetScore(string host) => HostScores.TryGetValue(host, out var scores)
            ? Convert.ToUInt16(scores.Sum(score => score.Score))
            : 0u;

        /// <summary>
        /// Get the current score history as a dictionary
        /// </summary>
        public Dictionary<string, List<HostScoreInfo>> GetScoreInfoState() =>
            HostScores.ToDictionary(pair => pair.Key, pair => pair.Value);

        /// <summary>
        /// Purge old scores from a host
        /// </summary>
        /// <param name="host">RemoteIp</param>
        public void PurgeOldScores(string host)
        {
            var oldScore = GetScore(host);
            if (oldScore >= MaxScore)
            {
                return;
            }

            if (!HostScores.TryGetValue(host, out var scores))
            {
                return;
            }

            var lowerDateBound = DateTime
                .ParseExact(CurrentDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                .AddDays(-DaysToKeepScores);
            var newScores = scores
                .Where(score =>
                    DateTime.ParseExact(score.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                    >= lowerDateBound)
                .ToList();

            if (newScores.Count == scores.Count)
            {
                return;
            }

            if (newScores.Count == 0)
            {
                _ = HostScores.TryRemove(host, out _);
                return;
            }

            _ = HostScores.TryUpdate(host, newScores, scores);
        }

        /// <summary>
        /// Reset all scores
        /// </summary>
        public void ResetScores() => HostScores.Clear();

        /// <summary>
        /// Update factory method for adding score to a host
        /// </summary>
        /// <param name="list"></param>
        /// <param name="score"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        private static List<HostScoreInfo> UpdateScore(List<HostScoreInfo> list, ushort score, string reason)
        {
            var current = list.FirstOrDefault(item => item.Date == CurrentDate);

            if (current != null)
            {
                current.Score += score;
                current.Reasons.Add(reason);
            }
            else
            {
                list.Add(new HostScoreInfo()
                {
                    Score = score,
                    Date = CurrentDate,
                    Reasons = new List<string>() { reason }
                });
            }

            return list;
        }
    }
}
