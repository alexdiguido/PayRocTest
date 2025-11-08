param(
    [int]$Count = 20
)

Write-Host "Sending $Count requests to http://localhost:8080 with new connections..." -ForegroundColor Cyan
Write-Host ""

for ($i = 1; $i -le $Count; $i++) {
    Write-Host "Request $i" -NoNewline -ForegroundColor White
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080" -Method Get -UseBasicParsing -DisableKeepAlive
        $content = $response.Content
        Write-Host " - Status: $($response.StatusCode) - $content" -ForegroundColor Green
    }
    catch {
        Write-Host " - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Done. Sent $Count requests." -ForegroundColor Cyan
