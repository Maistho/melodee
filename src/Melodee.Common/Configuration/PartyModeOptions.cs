namespace Melodee.Common.Configuration;

/// <summary>
/// Configuration options for Party Mode feature.
/// </summary>
public class PartyModeOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "PartyMode";

    /// <summary>
    /// Whether Party Mode is enabled. Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Heartbeat interval in seconds for endpoints. Default is 5 seconds.
    /// </summary>
    public int HeartbeatSeconds { get; set; } = 5;

    /// <summary>
    /// Time in seconds before an endpoint is considered stale. Default is 30 seconds.
    /// </summary>
    public int EndpointStaleSeconds { get; set; } = 30;
}
