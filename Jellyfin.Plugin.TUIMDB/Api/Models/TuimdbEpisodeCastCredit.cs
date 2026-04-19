using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a episode cast member credit returned by the TUIMDB API.
/// </summary>
public class TuimdbEpisodeCastCredit
{
    /// <summary>
    /// Gets or sets the unique identifier of the cast member.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the series.
    /// </summary>
    [JsonPropertyName("Series ID")]
    public int SeriesUid { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the person.
    /// </summary>
    [JsonPropertyName("Person ID")]
    public int PersonUid { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the person who added the cast member.
    /// </summary>
    [JsonPropertyName("Added By")]
    public int AddedBy { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the cast member in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last updated timestamp of the cast member in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Updated At")]
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    [JsonPropertyName("Character")]
    public string? Character { get; set; }
}
