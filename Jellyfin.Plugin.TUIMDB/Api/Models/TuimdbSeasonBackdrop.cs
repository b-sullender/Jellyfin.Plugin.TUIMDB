using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a backdrop of a season returned by the TUIMDB API.
    /// </summary>
    public class TuimdbSeasonBackdrop : TuimdbBackdropBase
    {
        /// <summary>
        /// Gets or sets the ID of the season this backdrop belongs to.
        /// </summary>
        [JsonPropertyName("Season ID")]
        public int SeasonId { get; set; }
    }
}
