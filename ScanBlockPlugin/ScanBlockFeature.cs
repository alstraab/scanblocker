using System.Net;
using Alstra.ScanBlockPlugin.Config;
using Alstra.ScanBlockPlugin.Registry;
using ServiceStack;
using ServiceStack.Host;
using ServiceStack.Web;

namespace Alstra.ScanBlockPlugin;

/// <summary>
/// Vulnerability scanner blocking plugin for ServiceStack. 
/// Targets scanners that survey the web for known vulnerabilities.
/// </summary>
/// <remarks>
/// Usage <code>AppHost.Plugins.Register(new(new()))</code>
/// </remarks>
/// <param name="config"></param>
public class ScanBlockFeature(ScanBlockFeatureConfig config) : IPlugin
{
    private HostScoreRegistry HostScoreRegistry { get; } = new();

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
            string.IsNullOrEmpty(request.UserHostAddress))
        {
            return;
        }

        // Should we list host scores?
        if (config.AllowHostScoreListing(request)
            && request.PathInfo.Equals(config.HostScoreListingPath, StringComparison.OrdinalIgnoreCase))
        {
            var table = HostScoreRegistry.GetScoreInfoState();
            response.StatusCode = (int)HttpStatusCode.OK;
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
        if (config.PermanentlyAllowedHosts.Contains(request.UserHostAddress))
        {
            return;
        }

        // Add score if necessary and update registry
        RegisterScores(request.UserHostAddress, request.PathInfo, request.RawUrl);

        // Evaluate score
        if (HostScoreRegistry.GetScore(request.UserHostAddress) < config.BlockScoreThreshold)
        {
            return;
        }

        // Only purge if score is above threshold so we don't impact all requests
        HostScoreRegistry.PurgeOldScores(request.UserHostAddress);

        // Check score again after purge and block if necessary
        if (HostScoreRegistry.GetScore(request.UserHostAddress) >= config.BlockScoreThreshold)
        {
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
    private void RegisterScores(string host, string path, string rawUrl)
    {
        var lowerCasePath = path.ToLowerInvariant();
        var lowerCaseRawUrl = rawUrl.ToLowerInvariant();

        if (config.ForbiddenFullPaths.Contains(lowerCasePath))
        {
            HostScoreRegistry.AddScore(
                host,
                config.ForbiddenFullPathsScore,
                $"Forbidden path: {lowerCasePath}");
        }
        else if (config.ForbiddenPartialPaths.Any(lowerCaseRawUrl.Contains))
        {
            var forbiddenPartialPath = config.ForbiddenPartialPaths.First(lowerCaseRawUrl.Contains);
            HostScoreRegistry.AddScore(
                host,
                config.ForbiddenPartialPathsScore,
                $"Forbidden partial path \"{forbiddenPartialPath}\" in {lowerCaseRawUrl}");
        }
        else if (config.BadFileEndings.Any(lowerCasePath.EndsWith))
        {
            HostScoreRegistry.AddScore(
                host,
                config.BadFileEndingsScore,
                $"Bad file ending: {lowerCasePath}");
        }
        else if (config.BadPartialPaths.Any(lowerCaseRawUrl.Contains))
        {
            var badPartialPath = config.BadPartialPaths.First(lowerCaseRawUrl.Contains);
            HostScoreRegistry.AddScore(
                host,
                config.BadPartialPathsScore,
                $"Bad partial path \"{badPartialPath}\" in {lowerCaseRawUrl}");
        }
    }
}
