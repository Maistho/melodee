using System.ComponentModel;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class AlbumSearchSettings : AlbumSettings
{
    private static readonly string[] ValidSortColumns = ["Artist", "Album", "Year", "Songs", "Added", "Status"];

    [Description("Search query for album name. Use '*' to match all albums. Optional when using --since.")]
    [CommandArgument(0, "[QUERY]")]
    public string Query { get; init; } = "*";

    [Description("Output results in JSON format.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }

    [Description("Maximum number of results to return.")]
    [CommandOption("-n|--limit")]
    [DefaultValue(25)]
    public int Limit { get; init; }

    [Description("Only show albums created within the last N days.")]
    [CommandOption("--since")]
    public int? SinceDays { get; init; }

    [Description("Sort results by column: Artist, Album, Year, Songs, Added, Status.")]
    [CommandOption("--sort")]
    public string? SortBy { get; init; }

    [Description("Sort direction: asc or desc.")]
    [CommandOption("--sort-dir")]
    [DefaultValue("asc")]
    public string SortDirection { get; init; } = "asc";

    [Description("Delete all albums matching the search criteria. ⚠️ DESTRUCTIVE - cannot be undone.")]
    [CommandOption("--delete")]
    [DefaultValue(false)]
    public bool Delete { get; init; }

    [Description("Skip confirmation prompt when deleting (use with caution).")]
    [CommandOption("-y|--yes")]
    [DefaultValue(false)]
    public bool SkipConfirmation { get; init; }

    [Description("Keep album files on disk when deleting (only remove from database).")]
    [CommandOption("--keep-files")]
    [DefaultValue(false)]
    public bool KeepFiles { get; init; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Query) && !SinceDays.HasValue)
        {
            return ValidationResult.Error("Either provide a search query or use --since to filter by date.");
        }

        if (!string.IsNullOrWhiteSpace(SortBy) && !ValidSortColumns.Contains(SortBy, StringComparer.OrdinalIgnoreCase))
        {
            return ValidationResult.Error($"Invalid sort column '{SortBy}'. Valid options: {string.Join(", ", ValidSortColumns)}");
        }

        if (!SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
            !SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            return ValidationResult.Error("Sort direction must be 'asc' or 'desc'.");
        }

        return ValidationResult.Success();
    }
}
