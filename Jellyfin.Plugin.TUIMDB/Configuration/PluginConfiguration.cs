using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TUIMDB.Configuration;

/// <summary>
/// Configuration for the TUIMDB plugin.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        ApiKey = string.Empty;
    }

    /// <summary>
    /// Gets or sets the API base URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://tuimdb.com/api";

    /// <summary>
    /// Gets or sets the URL where movie posters are hosted.
    /// </summary>
    public string MoviePostersUrl { get; set; } = "https://tuimdb.com/movies/posters";

    /// <summary>
    /// Gets or sets the URL where movie backdrops are hosted.
    /// </summary>
    public string MovieBackdropsUrl { get; set; } = "https://tuimdb.com/movies/backdrops";

    /// <summary>
    /// Gets or sets the URL where movie logos are hosted.
    /// </summary>
    public string MovieLogosUrl { get; set; } = "https://tuimdb.com/movies/logos";

    /// <summary>
    /// Gets or sets the URL where people images are hosted.
    /// </summary>
    public string PeopleImagesUrl { get; set; } = "https://tuimdb.com/people/images";

    /// <summary>
    /// Gets or sets the URL where series posters are hosted.
    /// </summary>
    public string SeriesPostersUrl { get; set; } = "https://tuimdb.com/series/posters";

    /// <summary>
    /// Gets or sets the URL where series backdrops are hosted.
    /// </summary>
    public string SeriesBackdropsUrl { get; set; } = "https://tuimdb.com/series/backdrops";

    /// <summary>
    /// Gets or sets the URL where series logos are hosted.
    /// </summary>
    public string SeriesLogosUrl { get; set; } = "https://tuimdb.com/series/logos";

    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
