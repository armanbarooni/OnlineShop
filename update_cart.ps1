
$path = "src/WebAPI/wwwroot/fa/cart.html"
if (Test-Path $path) {
    $content = Get-Content $path -Raw -Encoding UTF8

    # 1. Inject Script
    if ($content -notmatch "cart-page.js") {
        $content = $content -replace "</body>", "<script type=""module"" src=""assets/js/modules/cart-page.js""></script>`n</body>"
    }

    # 2. Add Container ID and clear static items
    # We look for the specific ul
    if ($content -match '<ul class="space-y-4">') {
        # We want to replace the whole UL content with empty one or just add ID and let JS clear it?
        # JS clearing is easier if I just add ID.
        # But for aesthetics before JS loads, empty is better if I want "Skeleton" or just empty.
        # Let's just add the ID and EMPTY it to avoid flash of wrong content.
        
        # Regex to match ul and its content is hard.
        # I'll just add the ID and let JS overwrite it (innerHTML = ...). 
        # But I'll delete the static items roughly by replacing the start tag.
        
        $content = $content -replace '<ul class="space-y-4">', '<ul id="cart-items-container" class="space-y-4">'
    }

    # 3. Add Summary IDs
    # Strategy: Find valid anchor points.
    $content = $content -replace '<h6 class="font-bold">قیمت کالاها</h6>\r?\n\s*<p>1,220,000 تومان</p>', '<h6 class="font-bold">قیمت کالاها</h6><p id="cart-subtotal">0 تومان</p>'
    $content = $content -replace '<h6 class="font-bold">تخفیف کالاها</h6>\r?\n\s*<p class="text-primary font-bold">1,220,000 تومان</p>', '<h6 class="font-bold">تخفیف کالاها</h6><p id="cart-discount" class="text-primary font-bold">0 تومان</p>'
    $content = $content -replace '<h6 class="font-bold">مجموع</h6>\r?\n\s*<p>3,220,000 تومان</p>', '<h6 class="font-bold">مجموع</h6><p id="cart-total">0 تومان</p>'

    # 4. Checkout Button Link
    $content = $content -replace 'href="" class="p-2 bg-primary-grad w-full block text-center rounded-lg text-white">تسویه حساب</a>', 'href="checkout.html" class="p-2 bg-primary-grad w-full block text-center rounded-lg text-white">تسویه حساب</a>'

    Set-Content -Path $path -Value $content -Encoding UTF8
    Write-Host "Updated cart.html"
}
