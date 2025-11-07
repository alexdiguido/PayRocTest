namespace Payroc.App.LoadBalancer;


public class BackendServer
{
    public string Host { get; }
    public int Port { get; }
    public bool IsHealthy { get; set; }
    public DateTime LastChecked { get; set; }
    public string Address => $"{Host}:{Port}";

    public BackendServer(string host, int port)
    {
        Host = host;
        Port = port;
        IsHealthy = false;
        LastChecked = DateTime.MinValue;
    }

    public override string ToString() => $"{Address} [{(IsHealthy ? "UP" : "DOWN")}]";
}