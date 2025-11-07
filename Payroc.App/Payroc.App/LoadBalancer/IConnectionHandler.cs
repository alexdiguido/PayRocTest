using System.Net.Sockets;

namespace Payroc.App.LoadBalancer
{
    public interface IConnectionHandler
    {
        Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken);
    }
}