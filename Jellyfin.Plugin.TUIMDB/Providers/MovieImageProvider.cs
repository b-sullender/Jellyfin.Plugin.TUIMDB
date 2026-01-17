using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TUIMDB.Api.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TUIMDB.Providers
{
    /// <summary>
    /// Movie image provider powered by TUIMDB.
    /// </summary>
    public class MovieImageProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly ILogger<MovieImageProvider> _logger;

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

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MovieImageProvider"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for this provider.</param>
        public MovieImageProvider(ILogger<MovieImageProvider> logger)
        {
            _logger = logger;
            _logger.LogInformation("TUIMDB MovieImageProvider constructed");
        }

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public string Name => "TUIMDB";

        /// <inheritdoc />
        public bool Supports(BaseItem item)
        {
            return item is Movie;
        }

        /// <inheritdoc />
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new[]
            {
                ImageType.Primary,
                ImageType.Backdrop,
                ImageType.Logo,
                ImageType.Thumb
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(
            BaseItem item,
            CancellationToken cancellationToken)
        {
            if (Plugin.Instance?.Configuration == null)
            {
                _logger.LogError("TUIMDB ImageProvider: Plugin configuration is null");
                return Array.Empty<RemoteImageInfo>();
            }

            var config = Plugin.Instance.Configuration;

            var tuimdbIdString = item.GetProviderId("TUIMDB");
            if (string.IsNullOrWhiteSpace(tuimdbIdString) ||
                !int.TryParse(tuimdbIdString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var tuimdbId))
            {
                _logger.LogDebug("TUIMDB ImageProvider: No TUIMDB provider ID found");
                return Array.Empty<RemoteImageInfo>();
            }

            var language = item.GetPreferredMetadataLanguage() ?? "en";

            var url = $"{config.ApiBaseUrl}/movies/images/?uid={tuimdbId}&language={language}";
            _logger.LogDebug("TUIMDB ImageProvider: Fetching images from {Url}", url);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(config.ApiKey))
            {
                request.Headers.Add("apiKey", config.ApiKey);
            }

            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "TUIMDB ImageProvider: Failed to fetch movie images. Status: {Status}",
                        response.StatusCode);
                    return Array.Empty<RemoteImageInfo>();
                }

                var movieImages = await response.Content
                    .ReadFromJsonAsync<TuimdbMovieImages>(_jsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                if (movieImages == null)
                {
                    return Array.Empty<RemoteImageInfo>();
                }

                var images = new List<RemoteImageInfo>();

                // Primary poster
                if (movieImages.PrimaryPoster != null)
                {
                    images.Add(new RemoteImageInfo
                    {
                        Url = $"{config.MoviePostersUrl}/{movieImages.PrimaryPoster.Name}",
                        Type = ImageType.Primary,
                        ProviderName = Name,
                        Language = language
                    });
                }

                // Additional posters
                if (movieImages.Posters != null)
                {
                    foreach (var poster in movieImages.Posters)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.MoviePostersUrl}/{poster.Name}",
                            Type = ImageType.Primary,
                            ProviderName = Name,
                            Language = language
                        });
                    }
                }

                // Backdrops
                if (movieImages.Backdrops != null)
                {
                    foreach (var backdrop in movieImages.Backdrops)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.MovieBackdropsUrl}/{backdrop.Name}",
                            Type = ImageType.Backdrop,
                            ProviderName = Name,
                            Language = language
                        });
                    }
                }

                // Logos
                if (movieImages.Logos != null)
                {
                    foreach (var logo in movieImages.Logos)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.MovieLogosUrl}/{logo.Name}",
                            Type = ImageType.Logo,
                            ProviderName = Name,
                            Language = language
                        });
                    }
                }

                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TUIMDB ImageProvider: Exception while fetching images");
                return Array.Empty<RemoteImageInfo>();
            }
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> GetImageResponse(
            string url,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("TUIMDB ImageProvider: Fetching image {Url}", url);
            return _httpClient.GetAsync(url, cancellationToken);
        }
    }
}
