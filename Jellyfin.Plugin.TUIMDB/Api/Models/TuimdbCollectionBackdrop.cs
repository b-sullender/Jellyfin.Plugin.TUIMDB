using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a backdrop of a collection returned by the TUIMDB API.
    /// </summary>
    public class TuimdbCollectionBackdrop : TuimdbBackdropBase
    {
        /// <summary>
        /// Gets or sets the ID of the collection this backdrop belongs to.
        /// </summary>
        [JsonPropertyName("Collection ID")]
        public int CollectionId { get; set; }
    }
}
