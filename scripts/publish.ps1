# PowerShell script to build and publish the project
# This script prepares frontend files and publishes the backend

param(
    [string]$Configuration = "Release",
    [string]$Environment = "production",
    [string]$OutputPath = "./publish"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  OnlineShop - Publish Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Prepare frontend files
Write-Host "Step 1: Preparing frontend files..." -ForegroundColor Yellow
$prepareScript = Join-Path $PSScriptRoot "prepare-publish.ps1"
if (Test-Path $prepareScript) {
    & $prepareScript -Environment $Environment
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Failed to prepare frontend files" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Warning: prepare-publish.ps1 not found. Skipping frontend sync." -ForegroundColor Yellow
}

Write-Host ""

# Step 2: Build and publish
Write-Host "Step 2: Building and publishing backend..." -ForegroundColor Yellow
$projectPath = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..")) "src\WebAPI\OnlineShop.WebAPI.csproj"

if (-not (Test-Path $projectPath)) {
    Write-Host "Error: Project file not found: $projectPath" -ForegroundColor Red
    exit 1
}

Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Output: $OutputPath" -ForegroundColor Cyan
Write-Host ""

# Clean previous publish
if (Test-Path $OutputPath) {
    Write-Host "Cleaning previous publish output..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Publish
Write-Host "Publishing project..." -ForegroundColor Cyan
$publishArgs = @(
    "publish",
    $projectPath,
    "-c", $Configuration,
    "-o", $OutputPath,
    "--verbosity", "minimal"
)

& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Publish completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output location: $OutputPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Review the published files in: $OutputPath" -ForegroundColor White
Write-Host "  2. Deploy to your server" -ForegroundColor White
Write-Host "  3. Configure environment variables and connection strings" -ForegroundColor White
Write-Host ""








