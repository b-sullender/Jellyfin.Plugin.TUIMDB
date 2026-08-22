using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.TUIMDB.Api.Models;
using Jellyfin.Plugin.TUIMDB.Configuration;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TUIMDB.Providers;

/// <summary>
/// Custom provider that creates/updates BoxSets based on TUIMDB collections.
/// </summary>
public class SeriesCollectionProvider : ICustomMetadataProvider<Series>, IHasOrder
{
    private readonly ICollectionManager _collectionManager;
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<SeriesCollectionProvider> _logger;

    /// <summary>
    /// The HTTP client used to call TUIMDB API.
    /// </summary>
    private static readonly HttpClient _httpClient = new HttpClient();

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
    /// Initializes a new instance of the <see cref="SeriesCollectionProvider"/> class.
    /// </summary>
    /// <param name="collectionManager">The collection manager.</param>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    public SeriesCollectionProvider(
        ICollectionManager collectionManager,
        ILibraryManager libraryManager,
        ILogger<SeriesCollectionProvider> logger)
    {
        _collectionManager = collectionManager;
        _libraryManager = libraryManager;
        _logger = logger;
        _logger.LogInformation("TUIMDB SeriesCollectionProvider constructed");
    }

    /// <inheritdoc />
    public string Name => "TUIMDB Collections";

    /// <summary>
    /// Gets the order in which this provider is queried.
    /// Custom (non-pre-refresh "IPreRefreshProvider") providers run last (after remote metadata providers).
    /// </summary>
    public int Order => 0;

    /// <inheritdoc />
    public async Task<ItemUpdateType> FetchAsync(
        Series item,
        MetadataRefreshOptions options,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "TUIMDB SeriesCollectionProvider item dump: {ItemJson}",
            JsonSerializer.Serialize(item, _jsonOptions));

        // Only operate on items that have a TUIMDB id.
        var tuimdbId = item.GetProviderId("TUIMDB");
        if (string.IsNullOrEmpty(tuimdbId))
        {
            return ItemUpdateType.None;
        }

        // Get collection IDs from the item (set by SeriesProvider).
        var collectionsIdString = item.GetProviderId("TUIMDB_COLLECTIONS");
        if (string.IsNullOrWhiteSpace(collectionsIdString))
        {
            _logger.LogDebug(
                "TUIMDB SeriesCollectionProvider: Item '{ItemName}' (Id {ItemId}) has no TUIMDB_COLLECTIONS provider id.",
                item.Name,
                item.Id);
            return ItemUpdateType.None;
        }

        var collectionIds = collectionsIdString
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var distinctCollectionIds = new HashSet<string>(collectionIds, StringComparer.Ordinal);
        if (distinctCollectionIds.Count == 0)
        {
            _logger.LogDebug(
                "TUIMDB SeriesCollectionProvider: Item '{ItemName}' (Id {ItemId}) has an empty TUIMDB_COLLECTIONS list.",
                item.Name,
                item.Id);
            return ItemUpdateType.None;
        }

        // Check plugin configuration exists
        if (Plugin.Instance?.Configuration == null)
        {
            _logger.LogError("TUIMDB SeriesCollectionProvider: Plugin configuration is null");
            return ItemUpdateType.None;
        }

        var config = Plugin.Instance.Configuration;

        foreach (var collectionUidString in distinctCollectionIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Fetch collection details from TUIMDB to get name and overview.
            var collectionInfo = await GetCollectionFromApiAsync(
                config,
                collectionUidString,
                cancellationToken).ConfigureAwait(false);

            if (collectionInfo == null)
            {
                _logger.LogDebug(
                    "TUIMDB SeriesCollectionProvider: Failed to fetch collection info for UID {Uid}.",
                    collectionUidString);
                continue;
            }

            var collectionName = collectionInfo.Name;
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                // Fallback generic name if TUIMDB did not provide one.
                collectionName = $"Collection {collectionUidString}";
            }

            // Try to find an existing BoxSet with this TUIMDB collection id.
            var existing = FindBoxSetByTuimdbId(collectionUidString);

            if (existing == null)
            {
                _logger.LogDebug(
                    "TUIMDB SeriesCollectionProvider: Creating new BoxSet '{Name}' for collection UID {Uid}.",
                    collectionName,
                    collectionUidString);

                var optionsCreate = new CollectionCreationOptions
                {
                    Name = collectionName,
                    ProviderIds = new Dictionary<string, string>
                    {
                        { "TUIMDB", collectionUidString }
                    },
                    ItemIdList = new List<string>
                    {
                        item.Id.ToString("N", CultureInfo.InvariantCulture)
                    }
                };

                var created = await _collectionManager.CreateCollectionAsync(optionsCreate).ConfigureAwait(false);

                if (collectionInfo.Overview != null)
                {
                    created.Overview = collectionInfo.Overview;
                    await created.UpdateToRepositoryAsync(ItemUpdateType.MetadataEdit, CancellationToken.None).ConfigureAwait(false);
                }
            }
            else
            {
                _logger.LogDebug(
                    "TUIMDB SeriesCollectionProvider: Adding item '{ItemName}' to existing BoxSet '{CollectionName}' (UID {Uid}).",
                    item.Name,
                    existing.Name,
                    collectionUidString);

                await _collectionManager
                    .AddToCollectionAsync(existing.Id, new[] { item.Id })
                    .ConfigureAwait(false);
            }
        }

        // This provider only manipulates BoxSets; it does not change the item itself.
        return ItemUpdateType.None;
    }

    private BoxSet? FindBoxSetByTuimdbId(string collectionUid)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = new[] { BaseItemKind.BoxSet },
            CollapseBoxSetItems = false,
            Recursive = true,
            HasAnyProviderId = new Dictionary<string, string>
            {
                { "TUIMDB", collectionUid }
            }
        };

        var result = _libraryManager.QueryItems(query);
        return result.Items.Count > 0 ? result.Items[0] as BoxSet : null;
    }

    /// <summary>
    /// Calls the TUIMDB collections API to fetch collection details.
    /// </summary>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="collectionUid">The TUIMDB collection UID as string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The <see cref="TuimdbCollectionSet"/> or null if the call fails.</returns>
    private async Task<TuimdbCollectionSet?> GetCollectionFromApiAsync(
        PluginConfiguration config,
        string collectionUid,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(collectionUid))
        {
            return null;
        }

        var url = $"{config.ApiBaseUrl}/collections/get/?uid={collectionUid}&includeOverview=true";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Add API key header if provided
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            request.Headers.Add("apiKey", config.ApiKey);
        }

        request.Headers.UserAgent.Add(
            new System.Net.Http.Headers.ProductInfoHeaderValue(config.PluginUserAgent, config.PluginVersion));

        // Log HttpClient default headers
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            _logger.LogDebug(
                "TUIMDB Collections API: HttpClient Default Header: {Name} = {Values}",
                header.Key,
                string.Join(", ", header.Value));
        }

        // Log request-specific headers
        foreach (var header in request.Headers)
        {
            _logger.LogDebug(
                "TUIMDB Collections API: Request Header: {Name} = {Values}",
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
                    "TUIMDB Collections API: HTTP request failed.\nStatus Code: {StatusCode}\nReason: {ReasonPhrase}\nURL: {Url}\nResponse Content: {Content}",
                    httpResponse.StatusCode,
                    httpResponse.ReasonPhrase,
                    url,
                    content);
                return null;
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<TuimdbCollectionSet>(_jsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (response == null)
            {
                _logger.LogDebug("TUIMDB Collections API: Response was empty for URL {Url}", url);
            }
            else
            {
                _logger.LogDebug(
                    "TUIMDB Collections API: Collection info dump for UID {Uid}: {Json}",
                    collectionUid,
                    JsonSerializer.Serialize(response, _jsonOptions));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TUIMDB Collections API: Failed to fetch collection data from URL {Url}", url);
            return null;
        }
    }
}
