using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;
using Melodee.Common.Models;

namespace Melodee.Common.Services;

/// <summary>
/// Interface for party session endpoint registry operations.
/// </summary>
public interface IPartySessionEndpointRegistryService
{
    /// <summary>
    /// Registers a new endpoint.
    /// </summary>
    Task<OperationResult<PartySessionEndpoint>> RegisterAsync(string name, PartySessionEndpointType type, int? ownerUserId, string? capabilitiesJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an endpoint by ID.
    /// </summary>
    Task<OperationResult<PartySessionEndpoint?>> GetAsync(Guid endpointApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last seen timestamp for an endpoint.
    /// </summary>
    Task<OperationResult<bool>> UpdateLastSeenAsync(Guid endpointApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attaches an endpoint to a session.
    /// </summary>
    Task<OperationResult<bool>> AttachToSessionAsync(Guid endpointApiKey, Guid sessionApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detaches an endpoint from its session.
    /// </summary>
    Task<OperationResult<bool>> DetachAsync(Guid endpointApiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stale endpoints.
    /// </summary>
    Task<OperationResult<IEnumerable<PartySessionEndpoint>>> GetStaleEndpointsAsync(int staleThresholdSeconds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets endpoints for a user.
    /// </summary>
    Task<OperationResult<IEnumerable<PartySessionEndpoint>>> GetEndpointsForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates endpoint capabilities.
    /// </summary>
    Task<OperationResult<PartySessionEndpoint>> UpdateCapabilitiesAsync(Guid endpointApiKey, string capabilitiesJson, CancellationToken cancellationToken = default);
}
