using Melodee.Mql.Models;

namespace Melodee.Mql.Interfaces;

/// <summary>
/// Interface for tokenizing MQL query strings into tokens.
/// </summary>
public interface IMqlTokenizer
{
    /// <summary>
    /// Tokenizes the given query string into a sequence of tokens.
    /// </summary>
    /// <param name="query">The query string to tokenize.</param>
    /// <returns>An enumerable of tokens.</returns>
    IEnumerable<MqlToken> Tokenize(string query);
}
