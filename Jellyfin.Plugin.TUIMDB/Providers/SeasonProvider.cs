using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Extensions;
using Jellyfin.Plugin.TUIMDB.Api.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TUIMDB.Providers;

/// <summary>
/// Provides season metadata and search results from TUIMDB.
/// </summary>
public class SeasonProvider :
    IRemoteMetadataProvider<Season, SeasonInfo>,
    IHasOrder
{
    /// <summary>
    /// The logger for this provider.
    /// </summary>
    private readonly ILogger<SeasonProvider> _logger;

    /// <summary>
    /// The HTTP client used to call TUIMDB API.
    /// </summary>
    private static readonly HttpClient _httpClient = new HttpClient
    {
        DefaultRequestHeaders =
        {
            UserAgent =
            {
                new System.Net.Http.Headers.ProductInfoHeaderValue("Jellyfin_Plugin", "1.0.0.0")
            }
        }
    };

    /// <summary>
    /// JSON serialization options used for logging and API requests.
    /// Configured to produce indented (pretty-printed) JSON for easier readability in logs.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for this provider.</param>
    public SeasonProvider(ILogger<SeasonProvider> logger)
    {
        _logger = logger;
        _logger.LogInformation("TUIMDB SeasonProvider constructed");
    }

    /// <summary>
    /// Gets the display name of the provider.
    /// </summary>
    public string Name => "TUIMDB";

    /// <summary>
    /// Gets the order in which this provider is queried.
    /// </summary>
    public int Order => 0;

    /// <summary>
    /// Gets an image response from a URL.
    /// </summary>
    /// <param name="url">The URL of the image.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="HttpResponseMessage"/> containing the image response.</returns>
    public async Task<HttpResponseMessage> GetImageResponse(
        string url,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("TUIMDB Image: Fetching image from {Url}", url);

        return await _httpClient
            .GetAsync(url, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a GET request to the specified URL, logs headers and errors,
    /// and deserializes the JSON response into the specified type.
    /// </summary>
    /// <typeparam name="T">The expected type of the JSON response.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="apiKey">The TUIMDB API key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deserialized response, or null if the request failed.</returns>
    private async Task<T?> GetFromApiAsync<T>(string url, string? apiKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Add API key header if provided
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Add("apiKey", apiKey);
        }

        // Log HttpClient default headers
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            _logger.LogDebug(
                "TUIMDB API: HttpClient Default Header: {Name} = {Values}",
                header.Key,
                string.Join(", ", header.Value));
        }

        // Log request-specific headers
        foreach (var header in request.Headers)
        {
            _logger.LogDebug(
                "TUIMDB API: Request Header: {Name} = {Values}",
                header.Key,
                string.Join(", ", header.Value));
        }

        try
        {
            using var httpResponse = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                string content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogError(
                    "TUIMDB API: HTTP request failed.\nStatus Code: {StatusCode}\nReason: {ReasonPhrase}\nURL: {Url}\nResponse Content: {Content}",
                    httpResponse.StatusCode,
                    httpResponse.ReasonPhrase,
                    url,
                    content);
                return default;
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken).ConfigureAwait(false);
            if (response == null)
            {
                _logger.LogDebug("TUIMDB API: Response was empty for URL {Url}", url);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TUIMDB API: Failed to fetch data from URL {Url}", url);
            return default;
        }
    }

    /// <summary>
    /// Gets search results for a given <see cref="SeasonInfo"/>.
    /// </summary>
    /// <param name="searchInfo">The season search information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of <see cref="RemoteSearchResult"/>.</returns>
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        SeasonInfo searchInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "TUIMDB GetSearchResults SeasonInfo dump: {SeasonInfoJson}",
            JsonSerializer.Serialize(searchInfo, _jsonOptions));

        var results = new List<RemoteSearchResult>();
        await Task.CompletedTask.ConfigureAwait(false);

        return results;
    }

    /// <summary>
    /// Gets metadata for a season given its <see cref="SeasonInfo"/>.
    /// </summary>
    /// <param name="info">The season information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="MetadataResult{Season}"/> containing metadata.</returns>
    public async Task<MetadataResult<Season>> GetMetadata(
        SeasonInfo info,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "TUIMDB GetMetadata SeasonInfo dump: {SeasonInfoJson}",
            JsonSerializer.Serialize(info, _jsonOptions));

        var result = new MetadataResult<Season>();
        result.HasMetadata = false;

        // Check plugin configuration exists
        if (Plugin.Instance?.Configuration == null)
        {
            _logger.LogError("TUIMDB Season GetMetadata: Plugin configuration is null");
            return result;
        }

        var config = Plugin.Instance.Configuration;
        var url = string.Empty;

        // Get user metadata language
        string metadataLanguage = info.MetadataLanguage ?? "en";

        // User selected title from the search feature in Jellyfin
        info.SeriesProviderIds.TryGetValue("TUIMDB", out var seriesUid);
        if (string.IsNullOrEmpty(seriesUid))
        {
            _logger.LogDebug("TUIMDB Season GetMetadata: No series UID");
        }
        else
        {
            _logger.LogDebug("TUIMDB Season GetMetadata: get series info for uid: {SeriesUid}", seriesUid);
        }

        var seasonNumber = info.IndexNumber;
        if (!seasonNumber.HasValue)
        {
            _logger.LogDebug("TUIMDB Season GetMetadata: No season index number");
        }
        else
        {
            _logger.LogDebug("TUIMDB Season GetMetadata: get season info for index #{SeasonNumber}", seasonNumber);
        }

        info.SeriesProviderIds.TryGetValue("TUIMDB_EpisodeOrder", out var episodeOrder);
        _logger.LogDebug("TUIMDB Episode order: {EpisodeOrder}", episodeOrder);

        info.SeriesProviderIds.TryGetValue("TUIMDB_EpisodeOrderUid", out var episodeOrderUid);
        _logger.LogDebug("TUIMDB Episode order UID: {EpisodeOrderUid}", episodeOrderUid);

        await Task.CompletedTask.ConfigureAwait(false);

        return result;
    }
}
