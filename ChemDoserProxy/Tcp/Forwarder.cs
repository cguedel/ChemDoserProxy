using System.Net;
using System.Net.Sockets;
using ChemDoserProxy.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChemDoserProxy.Tcp;

public class Forwarder
{
    private readonly IOptions<ProxySettings> _settings;
    private readonly ILogger<Forwarder> _logger;

    public Forwarder(IOptions<ProxySettings> settings, ILogger<Forwarder> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task Forward(Memory<byte> data)
    {
        if (string.IsNullOrEmpty(_settings.Value.ForwardHost) || _settings.Value.ForwardPort == null)
            return;

        using var client = new TcpClient();

        try
        {
            await client.ConnectAsync(_settings.Value.ForwardHost, _settings.Value.ForwardPort.Value);

            await using var stream = client.GetStream();
            await stream.WriteAsync(data);
            await stream.WriteAsync(new[] { (byte)'\n' });
        }
        catch (Exception ex)
        {
            client.Close();

            _logger.LogWarning(ex, "Exception while forwarding data");
        }
    }
}
