# PowerShell script to bulk assign products to categories using API
# Requires: Admin authentication token

param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$AuthToken = "",
    [string]$CategoryId = "",
    [string[]]$ProductIds = @()
)

# Function to update product category
function Update-ProductCategory {
    param(
        [string]$ProductId,
        [string]$CategoryId,
        [string]$Token
    )
    
    $headers = @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $Token"
    }
    
    # First, get the product to preserve existing data
    try {
        $getResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Product/$ProductId" -Method GET -Headers @{"Accept" = "application/json"}
        
        if ($getResponse.isSuccess -and $getResponse.data) {
            $product = $getResponse.data
            
            # Prepare update payload
            $updatePayload = @{
                id = $product.id
                name = $product.name
                description = $product.description
                price = $product.price
                stockQuantity = $product.stockQuantity
                categoryId = $CategoryId
            } | ConvertTo-Json
            
            # Update product
            $updateResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Product/$ProductId" -Method PUT -Headers $headers -Body $updatePayload
            
            if ($updateResponse.isSuccess) {
                Write-Host "✓ Product $ProductId assigned to category $CategoryId" -ForegroundColor Green
                return $true
            } else {
                Write-Host "✗ Failed to update product $ProductId : $($updateResponse.errorMessage)" -ForegroundColor Red
                return $false
            }
        } else {
            Write-Host "✗ Product $ProductId not found" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "✗ Error updating product $ProductId : $_" -ForegroundColor Red
        return $false
    }
}

# Main execution
if ([string]::IsNullOrEmpty($AuthToken)) {
    Write-Host "Error: AuthToken is required. Please provide your admin authentication token." -ForegroundColor Red
    Write-Host "Usage: .\bulk-assign-products-api.ps1 -AuthToken 'your-token' -CategoryId 'category-id' -ProductIds @('product-id-1', 'product-id-2')" -ForegroundColor Yellow
    exit 1
}

if ([string]::IsNullOrEmpty($CategoryId)) {
    Write-Host "Error: CategoryId is required." -ForegroundColor Red
    exit 1
}

if ($ProductIds.Count -eq 0) {
    Write-Host "Error: At least one ProductId is required." -ForegroundColor Red
    exit 1
}

Write-Host "Starting bulk assignment..." -ForegroundColor Cyan
Write-Host "Category ID: $CategoryId" -ForegroundColor Cyan
Write-Host "Products to update: $($ProductIds.Count)" -ForegroundColor Cyan
Write-Host ""

$successCount = 0
$failCount = 0

foreach ($productId in $ProductIds) {
    if (Update-ProductCategory -ProductId $productId -CategoryId $CategoryId -Token $AuthToken) {
        $successCount++
    } else {
        $failCount++
    }
    Start-Sleep -Milliseconds 100  # Small delay to avoid overwhelming the API
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Success: $successCount" -ForegroundColor Green
Write-Host "  Failed: $failCount" -ForegroundColor Red

