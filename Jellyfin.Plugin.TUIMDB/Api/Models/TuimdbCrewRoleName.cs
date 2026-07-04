using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a crew role name object returned by the TUIMDB API.
/// </summary>
public class TuimdbCrewRoleName
{
    /// <summary>
    /// Gets or sets the unique identifier of the name.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the crew role.
    /// </summary>
    [JsonPropertyName("Role ID")]
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the language code of the name.
    /// </summary>
    [JsonPropertyName("Language Code")]
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary name boolean value.
    /// </summary>
    [JsonPropertyName("Primary")]
    public int Primary { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the person who added the crew role name.
    /// </summary>
    [JsonPropertyName("Added By")]
    public int AddedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the crew role name is locked.
    /// </summary>
    [JsonPropertyName("Locked")]
    public int Locked { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the crew role name in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last updated timestamp of the crew role name in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Updated At")]
    public string UpdatedAt { get; set; } = string.Empty;
}
