using ChemDoserProxy.Dto;

namespace ChemDoserProxy.State;

public class StateManager
{
    private readonly ILogger<StateManager> _logger;

    public StateManager(ILogger<StateManager> logger)
    {
        _logger = logger;
    }

    public DoserState State { get; private set; } = new(DeviceState.NoError, 0, 0, 0, 0);

    public void Set(DeviceState state, decimal clFree, decimal clFreeMv, decimal pH, decimal waterTemp)
    {
        State = new DoserState(state, clFree, clFreeMv, pH, waterTemp);
        _logger.LogDebug("State updated to {}", State);
    }
}
