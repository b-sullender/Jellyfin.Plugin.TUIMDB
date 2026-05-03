using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models;

/// <summary>
/// Represents a person images object returned by the TUIMDB API.
/// </summary>
public class TuimdbPersonImages
{
    /// <summary>
    /// Gets the list of images for the person.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Images")]
    public Collection<TuimdbPersonImage> Images { get; private set; } = new();

    /// <summary>
    /// Gets or sets the primary image for the person.
    /// </summary>
    [JsonPropertyName("Primary Image")]
    public TuimdbPersonImage? PrimaryImage { get; set; }
}
