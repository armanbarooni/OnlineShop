<!-- d5ecce94-aac8-4cd1-96f1-8ee6ba279eca 4dd55003-ac71-4102-83da-a8f33d9ac64f -->
# پلن رفع باگ‌های User Panel

## بخش 1: user-panel-index.html

### 1.1 Search Bar Functionality

- فایل: `presentation/user-panel-index.html`
- مشکل: search bar با Enter کار نمی‌کند
- راه‌حل: اضافه کردن event listener برای Enter key و search icon click
- کد: افزودن JavaScript handler در بخش script

### 1.2 Profile Dropdown Links

- فایل: `presentation/user-panel-index.html`
- مشکل: لینک‌های profile، settings، logout با `href="#"` کار نمی‌کنند
- راه‌حل: 
- Profile link: تغییر به `user-panel-profile.html`
- Settings link: حذف (طبق گزارش نیازی نیست)
- Logout link: اضافه کردن JavaScript handler برای logout
- خطوط: 539, 550, 563

### 1.3 Settings Icon Overlap

- فایل: `presentation/user-panel-index.html`
- مشکل: ایکون settings اورلپ دارد
- راه‌حل: بررسی و اصلاح CSS/SVG icon
- خطوط: 550-551

### 1.4 View All Links

- فایل: `presentation/user-panel-index.html`
- مشکل: لینک‌های "مشاهده همه" با `href="#"` هستند
- راه‌حل:
- خط 632: تغییر به `user-panel-order.html`
- خط 650: تغییر به `user-panel-favorite.html`

### 1.5 Recent Orders Section

- فایل: `presentation/user-panel-index.html`
- مشکل: لینک‌های "جزئیات" و "مشاهده همه" کار نمی‌کنند
- راه‌حل:
- خط 777-778: لینک به `user-panel-order.html`
- خطوط 808, 827, 846: لینک به `user-panel-order-detail.html?id={orderId}`

### 1.6 Suggested Products Section (OPTIONAL)

- فایل: `presentation/user-panel-index.html`
- مشکل: بخش کامل محصولات پیشنهادی مشکل دارد
- تصمیم: بررسی با کاربر - حذف یا اصلاح؟
- خطوط: 859-1170

## بخش 2: user-panel-profile.html

### 2.1 Dashboard Selection Indicator

- فایل: `presentation/user-panel-profile.html`
- مشکل: Dashboard همیشه selected است
- راه‌حل: اصلاح active state logic با JavaScript
- خطوط: 44-54

### 2.2 Unknown Error Toast

- فایل: `presentation/user-panel-profile.html`
- مشکل: error toast با متن به‌هم‌ریخته ظاهر می‌شود
- راه‌حل: بررسی و اصلاح encoding در error messages
- محل: بخش script

### 2.3 Profile Dropdown

- فایل: `presentation/user-panel-profile.html`
- مشکل: profile dropdown در این صفحه کار نمی‌کند
- راه‌حل: کپی کردن کد کامل از user-panel-index.html
- خطوط: 535-537

### 2.4 Settings Button

- فایل: `presentation/user-panel-profile.html`
- مشکل: دکمه تنظیمات نیازی نیست
- راه‌حل: حذف کامل
- خطوط: 587-592

### 2.5 Account Status Div

- فایل: `presentation/user-panel-profile.html`
- مشکل: div وضعیت حساب نیازی نیست
- راه‌حل: حذف کامل
- خطوط: 671-716

### 2.6 Hardcoded Personal Information

- فایل: `presentation/user-panel-profile.html`
- مشکل: اطلاعات شخصی hardcoded است (مریم محمدی)
- راه‌حل: پاک کردن value ها و load از API با JavaScript
- خطوط: 
- 631-632: firstName
- 635-636: lastName
- 640-641: email
- 643-646: mobile
- 647-650: birthDate
- 652-655: nationalCode

### 2.7 Save Changes Button

- فایل: `presentation/user-panel-profile.html`
- مشکل: دکمه ذخیره تغییرات کار نمی‌کند
- راه‌حل: اضافه کردن event handler برای form submit
- خطوط: 657-660

### 2.8 Change Profile Picture

- فایل: `presentation/user-panel-profile.html`
- مشکل: دکمه تغییر تصویر کار نمی‌کند
- راه‌حل: اضافه کردن file input و upload handler
- خطوط: 616-624

## بخش 3: user-panel-order.html

### 3.1 Unknown Error Toast

- فایل: `presentation/user-panel-order.html`
- مشکل: همان error toast با متن به‌هم‌ریخته
- راه‌حل: اصلاح encoding
- محل: بخش script

### 3.2 Filter Section

- فایل: `presentation/user-panel-order.html`
- مشکل: بخش filter نیازی نیست
- راه‌حل: حذف کامل
- خطوط: 580-636

### 3.3 Order Status Filter Buttons

