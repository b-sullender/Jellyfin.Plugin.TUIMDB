using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a series search result returned by the TUIMDB API.
/// </summary>
public class TuimdbSeriesSearchResult
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
    /// Gets or sets the adult boolean flag for the movie.
    /// </summary>
    [JsonPropertyName("Adult")]
    public int? Adult { get; set; }

    /// <summary>
    /// Gets or sets the series title.
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the match score for the search result.
    /// </summary>
    [JsonPropertyName("Match Score")]
    public int MatchScore { get; set; }

    /// <summary>
    /// Gets or sets the primary poster of the series.
    /// </summary>
    [JsonPropertyName("Primary Poster")]
    public TuimdbSeriesPoster? PrimaryPoster { get; set; }
}
