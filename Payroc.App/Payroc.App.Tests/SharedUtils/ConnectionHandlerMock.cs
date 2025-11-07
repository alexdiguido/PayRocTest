using Payroc.App.LoadBalancer;
using System.Net.Sockets;

namespace Payroc.App.Tests.LoadBalancer;

public class ConnectionHandlerMock : IConnectionHandler
{
    public bool Handled { get; private set; }
    public Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        Handled = true;
        client.Dispose();
        return Task.CompletedTask;
    }
}