- فایل: `presentation/user-panel-order.html`
- مشکل: دکمه‌های فیلتر (همه، درحال پردازش، ارسال شده) کار نمی‌کنند
- راه‌حل: اضافه کردن JavaScript handler برای filtering
- خطوط: 642-644

### 3.4 Orders Table Loading

- فایل: `presentation/user-panel-order.html`
- مشکل: جدول سفارشات load نمی‌شود
- راه‌حل: اضافه کردن JavaScript برای load از API
- خطوط: 658-767

### 3.5 Pagination

- فایل: `presentation/user-panel-order.html`
- مشکل: pagination کار نمی‌کند
- راه‌حل: پیاده‌سازی JavaScript pagination handler
- خطوط: 771-785

### 3.6 Order Tracking Section

- فایل: `presentation/user-panel-order.html`
- مشکل: نیازی نیست
- راه‌حل: حذف کامل
- خطوط: 789-798

### 3.7 Order Status Guide

- فایل: `presentation/user-panel-order.html`
- مشکل: نیازی نیست
- راه‌حل: حذف کامل
- خطوط: 800-833

## بخش 4: user-panel-favorite.html

### 4.1 Delete Selected Button

- فایل: `presentation/user-panel-favorite.html`
- مشکل: دکمه حذف انتخاب‌شده‌ها کار نمی‌کند
- راه‌حل: اضافه کردن JavaScript handler برای bulk delete
- خطوط: 587-592

### 4.2 Add All to Cart Button

- فایل: `presentation/user-panel-favorite.html`
- مشکل: دکمه افزودن همه به سبد خرید کار نمی‌کند
- راه‌حل: اضافه کردن JavaScript handler برای bulk add to cart
- خطوط: 593-598

### 4.3 Filters and Sorting Section

- فایل: `presentation/user-panel-favorite.html`
- مشکل: بخش فیلتر نیازی نیست
- راه‌حل: حذف کامل
- خطوط: 602-651

### 4.4 Product Links

- فایل: `presentation/user-panel-favorite.html`
- مشکل: عکس و نام محصولات لینک نیستند
- راه‌حل: اضافه کردن link wrapper به صفحه محصول
- خطوط: 680-686, 721-728, 761-767

### 4.5 Product Status and Prices

- فایل: `presentation/user-panel-favorite.html`
- مشکل: وضعیت و قیمت محصولات باید از API load شوند
- راه‌حل: پیاده‌سازی dynamic loading
- خطوط: 694, 734, 744 (status) و 690-691, 731, 771 (prices)

### 4.6 Product Action Buttons

- فایل: `presentation/user-panel-favorite.html`
- مشکل: دکمه‌های اضافه به سبد و علاقه‌مندی کار نمی‌کنند
- راه‌حل: اضافه کردن event handlers
- محل: Product 1, 2, 3 sections

### 4.7 Pagination

- فایل: `presentation/user-panel-favorite.html`
- مشکل: pagination کار نمی‌کند
- راه‌حل: پیاده‌سازی JavaScript pagination
- خطوط: 798-824

### 4.8 Unknown Error Toast

- فایل: `presentation/user-panel-favorite.html`
- مشکل: همان error toast با متن به‌هم‌ریخته
- راه‌حل: اصلاح encoding
- محل: بخش script

### 4.9 Suggested Products Section (OPTIONAL)

- فایل: `presentation/user-panel-favorite.html`
- مشکل: بخش محصولات پیشنهادی نیازی نیست
- راه‌حل: حذف کامل یا بررسی با کاربر
- خطوط: 828-1140

## بخش 5: باگ‌های عمومی

### 5.1 Kickout Bug (Token Refresh)

- فایل‌ها: `presentation/assets/js/auth-service.js`, `auth-guard.js`
- مشکل: بعد از 1-2 ساعت کاربر kick out می‌شود
- وضعیت: احتمالا حل شده (token refresh پیاده‌سازی شد)
- راه‌حل: بررسی و تست

### 5.2 ID Mismatch (Hardcoded User Data)

- فایل‌ها: تمام صفحات user panel
- مشکل: همه به عنوان "مریم محمدی" وارد می‌شوند
- وضعیت: احتمالا حل شده (اتصال به API user profile)
- راه‌حل: بررسی و تست

### 5.3 Unknown Error Encoding

- فایل‌ها: تمام صفحات user panel
- مشکل: متن errorها به‌هم‌ریخته است
- راه‌حل: اصلاح encoding در تمام error messages (UTF-8)

### 5.4 Scrollbar Theme

- فایل: `presentation/assets/css/app.css`
- مشکل: scrollbar با theme سازگار نیست
- راه‌حل: اضافه کردن custom scrollbar styles

## نکات پیاده‌سازی

