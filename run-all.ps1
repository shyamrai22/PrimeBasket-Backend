# PrimeBasket Microservices Startup Script
# This script starts all microservices in separate PowerShell windows.

$services = @(
    "PrimeBasket.Auth.API",
    "PrimeBasket.Product.API",
    "PrimeBasket.Cart.API",
    "PrimeBasket.Order.API",
    "PrimeBasket.Payment.API",
    "PrimeBasket.ApiGateway"
)

Write-Host "---------------------------------------------------" -ForegroundColor Yellow
Write-Host "Starting PrimeBasket Microservices..." -ForegroundColor Yellow
Write-Host "---------------------------------------------------" -ForegroundColor Yellow

foreach ($service in $services) {
    if (Test-Path $service) {
        Write-Host "Launching $service..." -ForegroundColor Cyan
        # Start-Process opens a new window for each service
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host 'Starting $service...'; cd $service; dotnet run"
        # Small delay to prevent resource contention
        Start-Sleep -Seconds 2
    } else {
        Write-Warning "Directory $service not found. Skipping..."
    }
}

Write-Host "`nAll services have been triggered. Check the individual windows for logs." -ForegroundColor Green
Write-Host "---------------------------------------------------" -ForegroundColor Yellow
