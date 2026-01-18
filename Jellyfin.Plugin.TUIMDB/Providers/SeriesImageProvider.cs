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
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TUIMDB.Providers
{
    /// <summary>
    /// Series image provider powered by TUIMDB.
    /// </summary>
    public class SeriesImageProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly ILogger<SeriesImageProvider> _logger;

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
        /// Initializes a new instance of the <see cref="SeriesImageProvider"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for this provider.</param>
        public SeriesImageProvider(ILogger<SeriesImageProvider> logger)
        {
            _logger = logger;
            _logger.LogInformation("TUIMDB SeriesImageProvider constructed");
        }

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public string Name => "TUIMDB";

        /// <inheritdoc />
        public bool Supports(BaseItem item)
        {
            return item is Series;
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

            var url = $"{config.ApiBaseUrl}/series/images/?uid={tuimdbId}&language={language}";
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
                        "TUIMDB ImageProvider: Failed to fetch series images. Status: {Status}",
                        response.StatusCode);
                    return Array.Empty<RemoteImageInfo>();
                }

                var seriesImages = await response.Content
                    .ReadFromJsonAsync<TuimdbSeriesImages>(_jsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                if (seriesImages == null)
                {
                    return Array.Empty<RemoteImageInfo>();
                }

                var images = new List<RemoteImageInfo>();

                // Primary poster
                if (seriesImages.PrimaryPoster != null)
                {
                    images.Add(new RemoteImageInfo
                    {
                        Url = $"{config.SeriesPostersUrl}/{seriesImages.PrimaryPoster.Name}",
                        Type = ImageType.Primary,
                        ProviderName = Name,
                        Language = language
                    });
                }

                // Additional posters
                if (seriesImages.Posters != null)
                {
                    foreach (var poster in seriesImages.Posters)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.SeriesPostersUrl}/{poster.Name}",
                            Type = ImageType.Primary,
                            ProviderName = Name,
                            Language = language
                        });
                    }
                }

                // Backdrops
                if (seriesImages.Backdrops != null)
                {
                    foreach (var backdrop in seriesImages.Backdrops)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.SeriesBackdropsUrl}/{backdrop.Name}",
                            Type = ImageType.Backdrop,
                            ProviderName = Name,
                            Language = language
                        });
                    }
                }

                // Logos
                if (seriesImages.Logos != null)
                {
                    foreach (var logo in seriesImages.Logos)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.SeriesLogosUrl}/{logo.Name}",
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
