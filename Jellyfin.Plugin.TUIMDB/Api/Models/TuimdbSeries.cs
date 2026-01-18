using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a series returned by the TUIMDB API.
/// </summary>
public class TuimdbSeries
{
    /// <summary>
    /// Gets or sets the unique identifier of the series.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the original language code of the series.
    /// </summary>
    [JsonPropertyName("Original Language")]
    public string OriginalLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the year the series was released.
    /// </summary>
    [JsonPropertyName("Release Year")]
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the adult boolean flag for the series.
    /// </summary>
    [JsonPropertyName("Adult")]
    public int? Adult { get; set; }

    /// <summary>
    /// Gets or sets the language code of the metadata.
    /// </summary>
    [JsonPropertyName("Language Code")]
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the series title.
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the series overview.
    /// </summary>
    [JsonPropertyName("Overview")]
    public string? Overview { get; set; }

    /// <summary>
    /// Gets the list of genres associated with the series.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Genres")]
    public Collection<TuimdbGenre> Genres { get; private set; } = new();

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
