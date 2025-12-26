param(
    [switch]$CleanDest,
    [string]$Environment = "development"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$source = Join-Path $repoRoot "presentation"
$destination = Join-Path $repoRoot "src\WebAPI\wwwroot\fa"
$runtimeConfigName = "config.runtime.json"
$environmentValue = if ([string]::IsNullOrWhiteSpace($Environment)) { "development" } else { $Environment }
$environmentKey = $environmentValue.ToLower()
$sourceConfig = Join-Path $repoRoot ("presentation\config.{0}.json" -f $environmentKey)
$destinationConfig = Join-Path $destination $runtimeConfigName

if (-not (Test-Path $source)) {
    throw "Source folder '$source' not found."
}

if (-not (Test-Path $destination)) {
    New-Item -ItemType Directory -Path $destination | Out-Null
}

if ($CleanDest) {
    Write-Host "Cleaning destination: $destination"
    Get-ChildItem $destination -Force | Remove-Item -Recurse -Force
}

$excludedDirs = @("node_modules", ".git", ".cursor", ".idea", ".vscode")
$excludedFiles = @("package.json", "package-lock.json", "server.ps1", "start-server.ps1", "test-data.json", ".htaccess")
$robocopyArgs = @(
    $source,
    $destination,
    "/MIR",
    "/NFL",
    "/NDL",
    "/NJH",
    "/NJS",
    "/NP",
    "/XD"
) + $excludedDirs + @("/XF") + $excludedFiles

Write-Host "Syncing frontend ($Environment) from '$source' to '$destination'..."

& robocopy @robocopyArgs | Out-Host
$exitCode = $LASTEXITCODE
if ($exitCode -ge 8) {
    throw "Robocopy failed with exit code $exitCode"
}

Write-Host "Frontend sync completed successfully."

if (Test-Path $sourceConfig) {
    Copy-Item $sourceConfig -Destination $destinationConfig -Force
    Write-Host "Runtime config copied: $sourceConfig -> $destinationConfig"
} else {
    Write-Warning "Runtime config '$sourceConfig' not found. Using defaults."
}

foreach ($file in $excludedFiles) {
    $target = Join-Path $destination $file
    if (Test-Path $target) {
        Remove-Item $target -Force
    }
}

$rootOnlyFiles = @("README.md")
foreach ($file in $rootOnlyFiles) {
    $target = Join-Path $destination $file
    if (Test-Path $target) {
        Remove-Item $target -Force
    }
}

$configArtifacts = Get-ChildItem $destination -Filter "config.*.json" -File | Where-Object { $_.Name -ne $runtimeConfigName }
foreach ($artifact in $configArtifacts) {
    Remove-Item $artifact.FullName -Force
}

