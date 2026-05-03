using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents an episode cast credit returned by the TUIMDB API.
/// </summary>
public class TuimdbEpisodeCastCredit
{
    /// <summary>
    /// Gets or sets the unique identifier of the cast credit.
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
    /// Gets or sets the user ID of the person who added the cast credit.
    /// </summary>
    [JsonPropertyName("Added By")]
    public int AddedBy { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the cast credit in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last updated timestamp of the cast credit in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Updated At")]
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original name of the character.
    /// </summary>
    [JsonPropertyName("Original Name")]
    public string? OriginalName { get; set; }

    /// <summary>
    /// Gets or sets the order number for the credit.
    /// </summary>
    [JsonPropertyName("Order")]
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the credit is for a voice actor.
    /// </summary>
    [JsonPropertyName("Voice Actor")]
    public int VoiceActor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the credit is uncredited.
    /// </summary>
    [JsonPropertyName("Uncredited")]
    public int Uncredited { get; set; }
}
