<!-- d5ecce94-aac8-4cd1-96f1-8ee6ba279eca 4dc80c94-2681-4df1-b3c8-a4e51675bed1 -->
# پلان رفع باگ‌های پنل کاربری

## 1. رفع باگ‌های user-panel-index.html

### 1.1. مشکلات عنوان و آدرس بار
- **فایل**: `presentation/user-panel-index.html`
- **مشکل**: عنوان صفحه و آدرس بار نیاز به تنظیم دارند
- **راه‌حل**: تغییر `<title>` و `<meta>` tags در بخش `<head>`

### 1.2. نوار جستجو
- **فایل**: `presentation/user-panel-index.html` (خط 486)
- **مشکل**: نوار جستجو کار نمی‌کند
- **راه‌حل**: اضافه کردن event listener برای `searchButton` و `searchInput`

### 1.3. دکمه حذف در sidebar اعلان‌ها
- **فایل**: `presentation/user-panel-index.html` (خط 1255-1329)
- **مشکل**: دکمه حذف در منوی اعلان‌ها کار نمی‌کند
- **راه‌حل**: پیاده‌سازی تابع حذف اعلان

### 1.4. دکمه حذف در sidebar پیام‌ها
- **فایل**: `presentation/user-panel-index.html` (خط 1173-1251)
- **مشکل**: دکمه حذف در منوی پیام‌ها کار نمی‌کند
- **راه‌حل**: پیاده‌سازی تابع حذف پیام

### 1.5. لینک‌های غیرفعال (href="#")
- **فایل**: `presentation/user-panel-index.html`
- **مشکل**: لینک‌های متعدد با `href="#"` کار نمی‌کنند
- **راه‌حل**: 
  - خط 586-593: لینک جستجو
  - خط 538-547: لینک پروفایل در dropdown
  - خط 549-560: لینک تنظیمات در dropdown
  - خط 562-575: لینک خروج در dropdown
  - خط 632: لینک سفارش‌ها
  - خط 650: لینک آدرس‌ها
  - خط 777-778: لینک مشاهده همه سفارش‌ها
  - خط 808, 827, 846: لینک‌های سفارش‌ها
  - خط 863-864: لینک محصولات پیشنهادی

### 1.6. مشکل overlap در آیکون تنظیمات
- **فایل**: `presentation/user-panel-index.html` (خط 550-551)
- **مشکل**: آیکون تنظیمات overlap دارد
- **راه‌حل**: اصلاح CSS positioning

### 1.7. مشکل theme در scrollbar
- **فایل**: `presentation/user-panel-index.html`
- **مشکل**: scrollbar با theme هماهنگ نیست
- **راه‌حل**: اضافه کردن CSS برای scrollbar در dark mode

### 1.8. حذف div های غیرضروری
- **فایل**: `presentation/user-panel-index.html`
- **مشکل**: چند div غیرضروری وجود دارد
- **راه‌حل**: 
  - خط 598-616: div جستجو
  - خط 654-675: div اضافی
  - خط 678-772: div چارت و تراکنش‌ها
  - خط 859-1170: div محصولات پیشنهادی (یا اصلاح)

### 1.9. لینک‌های محصولات (404)
- **فایل**: `presentation/user-panel-index.html` (خط 870-1170)
- **مشکل**: لینک‌های محصولات به `/product/...` می‌روند که 404 می‌دهد
- **راه‌حل**: اصلاح مسیر لینک‌های محصولات

### 1.10. مشکل ID mismatch
- **فایل**: `presentation/user-panel-index.html` (خط 528, 583, 452)
- **مشکل**: ID های تکراری یا نامناسب
- **راه‌حل**: بررسی و اصلاح ID های تکراری

## 2. رفع باگ‌های user-panel-profile.html

### 2.1. باگ kickout (session timeout)
- **فایل**: `presentation/user-panel-profile.html`
- **مشکل**: کاربر بعد از 3 دقیقه و 49 ثانیه از سیستم خارج می‌شود
- **راه‌حل**: بررسی و اصلاح `auth-guard.js` و تنظیمات session timeout

### 2.2. باگ dashboard
- **فایل**: `presentation/user-panel-profile.html` (خط 44-54)
- **مشکل**: مشکل در نمایش اطلاعات dashboard
- **راه‌حل**: بررسی و اصلاح کد JavaScript مربوط به dashboard

