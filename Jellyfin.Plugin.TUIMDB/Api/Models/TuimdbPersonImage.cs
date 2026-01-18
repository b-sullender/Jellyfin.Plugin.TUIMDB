using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a person image returned by the TUIMDB API.
    /// </summary>
    public class TuimdbPersonImage
    {
        /// <summary>
        /// Gets or sets the unique identifier of the image.
        /// </summary>
        [JsonPropertyName("UID")]
        public int Uid { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person this image belongs to.
        /// </summary>
        [JsonPropertyName("Person ID")]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the filename of the image.
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the order of the image.
        /// </summary>
        [JsonPropertyName("Order")]
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets the width of the image in pixels.
        /// </summary>
        [JsonPropertyName("Width")]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the image in pixels.
        /// </summary>
        [JsonPropertyName("Height")]
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the MD5 hash of the image file.
        /// </summary>
        [JsonPropertyName("MD5")]
        public string Md5 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user ID of the person who added the image.
        /// </summary>
        [JsonPropertyName("Added By")]
        public int AddedBy { get; set; }

        /// <summary>
        /// Gets or sets whether the image is locked.
        /// </summary>
        [JsonPropertyName("Locked")]
        public int Locked { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp of the image in the TUIMDB system.
        /// </summary>
        [JsonPropertyName("Created At")]
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last updated timestamp of the image in the TUIMDB system.
        /// </summary>
        [JsonPropertyName("Updated At")]
        public string UpdatedAt { get; set; } = string.Empty;
    }
}
