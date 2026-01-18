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
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
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
        WriteIndented = true,
        IncludeFields = true
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

        // Get user metadata language
        string metadataLanguage = searchInfo.MetadataLanguage ?? "en";

        // Check plugin configuration exists
        if (Plugin.Instance?.Configuration == null)
        {
            _logger.LogError("TUIMDB GetMetadata: Plugin configuration is null");
            return new List<RemoteSearchResult>();
        }

        var config = Plugin.Instance.Configuration;

        var url = $"{config.ApiBaseUrl}/movies/search/?queryString={Uri.EscapeDataString(queryString)}&includePosters=true&language={metadataLanguage}";
        _logger.LogDebug("TUIMDB GetSearchResults: Query URL = {Url}", url);

        var response = await GetFromApiAsync<List<TuimdbMovieSearchResult>>(url, config.ApiKey, cancellationToken).ConfigureAwait(false);
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

            if (movie.PrimaryPoster != null)
            {
                result.ImageUrl = $"{config.MoviePostersUrl}/low-res/w400/{movie.PrimaryPoster.Name}";
            }

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
        info.ProviderIds.TryGetValue("TUIMDB", out var movieUid);
        if (string.IsNullOrEmpty(movieUid))
        {
            // Use Year from MovieInfo if available, otherwise extract from Path
            int? year = info.Year ?? ExtractYearFromPath(info.Path);

            // Build query string including year if available
            string queryString = year.HasValue
                ? $"{info.Name} ({year.Value})"
                : info.Name;

            _logger.LogDebug("TUIMDB GetMetadata: Query string = {QueryString}", queryString);

            url = $"{config.ApiBaseUrl}/movies/search/?queryString={Uri.EscapeDataString(queryString)}";
            _logger.LogDebug("TUIMDB GetMetadata: Query URL = {Url}", url);

            var searchResults = await GetFromApiAsync<List<TuimdbMovieSearchResult>>(url, config.ApiKey, cancellationToken).ConfigureAwait(false);
            if (searchResults == null || searchResults.Count == 0)
            {
                _logger.LogDebug("TUIMDB Search: No results found.");
                return result;
            }

            movieUid = searchResults[0].Uid.ToString(CultureInfo.InvariantCulture);
        }

        url = $"{config.ApiBaseUrl}/movies/get/?uid={movieUid}&language={metadataLanguage}";
        _logger.LogDebug("TUIMDB GetMetadata: Query URL = {Url}", url);

        var movieInfo = await GetFromApiAsync<TuimdbMovie>(url, config.ApiKey, cancellationToken).ConfigureAwait(false);
        if (movieInfo == null)
        {
            _logger.LogDebug("TUIMDB Details: Failed to get movie info with UID {Uid}.", movieUid);
            return result;
        }

        _logger.LogDebug(
            "TUIMDB GetMetadata Movie Info dump: {MetadataJson}",
            JsonSerializer.Serialize(movieInfo, _jsonOptions));

        var movie = new Movie();
        movie.SetProviderId("TUIMDB", movieUid);

        movie.Name = movieInfo.Title;
        movie.Overview = movieInfo.Overview;
        movie.ProductionYear = movieInfo.ReleaseYear;

        foreach (var genre in movieInfo.Genres)
        {
            movie.AddGenre(genre.Name);
            _logger.LogDebug("Added genre: {Genre}", genre.Name);
        }

        movie.OfficialRating = movieInfo.ContentRating;

        result.HasMetadata = true;
        result.Provider = "TUIMDB";
        result.ResultLanguage = movieInfo.LanguageCode;
        result.Item = movie;

        if (movieInfo.Cast is not null && movieInfo.Cast.Count != 0)
        {
            foreach (var actor in movieInfo.Cast)
            {
                var personInfo = new PersonInfo
                {
                    Name = actor.Name,
                    Role = actor.Character,
                    Type = PersonKind.Actor,
                    SortOrder = actor.Order
                };

                if (actor.PrimaryImage is not null)
                {
                    personInfo.ImageUrl = $"{config.PeopleImagesUrl}/{actor.PrimaryImage.Name}";
                }

                personInfo.SetProviderId("TUIMDB", actor.PersonId.ToString(CultureInfo.InvariantCulture));

                result.AddPerson(personInfo);
            }
        }

        _logger.LogDebug(
            "TUIMDB GetMetadata Movie Class dump: {MetadataJson}",
            JsonSerializer.Serialize(movie, _jsonOptions));

        _logger.LogDebug(
            "TUIMDB GetMetadata MetadataResult<Movie> dump: {MetadataJson}",
            JsonSerializer.Serialize(result, _jsonOptions));

        return result;
    }
}
