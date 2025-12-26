# Script to fix logout buttons in all user panel pages
# Adds id="logoutButton" to logout links and includes user-panel-common.js

$presentationPath = Join-Path $PSScriptRoot "..\presentation"
$userPanelFiles = Get-ChildItem -Path $presentationPath -Filter "user-panel-*.html"

foreach ($file in $userPanelFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $modified = $false
    
    # Add id="logoutButton" to logout links that don't have it
    # Look for logout links that contain "خروج" text (encoded or not) but don't have id="logoutButton"
    if ($content -notmatch 'id="logoutButton"') {
        # Pattern 1: <a href="#" class="...flex items-center..." (single line or multi-line)
        if ($content -match '<a href="#"[^>]*class="[^"]*flex items-center[^"]*"[^>]*>') {
            # Check if this link contains logout icon (M15.75 9V5.25) and خروج text
            $logoutPattern = '(<a href="#")([^>]*class="[^"]*flex items-center[^"]*"[^>]*>)'
            if ($content -match $logoutPattern) {
                # Check if the link contains logout-related content
                $match = [regex]::Match($content, $logoutPattern)
                $linkContent = $content.Substring($match.Index, [Math]::Min(500, $content.Length - $match.Index))
                if ($linkContent -match '(M15\.75 9V5\.25|خروج|ط®ط±ظˆط¬)') {
                    $content = $content -replace $logoutPattern, '$1 id="logoutButton"$2'
                    $modified = $true
                    Write-Host "Added id to logout button in $($file.Name)" -ForegroundColor Green
                }
            }
        }
        # Pattern 2: <a href="#" with newline before class
        elseif ($content -match '<a href="#"\s+class="[^"]*flex items-center[^"]*"') {
            $logoutPattern = '(<a href="#")(\s+class="[^"]*flex items-center[^"]*")'
            $match = [regex]::Match($content, $logoutPattern)
            if ($match.Success) {
                $linkContent = $content.Substring($match.Index, [Math]::Min(500, $content.Length - $match.Index))
                if ($linkContent -match '(M15\.75 9V5\.25|خروج|ط®ط±ظˆط¬)') {
                    $content = $content -replace $logoutPattern, '$1 id="logoutButton"$2'
                    $modified = $true
                    Write-Host "Added id to logout button in $($file.Name)" -ForegroundColor Green
                }
            }
        }
    }
    
    # Add user-panel-common.js if auth-guard.js is present and user-panel-common.js is not
    if ($content -match 'auth-guard\.js' -and $content -notmatch 'user-panel-common\.js') {
        $content = $content -replace '(<script src="assets/js/auth-guard\.js"></script>)', '$1`n<script src="assets/js/user-panel-common.js"></script>'
        $modified = $true
        Write-Host "Added user-panel-common.js to $($file.Name)" -ForegroundColor Green
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "Updated $($file.Name)" -ForegroundColor Yellow
    } else {
        Write-Host "No changes needed for $($file.Name)" -ForegroundColor Gray
    }
}

Write-Host "`nDone! All user panel pages have been updated." -ForegroundColor Cyan