1. همه تغییرات باید در پوشه `presentation` انجام شوند
2. بعد از هر تغییر، فایل‌ها به `src/WebAPI/wwwroot/fa` کپی می‌شوند
3. هر task باید بعد از تکمیل تیک بخورد
4. برای بخش‌های OPTIONAL، قبل از حذف/اصلاح با کاربر هماهنگ شود
5. تست کامل هر بخش قبل از رفتن به بخش بعدی

## ترتیب اولویت

1. حذف بخش‌های غیرضروری (سریع و آسان)
2. رفع لینک‌های `href="#"` (سریع)
3. حذف hardcoded values (متوسط)
4. پیاده‌سازی functionality ها (طولانی)
5. رفع مشکلات UI/UX (پایین‌ترین اولویت)

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
- [ ] بهبود responsive در تمام صفحات
- [ ] تست نهایی تمام رفع‌ها
- [ ] حذف دکمه‌های شبکه‌های اجتماعی از login.html
- [ ] حذف آیکون اضافی در فیلد پسورد login.html
- [ ] رفع overlap placeholder و آیکون تلفن در login.html
- [ ] رفع مشکل encoding در پیام‌های خطای login.html
- [ ] اتصال لینک قوانین و مقررات به terms-and-rules.html
- [ ] حذف کد JavaScript تکراری از register.html
- [ ] رفع overlap در register.html
- [ ] رفع مشکل autofill گوگل در register.html
- [ ] اتصال نوار جستجو در user-panel-index.html
- [ ] اتصال دکمه‌های حذف notification/message slidebar
- [ ] رفع profile dropdown در تمام صفحات user panel
- [ ] اتصال تمام لینک‌های href="#" به صفحات مناسب
- [ ] جایگزینی مقادیر hardcoded با داده‌های API
- [ ] پیاده‌سازی pagination در صفحات order و favorite
- [ ] حذف بخش‌های غیرضروری از صفحات user panel
- [ ] رفع kickout bug با بهبود token refresh
- [ ] رفع ID mismatch با اتصال به API user profile
- [ ] بهبود error handling برای جلوگیری از ارورهای ناپدیدشونده
- [ ] بهبود responsive در تمام صفحات
- [ ] تست نهایی تمام رفع‌ها
- [ ] حذف دکمه‌های شبکه‌های اجتماعی از login.html
- [ ] حذف آیکون اضافی در فیلد پسورد login.html
- [ ] رفع overlap placeholder و آیکون تلفن در login.html
- [ ] رفع مشکل encoding در پیام‌های خطای login.html
- [ ] اتصال لینک قوانین و مقررات به terms-and-rules.html
- [ ] حذف کد JavaScript تکراری از register.html
- [ ] رفع overlap در register.html
- [ ] رفع مشکل autofill گوگل در register.html
- [ ] اتصال نوار جستجو در user-panel-index.html
- [ ] اتصال دکمه‌های حذف notification/message slidebar
- [ ] رفع profile dropdown در تمام صفحات user panel
- [ ] اتصال تمام لینک‌های href="#" به صفحات مناسب
- [ ] جایگزینی مقادیر hardcoded با داده‌های API
- [ ] پیاده‌سازی pagination در صفحات order و favorite
- [ ] حذف بخش‌های غیرضروری از صفحات user panel
- [ ] رفع kickout bug با بهبود token refresh
- [ ] رفع ID mismatch با اتصال به API user profile
- [ ] بهبود error handling برای جلوگیری از ارورهای ناپدیدشونده
- [ ] اضافه کردن search functionality با Enter key
- [ ] رفع لینک‌های profile dropdown
- [ ] رفع لینک‌های مشاهده همه
- [ ] رفع لینک‌های بخش سفارشات اخیر
- [ ] رفع overlap ایکون settings
- [ ] اصلاح active state indicator در dashboard
- [ ] رفع encoding error messages
- [ ] رفع profile dropdown
- [ ] حذف دکمه تنظیمات
- [ ] حذف div وضعیت حساب
- [ ] حذف اطلاعات hardcoded و load از API
- [ ] پیاده‌سازی دکمه ذخیره تغییرات
- [ ] پیاده‌سازی upload عکس پروفایل
- [ ] حذف بخش filter
- [ ] پیاده‌سازی دکمه‌های فیلتر وضعیت
- [ ] پیاده‌سازی loading جدول سفارشات از API
- [ ] پیاده‌سازی pagination
- [ ] حذف بخش order tracking
- [ ] حذف بخش order status guide
- [ ] پیاده‌سازی حذف انتخاب‌شده‌ها
- [ ] پیاده‌سازی افزودن همه به سبد
- [ ] حذف بخش filters
- [ ] اضافه کردن لینک به محصولات
- [ ] پیاده‌سازی dynamic loading وضعیت و قیمت
- [ ] پیاده‌سازی دکمه‌های action محصولات
- [ ] پیاده‌سازی pagination
- [ ] بررسی و تست kickout bug
- [ ] بررسی و تست ID mismatch
- [ ] رفع encoding در تمام error messages
- [ ] اصلاح theme scrollbar