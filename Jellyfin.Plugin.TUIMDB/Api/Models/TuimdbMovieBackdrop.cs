using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a backdrop of a movie returned by the TUIMDB API.
    /// </summary>
    public class TuimdbMovieBackdrop
    {
        /// <summary>
        /// Gets or sets the unique identifier of the backdrop.
        /// </summary>
        [JsonPropertyName("UID")]
        public int Uid { get; set; }

        /// <summary>
        /// Gets or sets the ID of the movie this backdrop belongs to.
        /// </summary>
        [JsonPropertyName("Movie ID")]
        public int MovieId { get; set; }

        /// <summary>
        /// Gets or sets the language code of the backdrop (e.g., "en").
        /// </summary>
        [JsonPropertyName("Language Code")]
        public string LanguageCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filename of the backdrop image.
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the order of the backdrop.
        /// </summary>
        [JsonPropertyName("Order")]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the width of the backdrop in pixels.
        /// </summary>
        [JsonPropertyName("Width")]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the backdrop in pixels.
        /// </summary>
        [JsonPropertyName("Height")]
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the MD5 hash of the backdrop file.
        /// </summary>
        [JsonPropertyName("MD5")]
        public string Md5 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user ID of the person who added the backdrop.
        /// </summary>
        [JsonPropertyName("Added By")]
        public int AddedBy { get; set; }

        /// <summary>
        /// Gets or sets whether the backdrop is locked.
        /// </summary>
        [JsonPropertyName("Locked")]
        public int Locked { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp of the backdrop in the TUIMDB system.
        /// </summary>
        [JsonPropertyName("Created At")]
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last updated timestamp of the backdrop in the TUIMDB system.
        /// </summary>
        [JsonPropertyName("Updated At")]
        public string UpdatedAt { get; set; } = string.Empty;
    }
}
