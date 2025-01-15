# ScanBlocker Plugin

## Overview

The ScanBlocker Plugin is a vulnerability scanner blocking plugin designed for ServiceStack. 

It intercepts incoming HTTP requests to your ServiceStack services to analyze them to determine if they match known patterns used by vulnerability scanners.

## Features

- **Automatic Detection**: Automatically detects and blocks requests on paths used by vulnerability scanners.
- **Customizable**: Allows customization of paths, keywords, allow lists, etc.

## Installation

To install the ScanBlocker Plugin, add the following NuGet package to your project:
[Alstra.ScanBlocker](https://www.nuget.org/packages/Alstra.ScanBlocker/)


## Usage

To use the ScanBlocker Plugin, register it in your ServiceStack AppHost:

```csharp
public override void Configure(Container container)
{
    Plugins.Add(new ScanBlockerPlugin(new()
    {
        // Allow authenticated users to access the host score listing
        AllowHostScoreListing = request => (request.SessionAs<User>() is { IsAuthenticated: true }),
        // Skip scores for logged in users
        SkipHostScoringForRequest = request => (request.SessionAs<User>() is { IsAuthenticated: true }),
        // Skip scores for internal requests
        PermanentlyAllowedHosts = ["123.456.789.10", "example.com"],
        // Use a custom URL for viewing host scores
        HostScoreListingPath = "/host-scores", // defaults to "/scanblock/hosts"
    }));
}
```

## How it works

 Here's a step-by-step breakdown of the process:

1. **Request Interception**: The plugin intercepts each incoming HTTP request to your ServiceStack services using global request filters and catch-all handlers.
2. **Pattern Matching**: It checks the request against a list of known paths, keywords, and other patterns commonly used by vulnerability scanners. 
3. **Custom Rules**: You can define custom rules to allow or block specific requests based on your application's requirements. This includes allowing authenticated users, skipping scores for logged-in users, and permanently allowing certain hosts.
4. **Scoring**: The plugin maintains a score for each host (IP address) based on the requests it makes. Score is calculated for the last 7 days. Restarting the server (or using `ScanBlockerPlugin.ResetScores()`) resets the scores.
5. **Blocking**: If a host's score for the last 7 days exceeds a configurable threshold, the plugin blocks further requests from that host by returning a `ServiceUnavailable` status. If a host exceeds 100 points, it will neither receive nor lose scores and is blocked permanently.

## Compatibility

The ScanBlocker Plugin is compatible with ServiceStack 8.4+ and 
[.NET Standard 2.1](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1)

## License
MIT
