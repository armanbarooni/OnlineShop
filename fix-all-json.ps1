$files = @(
    "tests\OnlineShop.IntegrationTests\Scenarios\ProductInventoryTests.cs",
    "tests\OnlineShop.IntegrationTests\Scenarios\ProductReviewTests.cs",
    "tests\OnlineShop.IntegrationTests\Scenarios\ProductVariantTests.cs",
    "tests\OnlineShop.IntegrationTests\Scenarios\StockAlertTests.cs",
    "tests\OnlineShop.IntegrationTests\Scenarios\SavedCartTests.cs",
    "tests\OnlineShop.IntegrationTests\Scenarios\UserAddressTests.cs",
    "tests\OnlineShop.IntegrationTests\Scenarios\UserReturnRequestTests.cs",
    "tests\OnlineShop.IntegrationTests\Scenarios\UserProfileTests.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # Replace all instances where we read JSON and then use content variable that doesn't exist
        $content = $content -replace '(\s+)var result = await (\w+)\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+(\w+) = JsonHelper\.GetNestedProperty\(content, "data", "id"\)', '$1var content = await $2.Content.ReadAsStringAsync();$1$3 = JsonHelper.GetNestedProperty(content, "data", "id")'
        
        # Fix CreateTest* methods
        $content = $content -replace '(\s+)var response = await _client\.PostAsJsonAsync\("/api/product", productDto\);\s+var result = await response\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+return Guid\.Parse\(JsonHelper\.GetNestedProperty\(content, "data", "id"\) \?\? Guid\.Empty\.ToString\(\)\);', '$1var response = await _client.PostAsJsonAsync("/api/product", productDto);$1var content = await response.Content.ReadAsStringAsync();$1var productId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();$1return Guid.Parse(productId);'
        
        $content = $content -replace '(\s+)var response = await _client\.PostAsJsonAsync\("/api/productcategory", categoryDto\);\s+var result = await response\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+return Guid\.Parse\(JsonHelper\.GetNestedProperty\(content, "data", "id"\) \?\? Guid\.Empty\.ToString\(\)\);', '$1var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);$1var content = await response.Content.ReadAsStringAsync();$1var categoryId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();$1return Guid.Parse(categoryId);'
        
        $content = $content -replace '(\s+)var response = await _client\.PostAsJsonAsync\("/api/unit", unitDto\);\s+var result = await response\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+return Guid\.Parse\(JsonHelper\.GetNestedProperty\(content, "data", "id"\) \?\? Guid\.Empty\.ToString\(\)\);', '$1var response = await _client.PostAsJsonAsync("/api/unit", unitDto);$1var content = await response.Content.ReadAsStringAsync();$1var unitId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();$1return Guid.Parse(unitId);'
        
        $content = $content -replace '(\s+)var response = await _client\.PostAsJsonAsync\("/api/brand", brandDto\);\s+var result = await response\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+return Guid\.Parse\(JsonHelper\.GetNestedProperty\(content, "data", "id"\) \?\? Guid\.Empty\.ToString\(\)\);', '$1var response = await _client.PostAsJsonAsync("/api/brand", brandDto);$1var content = await response.Content.ReadAsStringAsync();$1var brandId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();$1return Guid.Parse(brandId);'
        
        $content = $content -replace '(\s+)var response = await _client\.PostAsJsonAsync\("/api/useraddress", addressDto\);\s+if \(response\.IsSuccessStatusCode\)\s+\{\s+var result = await response\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+var addressId = JsonHelper\.GetNestedProperty\(content, "data", "id"\);', '$1var response = await _client.PostAsJsonAsync("/api/useraddress", addressDto);$1if (response.IsSuccessStatusCode)$1{$1    var addressContent = await response.Content.ReadAsStringAsync();$1    var addressId = JsonHelper.GetNestedProperty(addressContent, "data", "id");'
        
        $content = $content -replace '(\s+)var response = await _client\.PostAsJsonAsync\("/api/productvariant", variantDto\);\s+if \(response\.IsSuccessStatusCode\)\s+\{\s+var result = await response\.Content\.ReadFromJsonAsync<dynamic>\(\);\s+var variantId = JsonHelper\.GetNestedProperty\(content, "data", "id"\);', '$1var response = await _client.PostAsJsonAsync("/api/productvariant", variantDto);$1if (response.IsSuccessStatusCode)$1{$1    var variantContent = await response.Content.ReadAsStringAsync();$1    var variantId = JsonHelper.GetNestedProperty(variantContent, "data", "id");'
        
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "Fixed: $file"
    }
}

Write-Host "Done!"

