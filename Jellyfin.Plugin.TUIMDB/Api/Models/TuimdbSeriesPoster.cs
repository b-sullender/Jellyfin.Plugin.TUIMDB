using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a poster of a series returned by the TUIMDB API.
    /// </summary>
    public class TuimdbSeriesPoster : TuimdbPosterBase
    {
        /// <summary>
        /// Gets or sets the ID of the series this poster belongs to.
        /// </summary>
        [JsonPropertyName("Series ID")]
        public int SeriesId { get; set; }
    }
}
