
$path = "src/WebAPI/wwwroot/fa/product.html"
if (Test-Path $path) {
    $content = Get-Content $path -Raw -Encoding UTF8

    # 1. Inject Script
    if ($content -notmatch "product-details-page.js") {
        $content = $content -replace "</body>", "<script type=""module"" src=""assets/js/modules/product-details-page.js""></script>`n</body>"
    }

    # 2. Add IDs
    $content = $content -replace '<h1 class="font-bold text-xl border-b border-b-gray-300 pb-3" itemprop="name">', '<h1 id="product-title-fa" class="font-bold text-xl border-b border-b-gray-300 pb-3" itemprop="name">'
    $content = $content -replace '<h2 class="text-zinc-500 text-base" itemprop="model">', '<h2 id="product-title-en" class="text-zinc-500 text-base" itemprop="model">'
    
    # Breadcrumb (Specific span in the last item)
    # This is tricky with regex. Let's look for "itemprop="name">\s*گوشی موبایل" pattern
    # Actually, simplistic replace might fail if there are multiple.
    # The last one is usually the product title.
    # We will assume specific context.
    
    # Price
    $content = $content -replace '<span class="text-xl font-bold dark:text-white" itemprop="price"', '<span id="product-current-price" class="text-xl font-bold dark:text-white" itemprop="price"'
    $content = $content -replace '<del class="text-zinc-400" itemprop="priceSpecification"', '<del id="product-old-price" class="text-zinc-400 hidden" itemprop="priceSpecification"' 
    $content = $content -replace 'itemprop="discount">10%</span>', 'id="product-discount-badge" itemprop="discount"></span>'

    # Features
    $content = $content -replace '<ul class="flex flex-col items-start items-baseline flex-wrap space-x-3 space-y-3"', '<ul id="product-features-list" class="flex flex-col items-start items-baseline flex-wrap space-x-3 space-y-3"'

    # Buttons
    $content = $content -replace '<button class="bg-green-600 w-full mt-3 hover:bg-green-700 text-white font-semibold rounded-md px-6 py-3 text-sm">', '<button id="product-add-to-cart-btn" class="bg-green-600 w-full mt-3 hover:bg-green-700 text-white font-semibold rounded-md px-6 py-3 text-sm">'
    
    # Counter
    $content = $content -replace 'onclick="increment\(''count1''\)"', 'id="product-increment-btn"'
    $content = $content -replace 'onclick="decrement\(''count1''\)"', 'id="product-decrement-btn"'
    $content = $content -replace 'id="count1"', 'id="product-count-display"'
    
    # Remove static feature items (optional, but clean)
    # ...

    Set-Content -Path $path -Value $content -Encoding UTF8
    Write-Host "Updated product.html"
}
