using Melodee.Mql.Models;

namespace Melodee.Mql.Interfaces;

/// <summary>
/// Interface for parsing MQL tokens into an AST.
/// </summary>
public interface IMqlParser
{
    /// <summary>
    /// Parses the given tokens into an AST.
    /// </summary>
    /// <param name="tokens">The tokens to parse.</param>
    /// <param name="entityType">The entity type being queried (songs, albums, artists).</param>
    /// <returns>The parse result containing the AST or errors.</returns>
    MqlParseResult Parse(IEnumerable<MqlToken> tokens, string entityType);
}
