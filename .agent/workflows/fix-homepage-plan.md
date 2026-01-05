---
description: پلن جامع برای اصلاح صفحه اصلی فرانت
---

# پلن اصلاح صفحه اصلی (Homepage Fix Plan)

## مشکلات فعلی

1. **اسلایدر تصاویر**: اسلایدر باید به صورت استاتیک نمایش داده شود (4 تصویر موجود)
2. **محصولات در صفحه اصلی**: محصولات نباید به صورت پیش‌فرض نمایش داده شوند
3. **جستجو**: جستجوی Real-time نباید باشد - فقط با Enter یا کلیک روی دکمه جستجو
4. **دسته‌بندی‌ها**: باید لیست دسته‌بندی‌ها نمایش داده شود
5. **کلیک روی دسته‌بندی**: باید به صفحه جدید برود و محصولات آن دسته را نمایش دهد
6. **تامبنیل محصولات**: باید در لیست محصولات نمایش داده شود

## راه‌حل‌های پیشنهادی

### 1. اسلایدر تصاویر (خطوط 190-243)
**وضعیت فعلی**: اسلایدر به صورت استاتیک در HTML موجود است
**اقدام مورد نیاز**: هیچ تغییری لازم نیست - اسلایدر قبلاً استاتیک است

```html
<!-- اسلایدر از خط 190 تا 243 - بدون تغییر -->
<section class="py-5">
    <div class="swiper default-carousel">
        <div class="swiper-wrapper">
            <div class="swiper-slide">
                <img src="assets/images/slider/slider-2-1.jpg" ...>
            </div>
            <!-- 3 اسلاید دیگر -->
        </div>
    </div>
</section>
```

### 2. حذف نمایش خودکار محصولات در صفحه اصلی

**فایل‌های نیازمند تغییر**:
- `src/WebAPI/wwwroot/fa/assets/js/pages/index-page.js`

**تغییرات**:
```javascript
// خطوط 36-46: کامنت کردن یا حذف
// await loadFeaturedProducts();
// await loadNewProducts();
// await loadBestSellingProducts();
```

### 3. غیرفعال کردن جستجوی Real-time

**فایل**: `src/WebAPI/wwwroot/fa/assets/js/pages/index-page.js`

**تغییرات در تابع setupSearch() (خطوط 265-305)**:
```javascript
function setupSearch() {
    const searchInput = document.getElementById('searchInput');
    const searchButton = searchInput?.nextElementSibling;
    const searchResults = document.getElementById('searchResults');

    if (!searchInput) return;

    // حذف event listener برای input
    // فقط نگه داشتن Enter و Click

    if (searchButton) {
        searchButton.addEventListener('click', async () => {
            const query = searchInput.value.trim();
            if (query) {
                window.location.href = `shop.html?search=${encodeURIComponent(query)}`;
            }
        });
    }

    searchInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            const query = searchInput.value.trim();
            if (query) {
                window.location.href = `shop.html?search=${encodeURIComponent(query)}`;
            }
        }
    });
}
```

### 4. نمایش لیست دسته‌بندی‌ها در صفحه اصلی

**وضعیت فعلی**: تابع `loadCategories()` موجود است اما نیاز به بررسی دارد

**اقدامات**:
1. بررسی وجود container با `data-categories` در HTML
2. اگر وجود ندارد، اضافه کردن یک section جدید برای دسته‌بندی‌ها
3. اطمینان از اینکه دسته‌بندی‌ها به `shop.html?category=ID` لینک می‌شوند

**HTML مورد نیاز** (اضافه کردن بعد از بخش features):
```html
<section class="py-5">
    <div class="container">
        <div class="mb-5">
            <header class="grid place-items-center grid-cols-6 gap-y-10">
                <div class="ps-15 section-heading sm:col-span-4 w-full col-span-6">
                    <h2 class="font-black text-2xl">
                        <span class="dark:text-white">دسته‌بندی‌ها</span>
                    </h2>
                    <p class="text-neutral-600 dark:text-white">محصولات را بر اساس دسته‌بندی مشاهده کنید</p>
                </div>
            </header>
        </div>
        <div class="grid grid-cols-12 gap-4" data-categories>
            <!-- دسته‌بندی‌ها از API لود می‌شوند -->
        </div>
    </div>
</section>
```

### 5. تغییر رفتار Mega Menu

**فایل**: `src/WebAPI/wwwroot/fa/index.html`

**تغییرات در خطوط 2636-2678**:
```javascript
// حذف کد نمایش محصولات در همان صفحه
// تغییر به redirect به shop.html

document.addEventListener('click', async (e) => {
    const categoryLink = e.target.closest('a[data-category-id]');
    if (categoryLink) {
        e.preventDefault();
        const categoryId = categoryLink.getAttribute('data-category-id');
        // Redirect به صفحه shop
        window.location.href = `shop.html?category=${categoryId}`;
    }
});
```

### 6. اطمینان از نمایش تامبنیل در صفحه shop.html

**فایل**: `src/WebAPI/wwwroot/fa/assets/js/pages/shop-page.js`

**بررسی تابع createProductCard**:
- اطمینان از استفاده از `product.productImages[0].imageUrl`
- اگر تصویر وجود ندارد، استفاده از placeholder

## ترتیب اجرا

1. ✅ **مرحله 1**: اسلایدر (بدون تغییر - قبلاً OK است)

2. **مرحله 2**: غیرفعال کردن جستجوی Real-time
   - فایل: `assets/js/pages/index-page.js`
   - تابع: `setupSearch()`

3. **مرحله 3**: حذف نمایش خودکار محصولات
   - فایل: `assets/js/pages/index-page.js`
   - کامنت کردن: `loadFeaturedProducts()`, `loadNewProducts()`, `loadBestSellingProducts()`

4. **مرحله 4**: اضافه کردن section دسته‌بندی‌ها
   - فایل: `index.html`
   - مکان: بعد از بخش features (خط ~383)

5. **مرحله 5**: تغییر رفتار Mega Menu
   - فایل: `index.html`
   - تغییر event handler برای redirect

6. **مرحله 6**: بررسی و تست صفحه shop.html
   - اطمینان از نمایش تامبنیل‌ها
   - اطمینان از فیلتر بر اساس دسته‌بندی

## فایل‌های نیازمند تغییر

1. `src/WebAPI/wwwroot/fa/index.html` (خطوط 2636-2678 و اضافه کردن section دسته‌بندی)
2. `src/WebAPI/wwwroot/fa/assets/js/pages/index-page.js` (setupSearch و حذف loadProducts)
3. بررسی: `src/WebAPI/wwwroot/fa/assets/js/pages/shop-page.js` (تامبنیل‌ها)

## نکات مهم

- **اسلایدر**: قبلاً استاتیک است، نیازی به تغییر ندارد
- **محصولات**: نباید در صفحه اصلی نمایش داده شوند
- **جستجو**: فقط با Enter یا کلیک دکمه
- **دسته‌بندی‌ها**: باید لیست شوند و به shop.html redirect کنند
- **Mega Menu**: نباید محصولات را در همان صفحه نمایش دهد

## تست

بعد از اعمال تغییرات:
1. بررسی اسلایدر (باید 4 تصویر استاتیک باشد) ✅
2. بررسی عدم نمایش محصولات در صفحه اصلی
3. تست جستجو (فقط با Enter/Click)
4. بررسی نمایش دسته‌بندی‌ها
5. کلیک روی دسته‌بندی و redirect به shop.html
6. بررسی نمایش تامبنیل در shop.html
