using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a backdrop of a series returned by the TUIMDB API.
    /// </summary>
    public class TuimdbSeriesBackdrop : TuimdbBackdropBase
    {
        /// <summary>
        /// Gets or sets the ID of the series this backdrop belongs to.
        /// </summary>
        [JsonPropertyName("Series ID")]
        public int SeriesId { get; set; }
    }
}
