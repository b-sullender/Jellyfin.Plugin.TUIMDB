using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a logo of a collection returned by the TUIMDB API.
    /// </summary>
    public class TuimdbCollectionLogo : TuimdbLogoBase
    {
        /// <summary>
        /// Gets or sets the ID of the collection this logo belongs to.
        /// </summary>
        [JsonPropertyName("Collection ID")]
        public int CollectionId { get; set; }
    }
}
