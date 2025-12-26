# PowerShell script to prepare project for publishing
# This script syncs presentation files to wwwroot/fa before publishing

param(
    [string]$Environment = "production"
)

$ErrorActionPreference = "Stop"

Write-Host "Preparing project for publishing..." -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$source = Join-Path $repoRoot "presentation"
$destination = Join-Path $repoRoot "src\WebAPI\wwwroot\fa"

if (-not (Test-Path $source)) {
    Write-Host "Error: Presentation folder not found: $source" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $destination)) {
    Write-Host "Creating destination folder: $destination" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
}

# Use the existing sync-frontend script
$syncScript = Join-Path $PSScriptRoot "sync-frontend.ps1"
if (Test-Path $syncScript) {
    Write-Host "Syncing frontend files using sync-frontend.ps1..." -ForegroundColor Cyan
    & $syncScript -Environment $Environment -CleanDest
} else {
    Write-Host "Warning: sync-frontend.ps1 not found. Using basic copy..." -ForegroundColor Yellow
    
    # Basic copy as fallback
    Write-Host "Copying HTML files..." -ForegroundColor Cyan
    Get-ChildItem -Path $source -Filter *.html -File | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination (Join-Path $destination $_.Name) -Force
    }

    # Copy config.js if exists
    $configFile = Join-Path $source "config.js"
    if (Test-Path $configFile) {
        Copy-Item -Path $configFile -Destination (Join-Path $destination "config.js") -Force
    }

    # Mirror assets directory
    $sourceAssets = Join-Path $source "assets"
    $destinationAssets = Join-Path $destination "assets"
    if (Test-Path $sourceAssets) {
        if (-not (Test-Path $destinationAssets)) {
            New-Item -ItemType Directory -Path $destinationAssets -Force | Out-Null
        }
        $robocopyArgs = @(
            $sourceAssets,
            $destinationAssets,
            "/MIR",
            "/NFL","/NDL","/NJH","/NJS","/NP"
        )
        $robocopyResult = Start-Process -FilePath "robocopy" -ArgumentList $robocopyArgs -Wait -NoNewWindow -PassThru
        if ($robocopyResult.ExitCode -ge 8) {
            Write-Host "Robocopy failed with exit code $($robocopyResult.ExitCode)" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "Frontend files synced successfully!" -ForegroundColor Green
Write-Host "Destination: $destination" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now publish the project using:" -ForegroundColor Yellow
Write-Host "  dotnet publish src/WebAPI/OnlineShop.WebAPI.csproj -c Release -o ./publish" -ForegroundColor White
Write-Host ""







