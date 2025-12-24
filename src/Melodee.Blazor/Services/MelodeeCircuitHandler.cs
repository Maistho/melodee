using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Melodee.Blazor.Services;

public sealed class MelodeeCircuitHandler : CircuitHandler
{
    private readonly ILogger<MelodeeCircuitHandler> _logger;

    public MelodeeCircuitHandler(ILogger<MelodeeCircuitHandler> logger)
    {
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit opened: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit closed: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Connection established for circuit: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Connection lost for circuit: {CircuitId}. Client may be attempting to reconnect.", circuit.Id);
        return Task.CompletedTask;
    }
}
