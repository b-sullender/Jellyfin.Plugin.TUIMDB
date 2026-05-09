using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
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
/// Provides episode metadata and search results from TUIMDB.
/// </summary>
public class EpisodeProvider :
    IRemoteMetadataProvider<Episode, EpisodeInfo>,
    IHasOrder
{
    /// <summary>
    /// The logger for this provider.
    /// </summary>
    private readonly ILogger<EpisodeProvider> _logger;

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
    /// Initializes a new instance of the <see cref="EpisodeProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for this provider.</param>
    public EpisodeProvider(ILogger<EpisodeProvider> logger)
    {
        _logger = logger;
        _logger.LogInformation("TUIMDB EpisodeProvider constructed");
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
    /// Gets search results for a given <see cref="EpisodeInfo"/>.
    /// </summary>
    /// <param name="searchInfo">The episode search information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of <see cref="RemoteSearchResult"/>.</returns>
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        EpisodeInfo searchInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "TUIMDB GetSearchResults EpisodeInfo dump: {EpisodeInfoJson}",
            JsonSerializer.Serialize(searchInfo, _jsonOptions));

        var results = Array.Empty<RemoteSearchResult>();

        // TODO: Implement episode search against TUIMDB API and populate results.

        // NOTE: This method is not currently used by Jellyfin's standard metadata workflows
        // for Episode items. Episode metadata is typically resolved through Series-level
        // identification and season/episode ordering derived from library structure and
        // provider data, rather than direct episode-level search or identify flows.

        await Task.CompletedTask.ConfigureAwait(false);

        return results;
    }

    /// <summary>
    /// Gets metadata for a episode given its <see cref="EpisodeInfo"/>.
    /// </summary>
    /// <param name="info">The episode information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="MetadataResult{Episode}"/> containing metadata.</returns>
    public async Task<MetadataResult<Episode>> GetMetadata(
        EpisodeInfo info,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "TUIMDB GetMetadata EpisodeInfo dump: {EpisodeInfoJson}",
            JsonSerializer.Serialize(info, _jsonOptions));

        var result = new MetadataResult<Episode>();
        result.HasMetadata = false;

        // Check plugin configuration exists
        if (Plugin.Instance?.Configuration == null)
        {
            _logger.LogError("TUIMDB Episode GetMetadata: Plugin configuration is null");
            return result;
        }

        var config = Plugin.Instance.Configuration;
        var url = string.Empty;

        // Get user metadata language
        string metadataLanguage = info.MetadataLanguage ?? "en";

        // Get series UID and season UID
        info.SeriesProviderIds.TryGetValue("TUIMDB", out var seriesUid);
        info.SeasonProviderIds.TryGetValue("TUIMDB", out var seasonUid);
        var episodeNumber = info.IndexNumber;

        _logger.LogDebug("TUIMDB Episode GetMetadata: Series UID: {SeriesUid}", seriesUid);
        _logger.LogDebug("TUIMDB Episode GetMetadata: Season UID: {SeasonUid}", seasonUid);
        _logger.LogDebug("TUIMDB Episode GetMetadata: Episode number: {EpisodeNumber}", episodeNumber);

        if (string.IsNullOrEmpty(seriesUid) || string.IsNullOrEmpty(seasonUid) || !episodeNumber.HasValue)
        {
            _logger.LogDebug("TUIMDB Episode GetMetadata: Missing series UID, season UID, or episode number");
            return result;
        }

        url = $"{config.ApiBaseUrl}/series/season/episodes/?seriesId={seriesUid}&seasonId={seasonUid}&episodeNumber={episodeNumber}&language={metadataLanguage}&includeCast=true";
        _logger.LogDebug("TUIMDB Season GetMetadata: Query URL = {Url}", url);

        var episodes = await GetFromApiAsync<List<TuimdbEpisode>>(url, config.ApiKey, cancellationToken).ConfigureAwait(false);
        if (episodes == null || episodes.Count == 0)
        {
            _logger.LogDebug("TUIMDB: Failed to get episode info.");
            return result;
        }

        var episodeInfo = episodes[0];

        _logger.LogDebug(
            "TUIMDB GetMetadata Episode Info dump: {MetadataJson}",
            JsonSerializer.Serialize(episodeInfo, _jsonOptions));

        result.HasMetadata = true;

        DateTime? premiereDate = null;
        if (DateTime.TryParseExact(
            episodeInfo.AirDate,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedDate))
        {
            premiereDate = parsedDate;
        }

        result.Item = new Episode
        {
            IndexNumber = episodeNumber,
            Name = episodeInfo.Name,
            Overview = episodeInfo.Overview,
            PremiereDate = premiereDate
        };

        if (episodeInfo.Cast is not null && episodeInfo.Cast.Count != 0)
        {
            foreach (var actor in episodeInfo.Cast)
            {
                var personInfo = new PersonInfo
                {
                    Name = actor.Name,
                    Role = actor.Characters,
                    Type = PersonKind.Actor,
                    SortOrder = actor.Order
                };

                if (actor.PrimaryImage is not null)
                {
                    personInfo.ImageUrl = $"{config.PeopleImagesUrl}/{actor.PrimaryImage.Name}";
                }

                personInfo.SetProviderId("TUIMDB", actor.Uid.ToString(CultureInfo.InvariantCulture));

                result.AddPerson(personInfo);
            }
        }

        result.Item.SetProviderId("TUIMDB", episodeInfo.Uid.ToString(CultureInfo.InvariantCulture));

        return result;
    }
}
