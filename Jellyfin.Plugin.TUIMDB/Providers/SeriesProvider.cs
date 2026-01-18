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
    /// Attempts to extract a 4-digit year from a file path or filename.
    /// Looks for a year enclosed in parentheses at the end of the filename, e.g., "Title (1999).mp4".
    /// Returns the year as an <see cref="int"/> if found; otherwise, returns null.
    /// </summary>
    /// <param name="path">The full path or filename of the file.</param>
    /// <returns>The extracted year, or null if no valid year is found.</returns>
    private static int? ExtractYearFromPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
        int start = fileName.LastIndexOf('(');
        int end = fileName.LastIndexOf(')');
        if (start >= 0 && end > start)
        {
            string inside = fileName.Substring(start + 1, end - start - 1);
            if (int.TryParse(inside, out int year))
            {
                return year;
            }
        }

        return null;
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

        // Use Year from SeriesInfo if available, otherwise extract from Path
        int? year = searchInfo.Year ?? ExtractYearFromPath(searchInfo.Path);

        // Build query string including year if available
        string queryString = year.HasValue
            ? $"{searchInfo.Name} ({year.Value})"
            : searchInfo.Name;

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

        // User selected title from the search feature in Jellyfin
        info.ProviderIds.TryGetValue("TUIMDB", out var seriesUid);
        if (string.IsNullOrEmpty(seriesUid))
        {
            // Use Year from SeriesInfo if available, otherwise extract from Path
            int? year = info.Year ?? ExtractYearFromPath(info.Path);

            // Build query string including year if available
            string queryString = year.HasValue
                ? $"{info.Name} ({year.Value})"
                : info.Name;

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
}