### 2.3. خطای unknown در script
- **فایل**: `presentation/user-panel-profile.html` (بخش script)
- **مشکل**: خطای JavaScript در console
- **راه‌حل**: بررسی console errors و رفع آن‌ها

### 2.4. مشکل dropdown پروفایل
- **فایل**: `presentation/user-panel-profile.html` (خط 535-537)
- **مشکل**: dropdown پروفایل درست کار نمی‌کند
- **راه‌حل**: بررسی و اصلاح تابع `toggleUserDropdown`

### 2.5. لینک جستجو
- **فایل**: `presentation/user-panel-profile.html` (خط 587-592)
- **مشکل**: لینک جستجو کار نمی‌کند
- **راه‌حل**: اضافه کردن event listener

### 2.6. حذف div غیرضروری
- **فایل**: `presentation/user-panel-profile.html` (خط 671-716)
- **مشکل**: div غیرضروری وجود دارد
- **راه‌حل**: حذف div

### 2.7. مقادیر hardcoded در فرم
- **فایل**: `presentation/user-panel-profile.html`
- **مشکل**: فیلدهای فرم با مقادیر hardcoded پر شده‌اند
- **راه‌حل**: 
  - خط 629-632: نام
  - خط 633-636: نام خانوادگی
  - خط 638-641: ایمیل
  - خط 643-646: موبایل
  - خط 647-650: تاریخ تولد
  - خط 652-655: کد ملی
- **راه‌حل**: بارگذاری داده‌های واقعی از API

### 2.8. مشکل فیلدهای فرم
- **فایل**: `presentation/user-panel-profile.html`
- **مشکل**: 
  - خط 657-660: فیلد آدرس کار نمی‌کند
  - خط 622-624: فیلد کد پستی کار نمی‌کند
  - خط 616-621: فیلد شهر کار نمی‌کند
- **راه‌حل**: بررسی و اصلاح validation و submit handler

## 3. رفع باگ‌های user-panel-order.html

### 3.1. خطای unknown در script
- **فایل**: `presentation/user-panel-order.html` (بخش script)
- **مشکل**: خطای JavaScript در console
- **راه‌حل**: بررسی console errors و رفع آن‌ها

### 3.2. حذف بخش فیلتر
- **فایل**: `presentation/user-panel-order.html` (خط 580-636)
- **مشکل**: بخش فیلتر غیرضروری است
- **راه‌حل**: حذف div فیلتر

### 3.3. مشکل جدول سفارش‌ها
- **فایل**: `presentation/user-panel-order.html` (خط 642-767)
- **مشکل**: 
  - خط 642: عنوان جدول
  - خط 643: ستون تاریخ
  - خط 644: ستون مبلغ
  - خط 658-767: مشکل در `<tbody>` و نمایش داده‌ها
- **راه‌حل**: بررسی و اصلاح ساختار جدول و JavaScript مربوط به بارگذاری داده‌ها

### 3.4. مشکل pagination
- **فایل**: `presentation/user-panel-order.html` (خط 771-785)
- **مشکل**: pagination کار نمی‌کند
- **راه‌حل**: پیاده‌سازی pagination با JavaScript

### 3.5. حذف بخش‌های غیرضروری
- **فایل**: `presentation/user-panel-order.html`
- **مشکل**: 
  - خط 789-798: بخش Order tracking
  - خط 800-833: بخش Order status guide
- **راه‌حل**: حذف این بخش‌ها

### 3.6. باگ kickout
- **فایل**: `presentation/user-panel-order.html`
- **مشکل**: کاربر بعد از 1 دقیقه و 40 ثانیه از سیستم خارج می‌شود
- **راه‌حل**: بررسی و اصلاح `auth-guard.js`

## 4. رفع باگ‌های user-panel-favorite.html

### 4.1. مشکل لینک جستجو
- **فایل**: `presentation/user-panel-favorite.html` (خط 587-592)
- **مشکل**: لینک جستجو کار نمی‌کند
- **راه‌حل**: اضافه کردن event listener

### 4.2. مشکل لینک سفارش‌ها
- **فایل**: `presentation/user-panel-favorite.html` (خط 593-598)
- **مشکل**: لینک سفارش‌ها کار نمی‌کند
- **راه‌حل**: اصلاح href

