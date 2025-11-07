namespace Payroc.App.LoadBalancer;


public static class Config
{
    public const int ListenPort = 8080;
    public const int HealthCheckIntervalSeconds = 10;
    public const int HealthCheckTimeoutSeconds = 2;
    public const int ConnectionTimeoutSeconds = 30;

    public static readonly List<(string Host, int Port)> Backends = new()
    {
        ("backend1", 80),
        ("backend2", 80),
        ("backend3", 80)
    };
}