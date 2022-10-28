using System.Threading.Channels;
using ChemDoserProxy.Dto;
using ChemDoserProxy.State;
using ChemDoserProxy.Tcp;

namespace ChemDoserProxy.Logic;

public class DataFrameProcessor : BackgroundService
{
    private readonly ILogger<DataFrameProcessor> _logger;
    private readonly ChemicalsManager _chemicalsManager;
    private readonly StateManager _stateManager;
    private readonly ChannelReader<DataFrame> _queueReader;

    private ChemicalType? _activePump;
    private DateTime? _pumpLastActive;

    public DataFrameProcessor(ILogger<DataFrameProcessor> logger, DataFrameQueue queue,
        ChemicalsManager chemicalsManager,
        StateManager stateManager)
    {
        _logger = logger;
        _chemicalsManager = chemicalsManager;
        _stateManager = stateManager;
        _queueReader = queue.Reader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var frame in _queueReader.ReadAllAsync(stoppingToken))
        {
            _logger.LogDebug("Processing data frame {{{}}}", frame);

            // pump will be set to active first and in another data frame to inactive again
            // no two pumps will ever run at the same time
            // time between pump activation/deactivation is used to calculate amount of chemical used
            // flow rate is contained in data frame for each pump
            if (frame.ActivePump != null && _pumpLastActive == null)
            {
                _pumpLastActive = frame.ReceiveDate;
                _activePump = frame.ActivePump.Value;

                _logger.LogInformation("Pump {} activated at {}", frame.ActivePump, frame.ReceiveDate);
            }
            else if (frame.ActivePump == null && _pumpLastActive != null && _activePump != null)
            {
                var activeDuration = frame.ReceiveDate - _pumpLastActive.Value;
                var flowRate = GetFlowRate(_activePump.Value, frame);
                var consumedAmount = flowRate / 60m * (decimal)activeDuration.TotalSeconds;

                _logger.LogInformation("Pump {} no longer active, ran for {}ms, consumed {}ml", _activePump,
                    activeDuration.TotalMilliseconds, consumedAmount);

                await _chemicalsManager.IncrementAmountConsumed(_activePump.Value, consumedAmount);

                _activePump = null;
                _pumpLastActive = null;
            }

            _stateManager.Set(frame.clFreeValue / 100m, frame.clFreeMilliVolts, frame.pHValue / 100m,
                frame.WaterTemperature / 10m);
        }
    }

    private static int GetFlowRate(ChemicalType type, DataFrame frame)
    {
        return type switch
        {
            ChemicalType.ChlorPure => frame.FlowRates.ChlorPure,
            ChemicalType.pHMinus => frame.FlowRates.pHMinus,
            ChemicalType.pHPlus => frame.FlowRates.pHPlus,
            ChemicalType.FlocPlusC => frame.FlowRates.FlocPlusC,
            _ => 0
        };
    }
}