### 4.3. مشکل بخش فیلتر و مرتب‌سازی
- **فایل**: `presentation/user-panel-favorite.html` (خط 602-651)
- **مشکل**: بخش فیلتر و مرتب‌سازی کار نمی‌کند
- **راه‌حل**: پیاده‌سازی JavaScript برای فیلتر و مرتب‌سازی

### 4.4. مشکل نمایش محصولات
- **فایل**: `presentation/user-panel-favorite.html`
- **مشکل**: 
  - خط 680-686: محصول 1
  - خط 721-728: محصول 2
  - خط 761-767: محصول 3
  - خط 694, 734, 744: مشکل در badge موجودی
  - خط 690-691, 731, 771: مشکل در نمایش نام محصول
- **راه‌حل**: بارگذاری داده‌های واقعی از API و اصلاح template

### 4.5. مشکل دکمه‌های محصولات
- **فایل**: `presentation/user-panel-favorite.html`
- **مشکل**: دکمه‌های محصولات (افزودن به سبد، حذف از علاقه‌مندی) کار نمی‌کنند
- **راه‌حل**: پیاده‌سازی event handlers

### 4.6. مشکل pagination
- **فایل**: `presentation/user-panel-favorite.html` (خط 798-824)
- **مشکل**: pagination کار نمی‌کند
- **راه‌حل**: پیاده‌سازی pagination با JavaScript

### 4.7. خطای unknown در script
- **فایل**: `presentation/user-panel-favorite.html` (بخش script)
- **مشکل**: خطای JavaScript در console
- **راه‌حل**: بررسی console errors و رفع آن‌ها

### 4.8. مشکل بخش محصولات پیشنهادی
- **فایل**: `presentation/user-panel-favorite.html` (خط 828-1140)
- **مشکل**: 
  - بخش محصولات پیشنهادی نیاز به اصلاح دارد
  - لینک‌ها با `href="#"` کار نمی‌کنند
  - لینک‌های محصولات به `/product/...` می‌روند که 404 می‌دهد
- **راه‌حل**: اصلاح لینک‌ها و بارگذاری داده‌های واقعی

## 5. اقدامات عمومی

### 5.1. بررسی auth-guard.js
- **فایل**: `presentation/assets/js/auth-guard.js`
- **مشکل**: مشکل kickout در چند صفحه
- **راه‌حل**: بررسی و اصلاح session timeout و token refresh logic

### 5.2. همگام‌سازی فایل‌ها
- **فایل**: همه فایل‌های تغییر یافته
- **راه‌حل**: اجرای `scripts/sync-frontend.ps1` برای همگام‌سازی با `wwwroot/fa`

### To-dos

