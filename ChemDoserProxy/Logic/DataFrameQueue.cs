using System.Threading.Channels;
using ChemDoserProxy.Dto;

namespace ChemDoserProxy.Logic;

public class DataFrameQueue
{
    private readonly Channel<DataFrame> _channel;

    public DataFrameQueue()
    {
        _channel = Channel.CreateUnbounded<DataFrame>(new UnboundedChannelOptions
        {
            SingleWriter = true
        });
    }

    public ChannelWriter<DataFrame> Writer => _channel.Writer;

    public ChannelReader<DataFrame> Reader => _channel.Reader;
}
