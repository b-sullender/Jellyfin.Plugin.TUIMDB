using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a logo of a movie returned by the TUIMDB API.
    /// </summary>
    public class TuimdbMovieLogo : TuimdbLogoBase
    {
        /// <summary>
        /// Gets or sets the ID of the movie this logo belongs to.
        /// </summary>
        [JsonPropertyName("Movie ID")]
        public int MovieId { get; set; }
    }
}
