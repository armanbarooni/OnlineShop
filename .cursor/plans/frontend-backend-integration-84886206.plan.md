<!-- 84886206-dc95-433f-978f-ddc4a6852be0 b593cd85-6a7d-42db-8c92-ab3a7e22bb3b -->
# پلن بررسی کامل Frontend و Backend Integration

## 🎯 هدف

بررسی تک‌تک صفحات presentation، شناسایی تمام دکمه‌ها، فرم‌ها، و کامپوننت‌ها، و mapping آن‌ها با API endpoints موجود

## 📊 مرحله 1: شناسایی Backend API Endpoints

### Controllers موجود:

1. **AuthController** - احراز هویت
2. **ProductController** - محصولات
3. **ProductCategoryController** - دسته‌بندی محصولات
4. **CartController** - سبد خرید
5. **CheckoutController** - تسویه حساب
6. **UserProfileController** - پروفایل کاربر
7. **UserAddressController** - آدرس‌های کاربر
8. **UserOrderController** - سفارشات کاربر
9. **UserPaymentController** - پرداخت‌های کاربر
10. **WishlistController** - لیست علاقه‌مندی‌ها
11. **ProductReviewController** - نظرات محصولات
12. **CouponController** - کوپن‌های تخفیف
13. **ProductComparisonController** - مقایسه محصولات
14. **StockAlertController** - هشدار موجودی
15. **UserReturnRequestController** - درخواست مرجوعی

### Auth Endpoints:

- `POST /api/auth/login` - ورود با ایمیل/پسورد
- `POST /api/auth/register` - ثبت‌نام
- `POST /api/auth/send-otp` - ارسال کد OTP
- `POST /api/auth/verify-otp` - تایید کد OTP
- `POST /api/auth/register-phone` - ثبت‌نام با موبایل
- `POST /api/auth/login-phone` - ورود با موبایل
- `POST /api/auth/refresh` - تازه‌سازی توکن
- `POST /api/auth/logout` - خروج
- `GET /api/auth/me` - اطلاعات کاربر جاری

## 📋 مرحله 2: بررسی صفحات Authentication

### 2.1. login.html

**وضعیت**: ✅ تکمیل شده
**دکمه‌ها و عملکردها**:

- ✅ دکمه ورود با پسورد → `POST /api/auth/login`
- ✅ دکمه ورود با SMS → `POST /api/auth/send-otp` + `POST /api/auth/verify-otp`
- ✅ لینک ثبت‌نام → `register.html`
- ✅ لینک فراموشی رمز عبور
- ✅ Toggle نمایش پسورد

**نیازمندی‌ها**:

- ✅ اتصال به API
- ✅ مدیریت توکن
- ✅ Redirect به dashboard

### 2.2. register.html

**وضعیت**: ⚠️ نیاز به بررسی
**دکمه‌ها و عملکردها**:

- ✅ دکمه مرحله بعد (Step 1 → 2)
- ✅ دکمه مرحله بعد (Step 2 → 3)
- ✅ دکمه مرحله قبل
- ⚠️ دکمه ثبت‌نام → `POST /api/auth/register`
- ✅ لینک ورود → `login.html`
- ✅ Toggle نمایش پسورد
- ✅ Password strength indicator

**نیازمندی‌ها**:

- ⚠️ بررسی validation
- ⚠️ بررسی API call
- ⚠️ بررسی error handling

## 📋 مرحله 3: بررسی صفحات User Panel

### 3.1. user-panel-index.html (Dashboard)

**دکمه‌ها و عملکردها**:

- 🔍 دکمه خروج → `POST /api/auth/logout`
- 🔍 نمایش اطلاعات کاربر → `GET /api/auth/me`
- 🔍 نمایش سفارشات اخیر → `GET /api/userorder`
- 🔍 نمایش محصولات مشاهده شده
- 🔍 لینک‌های منو

**API مورد نیاز**:

- `GET /api/auth/me`
- `GET /api/userorder`
- `GET /api/userprofile`

### 3.2. user-panel-profile.html

**دکمه‌ها و عملکردها**:

