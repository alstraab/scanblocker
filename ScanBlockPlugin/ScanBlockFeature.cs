using System;
using System.IO;
using System.Linq;
using System.Net;
using Alstra.ScanBlockPlugin.Config;
using Alstra.ScanBlockPlugin.Registry;
using ServiceStack;
using ServiceStack.Host;
using ServiceStack.Web;

namespace Alstra.ScanBlockPlugin
{
    /// <summary>
    /// Vulnerability scanner blocking plugin for ServiceStack. 
    /// Targets scanners that survey the web for known vulnerabilities.
    /// </summary>
    /// <remarks>
    /// Usage <code>AppHost.Plugins.Register(new(new()))</code>
    /// </remarks>
    public class ScanBlockFeature : IPlugin
    {
        private readonly ScanBlockFeatureConfig config;

        private HostScoreRegistry HostScoreRegistry { get; } = new HostScoreRegistry();

        /// <summary>
        /// Create a new instance of <see cref="ScanBlockFeature"/>
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ScanBlockFeature(ScanBlockFeatureConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.config = config;
        }

        /// <summary>
        /// <inheritdoc cref="IPlugin.Register"/>
        /// </summary>
        public void Register(IAppHost appHost)
        {
            VerifyConstants();
            appHost.GlobalRequestFilters.Add(FilterHandler);
            appHost.CatchAllHandlers.Add(CatchAllHandler);
        }

        /// <summary>
        /// Reset all scores and unblock all hosts
        /// </summary>
        public void ResetScores() => HostScoreRegistry.ResetScores();

        private IHttpHandler? CatchAllHandler(IRequest request)
        {
            HandleRequest(request, request.Response);
            return null;
        }

        private void FilterHandler(IRequest request, IResponse response, object requestDto) =>
            HandleRequest(request, response);

        /// <summary>
        /// Handle incoming requests, add score to host if necessary and block if score is too high
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void HandleRequest(IRequest request, IResponse response)
        {
            // No checks on empty requests
            if (request is null ||
                string.IsNullOrEmpty(request.PathInfo) ||
                string.IsNullOrEmpty(request.RemoteIp))
            {
                return;
            }

            // Request is already handled
            if (request.Items.ContainsKey(config.RequestItemKey))
            {
                return;
            }
            request.Items[config.RequestItemKey] = new object();

            // Should we list host scores?
            if (config.AllowHostScoreListing(request)
                && request.PathInfo.Equals(config.HostScoreListingPath, StringComparison.OrdinalIgnoreCase))
            {
                var table = HostScoreRegistry.GetScoreInfoState();
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = config.HostScoreInfoFormatter.MimeType;
                response.WriteToResponse(
                    config.HostScoreInfoFormatter.Format(table, config),
                    config.HostScoreInfoFormatter.MimeType).Wait();
                response.EndRequest();
                return;
            }

            // No checks on logged in users
            if (config.SkipHostScoringForRequest(request))
            {
                return;
            }

            // We don't check allowed hosts
            if (config.PermanentlyAllowedHosts.Contains(request.RemoteIp))
            {
                return;
            }

            // Add score if necessary and update registry
            RegisterScores(request);

            // Evaluate score
            if (HostScoreRegistry.GetScore(request.RemoteIp) < config.BlockScoreThreshold)
            {
                return;
            }

            // Only purge if score is above threshold so we don't impact all requests
            HostScoreRegistry.PurgeOldScores(request.RemoteIp);

            // Check score again after purge and block if necessary
            var score = HostScoreRegistry.GetScore(request.RemoteIp);
            if (score >= config.BlockScoreThreshold)
            {
                config.OnBlockedRequest(request, $"{request.RemoteIp} blocked due to a score of {score}");

                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.Dto = DtoUtils.CreateErrorResponse(request, new HttpError(HttpStatusCode.ServiceUnavailable, "ServiceUnavailable"));
                response.EndRequest();
            }
        }

        /// <summary>
        /// Verify that all constants are lowercase and begins with correct letters
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        private void VerifyConstants()
        {
            if (config.ForbiddenFullPaths.Any(s => s.Any(char.IsUpper)))
            {
                throw new InvalidDataException($"{nameof(config.ForbiddenFullPaths)} must be lowercase");
            }

            if (config.ForbiddenFullPaths.Any(s => !s.StartsWith('/')))
            {
                throw new InvalidDataException($"{nameof(config.ForbiddenFullPaths)} must start with '/'");
            }

            if (config.ForbiddenPartialPaths.Any(s => s.Contains(' ')))
            {
                throw new InvalidDataException($"{nameof(config.ForbiddenPartialPaths)} must be url encoded");
            }

            if (config.BadFileEndings.Any(s => s.Any(char.IsUpper)))
            {
                throw new InvalidDataException($"{nameof(config.BadFileEndings)} must be lowercase");
            }

            if (config.BadFileEndings.Any(s => !s.StartsWith('.')))
            {
                throw new InvalidDataException($"{nameof(config.BadFileEndings)} must start with '.'");
            }

            if (config.BadPartialPaths.Any(s => s.Any(char.IsUpper)))
            {
                throw new InvalidDataException($"{nameof(config.BadPartialPaths)} must be lowercase");
            }
        }

        /// <summary>
        /// Add score if request path is forbidden
        /// </summary>
        private void RegisterScores(IRequest request)
        {
            var lowerCasePath = request.PathInfo.ToLowerInvariant();
            var lowerCaseRawUrl = request.RawUrl.ToLowerInvariant();
            ushort score = 0;
            var reason = string.Empty;

            if (config.ForbiddenFullPaths.Contains(lowerCasePath))
            {
                score = config.ForbiddenFullPathsScore;
                reason = $"Forbidden path: {lowerCasePath}";
            }
            else if (config.ForbiddenPartialPaths.Any(lowerCaseRawUrl.Contains))
            {
                var forbiddenPartialPath = config.ForbiddenPartialPaths.First(lowerCaseRawUrl.Contains);

                score = config.ForbiddenPartialPathsScore;
                reason = $"Forbidden partial path \"{forbiddenPartialPath}\" in {lowerCaseRawUrl}";
            }
            else if (config.BadFileEndings.Any(lowerCasePath.EndsWith))
            {
                score = config.BadFileEndingsScore;
                reason = $"Bad file ending: {lowerCasePath}";
            }
            else if (config.BadPartialPaths.Any(lowerCaseRawUrl.Contains))
            {
                var badPartialPath = config.BadPartialPaths.First(lowerCaseRawUrl.Contains);

                score = config.BadPartialPathsScore;
                reason = $"Bad partial path \"{badPartialPath}\" in {lowerCaseRawUrl}";
            }

            if (score > 0)
            {
                HostScoreRegistry.AddScore(request.RemoteIp, score, reason);
                config.OnScoredRequest(request, reason);
            }
        }
    }
}
