namespace Melodee.Common.Enums.PartyMode;

/// <summary>
/// Represents the type of endpoint that can be attached to a party session.
/// </summary>
public enum PartySessionEndpointType
{
    /// <summary>
    /// A web-based player endpoint.
    /// </summary>
    WebPlayer = 0,

    /// <summary>
    /// An MPV backend endpoint for server-side playback.
    /// </summary>
    MpvBackend = 1
}
