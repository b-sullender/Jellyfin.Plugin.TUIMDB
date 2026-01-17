using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TUIMDB.Api.Models;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TUIMDB.Providers;

/// <summary>
/// Provides movie metadata and search results from TUIMDB.
/// </summary>
public class MovieProvider :
    IRemoteMetadataProvider<Movie, MovieInfo>,
    IRemoteSearchProvider<MovieInfo>,
    IHasOrder
{
    /// <summary>
    /// The logger for this provider.
    /// </summary>
    private readonly ILogger<MovieProvider> _logger;

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
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="MovieProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for this provider.</param>
    public MovieProvider(ILogger<MovieProvider> logger)
    {
        _logger = logger;
        _logger.LogInformation("TUIMDB MovieProvider constructed");
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
    /// Attempts to extract a 4-digit year from a movie file path or filename.
    /// Looks for a year enclosed in parentheses at the end of the filename, e.g., "Movie Name (1999).mp4".
    /// Returns the year as an <see cref="int"/> if found; otherwise, returns null.
    /// </summary>
    /// <param name="path">The full path or filename of the movie file.</param>
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
    /// Gets search results for a given <see cref="MovieInfo"/>.
    /// </summary>
    /// <param name="searchInfo">The movie search information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of <see cref="RemoteSearchResult"/>.</returns>
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        MovieInfo searchInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "TUIMDB GetSearchResults MovieInfo dump: {MovieInfoJson}",
            JsonSerializer.Serialize(searchInfo, _jsonOptions));

        // Use Year from MovieInfo if available, otherwise extract from Path
        int? year = searchInfo.Year ?? ExtractYearFromPath(searchInfo.Path);

        // Build query string including year if available
        string queryString = year.HasValue
            ? $"{searchInfo.Name} ({year.Value})"
            : searchInfo.Name;

        _logger.LogDebug("TUIMDB GetSearchResults: Query string = {QueryString}", queryString);

        // Check plugin configuration exists
        if (Plugin.Instance?.Configuration == null)
        {
            _logger.LogError("TUIMDB GetMetadata: Plugin configuration is null");
            return new List<RemoteSearchResult>();
        }

        var config = Plugin.Instance.Configuration;
        var url = $"{config.ApiBaseUrl}/movies/search/?queryString={Uri.EscapeDataString(queryString)}";

        _logger.LogDebug("TUIMDB GetMetadata: Query URL = {Url}", url);

        List<TuimdbMovieSearchResult>? response;
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Log all default headers and their values
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            _logger.LogDebug(
                "TUIMDB GetSearchResults: HttpClient Default Header: {Name} = {Values}",
                header.Key,
                string.Join(", ", header.Value));
        }

        // Log request-specific headers
        foreach (var header in request.Headers)
        {
            _logger.LogDebug(
                "TUIMDB GetSearchResults: Request Header: {Name} = {Values}",
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
                    "TUIMDB Search: HTTP request failed.\nStatus Code: {StatusCode}\nReason: {ReasonPhrase}\nURL: {Url}\nResponse Content: {Content}",
                    httpResponse.StatusCode,
                    httpResponse.ReasonPhrase,
                    url,
                    content);
                return new List<RemoteSearchResult>();
            }

            response = await httpResponse.Content.ReadFromJsonAsync<List<TuimdbMovieSearchResult>>(_jsonOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TUIMDB Search: Failed to fetch search results.");
            return new List<RemoteSearchResult>();
        }

        if (response == null || response.Count == 0)
        {
            _logger.LogDebug("TUIMDB Search: No results found.");
            return new List<RemoteSearchResult>();
        }

        var results = new List<RemoteSearchResult>();
        foreach (var movie in response)
        {
            _logger.LogDebug(
                "TUIMDB Search: Found movie '{Title}' ({Year}) with UID {Uid}",
                movie.Title,
                movie.ReleaseYear,
                movie.Uid);

            var result = new RemoteSearchResult
            {
                Name = movie.Title,
                ProductionYear = movie.ReleaseYear
            };
            result.ProviderIds["TUIMDB"] = movie.Uid.ToString(CultureInfo.InvariantCulture);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Gets metadata for a movie given its <see cref="MovieInfo"/>.
    /// </summary>
    /// <param name="info">The movie information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="MetadataResult{Movie}"/> containing metadata.</returns>
    public async Task<MetadataResult<Movie>> GetMetadata(
        MovieInfo info,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "TUIMDB GetMetadata MovieInfo dump: {MovieInfoJson}",
            JsonSerializer.Serialize(info, _jsonOptions));

        var result = new MetadataResult<Movie>();

        // Use Year from MovieInfo if available, otherwise extract from Path
        int? year = info.Year ?? ExtractYearFromPath(info.Path);

        // Build query string including year if available
        string queryString = year.HasValue
            ? $"{info.Name} ({year.Value})"
            : info.Name;

        _logger.LogDebug("TUIMDB GetMetadata: Query string = {QueryString}", queryString);

        // Check plugin configuration exists
        if (Plugin.Instance?.Configuration == null)
        {
            _logger.LogError("TUIMDB GetMetadata: Plugin configuration is null");
            return result;
        }

        var config = Plugin.Instance.Configuration;
        var url = $"{config.ApiBaseUrl}/movies/search/?queryString={Uri.EscapeDataString(queryString)}";

        _logger.LogDebug("TUIMDB GetMetadata: Query URL = {Url}", url);

        // Code placeholder
        await Task.CompletedTask.ConfigureAwait(false);

        result.HasMetadata = false;

        return result;
    }
}