- 🔍 دکمه ذخیره اطلاعات → `PUT /api/userprofile`
- 🔍 آپلود عکس پروفایل
- 🔍 فرم ویرایش اطلاعات شخصی

**API مورد نیاز**:

- `GET /api/userprofile/{id}`
- `PUT /api/userprofile/{id}`

### 3.3. user-panel-address.html

**دکمه‌ها و عملکردها**:

- 🔍 دکمه افزودن آدرس → `POST /api/useraddress`
- 🔍 دکمه ویرایش آدرس → `PUT /api/useraddress/{id}`
- 🔍 دکمه حذف آدرس → `DELETE /api/useraddress/{id}`
- 🔍 نمایش لیست آدرس‌ها → `GET /api/useraddress`

**API مورد نیاز**:

- `GET /api/useraddress`
- `POST /api/useraddress`
- `PUT /api/useraddress/{id}`
- `DELETE /api/useraddress/{id}`

### 3.4. user-panel-order.html

**دکمه‌ها و عملکردها**:

- 🔍 نمایش لیست سفارشات → `GET /api/userorder`
- 🔍 فیلتر سفارشات
- 🔍 جستجوی سفارش
- 🔍 لینک جزئیات سفارش

**API مورد نیاز**:

- `GET /api/userorder`
- `GET /api/userorder/{id}`

### 3.5. user-panel-order-detail.html

**دکمه‌ها و عملکردها**:

- 🔍 نمایش جزئیات سفارش → `GET /api/userorder/{id}`
- 🔍 دکمه مرجوعی کالا
- 🔍 دکمه لغو سفارش
- 🔍 دکمه پیگیری سفارش

**API مورد نیاز**:

- `GET /api/userorder/{id}`
- `POST /api/userreturnrequest`
- `PUT /api/userorder/{id}`

### 3.6. user-panel-favorite.html

**دکمه‌ها و عملکردها**:

- 🔍 نمایش لیست علاقه‌مندی‌ها → `GET /api/wishlist`
- 🔍 دکمه حذف از علاقه‌مندی‌ها → `DELETE /api/wishlist/{id}`
- 🔍 دکمه افزودن به سبد خرید

**API مورد نیاز**:

- `GET /api/wishlist`
- `DELETE /api/wishlist/{id}`
- `POST /api/cart`

### 3.7. user-panel-wallet.html

**دکمه‌ها و عملکردها**:

- 🔍 نمایش موجودی کیف پول
- 🔍 دکمه افزایش موجودی
- 🔍 دکمه انتقال وجه
- 🔍 تاریخچه تراکنش‌ها

**API مورد نیاز**:

- `GET /api/userpayment`
- `POST /api/userpayment`

### 3.8. user-panel-change-password.html

**دکمه‌ها و عملکردها**:

- 🔍 فرم تغییر رمز عبور
- 🔍 دکمه ذخیره
- 🔍 Toggle نمایش پسورد

**API مورد نیاز**:

- `PUT /api/userprofile/change-password`

### 3.9. user-panel-comment.html

**دکمه‌ها و عملکردها**:

- 🔍 نمایش لیست نظرات → `GET /api/productreview`
- 🔍 دکمه ویرایش نظر
- 🔍 دکمه حذف نظر

**API مورد نیاز**:

- `GET /api/productreview`
- `PUT /api/productreview/{id}`
- `DELETE /api/productreview/{id}`

### 3.10. user-panel-discount.html

**دکمه‌ها و عملکردها**:

- 🔍 نمایش کوپن‌های تخفیف → `GET /api/coupon`
- 🔍 دکمه استفاده از کوپن

**API مورد نیاز**:

- `GET /api/coupon`
- `POST /api/coupon/apply`

### 3.11. user-panel-ticket.html

**دکمه‌ها و عملکردها**:

- 🔍 نمایش لیست تیکت‌ها
- 🔍 دکمه ایجاد تیکت جدید
- 🔍 لینک مشاهده تیکت

**API مورد نیاز**:

- ❌ API تیکت وجود ندارد (نیاز به ایجاد)

