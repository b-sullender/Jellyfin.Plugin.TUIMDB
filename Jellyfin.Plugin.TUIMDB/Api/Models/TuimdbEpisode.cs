using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a episode returned by the TUIMDB API.
/// </summary>
public class TuimdbEpisode
{
    /// <summary>
    /// Gets or sets the unique identifier of the episode.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the series.
    /// </summary>
    [JsonPropertyName("Series ID")]
    public int SeriesUid { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the episode order.
    /// </summary>
    [JsonPropertyName("Runtime")]
    public int Runtime { get; set; }

    /// <summary>
    /// Gets or sets the air date of the episode.
    /// </summary>
    [JsonPropertyName("Air Date")]
    public string AirDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp of the episode in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the episode order number (episode number).
    /// </summary>
    [JsonPropertyName("Order")]
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the episode name.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the episode overview.
    /// </summary>
    [JsonPropertyName("Overview")]
    public string Overview { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of posters for the episode.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Backdrops")]
    public Collection<TuimdbEpisodeBackdrop> Backdrops { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary poster for the season.
    /// </summary>
    [JsonPropertyName("Primary Backdrop")]
    public TuimdbEpisodeBackdrop? PrimaryBackdrop { get; set; }
}
