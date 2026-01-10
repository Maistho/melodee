using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;
using Melodee.Common.Models;

namespace Melodee.Common.Services;

/// <summary>
/// Interface for party session operations.
/// </summary>
public interface IPartySessionService
{
    /// <summary>
    /// Creates a new party session.
    /// </summary>
    Task<OperationResult<PartySession>> CreateAsync(string name, int ownerUserId, string? joinCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a party session by ID.
    /// </summary>
    Task<OperationResult<PartySession?>> GetAsync(Guid sessionApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Joins a party session.
    /// </summary>
    Task<OperationResult<PartySessionParticipant>> JoinAsync(Guid sessionApiKey, int userId, string? joinCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Leaves a party session.
    /// </summary>
    Task<OperationResult<bool>> LeaveAsync(Guid sessionApiKey, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends a party session.
    /// </summary>
    Task<OperationResult<bool>> EndAsync(Guid sessionApiKey, int requestingUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets participants for a session.
    /// </summary>
    Task<OperationResult<IEnumerable<PartySessionParticipant>>> GetParticipantsAsync(Guid sessionApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is a member of a session.
    /// </summary>
    Task<OperationResult<PartySessionParticipant?>> GetParticipantAsync(Guid sessionApiKey, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user's role in a session.
    /// </summary>
    Task<OperationResult<PartyRole?>> GetUserRoleAsync(Guid sessionApiKey, int userId, CancellationToken cancellationToken = default);
}