## 📋 مرحله 4: بررسی صفحات محصولات

### 4.1. product.html

**دکمه‌ها و عملکردها**:

- 🔍 نمایش جزئیات محصول → `GET /api/product/{id}`
- 🔍 دکمه افزودن به سبد → `POST /api/cart/items`
- 🔍 دکمه افزودن به علاقه‌مندی‌ها → `POST /api/wishlist`
- 🔍 دکمه مقایسه → `POST /api/productcomparison`
- 🔍 فرم ثبت نظر → `POST /api/productreview`
- 🔍 نمایش نظرات → `GET /api/productreview/product/{productId}`
- 🔍 گالری تصاویر
- 🔍 انتخاب رنگ/سایز
- 🔍 محصولات مرتبط → `GET /api/product/{id}/related`
- 🔍 محصولات خریداری شده با هم → `GET /api/product/{id}/frequently-bought-together`

**API مورد نیاز**:

- ✅ `GET /api/product/{id}` - موجود
- ✅ `GET /api/productreview/product/{productId}` - موجود
- ✅ `POST /api/cart/items` - موجود
- ✅ `POST /api/wishlist` - موجود
- ✅ `POST /api/productcomparison` - موجود
- ✅ `POST /api/productreview` - موجود
- ✅ `GET /api/product/{id}/related` - موجود
- ✅ `GET /api/product/{id}/frequently-bought-together` - موجود

## 📋 مرحله 5: Components مشترک

### 5.1. Header/Navigation

**عملکردها**:

- 🔍 جستجوی محصولات
- 🔍 منوی دسته‌بندی
- 🔍 سبد خرید
- 🔍 لینک پروفایل
- 🔍 دکمه خروج

### 5.2. Sidebar (User Panel)

**عملکردها**:

- 🔍 منوی کاربری
- 🔍 نمایش نام کاربر
- 🔍 لینک‌های صفحات

## 🔧 مرحله 6: اقدامات مورد نیاز

### 6.1. صفحات نیازمند API Integration

1. **user-panel-index.html** - Dashboard
2. **user-panel-profile.html** - پروفایل
3. **user-panel-address.html** - آدرس‌ها
4. **user-panel-order.html** - سفارشات
5. **user-panel-favorite.html** - علاقه‌مندی‌ها
6. **user-panel-wallet.html** - کیف پول
7. **user-panel-comment.html** - نظرات
8. **product.html** - جزئیات محصول

### 6.2. API های موجود ولی استفاده نشده

- ProductController endpoints
- CartController endpoints
- CheckoutController endpoints
- CouponController endpoints
- ProductComparisonController endpoints

### 6.3. API های مورد نیاز ولی موجود نیست

- TicketController (تیکت پشتیبانی)
- NotificationController (اعلان‌ها)

## 📊 خلاصه وضعیت

### ✅ کامل شده:

- login.html
- register.html (با مشکلات جزئی)

### ⚠️ نیاز به کار:

- تمام صفحات user-panel
- product.html
- Components مشترک

### ❌ API موجود نیست:

- Ticket system
- Notifications

## 🎯 اولویت‌بندی

### Priority 1 (بحرانی):

1. user-panel-index.html - Dashboard
2. user-panel-profile.html
3. user-panel-order.html
4. product.html

### Priority 2 (مهم):

5. user-panel-address.html
6. user-panel-favorite.html
7. user-panel-wallet.html

### Priority 3 (متوسط):

8. user-panel-comment.html
9. user-panel-discount.html
10. سایر صفحات

### To-dos

- [ ] رفع مشکلات api-client.js: اضافه کردن setTokens() و handleError()
- [ ] تصحیح auth-service.js: رفع مشکل response structure
- [ ] رفع مشکلات login.html: اضافه کردن config.js و تصحیح لینک‌ها
- [ ] رفع مشکلات register.html: تصحیح لینک‌ها و redirect paths
- [ ] تصحیح auth-guard.js: رفع مشکل redirect paths
- [ ] تست کامل integration: login, register, navigation, و auth flow