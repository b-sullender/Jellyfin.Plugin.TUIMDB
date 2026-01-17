using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a movie images object returned by the TUIMDB API.
/// </summary>
public class TuimdbMovieImages
{
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

    /// <summary>
    /// Gets the list of backdrops for the movie.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Backdrops")]
    public Collection<TuimdbMovieBackdrop> Backdrops { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary backdrop for the movie.
    /// </summary>
    [JsonPropertyName("Primary Backdrop")]
    public TuimdbMovieBackdrop? PrimaryBackdrop { get; set; }

    /// <summary>
    /// Gets the list of Logos for the movie.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Logos")]
    public Collection<TuimdbMovieLogo> Logos { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary logo for the movie.
    /// </summary>
    [JsonPropertyName("Primary Logo")]
    public TuimdbMovieLogo? PrimaryLogo { get; set; }
}
