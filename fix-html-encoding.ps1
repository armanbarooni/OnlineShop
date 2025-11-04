# PowerShell script to convert HTML files to UTF-8 encoding
# This fixes Persian text display issues (mojibake)

Write-Host "Starting HTML encoding conversion to UTF-8..." -ForegroundColor Green
Write-Host ""

$htmlFiles = Get-ChildItem -Path "presentation\*.html"
$count = 0

foreach ($file in $htmlFiles) {
    try {
        # Read file content (PowerShell will auto-detect encoding)
        $content = Get-Content $file.FullName -Raw -Encoding Default
        
        # Write back with UTF-8 encoding
        [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
        
        $count++
        Write-Host "Converted: $($file.Name)" -ForegroundColor Cyan
    }
    catch {
        Write-Host "Error converting $($file.Name): $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Conversion complete! Converted $count file(s)." -ForegroundColor Green
Write-Host "Please verify the files in your browser to ensure Persian text displays correctly." -ForegroundColor Yellow

