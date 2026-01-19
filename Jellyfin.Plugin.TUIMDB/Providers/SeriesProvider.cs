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
/// Provides series metadata and search results from TUIMDB.
/// </summary>
public class SeriesProvider :
    IRemoteMetadataProvider<Series, SeriesInfo>,
    IHasOrder
{
    /// <summary>
    /// The logger for this provider.
    /// </summary>
    private readonly ILogger<SeriesProvider> _logger;

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
    /// Initializes a new instance of the <see cref="SeriesProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for this provider.</param>
    public SeriesProvider(ILogger<SeriesProvider> logger)
    {
        _logger = logger;
        _logger.LogInformation("TUIMDB SeriesProvider constructed");
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
    /// Parses a Jellyfin-style series name and extracts:
    /// - Title (always returned)
    /// - Year (optional, from parentheses)
    /// - Metadata provider IDs (optional, from square brackets)
    /// - Episode order (optional, from curly braces)
    /// Example:
    /// "Friends (1994) [TUIMDB=1] {Standard Order}".
    /// </summary>
    /// <param name="path">Full path or filename.</param>
    /// <returns>A <see cref="SeriesNameInfo"/> containing extracted values.</returns>
    private static SeriesNameInfo ParseSeriesName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return new SeriesNameInfo(
                Title: string.Empty,
                Year: null,
                ProviderIds: new Dictionary<string, string>(),
                EpisodeOrder: null);
        }

        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

        int? year = null;
        string? episodeOrder = null;
        var providerIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // --- Extract year (parentheses) ---
        var yearMatch = Regex.Match(
            fileName,
            @"\((\d{4})\)");

        if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out int parsedYear))
        {
            year = parsedYear;
            fileName = fileName.Replace(yearMatch.Value, string.Empty, StringComparison.Ordinal);
        }

        // --- Extract provider IDs (square brackets) ---
        var providerMatches = Regex.Matches(
            fileName,
            @"\[(?<key>[^\]=]+)=(?<value>[^\]]+)\]");

        foreach (Match match in providerMatches)
        {
            providerIds[match.Groups["key"].Value.Trim()] =
                match.Groups["value"].Value.Trim();

            fileName = fileName.Replace(match.Value, string.Empty, StringComparison.Ordinal);
        }

        // --- Extract episode order (curly braces) ---
        var orderMatch = Regex.Match(
            fileName,
            @"\{([^}]+)\}");

        if (orderMatch.Success)
        {
            episodeOrder = orderMatch.Groups[1].Value.Trim();
            fileName = fileName.Replace(orderMatch.Value, string.Empty, StringComparison.Ordinal);
        }

        // --- Remaining text is the title ---
        string title = fileName.Trim();

        return new SeriesNameInfo(
            Title: title,
            Year: year,
            ProviderIds: providerIds,
            EpisodeOrder: episodeOrder);
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
    /// Gets search results for a given <see cref="SeriesInfo"/>.
    /// </summary>
    /// <param name="searchInfo">The series search information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of <see cref="RemoteSearchResult"/>.</returns>
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        SeriesInfo searchInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "TUIMDB GetSearchResults SeriesInfo dump: {SeriesInfoJson}",
            JsonSerializer.Serialize(searchInfo, _jsonOptions));

        // Parse series name (title, year, provider ids, episode order)
        var parsedName = ParseSeriesName(searchInfo.Path);

        // Log parsed components for debugging
        _logger.LogDebug(
            "TUIMDB Parsed series name info (search): {ParsedSeriesNameJson}",
            JsonSerializer.Serialize(parsedName, _jsonOptions));

        // Prefer SeriesInfo.Year, otherwise parsed year
        int? year = searchInfo.Year ?? parsedName.Year;

        // Build query string including year if available
        string queryString = year.HasValue
            ? $"{parsedName.Title} ({year.Value})"
            : parsedName.Title;

        _logger.LogDebug("TUIMDB GetSearchResults: Query string = {QueryString}", queryString);

        // Get user metadata language
        string metadataLanguage = searchInfo.MetadataLanguage ?? "en";

        // Check plugin configuration exists
        if (Plugin.Instance?.Configuration == null)
        {
            _logger.LogError("TUIMDB GetMetadata: Plugin configuration is null");
            return new List<RemoteSearchResult>();
        }

        var config = Plugin.Instance.Configuration;

        var url = $"{config.ApiBaseUrl}/series/search/?queryString={Uri.EscapeDataString(queryString)}&includePosters=true&language={metadataLanguage}";
        _logger.LogDebug("TUIMDB GetSearchResults: Query URL = {Url}", url);

        var response = await GetFromApiAsync<List<TuimdbSeriesSearchResult>>(url, config.ApiKey, cancellationToken).ConfigureAwait(false);
        if (response == null || response.Count == 0)
        {
            _logger.LogDebug("TUIMDB Search: No results found.");
            return new List<RemoteSearchResult>();
        }

        var results = new List<RemoteSearchResult>();
        foreach (var series in response)
        {
            _logger.LogDebug(
                "TUIMDB Search: Found series '{Title}' ({Year}) with UID {Uid}",
                series.Title,
                series.ReleaseYear,
                series.Uid);

            var result = new RemoteSearchResult
            {
                Name = series.Title,
                ProductionYear = series.ReleaseYear
            };

            if (series.PrimaryPoster != null)
            {
                result.ImageUrl = $"{config.SeriesPostersUrl}/low-res/w400/{series.PrimaryPoster.Name}";
            }

            result.ProviderIds["TUIMDB"] = series.Uid.ToString(CultureInfo.InvariantCulture);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Gets metadata for a series given its <see cref="SeriesInfo"/>.
    /// </summary>
    /// <param name="info">The series information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="MetadataResult{Series}"/> containing metadata.</returns>
    public async Task<MetadataResult<Series>> GetMetadata(
        SeriesInfo info,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "TUIMDB GetMetadata SeriesInfo dump: {SeriesInfoJson}",
            JsonSerializer.Serialize(info, _jsonOptions));

        var result = new MetadataResult<Series>();
        result.HasMetadata = false;

        // Check plugin configuration exists
        if (Plugin.Instance?.Configuration == null)
        {
            _logger.LogError("TUIMDB GetMetadata: Plugin configuration is null");
            return result;
        }

        var config = Plugin.Instance.Configuration;
        var url = string.Empty;

        // Get user metadata language
        string metadataLanguage = info.MetadataLanguage ?? "en";

        // Parse series name (title, year, provider ids, episode order)
        var parsedName = ParseSeriesName(info.Path);

        // Log parsed components for debugging
        _logger.LogDebug(
            "TUIMDB Parsed series name info: {ParsedSeriesNameJson}",
            JsonSerializer.Serialize(parsedName, _jsonOptions));

        // User selected title from the search feature in Jellyfin
        info.ProviderIds.TryGetValue("TUIMDB", out var seriesUid);
        if (string.IsNullOrEmpty(seriesUid))
        {
            // Prefer SeriesInfo.Year, otherwise parsed year
            int? year = info.Year ?? parsedName.Year;

            // Build query string including year if available
            string queryString = year.HasValue
                ? $"{parsedName.Title} ({year.Value})"
                : parsedName.Title;

            _logger.LogDebug("TUIMDB GetMetadata: Query string = {QueryString}", queryString);

            url = $"{config.ApiBaseUrl}/series/search/?queryString={Uri.EscapeDataString(queryString)}";
            _logger.LogDebug("TUIMDB GetMetadata: Query URL = {Url}", url);

            var searchResults = await GetFromApiAsync<List<TuimdbSeriesSearchResult>>(url, config.ApiKey, cancellationToken).ConfigureAwait(false);
            if (searchResults == null || searchResults.Count == 0)
            {
                _logger.LogDebug("TUIMDB Search: No results found.");
                return result;
            }

            seriesUid = searchResults[0].Uid.ToString(CultureInfo.InvariantCulture);
        }

        url = $"{config.ApiBaseUrl}/series/get/?uid={seriesUid}&language={metadataLanguage}";
        _logger.LogDebug("TUIMDB GetMetadata: Query URL = {Url}", url);

        var seriesInfo = await GetFromApiAsync<TuimdbSeries>(url, config.ApiKey, cancellationToken).ConfigureAwait(false);
        if (seriesInfo == null)
        {
            _logger.LogDebug("TUIMDB Details: Failed to get series info with UID {Uid}.", seriesUid);
            return result;
        }

        _logger.LogDebug(
            "TUIMDB GetMetadata Series Info dump: {MetadataJson}",
            JsonSerializer.Serialize(seriesInfo, _jsonOptions));

        var series = new Series();
        series.SetProviderId("TUIMDB", seriesUid);

        series.Name = seriesInfo.Title;
        series.Overview = seriesInfo.Overview;
        series.ProductionYear = seriesInfo.ReleaseYear;

        foreach (var genre in seriesInfo.Genres)
        {
            series.AddGenre(genre.Name);
            _logger.LogDebug("Added genre: {Genre}", genre.Name);
        }

        int? episodeOrderUid = null;

        if (seriesInfo.Order is not null && seriesInfo.Order.Count != 0)
        {
            if (parsedName.EpisodeOrder is not null)
            {
                foreach (var order in seriesInfo.Order)
                {
                    if (order.Name == parsedName.EpisodeOrder)
                    {
                        episodeOrderUid = order.Uid;
                    }
                }
            }

            if (episodeOrderUid == null)
            {
                episodeOrderUid = seriesInfo.Order[0].Uid;
            }
        }

        if (!string.IsNullOrWhiteSpace(parsedName.EpisodeOrder))
        {
            series.SetProviderId("TUIMDB_EpisodeOrder", parsedName.EpisodeOrder);
        }

        if (episodeOrderUid.HasValue)
        {
            series.SetProviderId("TUIMDB_EpisodeOrderUid", episodeOrderUid.Value.ToString(CultureInfo.InvariantCulture));
        }

        result.HasMetadata = true;
        result.Provider = "TUIMDB";
        result.ResultLanguage = seriesInfo.LanguageCode;
        result.Item = series;

        _logger.LogDebug(
            "TUIMDB GetMetadata Series Class dump: {MetadataJson}",
            JsonSerializer.Serialize(series, _jsonOptions));

        _logger.LogDebug(
            "TUIMDB GetMetadata MetadataResult<Series> dump: {MetadataJson}",
            JsonSerializer.Serialize(result, _jsonOptions));

        return result;
    }

    /// <summary>
    /// Represents metadata extracted from a Jellyfin-style series name,
    /// including the normalized title and optional identifying information
    /// such as release year, metadata provider IDs, and plugin-defined
    /// episode ordering hints.
    /// </summary>
    private sealed record SeriesNameInfo(
        string Title,
        int? Year,
        Dictionary<string, string> ProviderIds,
        string? EpisodeOrder
    );
}
