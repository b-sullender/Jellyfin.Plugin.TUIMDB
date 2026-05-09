using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TUIMDB.Api.Models;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TUIMDB.Providers
{
    /// <summary>
    /// Person provider powered by TUIMDB.
    /// </summary>
    public class PersonProvider : IRemoteMetadataProvider<Person, PersonLookupInfo>
    {
        /// <summary>
        /// The logger for this provider.
        /// </summary>
        private readonly ILogger<PersonImageProvider> _logger;

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
        /// Initializes a new instance of the <see cref="PersonProvider"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for this provider.</param>
        public PersonProvider(ILogger<PersonImageProvider> logger)
        {
            _logger = logger;
            _logger.LogInformation("TUIMDB PersonProvider constructed");
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

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo, CancellationToken cancellationToken)
        {
            _logger.LogDebug(
                "TUIMDB GetSearchResults PersonLookupInfo dump: {PersonLookupInfoJson}",
                JsonSerializer.Serialize(searchInfo, _jsonOptions));

            // Get user metadata language
            string metadataLanguage = searchInfo.MetadataLanguage ?? "en";

            // Get person name
            string queryString = searchInfo.Name;

            // Check plugin configuration exists
            if (Plugin.Instance?.Configuration == null)
            {
                _logger.LogError("TUIMDB GetMetadata: Plugin configuration is null");
                return new List<RemoteSearchResult>();
            }

            var config = Plugin.Instance.Configuration;

            var url = $"{config.ApiBaseUrl}/people/search/?queryString={Uri.EscapeDataString(queryString)}&includeImages=true&language={metadataLanguage}";
            _logger.LogDebug("TUIMDB GetSearchResults: Query URL = {Url}", url);

            var response = await GetFromApiAsync<List<TuimdbPeopleSearchResult>>(url, config.ApiKey, cancellationToken).ConfigureAwait(false);
            if (response == null || response.Count == 0)
            {
                _logger.LogDebug("TUIMDB Search: No results found.");
                return new List<RemoteSearchResult>();
            }

            var results = new List<RemoteSearchResult>();
            foreach (var person in response)
            {
                var result = new RemoteSearchResult
                {
                    SearchProviderName = "TUIMDB",
                    Name = person.Name,
                };

                if (person.PrimaryImage != null)
                {
                    result.ImageUrl = $"{config.PeopleImagesUrl}/low-res/w200/{person.PrimaryImage.Name}";
                }

                result.SetProviderId("TUIMDB", person.Uid.ToString(CultureInfo.InvariantCulture));
                results.Add(result);
            }

            return results;
        }

        /// <inheritdoc />
        public async Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            // Check plugin configuration exists
            if (Plugin.Instance?.Configuration == null)
            {
                _logger.LogError("TUIMDB PersonProvider: Plugin configuration is null");
                return new MetadataResult<Person>();
            }

            var config = Plugin.Instance.Configuration;

            var personId = Convert.ToInt32(info.GetProviderId("TUIMDB"), CultureInfo.InvariantCulture);

            // We don't already have an Id, need to fetch it
            if (personId <= 0)
            {
                // TODO: search for person using info.Name
                // if (searchResults?.Count > 0)
                // {
                //    personId = searchResults[0].Uid;
                // }
            }

            var result = new MetadataResult<Person>();

            if (personId > 0)
            {
                var url = $"{config.ApiBaseUrl}/people/get/?uid={personId}";
                _logger.LogDebug("TUIMDB PersonProvider GetMetadata: Query URL = {Url}", url);

                var person = await GetFromApiAsync<TuimdbPerson>(url, config.ApiKey, cancellationToken).ConfigureAwait(false);
                if (person is null)
                {
                    return result;
                }

                result.HasMetadata = true;

                var item = new Person
                {
                    Name = info.Name,
                    Overview = person.Biography
                };

                item.SetProviderId("TUIMDB", person.Uid.ToString(CultureInfo.InvariantCulture));

                result.HasMetadata = true;
                result.Item = item;
            }

            return result;
        }
    }
}
