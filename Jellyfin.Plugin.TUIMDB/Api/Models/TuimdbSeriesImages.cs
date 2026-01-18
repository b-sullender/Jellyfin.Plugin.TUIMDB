using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a series images object returned by the TUIMDB API.
/// </summary>
public class TuimdbSeriesImages
{
    /// <summary>
    /// Gets the list of posters for the series.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Posters")]
    public Collection<TuimdbSeriesPoster> Posters { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary poster for the series.
    /// </summary>
    [JsonPropertyName("Primary Poster")]
    public TuimdbSeriesPoster? PrimaryPoster { get; set; }

    /// <summary>
    /// Gets the list of backdrops for the series.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Backdrops")]
    public Collection<TuimdbSeriesBackdrop> Backdrops { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary backdrop for the series.
    /// </summary>
    [JsonPropertyName("Primary Backdrop")]
    public TuimdbSeriesBackdrop? PrimaryBackdrop { get; set; }

    /// <summary>
    /// Gets the list of Logos for the series.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Logos")]
    public Collection<TuimdbSeriesLogo> Logos { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary logo for the series.
    /// </summary>
    [JsonPropertyName("Primary Logo")]
    public TuimdbSeriesLogo? PrimaryLogo { get; set; }
}
