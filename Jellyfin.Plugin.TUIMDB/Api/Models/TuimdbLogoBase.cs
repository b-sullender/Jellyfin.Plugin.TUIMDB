using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a logo returned by the TUIMDB API.
    /// </summary>
    public abstract class TuimdbLogoBase
    {
        /// <summary>
        /// Gets or sets the unique identifier of the logo.
        /// </summary>
        [JsonPropertyName("UID")]
        public int Uid { get; set; }

        /// <summary>
        /// Gets or sets the language code of the logo (e.g., "en").
        /// </summary>
        [JsonPropertyName("Language Code")]
        public string LanguageCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filename of the logo image.
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the order of the logo.
        /// </summary>
        [JsonPropertyName("Order")]
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets the width of the logo in pixels.
        /// </summary>
        [JsonPropertyName("Width")]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the logo in pixels.
        /// </summary>
        [JsonPropertyName("Height")]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the MD5 hash of the logo file.
        /// </summary>
        [JsonPropertyName("MD5")]
        public string Md5 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user ID of the person who added the logo.
        /// </summary>
        [JsonPropertyName("Added By")]
        public int AddedBy { get; set; }

        /// <summary>
        /// Gets or sets whether the logo is locked.
        /// </summary>
        [JsonPropertyName("Locked")]
        public int Locked { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp of the logo in the TUIMDB system.
        /// </summary>
        [JsonPropertyName("Created At")]
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last updated timestamp of the logo in the TUIMDB system.
        /// </summary>
        [JsonPropertyName("Updated At")]
        public string? UpdatedAt { get; set; } = string.Empty;
    }
}
