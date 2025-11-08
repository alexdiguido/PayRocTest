# TCP Load Balancer

A simple TCP load balancer built with .NET 8 that distributes incoming traffic across multiple backend servers.

## What It Does

This load balancer:
- Listens on port **8080** for incoming HTTP requests
- Distributes requests across **3 backend servers** using round-robin
- Monitors backend health every 10 seconds
- Automatically removes unhealthy backends from rotation

## Quick Start

### Run with Docker

```powershell
# Start all services
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Test the Load Balancer

Use the provided PowerShell script to send requests:

```powershell
# Send 20 requests (default)
.\test-loadbalancer.ps1

# Send 50 requests
.\test-loadbalancer.ps1 -Count 50
```

**Expected output:**
```
Request 1 - Status: 200 - Response from Backend-1 at 2025-11-08...
Request 2 - Status: 200 - Response from Backend-2 at 2025-11-08...
Request 3 - Status: 200 - Response from Backend-3 at 2025-11-08...
Request 4 - Status: 200 - Response from Backend-1 at 2025-11-08...
```

Each request goes to a different backend in rotation.

## Architecture

```
Client ? Load Balancer (port 8080) ? Backend 1, 2, or 3 (port 80)
```

## Running Tests

```powershell
# Run all unit tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Requirements

- Docker Desktop 20.10+
- PowerShell 5.1+ (for testing script)
- .NET 8 SDK (for running tests locally)

## Configuration

Edit `Payroc.App/LoadBalancer/Config.cs` to modify:
- Listen port (default: 8080)
- Backend servers
- Health check intervals
- Connection timeouts

---

**Built with .NET 8**
