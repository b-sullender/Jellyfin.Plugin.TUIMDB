using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a backdrop of a movie returned by the TUIMDB API.
    /// </summary>
    public class TuimdbMovieBackdrop : TuimdbBackdropBase
    {
        /// <summary>
        /// Gets or sets the ID of the movie this backdrop belongs to.
        /// </summary>
        [JsonPropertyName("Movie ID")]
        public int MovieId { get; set; }
    }
}
