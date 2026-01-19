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
    /// Season image provider powered by TUIMDB.
    /// </summary>
    public class SeasonImageProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly ILogger<SeasonImageProvider> _logger;

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
        /// Initializes a new instance of the <see cref="SeasonImageProvider"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for this provider.</param>
        public SeasonImageProvider(ILogger<SeasonImageProvider> logger)
        {
            _logger = logger;
            _logger.LogInformation("TUIMDB SeasonImageProvider constructed");
        }

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public string Name => "TUIMDB";

        /// <inheritdoc />
        public bool Supports(BaseItem item)
        {
            return item is Season;
        }

        /// <inheritdoc />
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            yield return ImageType.Primary;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(
            BaseItem item,
            CancellationToken cancellationToken)
        {
            var season = (Season)item;
            var series = season?.Series;

            if (Plugin.Instance?.Configuration == null)
            {
                _logger.LogError("TUIMDB ImageProvider: Plugin configuration is null");
                return Array.Empty<RemoteImageInfo>();
            }

            var config = Plugin.Instance.Configuration;

            var seriesUidString = series?.GetProviderId("TUIMDB");
            if (string.IsNullOrWhiteSpace(seriesUidString) ||
                !int.TryParse(seriesUidString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seriesUid))
            {
                _logger.LogDebug("TUIMDB Season ImageProvider: No series TUIMDB provider ID found");
                return Array.Empty<RemoteImageInfo>();
            }

            var seasonUidString = season?.GetProviderId("TUIMDB");
            if (string.IsNullOrWhiteSpace(seasonUidString) ||
                !int.TryParse(seasonUidString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seasonUid))
            {
                _logger.LogDebug("TUIMDB Season ImageProvider: No season TUIMDB provider ID found");
                return Array.Empty<RemoteImageInfo>();
            }

            var language = item.GetPreferredMetadataLanguage() ?? "en";

            var url = $"{config.ApiBaseUrl}/series/season/posters/?seriesId={seriesUid}&seasonId={seasonUid}&language={language}";
            _logger.LogDebug("TUIMDB Season ImageProvider: Fetching posters from {Url}", url);

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
                        "TUIMDB Season ImageProvider: Failed to fetch season posters. Status: {Status}",
                        response.StatusCode);
                    return Array.Empty<RemoteImageInfo>();
                }

                var seasonImages = await response.Content
                    .ReadFromJsonAsync<TuimdbSeasonImages>(_jsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                if (seasonImages == null)
                {
                    return Array.Empty<RemoteImageInfo>();
                }

                var images = new List<RemoteImageInfo>();

                // Primary poster
                if (seasonImages.PrimaryPoster != null)
                {
                    images.Add(new RemoteImageInfo
                    {
                        Url = $"{config.SeasonPostersUrl}/{seasonImages.PrimaryPoster.Name}",
                        Type = ImageType.Primary,
                        ProviderName = Name,
                        Language = language
                    });
                }

                // Additional posters
                if (seasonImages.Posters != null)
                {
                    foreach (var poster in seasonImages.Posters)
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = $"{config.SeasonPostersUrl}/{poster.Name}",
                            Type = ImageType.Primary,
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
