using System.Text.Json;
using ChemDoserProxy.Configuration;
using ChemDoserProxy.Dto;
using Microsoft.Extensions.Options;

namespace ChemDoserProxy.State;

public class ChemicalsManager
{
    private readonly IOptions<ChemicalsSettings> _chemicalsSettings;
    private readonly SemaphoreSlim _stateLock = new(1);

    private bool _dataInitialized;

    public ChemicalsManager(IOptions<ChemicalsSettings> chemicalsSettings)
    {
        _chemicalsSettings = chemicalsSettings;
    }

    public ChemicalsLevels State { get; private set; } = new(0, 0, 0, 0);

    public async Task RefillChemical(ChemicalType chemical)
    {
        await Initialize();
        await WriteState(state => chemical switch
        {
            ChemicalType.ChlorPure => state with { ChlorPure = _chemicalsSettings.Value.ChlorPureCapacity },
            ChemicalType.pHMinus => state with { pHMinus = _chemicalsSettings.Value.pHMinusCapacity },
            ChemicalType.pHPlus => state with { pHPlus = _chemicalsSettings.Value.pHPlusCapacity },
            ChemicalType.FlocPlusC => state with { FlocPlusC = _chemicalsSettings.Value.FlocPlusCCapacity },
            _ => state
        });
    }

    public async Task IncrementAmountConsumed(ChemicalType chemical, decimal amount)
    {
        await Initialize();
        await WriteState(state => chemical switch
        {
            ChemicalType.ChlorPure => state with { ChlorPure = state.ChlorPure - amount },
            ChemicalType.pHMinus => state with { pHMinus = state.pHMinus - amount },
            ChemicalType.pHPlus => state with { pHPlus = state.pHPlus - amount },
            ChemicalType.FlocPlusC => state with { FlocPlusC = state.FlocPlusC - amount },
            _ => state
        });
    }

    public async Task Initialize()
    {
        if (!_dataInitialized)
        {
            await _stateLock.WaitAsync();
            if (_dataInitialized)
                return;

            try
            {
                if (File.Exists(_chemicalsSettings.Value.StateFile))
                {
                    await using var readFs = File.OpenRead(_chemicalsSettings.Value.StateFile);
                    var stateFromFile = await JsonSerializer.DeserializeAsync<ChemicalsLevels>(readFs);

                    if (stateFromFile != null)
                        State = stateFromFile;
                }

                _dataInitialized = true;
            }
            finally
            {
                _stateLock.Release();
            }
        }
    }

    private async Task WriteState(Func<ChemicalsLevels, ChemicalsLevels> modifier)
    {
        await _stateLock.WaitAsync();

        try
        {
            State = modifier(State);

            await using var writeFs = File.Create(_chemicalsSettings.Value.StateFile);
            await JsonSerializer.SerializeAsync(writeFs, State);
        }
        finally
        {
            _stateLock.Release();
        }
    }
}
