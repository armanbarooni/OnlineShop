# Script to fix JSON parsing in test files
$files = Get-ChildItem -Path "tests\OnlineShop.IntegrationTests\Scenarios" -Filter "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    
    # Fix pattern 1: result?.data?.id?.ToString()
    $content = $content -replace 'var result = await response\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+return Guid\.Parse\(result\?\.data\?\.id\?\.ToString\(\) \?\? Guid\.Empty\.ToString\(\)\);', 'var content = await response.Content.ReadAsStringAsync();
            var id = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(id);'
    
    # Fix pattern 2: result?.isSuccess
    $content = $content -replace 'var result = await response\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+result\?\.isSuccess\.Should\(\)\.Be\(true\);', 'var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");'
    
    # Fix pattern 3: SavedCartTests specific issue
    $content = $content -replace 'var save(Result|jsonHelper) = await save(Response|result)\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+var id = save(Result|jsonHelper)\?\.data\?\.id\?\.ToString\(\);', 'var saveContent = await saveResponse.Content.ReadAsStringAsync();
                var id = JsonHelper.GetNestedProperty(saveContent, "data", "id");'
    
    # Fix pattern 4: authResult/orderResult specific
    $content = $content -replace 'var (auth|order)(Result|jsonHelper) = await (auth|register|order)Response\.Content\.ReadFromJsonAsync<dynamic>\(\);', 'var $1Content = await $3Response.Content.ReadAsStringAsync();'
    
    Set-Content -Path $file.FullName -Value $content -NoNewline
    Write-Host "Fixed: $($file.Name)"
}

Write-Host "All files processed!"

