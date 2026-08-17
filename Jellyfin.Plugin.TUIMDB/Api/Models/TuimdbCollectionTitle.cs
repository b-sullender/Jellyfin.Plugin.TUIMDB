using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a title of a collection returned by the TUIMDB API.
/// </summary>
public class TuimdbCollectionTitle
{
    /// <summary>
    /// Gets or sets the type of the title.
    /// </summary>
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title data.
    /// </summary>
    [JsonPropertyName("Data")]
    public TuimdbCollectionTitleData Data { get; set; } = new();

    /// <summary>
    /// Gets or sets the unique identifier of the link.
    /// </summary>
    [JsonPropertyName("Link ID")]
    public long LinkId { get; set; }
}
