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
    /// BoxSet (collection) image provider powered by TUIMDB.
    /// </summary>
    public class CollectionImageProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly ILogger<CollectionImageProvider> _logger;

        private static readonly HttpClient _httpClient = new HttpClient
        {
            DefaultRequestHeaders =
            {
                UserAgent =
                {
                    new System.Net.Http.Headers.ProductInfoHeaderValue("Jellyfin_Plugin", "1.1.0.0")
                }
            }
        };

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionImageProvider"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for this provider.</param>
        public CollectionImageProvider(ILogger<CollectionImageProvider> logger)
        {
            _logger = logger;
            _logger.LogInformation("TUIMDB CollectionImageProvider constructed");
        }

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public string Name => "TUIMDB";

        /// <inheritdoc />
        public bool Supports(BaseItem item)
        {
            return item is BoxSet;
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
                _logger.LogError("TUIMDB CollectionImageProvider: Plugin configuration is null");
                return Array.Empty<RemoteImageInfo>();
            }

            var config = Plugin.Instance.Configuration;

            var tuimdbIdString = item.GetProviderId("TUIMDB");
            if (string.IsNullOrWhiteSpace(tuimdbIdString) ||
                !int.TryParse(tuimdbIdString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var tuimdbId))
            {
                _logger.LogDebug("TUIMDB CollectionImageProvider: No TUIMDB provider ID found on BoxSet");
                return Array.Empty<RemoteImageInfo>();
            }

            var language = item.GetPreferredMetadataLanguage() ?? "en";

            var url = $"{config.ApiBaseUrl}/collections/images/?uid={tuimdbId}&language={language}";
            _logger.LogDebug("TUIMDB CollectionImageProvider: Fetching images from {Url}", url);

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
                        "TUIMDB CollectionImageProvider: Failed to fetch collection images. Status: {Status}",
                        response.StatusCode);
                    return Array.Empty<RemoteImageInfo>();
                }

                var collectionImages = await response.Content
                    .ReadFromJsonAsync<TuimdbCollectionImages>(_jsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                if (collectionImages == null)
                {
                    return Array.Empty<RemoteImageInfo>();
                }

                var images = new List<RemoteImageInfo>();

                // Primary poster
                if (collectionImages.PrimaryPoster != null)
                {
                    images.Add(new RemoteImageInfo
                    {
                        Url = $"{config.CollectionPostersUrl}/{collectionImages.PrimaryPoster.Name}",
                        Type = ImageType.Primary,
                        ProviderName = Name,
                        Language = language
                    });
                }

                // Additional posters
                if (collectionImages.Posters != null)
                {
                    foreach (var poster in collectionImages.Posters)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.CollectionPostersUrl}/{poster.Name}",
                            Type = ImageType.Primary,
                            ProviderName = Name,
                            Language = language
                        });
                    }
                }

                // Backdrops
                if (collectionImages.Backdrops != null)
                {
                    foreach (var backdrop in collectionImages.Backdrops)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.CollectionBackdropsUrl}/{backdrop.Name}",
                            Type = ImageType.Backdrop,
                            ProviderName = Name,
                            Language = language
                        });
                    }
                }

                // Logos
                if (collectionImages.Logos != null)
                {
                    foreach (var logo in collectionImages.Logos)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.CollectionLogosUrl}/{logo.Name}",
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
                _logger.LogError(ex, "TUIMDB CollectionImageProvider: Exception while fetching images");
                return Array.Empty<RemoteImageInfo>();
            }
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> GetImageResponse(
            string url,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("TUIMDB CollectionImageProvider: Fetching image {Url}", url);
            return _httpClient.GetAsync(url, cancellationToken);
        }
    }
}
