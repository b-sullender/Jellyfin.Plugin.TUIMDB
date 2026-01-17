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
    /// Gets or sets the API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
