using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a collection images object returned by the TUIMDB API.
/// </summary>
public class TuimdbCollectionImages
{
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
    /// Gets the list of backdrops for the collection.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Backdrops")]
    public Collection<TuimdbCollectionBackdrop> Backdrops { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary backdrop for the collection.
    /// </summary>
    [JsonPropertyName("Primary Backdrop")]
    public TuimdbCollectionBackdrop? PrimaryBackdrop { get; set; }

    /// <summary>
    /// Gets the list of logos for the collection.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Logos")]
    public Collection<TuimdbCollectionLogo> Logos { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary logo for the collection.
    /// </summary>
    [JsonPropertyName("Primary Logo")]
    public TuimdbCollectionLogo? PrimaryLogo { get; set; }
}
