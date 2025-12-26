# Fix menu links in user panel pages
# Changes direct links to order-detail and edit-address to their list pages

$files = Get-ChildItem -Path "presentation" -Filter "user-panel-*.html"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $changed = $false
    
    # Replace order-detail link with order list
    if ($content -match 'href="user-panel-order-detail\.html"') {
        $content = $content -replace 'href="user-panel-order-detail\.html"', 'href="user-panel-order.html"'
        $changed = $true
    }
    
    # Replace edit-address link with address list
    if ($content -match 'href="user-panel-edit-address\.html"') {
        $content = $content -replace 'href="user-panel-edit-address\.html"', 'href="user-panel-address.html"'
        $changed = $true
    }
    
    if ($changed) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host "Done! All menu links have been fixed."

