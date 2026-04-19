using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a backdrop of a series returned by the TUIMDB API.
    /// </summary>
    public class TuimdbEpisodeBackdrop : TuimdbBackdropBase
    {
        /// <summary>
        /// Gets or sets the ID of the series this backdrop belongs to.
        /// </summary>
        [JsonPropertyName("Episode ID")]
        public int EpisodeId { get; set; }
    }
}
