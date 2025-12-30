using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class JobRunSettings : JobSettings
{
    [Description("Name of the job to run (e.g., ArtistHousekeepingJob, ChartUpdateJob).")]
    [CommandOption("-j|--job")]
    public required string JobName { get; init; }
}
