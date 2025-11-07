
namespace Payroc.App.LoadBalancer
{
    public interface IHealthChecker
    {
        Task StartAsync();
        void Stop();
    }
}