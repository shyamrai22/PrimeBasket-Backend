param(
    [string]$ProjectKey = "PrimeBasket",
    [string]$HostUrl = "http://localhost:9000",
    [string]$Token = "sqp_d7570e72fdf36ea80fb9b729d4dd7af0663575b9"
)

# Ensure the script stops on errors
$ErrorActionPreference = "Stop"

Write-Host "Starting SonarQube Scanner..." -ForegroundColor Green
dotnet sonarscanner begin /k:"$ProjectKey" /d:sonar.host.url="$HostUrl" /d:sonar.login="$Token"

Write-Host "Building the project..." -ForegroundColor Green
dotnet build PrimeBasket-Backend.sln

Write-Host "Ending SonarQube Scanner..." -ForegroundColor Green
dotnet sonarscanner end /d:sonar.login="$Token"

Write-Host "SonarQube analysis complete!" -ForegroundColor Green
