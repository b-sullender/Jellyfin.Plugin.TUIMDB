using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a season returned by the TUIMDB API.
/// </summary>
public class TuimdbSeason
{
    /// <summary>
    /// Gets or sets the unique identifier of the season.
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
    [JsonPropertyName("Order ID")]
    public int OrderUid { get; set; }

    /// <summary>
    /// Gets or sets the season number.
    /// </summary>
    [JsonPropertyName("Season Number")]
    public int SeasonNumber { get; set; }

    /// <summary>
    /// Gets or sets whether the season is locked.
    /// </summary>
    [JsonPropertyName("Locked")]
    public int Locked { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the season in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the season name.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of posters for the season.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Posters")]
    public Collection<TuimdbSeasonPoster> Posters { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary poster for the season.
    /// </summary>
    [JsonPropertyName("Primary Poster")]
    public TuimdbSeasonPoster? PrimaryPoster { get; set; }
}
