using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents the data for a movie or series in a collection.
/// </summary>
public class TuimdbCollectionTitleData
{
    /// <summary>
    /// Gets or sets the unique identifier of the title.
    /// </summary>
    [JsonPropertyName("UID")]
    public long Uid { get; set; }

    /// <summary>
    /// Gets or sets the original language.
    /// </summary>
    [JsonPropertyName("Original Language")]
    public string? OriginalLanguage { get; set; }

    /// <summary>
    /// Gets or sets the release year.
    /// </summary>
    [JsonPropertyName("Release Year")]
    public long? ReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the title is adult content.
    /// </summary>
    [JsonPropertyName("Adult")]
    public int? Adult { get; set; }

    /// <summary>
    /// Gets or sets the runtime in minutes. This field is specific to movies.
    /// </summary>
    [JsonPropertyName("Runtime")]
    public int? Runtime { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who added the title.
    /// </summary>
    [JsonPropertyName("Added By")]
    public long? AddedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the title is locked.
    /// </summary>
    [JsonPropertyName("Locked")]
    public int? Locked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cast is locked. This field is specific to movies.
    /// </summary>
    [JsonPropertyName("Cast Locked")]
    public int? CastLocked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the crew is locked. This field is specific to movies.
    /// </summary>
    [JsonPropertyName("Crew Locked")]
    public int? CrewLocked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if cast members are voice actors by default. This field is specific to series.
    /// </summary>
    [JsonPropertyName("Voice Actor Default")]
    public int? VoiceActorDefault { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    [JsonPropertyName("Updated At")]
    public string? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [JsonPropertyName("Title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the original title.
    /// </summary>
    [JsonPropertyName("Original Title")]
    public string? OriginalTitle { get; set; }

    /// <summary>
    /// Gets or sets the primary poster for the title.
    /// </summary>
    [JsonPropertyName("Primary Poster")]
    public TuimdbCollectionPoster? PrimaryPoster { get; set; }
}
