# PrimeBasket Microservices Shutdown Script
# This script stops all running .NET processes associated with the microservices.

Write-Host "---------------------------------------------------" -ForegroundColor Red
Write-Host "Stopping all PrimeBasket Microservices..." -ForegroundColor Red
Write-Host "---------------------------------------------------" -ForegroundColor Red

# Find all dotnet processes and stop them
$dotnetProcesses = Get-Process dotnet -ErrorAction SilentlyContinue

if ($dotnetProcesses) {
    Write-Host "Found $($dotnetProcesses.Count) dotnet processes. Stopping them..." -ForegroundColor Yellow
    $dotnetProcesses | Stop-Process -Force
    Write-Host "Successfully stopped all dotnet services." -ForegroundColor Green
} else {
    Write-Host "No running dotnet processes found." -ForegroundColor Gray
}

Write-Host "---------------------------------------------------" -ForegroundColor Red
Write-Host "Done. You may still need to manually close any empty PowerShell windows." -ForegroundColor Gray