- [x] حذف دکمه‌های شبکه‌های اجتماعی از login.html
- [x] حذف آیکون اضافی در فیلد پسورد login.html
- [x] رفع overlap placeholder و آیکون تلفن در login.html
- [x] رفع مشکل encoding در پیام‌های خطای login.html
- [x] اتصال لینک قوانین و مقررات به terms-and-rules.html
- [x] حذف کد JavaScript تکراری از register.html
- [x] رفع overlap در register.html
- [x] رفع مشکل autofill گوگل در register.html
- [x] اتصال نوار جستجو در user-panel-index.html
- [x] اتصال دکمه‌های حذف notification/message slidebar
- [x] رفع profile dropdown در تمام صفحات user panel
- [x] اتصال تمام لینک‌های href="#" به صفحات مناسب
- [x] جایگزینی مقادیر hardcoded با داده‌های API
- [x] پیاده‌سازی pagination در صفحات order و favorite
- [x] حذف بخش‌های غیرضروری از صفحات user panel
- [x] رفع kickout bug با بهبود token refresh
- [x] رفع ID mismatch با اتصال به API user profile
- [x] بهبود error handling برای جلوگیری از ارورهای ناپدیدشونده
- [x] حذف دکمه‌های شبکه‌های اجتماعی از login.html
- [x] حذف آیکون اضافی در فیلد پسورد login.html
- [x] حذف دکمه‌های شبکه‌های اجتماعی از login.html
- [x] حذف آیکون اضافی در فیلد پسورد login.html
- [x] رفع overlap placeholder و آیکون تلفن در login.html
- [x] رفع مشکل encoding در پیام‌های خطای login.html
- [x] اتصال لینک قوانین و مقررات به terms-and-rules.html
- [x] حذف کد JavaScript تکراری از register.html
- [x] رفع overlap در register.html
- [x] رفع مشکل autofill گوگل در register.html
- [x] اتصال نوار جستجو در user-panel-index.html
- [x] اتصال دکمه‌های حذف notification/message slidebar
- [x] رفع profile dropdown در تمام صفحات user panel
- [x] اتصال تمام لینک‌های href="#" به صفحات مناسب
- [x] جایگزینی مقادیر hardcoded با داده‌های API
- [x] پیاده‌سازی pagination در صفحات order و favorite
- [x] حذف بخش‌های غیرضروری از صفحات user panel
- [x] رفع kickout bug با بهبود token refresh
- [x] رفع ID mismatch با اتصال به API user profile
- [x] بهبود error handling برای جلوگیری از ارورهای ناپدیدشونده
- [ ] تغییر عنوان صفحه و meta tags در user-panel-index.html (خط 1-21)
- [ ] پیاده‌سازی عملکرد نوار جستجو در user-panel-index.html (خط 486)
- [ ] پیاده‌سازی دکمه حذف در sidebar اعلان‌ها در user-panel-index.html (خط 1255-1329)
- [ ] پیاده‌سازی دکمه حذف در sidebar پیام‌ها در user-panel-index.html (خط 1173-1251)
- [ ] اصلاح تمام لینک‌های href="#" در user-panel-index.html (خطوط 586-593, 538-547, 549-560, 562-575, 632, 650, 777-778, 808, 827, 846, 863-864)
- [ ] رفع مشکل overlap در آیکون تنظیمات در user-panel-index.html (خط 550-551)
- [ ] اصلاح theme scrollbar در user-panel-index.html
- [ ] حذف div های غیرضروری در user-panel-index.html (خطوط 598-616, 654-675, 678-772)
- [ ] اصلاح لینک‌های محصولات که 404 می‌دهند در user-panel-index.html (خط 870-1170)
- [ ] رفع مشکل ID mismatch در user-panel-index.html (خط 528, 583, 452)
- [ ] رفع باگ kickout در user-panel-profile.html (بررسی auth-guard.js)
- [ ] رفع باگ dashboard در user-panel-profile.html (خط 44-54)
- [ ] رفع خطای unknown در script بخش user-panel-profile.html
- [ ] رفع مشکل dropdown پروفایل در user-panel-profile.html (خط 535-537)
- [ ] اصلاح لینک جستجو در user-panel-profile.html (خط 587-592)
- [ ] حذف div غیرضروری در user-panel-profile.html (خط 671-716)
- [ ] بارگذاری داده‌های واقعی از API به جای مقادیر hardcoded در user-panel-profile.html (خطوط 629-655)
- [ ] رفع مشکل فیلدهای فرم در user-panel-profile.html (خطوط 616-624, 657-660)
- [ ] رفع خطای unknown در script بخش user-panel-order.html
- [ ] حذف بخش فیلتر در user-panel-order.html (خط 580-636)
- [ ] رفع مشکل جدول سفارش‌ها در user-panel-order.html (خطوط 642-767)
- [ ] پیاده‌سازی pagination در user-panel-order.html (خط 771-785)
- [ ] حذف بخش‌های غیرضروری در user-panel-order.html (خطوط 789-798, 800-833)
- [ ] رفع باگ kickout در user-panel-order.html (بررسی auth-guard.js)
- [ ] اصلاح لینک جستجو در user-panel-favorite.html (خط 587-592)
- [ ] اصلاح لینک سفارش‌ها در user-panel-favorite.html (خط 593-598)
- [ ] پیاده‌سازی بخش فیلتر و مرتب‌سازی در user-panel-favorite.html (خط 602-651)
- [ ] رفع مشکل نمایش محصولات در user-panel-favorite.html (خطوط 680-771)
- [ ] پیاده‌سازی دکمه‌های محصولات در user-panel-favorite.html
- [ ] پیاده‌سازی pagination در user-panel-favorite.html (خط 798-824)
- [ ] رفع خطای unknown در script بخش user-panel-favorite.html
- [ ] رفع مشکل بخش محصولات پیشنهادی در user-panel-favorite.html (خط 828-1140)
- [ ] بررسی و اصلاح auth-guard.js برای رفع مشکل kickout
- [ ] همگام‌سازی فایل‌های تغییر یافته با wwwroot/fa