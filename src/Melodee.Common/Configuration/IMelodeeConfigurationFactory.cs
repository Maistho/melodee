namespace Melodee.Common.Configuration;

public interface IMelodeeConfigurationFactory
{
    public void Reset();

    public Task<IMelodeeConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when configuration has been reset and components should reload their configuration.
    /// </summary>
    event EventHandler? ConfigurationChanged;
}
