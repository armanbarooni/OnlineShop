# ==========================================
# تحلیل نتایج تست از فایل TRX
# ==========================================

param(
    [string]$TrxFile = ""
)

# If no file specified, use the latest one
if ($TrxFile -eq "") {
    $latestTrx = Get-ChildItem -Path "test-results" -Filter "*.trx" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($null -eq $latestTrx) {
        Write-Host "No TRX files found in test-results directory!" -ForegroundColor Red
        Write-Host "Run .\run-tests.ps1 first to generate results." -ForegroundColor Yellow
        exit 1
    }
    $TrxFile = $latestTrx.FullName
}

Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "  Analyzing: $TrxFile" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""

# Parse TRX file
[xml]$trx = Get-Content $TrxFile

$results = $trx.TestRun.Results.UnitTestResult
$total = $results.Count
$passed = ($results | Where-Object { $_.outcome -eq "Passed" }).Count
$failed = ($results | Where-Object { $_.outcome -eq "Failed" }).Count
$skipped = ($results | Where-Object { $_.outcome -eq "NotExecuted" -or $_.outcome -eq "Skipped" }).Count

$percentage = if ($total -gt 0) { [math]::Round(($passed / $total) * 100, 1) } else { 0 }

# Display summary
Write-Host "📊 Overall Statistics:" -ForegroundColor Yellow
Write-Host "   Total:   $total tests" -ForegroundColor White
Write-Host "   Passed:  $passed tests ✓" -ForegroundColor Green
Write-Host "   Failed:  $failed tests ✗" -ForegroundColor Red
Write-Host "   Skipped: $skipped tests" -ForegroundColor Gray
Write-Host "   Success: $percentage%" -ForegroundColor Cyan
Write-Host ""

# Progress bar
$barLength = 50
$filledLength = if ($total -gt 0) { [int](($passed / $total) * $barLength) } else { 0 }
$emptyLength = $barLength - $filledLength
$bar = "█" * $filledLength + "░" * $emptyLength
Write-Host "   [$bar] $percentage%" -ForegroundColor Cyan
Write-Host ""

if ($failed -gt 0) {
    Write-Host "❌ Failed Tests:" -ForegroundColor Red
    Write-Host ""
    
    # Group failures by error type
    $failedTests = $results | Where-Object { $_.outcome -eq "Failed" }
    
    $errorGroups = @{}
    foreach ($test in $failedTests) {
        $errorMsg = $test.Output.ErrorInfo.Message
        if ($errorMsg -match "(\d{3}).*\{value: (\d+)\}") {
            $errorCode = $matches[2]
            $key = "HTTP $errorCode"
        }
        else {
            $key = "Other"
        }
        
        if (-not $errorGroups.ContainsKey($key)) {
            $errorGroups[$key] = @()
        }
        
        $testName = $test.testName -replace ".*\.([^.]+)$", '$1'
        $errorGroups[$key] += $testName
    }
    
    # Display grouped errors
    foreach ($group in $errorGroups.Keys | Sort-Object) {
        Write-Host "  $group ($($errorGroups[$group].Count) tests):" -ForegroundColor Yellow
        $errorGroups[$group] | Select-Object -First 10 | ForEach-Object {
            Write-Host "    - $_" -ForegroundColor Gray
        }
        if ($errorGroups[$group].Count -gt 10) {
            Write-Host "    ... and $($errorGroups[$group].Count - 10) more" -ForegroundColor DarkGray
        }
        Write-Host ""
    }
}

Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "📝 Recommendations:" -ForegroundColor Yellow
Write-Host ""

if ($failed -gt 0) {
    # Analyze error patterns
    $authErrorCount = ($failedTests | Where-Object { $_.Output.ErrorInfo.Message -match "401" }).Count
    $routeErrorCount = ($failedTests | Where-Object { $_.Output.ErrorInfo.Message -match "405" }).Count
    $validationErrorCount = ($failedTests | Where-Object { $_.Output.ErrorInfo.Message -match "400" }).Count
    
    if ($authErrorCount -gt $failed * 0.5) {
        Write-Host "🔴 Primary Issue: Authentication" -ForegroundColor Red
        Write-Host "   $authErrorCount tests failing with 401 Unauthorized" -ForegroundColor Gray
        Write-Host "   → Check AuthHelper.cs and CustomWebApplicationFactory.cs" -ForegroundColor White
        Write-Host "   → Verify JWT configuration in appsettings.Testing.json" -ForegroundColor White
        Write-Host "   → Run: .\run-tests.ps1 -Filter `"DebugTests.TestAuthentication`" -Detailed" -ForegroundColor Cyan
        Write-Host ""
    }
    
    if ($routeErrorCount -gt 5) {
        Write-Host "🟡 Route Issues: $routeErrorCount tests" -ForegroundColor Yellow
        Write-Host "   → Check Controller endpoints match test expectations" -ForegroundColor White
        Write-Host ""
    }
    
    if ($validationErrorCount -gt 5) {
        Write-Host "🟡 Validation Issues: $validationErrorCount tests" -ForegroundColor Yellow
        Write-Host "   → Check DTO field names and validation rules" -ForegroundColor White
        Write-Host ""
    }
    
    Write-Host "💡 Next Steps:" -ForegroundColor Cyan
    Write-Host "   1. Fix primary issue first (highest impact)" -ForegroundColor White
    Write-Host "   2. Re-run tests: .\run-tests.ps1" -ForegroundColor White
    Write-Host "   3. Repeat until 95%+ success rate" -ForegroundColor White
}
else {
    Write-Host "🎉 All tests passing! Great job!" -ForegroundColor Green
}

Write-Host ""
Write-Host "===========================================" -ForegroundColor Cyan




