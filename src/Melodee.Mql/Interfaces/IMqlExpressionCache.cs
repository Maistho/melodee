using System.Linq.Expressions;
using Melodee.Mql.Models;

namespace Melodee.Mql.Interfaces;

/// <summary>
/// Interface for caching compiled MQL expressions.
/// </summary>
public interface IMqlExpressionCache
{
    /// <summary>
    /// Gets or creates a cached expression for the given query.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="cacheKey">The cache key (entity type, normalized query, userId).</param>
    /// <param name="factory">Factory function to create the expression if not cached.</param>
    /// <param name="ttl">Optional time-to-live for this entry.</param>
    /// <returns>The cached or newly created expression.</returns>
    Expression<Func<TEntity, bool>> GetOrCreate<TEntity>(
        string cacheKey,
        Func<Expression<Func<TEntity, bool>>> factory,
        TimeSpan? ttl = null) where TEntity : class;

    /// <summary>
    /// Clears all cached entries for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    void Clear<TEntity>() where TEntity : class;

    /// <summary>
    /// Clears all cached entries.
    /// </summary>
    void ClearAll();

    /// <summary>
    /// Gets current cache statistics.
    /// </summary>
    /// <returns>Cache statistics.</returns>
    MqlCacheStatistics GetStatistics();

    /// <summary>
    /// Invalidates cache entries for a specific entity type.
    /// </summary>
    /// <param name="entityType">The entity type name.</param>
    void InvalidateByEntityType(string entityType);
}
