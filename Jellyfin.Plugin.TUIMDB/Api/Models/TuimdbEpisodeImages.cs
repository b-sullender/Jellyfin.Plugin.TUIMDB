using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a episode backdrops object returned by the TUIMDB API.
/// </summary>
public class TuimdbEpisodeImages
{
    /// <summary>
    /// Gets the list of backdrops for the episode.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Backdrops")]
    public Collection<TuimdbEpisodeBackdrop> Backdrops { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary backdrop for the episode.
    /// </summary>
    [JsonPropertyName("Primary Backdrop")]
    public TuimdbEpisodeBackdrop? PrimaryBackdrop { get; set; }
}
