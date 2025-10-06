# PowerShell script to run the modernized PartsUnlimited tests
# This script can be used once .NET 8 SDK is installed

Write-Host "PartsUnlimited .NET 8 Test Runner" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Check if .NET is available
try {
    $dotnetVersion = dotnet --version
    Write-Host "Using .NET SDK version: $dotnetVersion" -ForegroundColor Yellow
} catch {
    Write-Host "Error: .NET SDK not found. Please install .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Package restore failed!" -ForegroundColor Red
    exit 1
}

# Build the solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Run tests
Write-Host "Running unit tests..." -ForegroundColor Yellow
dotnet test --no-build --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "All tests passed!" -ForegroundColor Green
} else {
    Write-Host "Some tests failed!" -ForegroundColor Red
}

# Run the application
Write-Host "Starting the application..." -ForegroundColor Yellow
Write-Host "Navigate to https://localhost:5001 to view the application" -ForegroundColor Cyan
Set-Location "src\PartsUnlimitedWebsite"
dotnet run