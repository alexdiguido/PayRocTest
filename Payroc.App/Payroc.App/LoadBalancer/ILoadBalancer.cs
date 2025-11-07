
namespace Payroc.App.LoadBalancer
{
    public interface ILoadBalancer
    {
        Task StartAsync();
        void Stop();
    }
}