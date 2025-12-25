
$path = "src\WebAPI\wwwroot\fa\index.html"
$content = Get-Content $path
$newContent = @()

for ($i = 0; $i -lt $content.Count; $i++) {
    $lineNum = $i + 1
    
    # Story CSS
    if ($lineNum -eq 18) { continue }
    
    # Mega Menu Content (153-1516)
    if ($lineNum -ge 153 -and $lineNum -le 1516) {
        if ($lineNum -eq 153) {
            $newContent += '                                <div class="grid grid-cols-12">'
            $newContent += '                                     <div id="mega-menu-list-container" class="col-span-2 h-[400px] overflow-y-scroll border-e border-gray-400">'
            $newContent += '                                     </div>'
            $newContent += '                                     <div class="col-span-10 p-5">'
            $newContent += '                                        <p class="text-gray-500">یک دسته بندی را انتخاب کنید</p>'
            $newContent += '                                     </div>'
            $newContent += '                                </div>'
        }
        continue
    }
    
    # Story HTML (1592-1603)
    if ($lineNum -ge 1592 -and $lineNum -le 1603) { continue }
    
    # Amazing Section (1797-2382)
    if ($lineNum -ge 1797 -and $lineNum -le 2382) {
        if ($lineNum -eq 1797) {
            $newContent += '                    <div class="swiper-wrapper items-center" style="padding-bottom: 0 !important;" id="featured-products-wrapper"></div>'
        }
        continue
    }

    # Product Section (2750-3321)
    if ($lineNum -ge 2750 -and $lineNum -le 3321) {
        if ($lineNum -eq 2750) {
            # Fix indentation
            $newContent += '                <div class="swiper-wrapper items-center" style="padding-bottom: 0 !important;" id="new-arrivals-wrapper"></div>'
        }
        continue
    }

    # Story JS (5711-5820)
    if ($lineNum -ge 5711 -and $lineNum -le 5820) { continue }

    $newContent += $content[$i]
}

# Append Script
$newContent += '<script type="module">'
$newContent += '    import { startHomePage } from "./assets/js/modules/index-page.js";'
$newContent += '    document.addEventListener("DOMContentLoaded", startHomePage);'
$newContent += '</script>'

$newContent | Set-Content $path -Encoding UTF8
Write-Host "Index.html updated successfully."
