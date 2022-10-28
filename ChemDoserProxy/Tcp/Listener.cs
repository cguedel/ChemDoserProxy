using System.Net;
using System.Net.Sockets;
using ChemDoserProxy.Configuration;
using ChemDoserProxy.Logic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChemDoserProxy.Tcp;

public class Listener : BackgroundService
{
    private readonly IOptions<ProxySettings> _proxySettings;
    private readonly ILogger<Listener> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly DataFrameQueue _queue;
    private readonly Forwarder _forwarder;

    public Listener(IOptions<ProxySettings> proxySettings, ILogger<Listener> logger, ILoggerFactory loggerFactory,
        DataFrameQueue queue,
        Forwarder forwarder)
    {
        _proxySettings = proxySettings;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _queue = queue;
        _forwarder = forwarder;
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
                _logger.LogInformation("Connected to {}", client.Client.RemoteEndPoint);

                var connection = new ClientConnection(client, _loggerFactory.CreateLogger<ClientConnection>(), _queue,
                    _forwarder);

                await connection.Receive(stoppingToken);

                _logger.LogInformation("Disconnected from {}", client.Client.RemoteEndPoint);
            }
        }
        finally
        {
            listener.Stop();
        }
    }
}
