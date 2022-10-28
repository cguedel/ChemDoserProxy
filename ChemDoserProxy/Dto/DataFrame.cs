namespace ChemDoserProxy.Dto;

public class DataFrame
{
    public int Serial { get; init; }

    public DateTime Timestamp { get; init; }

    public DateTime ReceiveDate { get; init; }

    public bool Flow { get; init; }

    public int pHValue { get; init; }

    public int clFreeValue { get; init; }

    public int clFreeMilliVolts { get; init; }

    public int WaterTemperature { get; init; }

    public ChemicalType? ActivePump { get; init; }

    public int DelaySeconds { get; init; }

    public FlowRates FlowRates { get; init; } = new();

    public override string ToString()
    {
        return string.Join(Environment.NewLine, $"Serial={Serial}", $"Timestamp={Timestamp}", $"Received={ReceiveDate}",
            $"Flow={Flow}", $"pH={pHValue}",
            $"clF={clFreeValue}", $"clF(mv)={clFreeMilliVolts}", $"WaterTemp={WaterTemperature}",
            $"ActivePump={ActivePump}", $"Delay={DelaySeconds}s");
    }
}

public class FlowRates
{
    public int ChlorPure { get; init; }

    public int pHMinus { get; init; }

    public int pHPlus { get; init; }

    public int FlocPlusC { get; init; }
}
