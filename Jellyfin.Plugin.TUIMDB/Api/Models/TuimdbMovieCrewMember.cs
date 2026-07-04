using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a movie crew member returned by the TUIMDB API.
/// </summary>
public class TuimdbMovieCrewMember
{
    /// <summary>
    /// Gets or sets the unique identifier of the crew member.
    /// </summary>
    [JsonPropertyName("UID")]
    public int Uid { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the movie for the crew member.
    /// </summary>
    [JsonPropertyName("Movie ID")]
    public int MovieId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the person for the crew member.
    /// </summary>
    [JsonPropertyName("Person ID")]
    public int PersonId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the credited name for the crew member.
    /// </summary>
    [JsonPropertyName("Name ID")]
    public int? NameId { get; set; }

    /// <summary>
    /// Gets or sets the order number for the crew member.
    /// </summary>
    [JsonPropertyName("Order")]
    public int? Order { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the crew member is uncredited.
    /// </summary>
    [JsonPropertyName("Uncredited")]
    public int Uncredited { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the crew member role.
    /// </summary>
    [JsonPropertyName("Role ID")]
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the credited role name for the crew member.
    /// </summary>
    [JsonPropertyName("Role Name ID")]
    public int? RoleNameId { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the person who added the crew member.
    /// </summary>
    [JsonPropertyName("Added By")]
    public int AddedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the crew member is locked.
    /// </summary>
    [JsonPropertyName("Locked")]
    public int Locked { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the crew member in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Created At")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last updated timestamp of the crew member in the TUIMDB system.
    /// </summary>
    [JsonPropertyName("Updated At")]
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full person object for the crew member.
    /// </summary>
    [JsonPropertyName("Person")]
    public TuimdbPerson? Person { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the job department for the crew member.
    /// </summary>
    [JsonPropertyName("Department ID")]
    public int DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the job department name for the crew member.
    /// </summary>
    [JsonPropertyName("Department")]
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role name for the crew member.
    /// </summary>
    [JsonPropertyName("Role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role priority used for displaying crew members on the title page.
    /// </summary>
    [JsonPropertyName("Role Priority")]
    public int? RolePriority { get; set; }

    /// <summary>
    /// Gets or sets the credited name of the crew member.
    /// </summary>
    [JsonPropertyName("Credited As")]
    public TuimdbPersonName? CreditedAs { get; set; }

    /// <summary>
    /// Gets or sets the credited role name of the crew member.
    /// </summary>
    [JsonPropertyName("Credited Role Name")]
    public TuimdbCrewRoleName? CreditedRoleName { get; set; }
}
