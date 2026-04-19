using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a episode cast member returned by the TUIMDB API.
/// </summary>
public class TuimdbEpisodeCastMember
{
    /// <summary>
    /// Gets or sets the unique identifier of the person.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the birthday of the person.
    /// </summary>
    [JsonPropertyName("Birthday")]
    public string Birthday { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the deathday of the person.
    /// </summary>
    [JsonPropertyName("Deathday")]
    public string Deathday { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gender of the person.
    /// </summary>
    [JsonPropertyName("Gender")]
    public int Gender { get; set; }

    /// <summary>
    /// Gets or sets the original language of the person.
    /// </summary>
    [JsonPropertyName("Original Language")]
    public string OriginalLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID of the person who added the person.
    /// </summary>
    [JsonPropertyName("Added By")]
    public int AddedBy { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the person in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last updated timestamp of the person in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Updated At")]
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the person.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the place of birth of the person.
    /// </summary>
    [JsonPropertyName("Place of Birth")]
    public string PlaceOfBirth { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original name of the person.
    /// </summary>
    [JsonPropertyName("Original Name")]
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary image for the person.
    /// </summary>
    [JsonPropertyName("Primary Image")]
    public TuimdbPersonImage? PrimaryImage { get; set; }

    /// <summary>
    /// Gets the list of credits for the person.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Credits")]
    public Collection<TuimdbEpisodeCastCredit> Credits { get; private set; } = new();

    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    [JsonPropertyName("Characters")]
    public string? Characters { get; set; }

    /// <summary>
    /// Gets or sets the order number for the cast member.
    /// </summary>
    [JsonPropertyName("Order")]
    public int Order { get; set; }
}
