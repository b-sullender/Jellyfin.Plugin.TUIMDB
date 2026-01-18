using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a poster of a movie returned by the TUIMDB API.
    /// </summary>
    public class TuimdbMoviePoster : TuimdbPosterBase
    {
        /// <summary>
        /// Gets or sets the ID of the movie this poster belongs to.
        /// </summary>
        [JsonPropertyName("Movie ID")]
        public int MovieId { get; set; }
    }
}
