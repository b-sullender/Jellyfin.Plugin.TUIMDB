using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a cast member returned by the TUIMDB API.
/// </summary>
public class TuimdbCastMember
{
    /// <summary>
    /// Gets or sets the unique identifier of the cast member.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the movie for the cast member.
    /// </summary>
    [JsonPropertyName("Movie ID")]
    public int MovieId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the person for the cast member.
    /// </summary>
    [JsonPropertyName("Person ID")]
    public int PersonId { get; set; }

    /// <summary>
    /// Gets or sets the order number for the cast member.
    /// </summary>
    [JsonPropertyName("Order")]
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets a boolean indicating if the cast member is a voice actor.
    /// </summary>
    [JsonPropertyName("Voice Actor")]
    public int VoiceActor { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the person who added the cast member.
    /// </summary>
    [JsonPropertyName("Added By")]
    public int AddedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the cast member is locked.
    /// </summary>
    [JsonPropertyName("Locked")]
    public int Locked { get; set; }

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
    /// Gets or sets the person name.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    [JsonPropertyName("Character")]
    public string? Character { get; set; }

    /// <summary>
    /// Gets or sets the primary image for the person.
    /// </summary>
    [JsonPropertyName("Primary Image")]
    public TuimdbPersonImage? PrimaryImage { get; set; }
}
