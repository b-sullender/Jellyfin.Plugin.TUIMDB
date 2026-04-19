using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a logo of a season returned by the TUIMDB API.
    /// </summary>
    public class TuimdbSeasonLogo : TuimdbLogoBase
    {
        /// <summary>
        /// Gets or sets the ID of the season this logo belongs to.
        /// </summary>
        [JsonPropertyName("Season ID")]
        public int SeasonId { get; set; }
    }
}
