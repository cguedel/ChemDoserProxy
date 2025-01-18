using System.Threading.Channels;
using ChemDoserProxy.Dto;
using ChemDoserProxy.Logic;

namespace ChemDoserProxy.Tcp;

public class ClientConnectionHandler
{
    private readonly ILogger<ClientConnectionHandler> _logger;
    private readonly DataFrameParser _parser;
    private readonly Forwarder _forwarder;
    private readonly ChannelWriter<DataFrame> _queueWriter;
    private const int BufferSize = 256;

    public ClientConnectionHandler(ILogger<ClientConnectionHandler> logger, DataFrameParser parser, DataFrameQueue queue, Forwarder forwarder)
    {
        _logger = logger;
        _parser = parser;
        _forwarder = forwarder;
        _queueWriter = queue.Writer;
    }

    public async Task Receive(Stream stream, CancellationToken ct)
    {
        var buffer = new byte[BufferSize];
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, BufferSize), ct).ConfigureAwait(false);
                if (read == 0)
                    return;

                await _forwarder.Forward(buffer.AsMemory(0, read));

                if (!_parser.TryParse(buffer, out var frame))
                {
                    _logger.LogWarning("Failed to parse buffer {Buffer} into valid data frame, ignoring data", buffer);
                    continue;
                }

                await _queueWriter.WriteAsync(frame, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while handling connection");
        }

        try
        {
            stream.Close();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}
