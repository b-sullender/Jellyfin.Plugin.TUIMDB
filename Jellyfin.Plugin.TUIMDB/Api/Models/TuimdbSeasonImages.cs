using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a season images object returned by the TUIMDB API.
/// </summary>
public class TuimdbSeasonImages
{
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
