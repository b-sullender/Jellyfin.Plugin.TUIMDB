using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a poster of a season returned by the TUIMDB API.
    /// </summary>
    public class TuimdbSeasonPoster : TuimdbPosterBase
    {
        /// <summary>
        /// Gets or sets the ID of the season this poster belongs to.
        /// </summary>
        [JsonPropertyName("Season ID")]
        public int SeasonId { get; set; }
    }
}
