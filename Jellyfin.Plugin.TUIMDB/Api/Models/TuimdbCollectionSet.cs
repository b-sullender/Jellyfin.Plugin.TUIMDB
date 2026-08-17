using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a collection returned by the TUIMDB API.
/// </summary>
public class TuimdbCollectionSet
{
    /// <summary>
    /// Gets or sets the unique identifier of the collection.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the collection name.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection overview.
    /// </summary>
    [JsonPropertyName("Overview")]
    public string? Overview { get; set; }

    /// <summary>
    /// Gets the list of posters for the collection.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Posters")]
    public Collection<TuimdbCollectionPoster> Posters { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary poster for the collection.
    /// </summary>
    [JsonPropertyName("Primary Poster")]
    public TuimdbCollectionPoster? PrimaryPoster { get; set; }

    /// <summary>
    /// Gets the list of titles for the collection.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Titles")]
    public Collection<TuimdbCollectionTitle> Titles { get; private set; } = new();
}
