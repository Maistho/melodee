using System.Linq.Expressions;
using Melodee.Mql.Models;

namespace Melodee.Mql.Interfaces;

/// <summary>
/// Interface for compiling MQL AST into EF Core expressions.
/// </summary>
/// <typeparam name="TEntity">The entity type to compile for.</typeparam>
public interface IMqlCompiler<TEntity> where TEntity : class
{
    /// <summary>
    /// Compiles the given AST into an expression predicate.
    /// </summary>
    /// <param name="ast">The AST to compile.</param>
    /// <param name="userId">Optional user ID for user-scoped fields.</param>
    /// <returns>The compiled expression.</returns>
    Expression<Func<TEntity, bool>> Compile(MqlAstNode ast, int? userId = null);
}
