using System.Net;
using System.Net.Sockets;
using System.Text;

var port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "80");
var serverId = Environment.GetEnvironmentVariable("SERVER_ID") ?? "unknown";
var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL")?.ToUpper() ?? "INFO";
var isDebug = logLevel == "DEBUG";

var listener = new TcpListener(IPAddress.Any, port);
listener.Start();

Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Backend server {serverId} listening on port {port}");
Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Log Level: {logLevel}");

while (true)
{
    try
    {
        if (isDebug)
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [DEBUG] Waiting for connection...");
            
        var client = await listener.AcceptTcpClientAsync();
        var clientEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        
        if (isDebug)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [DEBUG] Connection received from {clientEndpoint}");
        }
        
        // Handle connection in background
        _ = Task.Run(async () =>
        {
            try
            {
                using (client)
                {
                    var stream = client.GetStream();
                    
                    // Read the request
                    var buffer = new byte[4096];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead > 0 && isDebug)
                    {
                        var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [DEBUG] Request: {request.Substring(0, Math.Min(100, request.Length))}...");
                    }
                    
                    // Create HTTP response
                    var responseBody = $"Response from {serverId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}";
                    var responseHeaders = 
                        "HTTP/1.1 200 OK\r\n" +
                        "Content-Type: text/plain\r\n" +
                        $"Content-Length: {responseBody.Length}\r\n" +
                        "Connection: close\r\n" +
                        "\r\n";
                    
                    var fullResponse = responseHeaders + responseBody;
                    var responseBytes = Encoding.UTF8.GetBytes(fullResponse);
                    
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    await stream.FlushAsync();
                    
                    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [INFO] Handled request from {clientEndpoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] Error handling client: {ex.GetType().Name}: {ex.Message}");
                if (isDebug)
                {
                    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] StackTrace:\n{ex.StackTrace}");
                }
            }
        });
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] Socket error: {ex.Message}");
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] ErrorCode: {ex.SocketErrorCode}");
        if (isDebug)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] StackTrace:\n{ex.StackTrace}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] Unexpected error: {ex.GetType().Name}: {ex.Message}");
        if (isDebug)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] StackTrace:\n{ex.StackTrace}");
        }
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] Inner Exception: {ex.InnerException.Message}");
            if (isDebug)
            {
                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] Inner StackTrace:\n{ex.InnerException.StackTrace}");
            }
        }
    }
}
