using System.Net.Sockets;
using System.Threading.Channels;
using ChemDoserProxy.Dto;
using ChemDoserProxy.Logic;
using Microsoft.Extensions.Logging;

namespace ChemDoserProxy.Tcp;

public class ClientConnection
{
    private readonly TcpClient _client;
    private readonly ILogger<ClientConnection> _logger;
    private readonly Forwarder _forwarder;
    private readonly NetworkStream _stream;
    private readonly ChannelWriter<DataFrame> _queueWriter;
    private const int BufferSize = 256;

    public ClientConnection(TcpClient client, ILogger<ClientConnection> logger, DataFrameQueue queue, Forwarder forwarder)
    {
        _client = client;
        _logger = logger;
        _forwarder = forwarder;
        _stream = client.GetStream();
        _queueWriter = queue.Writer;
    }

    public async Task Receive(CancellationToken ct)
    {
        var buffer = new byte[BufferSize];
        try
        {
            while (true)
            {
                var read = await _stream.ReadAsync(buffer.AsMemory(0, BufferSize), ct).ConfigureAwait(false);
                if (read == 0)
                    return;

                await _forwarder.Forward(buffer.AsMemory(0, read));

                var frame = new DataFrame
                {
                    Serial = BitConverter.ToInt32(buffer.AsSpan().GetBytes(0, 4)),
                    ReceiveDate = DateTime.UtcNow,
                    Timestamp = new DateTime(2000 + buffer[6], buffer[7], buffer[8],
                        buffer[9], buffer[10], buffer[11]),
                    Flow = buffer[13] == 0,
                    pHValue = BitConverter.ToInt16(buffer.AsSpan().GetBytes(14, 2)),
                    clFreeValue = BitConverter.ToInt16(buffer.AsSpan().GetBytes(16, 2)),
                    clFreeMilliVolts = BitConverter.ToInt16(buffer.AsSpan().GetBytes(20, 2)),
                    WaterTemperature = BitConverter.ToInt16(buffer.AsSpan().GetBytes(25, 2)),
                    ActivePump = GetPumpTypeFromByte(buffer[29]),
                    DelaySeconds = BitConverter.ToInt16(buffer.AsSpan().GetBytes(30, 2)),
                    FlowRates = new FlowRates
                    {
                        ChlorPure = buffer[96],
                        pHMinus = buffer[98],
                        pHPlus = buffer[100],
                        FlocPlusC = buffer[102]
                    }
                };

                await _queueWriter.WriteAsync(frame, ct);
            }
        }
        catch (OperationCanceledException)
        {
            if (_client.Connected)
                _stream.Close();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unhandled exception when handling connection");
        }
    }

    private static ChemicalType? GetPumpTypeFromByte(byte value)
    {
        var pumpType = (ChemicalType)value;
        return Enum.IsDefined(pumpType) ? pumpType : null;
    }
}
