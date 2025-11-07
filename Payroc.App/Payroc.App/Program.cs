using Payroc.App.LoadBalancer;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

// 1999 Load Balancer - Keep it simple!
var loadBalancer = new LoadBalancer();

// Handle Ctrl+C gracefully
Console.CancelKeyPress += (sender, args) =>
{
    args.Cancel = true;
    loadBalancer.Stop();
};

try
{
    await loadBalancer.StartAsync();
}
catch (Exception ex)
{
    Logger.Error($"Fatal error: {ex.Message}");
    return 1;
}

return 0;
