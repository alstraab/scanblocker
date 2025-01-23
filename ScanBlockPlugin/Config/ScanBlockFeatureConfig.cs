using System;
using System.ComponentModel.DataAnnotations;
using Alstra.ScanBlockPlugin.HostScoreInfoFormatters;
using Alstra.ScanBlockPlugin.Registry;
using ServiceStack.Web;

namespace Alstra.ScanBlockPlugin.Config
{
    /// <summary>
    /// Configuration for <see cref="ScanBlockFeature"/>
    /// </summary>
    public class ScanBlockFeatureConfig
    {
        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.BlockScoreThreshold"/>
        /// </summary>
        public ushort BlockScoreThreshold { get; set; } = ScanBlockFeatureConfigDefaults.BlockScoreThreshold;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.ForbiddenFullPathsScore"/>
        /// </summary>
        public ushort ForbiddenFullPathsScore { get; set; } = ScanBlockFeatureConfigDefaults.ForbiddenFullPathsScore;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.ForbiddenPartialPathsScore"/>
        /// </summary>
        public ushort ForbiddenPartialPathsScore { get; set; } = ScanBlockFeatureConfigDefaults.ForbiddenPartialPathsScore;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.BadFileEndingsScore"/>
        /// </summary>
        public ushort BadFileEndingsScore { get; set; } = ScanBlockFeatureConfigDefaults.BadFileEndingsScore;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.BadPartialPathsScore"/>
        /// </summary>
        public ushort BadPartialPathsScore { get; set; } = ScanBlockFeatureConfigDefaults.BadPartialPathsScore;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.PermanentlyAllowedHosts"/>
        /// </summary>
        public string[] PermanentlyAllowedHosts { get; set; } = ScanBlockFeatureConfigDefaults.PermanentlyAllowedHosts;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.ForbiddenFullPaths"/>
        /// </summary>
        public string[] ForbiddenFullPaths { get; set; } = ScanBlockFeatureConfigDefaults.ForbiddenFullPaths;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.ForbiddenPartialPaths"/>
        /// </summary>
        public string[] ForbiddenPartialPaths { get; set; } = ScanBlockFeatureConfigDefaults.ForbiddenPartialPaths;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.BadFileEndings"/>
        /// </summary>
        public string[] BadFileEndings { get; set; } = ScanBlockFeatureConfigDefaults.BadFileEndings;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.BadPartialPaths"/>
        /// </summary>
        public string[] BadPartialPaths { get; set; } = ScanBlockFeatureConfigDefaults.BadPartialPaths;

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.HostScoreListingPath"/>
        /// </summary>
        public string HostScoreListingPath { get; set; } = ScanBlockFeatureConfigDefaults.HostScoreListingPath;

        /// <summary>
        /// Allow host to view a list of hosts and their scores
        /// </summary>
        /// <remarks>REQUIRED</remarks>
        [Required]
        public Func<IRequest, bool> AllowHostScoreListing { get; set; }

        /// <summary>
        /// Do not score or block this request. Often used with logged in users or known bots.
        /// </summary>
        /// <remarks>REQUIRED</remarks>
        [Required]
        public Func<IRequest, bool> SkipHostScoringForRequest { get; set; }

        /// <summary>
        /// Do something when request is blocked
        /// </summary>
        public Action<IRequest, string> OnBlockedRequest { get; set; } = (request, reason) => { };

        /// <summary>
        /// Do something when request gets a score
        /// </summary>
        public Action<IRequest, string> OnScoredRequest { get; set; } = (request, reason) => { };

        /// <summary>
        /// Formatter for <see cref="HostScoreInfo"/>
        /// </summary>
        public HostScoreInfoFormatter HostScoreInfoFormatter { get; set; } = new PlainTextFormatter();

        /// <summary>
        /// <inheritdoc cref="ScanBlockFeatureConfigDefaults.RequestItemKey"/>
        /// </summary>
        public string RequestItemKey { get; set; } = ScanBlockFeatureConfigDefaults.RequestItemKey;
    }
}
