# Script to update all test classes to store factory and pass it to GetAdminTokenAsync

$testFiles = Get-ChildItem -Path "tests\OnlineShop.IntegrationTests\Scenarios" -Recurse -Filter "*Tests.cs" -File

foreach ($file in $testFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # Check if file uses IClassFixture with CustomWebApplicationFactory
    if ($content -match 'IClassFixture<CustomWebApplicationFactory<Program>>') {
        $modified = $false
        
        # Check if factory field exists
        if ($content -notmatch 'private readonly CustomWebApplicationFactory<Program> _factory;') {
            # Add factory field after HttpClient field
            if ($content -match '(private readonly HttpClient _client;)') {
                $content = $content -replace '(private readonly HttpClient _client;)', "`$1`n        private readonly CustomWebApplicationFactory<Program> _factory;"
                $modified = $true
            }
        }
        
        # Check if factory is assigned in constructor
        if ($content -match 'public.*Tests\(CustomWebApplicationFactory<Program> factory\)' -and 
            $content -notmatch '_factory = factory;') {
            # Add factory assignment
            if ($content -match '(_client = factory\.CreateClient\(\);)') {
                $content = $content -replace '(_client = factory\.CreateClient\(\);)', "            _factory = factory;`n            `$1"
                $modified = $true
            }
        }
        
        # Replace GetAdminTokenAsync(_client) with GetAdminTokenAsync(_client, _factory)
        if ($content -match 'GetAdminTokenAsync\(_client\);') {
            $content = $content -replace 'GetAdminTokenAsync\(_client\);', 'GetAdminTokenAsync(_client, _factory);'
            $modified = $true
        }
        
        if ($modified) {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            Write-Host "Updated: $($file.Name)" -ForegroundColor Green
        }
    }
}

Write-Host "`nUpdate complete!" -ForegroundColor Cyan

