using System.Net.Sockets;

namespace Payroc.App.LoadBalancer;

/// <summary>
/// Handles bidirectional TCP proxying between client and backend
/// </summary>
public class ConnectionHandler : IConnectionHandler
{
    private readonly IBackendPool _pool;

    public ConnectionHandler(IBackendPool pool)
    {
        _pool = pool;
    }

    public async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var clientEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";

        using (client)
        {
            try
            {
                var backend = _pool.GetNextHealthyBackend();
                if (backend == null)
                {
                    Logger.Warn($"Client {clientEndpoint}: No healthy backends available");
                    return;
                }
                Logger.Info($"Selected Beckend: {backend.Address}");

                Logger.Info($"Client {clientEndpoint} → Backend {backend.Address}");

                // Use using for backend connection too
                using var backendClient = new TcpClient();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(Config.ConnectionTimeoutSeconds));

                await backendClient.ConnectAsync(backend.Host, backend.Port, cts.Token);

                ConfigureSocket(client);
                ConfigureSocket(backendClient);

                // Bidirectional proxy
                await ProxyDataAsync(client, backendClient, cancellationToken);
                Logger.Info($"Connection closed: {clientEndpoint} ↔ {backend.Address}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Client {clientEndpoint}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Proxies data bidirectionally between client and backend
    /// </summary>
    private static async Task ProxyDataAsync(
        TcpClient client,
        TcpClient backend,
        CancellationToken cancellationToken)
    {
        var clientEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        await using var clientStream = client.GetStream();
        await using var backendStream = backend.GetStream();

        // Two tasks: client→backend and backend→client
        var clientToBackend = CopyStreamAsync(
            clientStream,
            backendStream,
            cancellationToken
        );

        var backendToClient = CopyStreamAsync(
            backendStream,
            clientStream,
            cancellationToken
        );

        // Wait for either direction to complete/fail
        await Task.WhenAny(clientToBackend, backendToClient);
    }

    private static async Task CopyStreamAsync(
        NetworkStream source,
        NetworkStream destination,
        CancellationToken cancellationToken)
    {
        try
        {
            await source.CopyToAsync(destination, cancellationToken);
        }
        catch (Exception ex)
        {
            // Only log if it's not a normal disconnection
            if (ex is not ObjectDisposedException && ex is not IOException)
            {
                Logger.Warn($"Stream copy error : {ex.Message}");
            }
        }
    }

    private static void ConfigureSocket(TcpClient client)
    {
        client.NoDelay = true;
        client.ReceiveTimeout = Config.ConnectionTimeoutSeconds * 1000;
        client.SendTimeout = Config.ConnectionTimeoutSeconds * 1000;

        // Enable socket reuse (helps with rapid connections)
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
    }
}