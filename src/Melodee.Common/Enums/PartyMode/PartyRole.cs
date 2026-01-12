namespace Melodee.Common.Enums.PartyMode;

/// <summary>
/// Represents the role of a participant in a party session.
/// </summary>
public enum PartyRole
{
    /// <summary>
    /// The owner of the party session who has full control.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// A DJ who can control playback and manage the queue.
    /// </summary>
    DJ = 1,

    /// <summary>
    /// A listener who can only view the session and request songs (if allowed).
    /// </summary>
    Listener = 2
}
