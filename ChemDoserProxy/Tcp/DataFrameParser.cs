using System.Diagnostics.CodeAnalysis;
using ChemDoserProxy.Dto;

namespace ChemDoserProxy.Tcp;

public class DataFrameParser
{
    public bool TryParse(byte[] buffer, [NotNullWhen(returnValue: true)] out DataFrame? frame)
    {
        frame = null;
        if (buffer.Length < 102)
            return false;

        frame = new DataFrame
        {
            Serial = BitConverter.ToInt32(buffer.AsSpan().GetBytes(0, 4)),
            ReceiveDate = DateTime.UtcNow,
            Timestamp = SafeGetTimestamp(buffer),
            State = (DeviceState)buffer[13],
            pHValue = BitConverter.ToInt16(buffer.AsSpan().GetBytes(14, 2)),
            clFreeValue = BitConverter.ToInt16(buffer.AsSpan().GetBytes(16, 2)),
            clFreeMilliVolts = BitConverter.ToInt16(buffer.AsSpan().GetBytes(20, 2)),
            WaterTemperature = BitConverter.ToInt16(buffer.AsSpan().GetBytes(25, 2)),
            ActivePump = GetPumpTypeFromByte(buffer[29]),
            DelaySeconds = BitConverter.ToInt16(buffer.AsSpan().GetBytes(30, 2)),
            FlowRates = new FlowRates
            {
                ChlorPure = buffer[95],
                pHMinus = buffer[97],
                pHPlus = buffer[99],
                FlocPlusC = buffer[101]
            }
        };

        return true;
    }

    private static DateTime? SafeGetTimestamp(byte[] buffer)
    {
        var year = 2000 + buffer[6];
        var month = buffer[7];
        var day = buffer[8];
        var hour = buffer[9];
        var minute = buffer[10];
        var second = buffer[11];

        if (month is < 1 or > 12)
            return null;

        if (day < 1 || day > DateTime.DaysInMonth(year, month))
            return null;

        if (hour > 24)
            return null;

        if (minute > 60)
            return null;

        if (second > 60)
            return null;

        return new DateTime(year, month, day, hour, minute, second);
    }

    private static ChemicalType? GetPumpTypeFromByte(byte value)
    {
        var pumpType = (ChemicalType)value;
        return Enum.IsDefined(pumpType) ? pumpType : null;
    }
}
