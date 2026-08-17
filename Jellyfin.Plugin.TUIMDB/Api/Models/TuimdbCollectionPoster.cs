using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TUIMDB.Api.Models
{
    /// <summary>
    /// Represents a poster of a collection returned by the TUIMDB API.
    /// </summary>
    public class TuimdbCollectionPoster : TuimdbPosterBase
    {
        /// <summary>
        /// Gets or sets the ID of the collection this poster belongs to.
        /// </summary>
        [JsonPropertyName("Collection ID")]
        public int CollectionId { get; set; }
    }
}
