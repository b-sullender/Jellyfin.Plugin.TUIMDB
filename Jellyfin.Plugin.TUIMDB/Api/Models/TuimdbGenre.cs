using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a genre returned by the TUIMDB API.
/// </summary>
public class TuimdbGenre
{
    /// <summary>
    /// Gets or sets the unique identifier of the genre in TUIMDB.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the display name of the genre.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}
