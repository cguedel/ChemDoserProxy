using System.Net;
using System.Net.Sockets;
using ChemDoserProxy.Configuration;
using Microsoft.Extensions.Options;

namespace ChemDoserProxy.Tcp;

public class Listener : BackgroundService
{
    private readonly IOptions<ProxySettings> _proxySettings;
    private readonly ILogger<Listener> _logger;
    private readonly ClientConnectionHandler _connectionHandler;

    public Listener(
        IOptions<ProxySettings> proxySettings,
        ILogger<Listener> logger,
        ClientConnectionHandler connectionHandler)
    {
        _proxySettings = proxySettings;
        _logger = logger;
        _connectionHandler = connectionHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var endpoint = new IPEndPoint(IPAddress.Parse(_proxySettings.Value.Listen), _proxySettings.Value.Port);
        var listener = new TcpListener(endpoint);

        try
        {
            listener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Waiting for connection");

                using var client = await listener.AcceptTcpClientAsync(stoppingToken);
                _logger.LogInformation("Connected to {@Endpoint}", client.Client.RemoteEndPoint);

                await _connectionHandler.Receive(client.GetStream(), stoppingToken);

                _logger.LogInformation("Disconnected from {@Endpoint}", client.Client.RemoteEndPoint);
            }
        }
        finally
        {
            listener.Stop();
        }
    }
}
