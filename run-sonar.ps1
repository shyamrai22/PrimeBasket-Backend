param(
    [string]$ProjectKey = "PrimeBasket-Backend",
    [string]$HostUrl = "http://localhost:9000",
    [string]$Token = "sqp_e3d53ce55c4316e6bcd34a8dade9ee407250affb"
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
