using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a series order returned by the TUIMDB API.
/// </summary>
public class TuimdbSeriesOrder
{
    /// <summary>
    /// Gets or sets the unique identifier of the order in TUIMDB.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the order in TUIMDB.
    /// </summary>
    [JsonPropertyName("Series ID")]
    public int SeriesId { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the order in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the order.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}
