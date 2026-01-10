using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums.PartyMode;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
/// Service for managing party session endpoint registry operations.
/// </summary>
public sealed class PartySessionEndpointRegistryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory)
    : ServiceBase(logger, cacheManager, contextFactory), IPartySessionEndpointRegistryService
{
    private const string CacheKeyTemplate = "urn:party:endpoint:{0}";

    public async Task<OperationResult<PartySessionEndpoint>> RegisterAsync(
        string name,
        PartySessionEndpointType type,
        int? ownerUserId,
        string? capabilitiesJson,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var endpoint = new PartySessionEndpoint
        {
            Name = name,
            Type = type,
            OwnerUserId = ownerUserId,
            CapabilitiesJson = capabilitiesJson,
            IsShared = type == PartySessionEndpointType.WebPlayer,
            LastSeenAt = SystemClock.Instance.GetCurrentInstant()
        };

        scopedContext.PartySessionEndpoints.Add(endpoint);
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Logger.Information("[PartySessionEndpointRegistryService] Registered endpoint {EndpointName} (ID: {EndpointId}) of type {EndpointType}",
            name, endpoint.Id, type);

        return new OperationResult<PartySessionEndpoint>(endpoint);
    }

    public async Task<OperationResult<PartySessionEndpoint?>> GetAsync(
        Guid endpointApiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var cacheKey = string.Format(CacheKeyTemplate, endpointApiKey);
        if (CacheManager.TryGet(cacheKey, out PartySessionEndpoint? cached))
        {
            return new OperationResult<PartySessionEndpoint?>(cached);
        }

        var endpoint = await scopedContext.PartySessionEndpoints
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApiKey == endpointApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (endpoint != null)
        {
            CacheManager.Set(cacheKey, endpoint, TimeSpan.FromMinutes(5));
        }

        return new OperationResult<PartySessionEndpoint?>(endpoint);
    }

    public async Task<OperationResult<bool>> UpdateLastSeenAsync(
        Guid endpointApiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var endpoint = await scopedContext.PartySessionEndpoints
            .FirstOrDefaultAsync(x => x.ApiKey == endpointApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (endpoint == null)
        {
            return new OperationResult<bool>("Endpoint not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        endpoint.LastSeenAt = SystemClock.Instance.GetCurrentInstant();
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, endpointApiKey));

        return new OperationResult<bool>(true);
    }

    public async Task<OperationResult<bool>> AttachToSessionAsync(
        Guid endpointApiKey,
        Guid sessionApiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var endpoint = await scopedContext.PartySessionEndpoints
            .FirstOrDefaultAsync(x => x.ApiKey == endpointApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (endpoint == null)
        {
            return new OperationResult<bool>("Endpoint not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        var session = await scopedContext.PartySessions
            .FirstOrDefaultAsync(x => x.ApiKey == sessionApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (session == null)
        {
            return new OperationResult<bool>("Session not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        if (session.Status == PartySessionStatus.Ended)
        {
            return new OperationResult<bool>("Cannot attach to ended session.")
            {
                Type = OperationResponseType.BadRequest
            };
        }

        session.ActiveEndpointId = endpoint.Id;
        endpoint.LastSeenAt = SystemClock.Instance.GetCurrentInstant();

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, endpointApiKey));
        CacheManager.RemoveByPrefix($"urn:party:session:{sessionApiKey}");

        Logger.Information("[PartySessionEndpointRegistryService] Attached endpoint {EndpointApiKey} to session {SessionApiKey}",
            endpointApiKey, sessionApiKey);

        return new OperationResult<bool>(true);
    }

    public async Task<OperationResult<bool>> DetachAsync(
        Guid endpointApiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var endpoint = await scopedContext.PartySessionEndpoints
            .FirstOrDefaultAsync(x => x.ApiKey == endpointApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (endpoint == null)
        {
            return new OperationResult<bool>("Endpoint not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        await using var scopedContext2 = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var session = await scopedContext2.PartySessions
            .FirstOrDefaultAsync(x => x.ActiveEndpointId == endpoint.Id, cancellationToken)
            .ConfigureAwait(false);

        if (session != null)
        {
            session.ActiveEndpointId = null;
            await scopedContext2.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            CacheManager.RemoveByPrefix($"urn:party:session:{session.ApiKey}");
        }

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, endpointApiKey));

        Logger.Information("[PartySessionEndpointRegistryService] Detached endpoint {EndpointApiKey}", endpointApiKey);

        return new OperationResult<bool>(true);
    }

    public async Task<OperationResult<IEnumerable<PartySessionEndpoint>>> GetStaleEndpointsAsync(
        int staleThresholdSeconds,
        CancellationToken cancellationToken = default)
    {
        var threshold = SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(staleThresholdSeconds);

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var staleEndpoints = await scopedContext.PartySessionEndpoints
            .AsNoTracking()
            .Where(x => x.LastSeenAt.HasValue && x.LastSeenAt < threshold)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<IEnumerable<PartySessionEndpoint>>(staleEndpoints);
    }

    public async Task<OperationResult<IEnumerable<PartySessionEndpoint>>> GetEndpointsForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var endpoints = await scopedContext.PartySessionEndpoints
            .AsNoTracking()
            .Where(x => x.OwnerUserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new OperationResult<IEnumerable<PartySessionEndpoint>>(endpoints);
    }

    public async Task<OperationResult<PartySessionEndpoint>> UpdateCapabilitiesAsync(
        Guid endpointApiKey,
        string capabilitiesJson,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var endpoint = await scopedContext.PartySessionEndpoints
            .FirstOrDefaultAsync(x => x.ApiKey == endpointApiKey, cancellationToken)
            .ConfigureAwait(false);

        if (endpoint == null)
        {
            return new OperationResult<PartySessionEndpoint>("Endpoint not found.")
            {
                Type = OperationResponseType.NotFound
            };
        }

        endpoint.CapabilitiesJson = capabilitiesJson;
        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        CacheManager.RemoveByPrefix(string.Format(CacheKeyTemplate, endpointApiKey));

        Logger.Information("[PartySessionEndpointRegistryService] Updated capabilities for endpoint {EndpointApiKey}", endpointApiKey);

        return new OperationResult<PartySessionEndpoint>(endpoint);
    }
}