using System.Linq.Expressions;
using Melodee.Common.Data.Models;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql;

/// <summary>
/// Cached MQL compiler that wraps IMqlCompiler instances with expression caching.
/// Supports songs, albums, and artists with entity-specific compilation.
/// </summary>
public sealed class MqlCachedCompiler
{
    private readonly IMqlExpressionCache _cache;
    private readonly IMqlFieldInfoProvider _fieldInfoProvider;
    private readonly MqlOptions _options;
    private readonly IMqlCompiler<Song> _songCompiler;
    private readonly IMqlCompiler<Album> _albumCompiler;
    private readonly IMqlCompiler<Artist> _artistCompiler;

    public MqlCachedCompiler(
        IMqlExpressionCache? cache = null,
        IMqlFieldInfoProvider? fieldInfoProvider = null,
        MqlOptions? options = null,
        IMqlCompiler<Song>? songCompiler = null,
        IMqlCompiler<Album>? albumCompiler = null,
        IMqlCompiler<Artist>? artistCompiler = null)
    {
        _cache = cache ?? new MqlExpressionCache();
        _fieldInfoProvider = fieldInfoProvider ?? new MqlFieldInfoProvider();
        _options = options ?? new MqlOptions();
        _songCompiler = songCompiler ?? new MqlSongCompiler(_fieldInfoProvider, _options);
        _albumCompiler = albumCompiler ?? new MqlAlbumCompiler(_fieldInfoProvider, _options);
        _artistCompiler = artistCompiler ?? new MqlArtistCompiler(_fieldInfoProvider, _options);
    }

    public Expression<Func<Song, bool>> CompileSong(MqlAstNode ast, int? userId = null)
    {
        var cacheKey = GenerateCacheKey("songs", ast, userId);
        var factory = () => _songCompiler.Compile(ast, userId);
        return _cache.GetOrCreate(cacheKey, factory);
    }

    public Expression<Func<Album, bool>> CompileAlbum(MqlAstNode ast, int? userId = null)
    {
        var cacheKey = GenerateCacheKey("albums", ast, userId);
        var factory = () => _albumCompiler.Compile(ast, userId);
        return _cache.GetOrCreate(cacheKey, factory);
    }

    public Expression<Func<Artist, bool>> CompileArtist(MqlAstNode ast, int? userId = null)
    {
        var cacheKey = GenerateCacheKey("artists", ast, userId);
        var factory = () => _artistCompiler.Compile(ast, userId);
        return _cache.GetOrCreate(cacheKey, factory);
    }

    private static string GenerateCacheKey(string entityType, MqlAstNode ast, int? userId)
    {
        var normalizedQuery = NormalizeAst(ast);
        var userIdPart = userId.HasValue ? $":user{userId.Value}" : ":anon";
        return $"{entityType}:{normalizedQuery}{userIdPart}";
    }

    private static string NormalizeAst(MqlAstNode node)
    {
        return node switch
        {
            FreeTextNode freeText => $"ft:{freeText.Text.ToLowerInvariant()}",
            FieldExpressionNode field => $"{field.Field}:{field.Operator}:{field.Value}".ToLowerInvariant(),
            RegexExpressionNode regex => $"{regex.Field}:/{regex.Pattern}/{regex.Flags}",
            BinaryExpressionNode binary => $"({NormalizeAst(binary.Left)}{binary.Operator.ToUpperInvariant()}{NormalizeAst(binary.Right)})",
            UnaryExpressionNode unary => $"NOT({NormalizeAst(unary.Operand)})",
            GroupNode group => NormalizeAst(group.Inner),
            RangeNode range => $"{range.Field}:{range.Min}-{range.Max}",
            _ => "unknown"
        };
    }

    public IMqlExpressionCache Cache => _cache;
}
