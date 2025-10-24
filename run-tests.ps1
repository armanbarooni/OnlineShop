# ==========================================
# اسکریپت اجرای تست‌ها - بدون هنگ در Cursor
# ==========================================

param(
    [string]$Filter = "",
    [switch]$OnlyIntegration,
    [switch]$OnlyApplication,
    [switch]$Detailed
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Starting Test Execution" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Create results directory if not exists
$resultsDir = "test-results"
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$trxFile = "$resultsDir/test_results_$timestamp.trx"
$summaryFile = "$resultsDir/test_summary_$timestamp.txt"

# Build first
Write-Host "[1/3] Building solution..." -ForegroundColor Yellow
dotnet build --no-restore -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Please fix compilation errors first." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green
Write-Host ""

# Determine which tests to run
$testCommand = "dotnet test"
if ($OnlyIntegration) {
    $testCommand += " tests/OnlineShop.IntegrationTests/OnlineShop.IntegrationTests.csproj"
    Write-Host "[2/3] Running Integration Tests only..." -ForegroundColor Yellow
}
elseif ($OnlyApplication) {
    $testCommand += " tests/OnlineShop.Application.Tests/OnlineShop.Application.Tests.csproj"
    Write-Host "[2/3] Running Application Tests only..." -ForegroundColor Yellow
}
else {
    Write-Host "[2/3] Running All Tests..." -ForegroundColor Yellow
}

# Add filter if specified
if ($Filter -ne "") {
    $testCommand += " --filter `"FullyQualifiedName~$Filter`""
    Write-Host "    Filter: $Filter" -ForegroundColor Gray
}

# Add logging and verbosity
$verbosity = if ($Detailed) { "normal" } else { "minimal" }
$testCommand += " --verbosity $verbosity --nologo --logger `"trx;LogFileName=$trxFile`""

Write-Host "    Output: $trxFile" -ForegroundColor Gray
Write-Host ""

# Run tests
$output = Invoke-Expression "$testCommand 2>&1"

# Parse results
Write-Host "[3/3] Analyzing results..." -ForegroundColor Yellow
Write-Host ""

# Extract summary from output
$passedLine = $output | Select-String "Passed!" | Select-Object -Last 1
$failedLine = $output | Select-String "Failed!" | Select-Object -Last 1
$summaryLine = $output | Select-String "Test summary:" | Select-Object -Last 1

# Create summary report
$summary = @"
==========================================
     Test Execution Summary
==========================================
Timestamp: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
TRX File: $trxFile

$($passedLine -join "`n")
$($failedLine -join "`n")
$($summaryLine -join "`n")

==========================================
     Detailed Analysis
==========================================

"@

# Extract failed tests
$failedTests = $output | Select-String "Failed.*\[FAIL\]" | ForEach-Object { $_.Line.Trim() }
if ($failedTests.Count -gt 0) {
    $summary += "Failed Tests ($($failedTests.Count)):`n"
    $failedTests | ForEach-Object {
        $summary += "  - $_`n"
    }
    $summary += "`n"
}

# Extract error types
$authErrors = ($output | Select-String "401.*Unauthorized").Count
$notFoundErrors = ($output | Select-String "404.*NotFound").Count
$badRequestErrors = ($output | Select-String "400.*BadRequest").Count
$methodNotAllowedErrors = ($output | Select-String "405.*MethodNotAllowed").Count

$summary += "Error Types:`n"
$summary += "  - 401 Unauthorized: $authErrors`n"
$summary += "  - 404 Not Found: $notFoundErrors`n"
$summary += "  - 400 Bad Request: $badRequestErrors`n"
$summary += "  - 405 Method Not Allowed: $methodNotAllowedErrors`n"
$summary += "`n"

# Calculate percentages from summary line
if ($summaryLine) {
    if ($summaryLine -match "total: (\d+), failed: (\d+), succeeded: (\d+)") {
        $total = [int]$matches[1]
        $failed = [int]$matches[2]
        $succeeded = [int]$matches[3]
        $percentage = [math]::Round(($succeeded / $total) * 100, 1)
        
        $summary += "Success Rate: $percentage% ($succeeded/$total)`n"
        
        # Visual progress bar
        $barLength = 40
        $filledLength = [int](($succeeded / $total) * $barLength)
        $emptyLength = $barLength - $filledLength
        $bar = "█" * $filledLength + "░" * $emptyLength
        
        $summary += "Progress: [$bar] $percentage%`n"
    }
}

$summary += "`n==========================================`n"

# Save summary to file
$summary | Out-File -FilePath $summaryFile -Encoding UTF8

# Display summary
Write-Host $summary

# Color-coded result
if ($failedLine) {
    Write-Host "⚠️  Some tests failed. Check details above." -ForegroundColor Yellow
    Write-Host "   Full results: $trxFile" -ForegroundColor Gray
    Write-Host "   Summary: $summaryFile" -ForegroundColor Gray
}
else {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
}

Write-Host ""
Write-Host "To analyze in Cursor, read:" -ForegroundColor Cyan
Write-Host "  $summaryFile" -ForegroundColor White
Write-Host ""

# Return exit code based on test results
if ($LASTEXITCODE -eq 0) {
    exit 0
} else {
    exit 1
}




