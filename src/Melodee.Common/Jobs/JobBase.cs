using Melodee.Common.Configuration;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Base class for all Melodee background jobs.
///     Provides common dependencies (logging, configuration) and implements the Quartz IJob interface.
/// </summary>
/// <remarks>
///     All jobs should inherit from this class to ensure consistent logging and configuration access.
///     Jobs are scheduled via Quartz.NET and configured with cron expressions in the Settings table.
/// </remarks>
public abstract class JobBase(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory) : IJob
{
    protected ILogger Logger { get; } = logger;

    protected IMelodeeConfigurationFactory ConfigurationFactory { get; } = configurationFactory;

    public abstract Task Execute(IJobExecutionContext context);
}
