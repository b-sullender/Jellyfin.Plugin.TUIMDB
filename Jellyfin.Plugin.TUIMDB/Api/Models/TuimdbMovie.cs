using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a movie returned by the TUIMDB API.
/// </summary>
public class TuimdbMovie
{
    /// <summary>
    /// Gets or sets the unique identifier of the movie.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the original language code of the movie.
    /// </summary>
    [JsonPropertyName("Original Language")]
    public string OriginalLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the year the movie was released.
    /// </summary>
    [JsonPropertyName("Release Year")]
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the adult boolean flag for the movie.
    /// </summary>
    [JsonPropertyName("Adult")]
    public int? Adult { get; set; }

    /// <summary>
    /// Gets or sets the runtime of the movie in minutes.
    /// </summary>
    [JsonPropertyName("Runtime")]
    public int? Runtime { get; set; }

    /// <summary>
    /// Gets or sets the language code of the metadata.
    /// </summary>
    [JsonPropertyName("Language Code")]
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the movie title.
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the movie overview.
    /// </summary>
    [JsonPropertyName("Overview")]
    public string? Overview { get; set; }

    /// <summary>
    /// Gets the list of genres associated with the movie.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Genres")]
    public Collection<TuimdbGenre> Genres { get; private set; } = new();

    /// <summary>
    /// Gets or sets the content rating of the movie.
    /// </summary>
    [JsonPropertyName("Content Rating")]
    public string ContentRating { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of posters for the movie.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Posters")]
    public Collection<TuimdbMoviePoster> Posters { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary poster for the movie.
    /// </summary>
    [JsonPropertyName("Primary Poster")]
    public TuimdbMoviePoster? PrimaryPoster { get; set; }
}
