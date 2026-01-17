using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a movie search result returned by the TUIMDB API.
/// </summary>
public class TuimdbMovieSearchResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the movie.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the movie title.
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the year the movie was released.
    /// </summary>
    [JsonPropertyName("Release Year")]
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the runtime of the movie in minutes.
    /// </summary>
    [JsonPropertyName("Runtime")]
    public int? Runtime { get; set; }

    /// <summary>
    /// Gets or sets the match score for the search result.
    /// </summary>
    [JsonPropertyName("Match Score")]
    public int MatchScore { get; set; }
}
