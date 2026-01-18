using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a logo of a series returned by the TUIMDB API.
    /// </summary>
    public class TuimdbSeriesLogo : TuimdbLogoBase
    {
        /// <summary>
        /// Gets or sets the ID of the series this logo belongs to.
        /// </summary>
        [JsonPropertyName("Series ID")]
        public int SeriesId { get; set; }
    }
}
