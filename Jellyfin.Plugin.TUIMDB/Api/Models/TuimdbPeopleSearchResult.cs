using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a people search result returned by the TUIMDB API.
/// </summary>
public class TuimdbPeopleSearchResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the person.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the person's birth date.
    /// </summary>
    [JsonPropertyName("Birthday")]
    public string? Birthday { get; set; }

    /// <summary>
    /// Gets or sets the person's death date, if applicable.
    /// </summary>
    [JsonPropertyName("Deathday")]
    public string? Deathday { get; set; }

    /// <summary>
    /// Gets or sets the gender of the person (typically numeric enum value from source system).
    /// </summary>
    [JsonPropertyName("Gender")]
    public int? Gender { get; set; }

    /// <summary>
    /// Gets or sets the original language code associated with the person entry.
    /// </summary>
    [JsonPropertyName("Original Language")]
    public string OriginalLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID or system ID that added this entry.
    /// </summary>
    [JsonPropertyName("Added By")]
    public int? AddedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the birthday field is locked from edits.
    /// </summary>
    [JsonPropertyName("Birthday Locked")]
    public int? BirthdayLocked { get; set; }

    /// <summary>
    /// Gets or sets whether the deathday field is locked from edits.
    /// </summary>
    [JsonPropertyName("Deathday Locked")]
    public int? DeathdayLocked { get; set; }

    /// <summary>
    /// Gets or sets whether the gender field is locked from edits.
    /// </summary>
    [JsonPropertyName("Gender Locked")]
    public int? GenderLocked { get; set; }

    /// <summary>
    /// Gets or sets whether the language field is locked from edits.
    /// </summary>
    [JsonPropertyName("Language Locked")]
    public int? LanguageLocked { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the record was created.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the record was last updated.
    /// </summary>
    [JsonPropertyName("Updated At")]
    public string? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the display name of the person.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original name of the person, if different from display name.
    /// </summary>
    [JsonPropertyName("Original Name")]
    public string? OriginalName { get; set; }

    /// <summary>
    /// Gets the alternate or known-as names for the person.
    /// </summary>
    [JsonPropertyName("AKA Names")]
    public Collection<string> AkaNames { get; } = new();

    /// <summary>
    /// Gets or sets the primary image of the person.
    /// </summary>
    [JsonPropertyName("Primary Image")]
    public TuimdbPersonImage? PrimaryImage { get; set; }

    /// <summary>
    /// Gets or sets the match score for the search result.
    /// </summary>
    [JsonPropertyName("Match Score")]
    public int MatchScore { get; set; }
}
