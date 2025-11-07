<!-- ce5c4a7d-36f6-4882-9610-4e2d11e2874d dd70d885-843e-4b89-a5fd-8958918e2212 -->
# پلن کامل تکمیل پروژه OnlineShop

## 🎯 هدف کلی

تکمیل کامل پروژه OnlineShop برای آماده‌سازی deployment در production با پوشش کامل فرانت اینتگریشن، تکمیل کنترلرها، امنیت و رفع خطاها

---

## 📋 Phase 1: تکمیل Frontend Integration

### 1.1 بررسی و تکمیل صفحات User Panel

#### 1.1.1 user-panel-order.html

**وضعیت:** ⚠️ نیاز به بررسی کامل
**کارهای لازم:**

- [ ] بررسی و رفع مشکل Search/Filter orders
- [ ] رفع مشکل Pagination
- [ ] پیاده‌سازی Download Invoice
- [ ] بررسی Order Status updates
- [ ] تست کامل نمایش لیست سفارشات
- [ ] رفع مشکل نمایش جزئیات سفارش

**API Endpoints مورد نیاز:**

- `GET /api/userorder` - دریافت لیست سفارشات کاربر
- `GET /api/userorder/{id}` - جزئیات سفارش
- `GET /api/userorder/{id}/invoice` - دانلود فاکتور
- `POST /api/userorder/search` - جستجوی سفارشات

#### 1.1.2 user-panel-address.html

**وضعیت:** ⚠️ نیاز به بررسی کامل
**کارهای لازم:**

- [ ] بررسی CRUD operations (Create, Read, Update, Delete)
- [ ] رفع مشکل Loading addresses
- [ ] بررسی Validation فرم‌ها
- [ ] تست Set as default address
- [ ] بررسی نمایش لیست آدرس‌ها

**API Endpoints مورد نیاز:**

- `GET /api/useraddress` - دریافت لیست آدرس‌ها
- `POST /api/useraddress` - ایجاد آدرس جدید
- `PUT /api/useraddress/{id}` - ویرایش آدرس
- `DELETE /api/useraddress/{id}` - حذف آدرس

#### 1.1.3 user-panel-favorite.html

**وضعیت:** ⚠️ نیاز به بررسی
**کارهای لازم:**

- [ ] بررسی نمایش لیست علاقه‌مندی‌ها
- [ ] تست Add to Cart از wishlist
- [ ] بررسی حذف از wishlist
- [ ] تست Pagination

**API Endpoints مورد نیاز:**

- `GET /api/wishlist` - دریافت لیست wishlist
- `DELETE /api/wishlist/{id}` - حذف از wishlist
- `POST /api/cart/add` - اضافه کردن به سبد خرید

#### 1.1.4 user-panel-wallet.html

**وضعیت:** ⚠️ نیاز به بررسی
**کارهای لازم:**

- [ ] بررسی نمایش موجودی کیف پول
- [ ] بررسی Transaction History
- [ ] تست Increase Money
- [ ] بررسی Transfer Money

**API Endpoints مورد نیاز:**

- بررسی وجود Wallet API
- در صورت نبود: پیاده‌سازی Wallet Controller

#### 1.1.5 سایر صفحات User Panel

**صفحات:**

- [ ] user-panel-ticket.html - سیستم تیکت
- [ ] user-panel-discount.html - کوپن‌ها و تخفیف‌ها
- [ ] user-panel-comment.html - نظرات کاربر
- [ ] user-panel-change-password.html - تغییر رمز عبور
- [ ] user-panel-last-viewed.html - محصولات مشاهده شده

### 1.2 تکمیل صفحات اصلی

#### 1.2.1 product.html

**وضعیت:** ⚠️ نیاز به تکمیل
**کارهای لازم:**

- [ ] بررسی و تکمیل Product Reviews section
- [ ] بررسی Product Gallery (تصاویر متعدد)
- [ ] بررسی Product Variants (رنگ، سایز)
- [ ] تست Add to Cart با variant
- [ ] بررسی Related Products
- [ ] تست Recently Viewed Products

**API Endpoints:**

- `GET /api/product/{id}/reviews` - نظرات محصول
- `POST /api/productreview` - ثبت نظر جدید
- `GET /api/product/{id}/related` - محصولات مرتبط
- `GET /api/productvariant/product/{productId}` - انواع محصول

#### 1.2.2 cart.html

**وضعیت:** ✅ نیاز به بررسی نهایی
**کارهای لازم:**

- [ ] بررسی Update Quantity
- [ ] بررسی Remove Item
- [ ] تست Apply Coupon
- [ ] بررسی Calculate Total
- [ ] تست Proceed to Checkout

#### 1.2.3 checkout.html

**وضعیت:** ⚠️ نیاز به بررسی
**کارهای لازم:**

- [ ] بررسی انتخاب آدرس
- [ ] بررسی انتخاب روش پرداخت
- [ ] تست Apply Coupon
- [ ] بررسی محاسبه هزینه نهایی
- [ ] تست Submit Order

#### 1.2.4 shop.html

**وضعیت:** ⚠️ نیاز به بررسی
**کارهای لازم:**

- [ ] بررسی Product Search
- [ ] بررسی Filtering (Category, Brand, Price)
- [ ] بررسی Sorting
- [ ] تست Pagination
- [ ] بررسی نمایش نتایج

### 1.3 صفحات جدید

#### 1.3.1 forgot-password.html

**وضعیت:** ❌ موجود نیست
**کارهای لازم:**

- [ ] ایجاد صفحه forgot-password.html
- [ ] پیاده‌سازی Send OTP for password reset
- [ ] پیاده‌سازی Verify OTP
- [ ] پیاده‌سازی Reset Password
- [ ] اتصال به API

**API Endpoints مورد نیاز:**

- بررسی وجود `POST /api/auth/forgot-password`
- در صورت نبود: پیاده‌سازی endpoint

---

## 📋 Phase 2: تکمیل و بررسی Controllers

### 2.1 بررسی Controllers موجود

#### 2.1.1 AuthController

**کارهای لازم:**

- [ ] بررسی Forgot Password endpoint
- [ ] بررسی Reset Password endpoint
- [ ] بررسی Change Password endpoint
- [ ] بررسی Logout endpoint (revoke token)
- [ ] بررسی Get Current User endpoint

#### 2.1.2 UserProfileController

**کارهای لازم:**

- [ ] بررسی Upload Profile Picture endpoint
- [ ] بررسی Update Profile endpoint
- [ ] بررسی Get Profile endpoint
- [ ] بررسی User Statistics endpoint

#### 2.1.3 UserOrderController

**کارهای لازم:**

- [ ] بررسی Generate Invoice endpoint
- [ ] بررسی Download Invoice endpoint
- [ ] بررسی Order Search endpoint
- [ ] بررسی Cancel Order endpoint
- [ ] بررسی Return Order endpoint

#### 2.1.4 CartController

**کارهای لازم:**

- [ ] بررسی Update Cart Item endpoint
- [ ] بررسی Remove Cart Item endpoint
- [ ] بررسی Clear Cart endpoint
- [ ] بررسی Apply Coupon endpoint
- [ ] بررسی Remove Coupon endpoint

#### 2.1.5 ProductController

**کارهای لازم:**

- [ ] بررسی Track Product View endpoint
- [ ] بررسی Get Recently Viewed endpoint
- [ ] بررسی Get Related Products endpoint
- [ ] بررسی Get Frequently Bought Together endpoint

#### 2.1.6 ProductReviewController

**کارهای لازم:**

- [ ] بررسی Get Reviews by Product endpoint
- [ ] بررسی Create Review endpoint
- [ ] بررسی Update Review endpoint
- [ ] بررسی Delete Review endpoint

#### 2.1.7 CheckoutController

**کارهای لازم:**

- [ ] بررسی Complete Checkout endpoint
- [ ] بررسی Payment Processing
- [ ] بررسی Order Creation
- [ ] بررسی Inventory Reservation

### 2.2 Controllers جدید

#### 2.2.1 NotificationController (در صورت نیاز)

**کارهای لازم:**

- [ ] بررسی نیاز به Notification system
- [ ] در صورت نیاز: پیاده‌سازی Notification endpoints
- [ ] `GET /api/notification` - دریافت اعلان‌ها
- [ ] `PUT /api/notification/{id}/read` - علامت‌گذاری به عنوان خوانده شده
- [ ] `DELETE /api/notification/{id}` - حذف اعلان

#### 2.2.2 WalletController (در صورت نیاز)

**کارهای لازم:**

- [ ] بررسی نیاز به Wallet system
- [ ] در صورت نیاز: پیاده‌سازی Wallet endpoints
- [ ] `GET /api/wallet/balance` - موجودی کیف پول
- [ ] `GET /api/wallet/transactions` - تاریخچه تراکنش‌ها
- [ ] `POST /api/wallet/increase` - افزایش موجودی
- [ ] `POST /api/wallet/transfer` - انتقال موجودی

---

## 📋 Phase 3: آماده‌سازی Production

### 3.1 امنیت (Security)

#### 3.1.1 Environment Variables

**کارهای لازم:**

- [ ] حذف hardcoded secrets از appsettings.json
- [ ] استفاده از Environment Variables برای:
- Database Connection String
- JWT Secret
- SMS API Keys
- Email Configuration
- [ ] ایجاد appsettings.Production.json
- [ ] مستندسازی Environment Variables

#### 3.1.2 CORS Configuration

**کارهای لازم:**

- [ ] حذف DefaultCors policy که همه origins را allow می‌کند
- [ ] اضافه کردن دامنه‌های production به CORS
- [ ] محدود کردن CORS به دامنه‌های مورد نیاز
- [ ] تست CORS در محیط production-like

#### 3.1.3 HTTPS Configuration

**کارهای لازم:**

- [ ] فعال کردن HTTPS Redirection در production
- [ ] تنظیم `RequireHttpsMetadata = true` در production
- [ ] بررسی SSL Certificate configuration
- [ ] تست HTTPS endpoints

#### 3.1.4 Authentication & Authorization

**کارهای لازم:**

- [ ] بررسی JWT Token expiration
- [ ] بررسی Refresh Token mechanism
- [ ] بررسی Role-based authorization
- [ ] بررسی Token revocation on logout
- [ ] بررسی Password hashing strength

### 3.2 Configuration

#### 3.2.1 Logging

**کارهای لازم:**

- [ ] تنظیم Log Level به Warning/Error در production
- [ ] حذف Debug logs در production
- [ ] تنظیم Logging به فایل یا external service
- [ ] بررسی Log Rotation
- [ ] مستندسازی Log Levels

#### 3.2.2 Database

**کارهای لازم:**

- [ ] بررسی Database Connection Pooling
- [ ] بررسی Database Indexes
- [ ] بررسی Migration scripts
- [ ] تهیه Backup strategy
- [ ] بررسی Database Performance

#### 3.2.3 API Configuration

**کارهای لازم:**

- [ ] تنظیم API Rate Limiting
- [ ] تنظیم Request Timeout
- [ ] بررسی API Versioning
- [ ] تنظیم Compression
- [ ] بررسی Caching Strategy

### 3.3 Deployment

#### 3.3.1 Build Configuration

**کارهای لازم:**

- [ ] بررسی dotnet publish configuration
- [ ] بررسی Build optimization
- [ ] بررسی Bundle size
- [ ] بررسی Static file serving
- [ ] بررسی wwwroot deployment

#### 3.3.2 CI/CD

**کارهای لازم:**

- [ ] بررسی CI/CD pipeline
- [ ] تنظیم Automated tests
- [ ] تنظیم Automated deployment
- [ ] بررسی Rollback strategy
- [ ] مستندسازی Deployment process

#### 3.3.3 Monitoring

**کارهای لازم:**

- [ ] تنظیم Health Check endpoints
- [ ] بررسی Application Insights یا مشابه
- [ ] تنظیم Error Tracking
- [ ] بررسی Performance Monitoring
- [ ] تنظیم Alerting

---

## 📋 Phase 4: رفع خطاها و بهبود کیفیت

### 4.1 Error Handling

#### 4.1.1 Global Error Handler

**کارهای لازم:**

- [ ] بررسی Global Exception Handler
- [ ] بررسی Error Response Format
- [ ] بررسی Error Logging
- [ ] بررسی User-friendly Error Messages
- [ ] بررسی Error Codes

#### 4.1.2 Validation Errors

**کارهای لازم:**

- [ ] بررسی FluentValidation pipeline
- [ ] بررسی Validation Error Messages
- [ ] بررسی Client-side Validation
- [ ] بررسی Server-side Validation
- [ ] بررسی Custom Validators

### 4.2 Frontend Error Handling

#### 4.2.1 JavaScript Error Handling

**کارهای لازم:**

- [ ] بررسی try-catch blocks
- [ ] بررسی Error Messages نمایش داده شده به کاربر
- [ ] بررسی Network Error Handling
- [ ] بررسی Timeout Handling
- [ ] بررسی Loading States

#### 4.2.2 Console Logs

**کارهای لازم:**

- [ ] حذف console.log از production build
- [ ] ایجاد Logger service
- [ ] تنظیم Log Levels
- [ ] بررسی Error Reporting

### 4.3 Code Quality

#### 4.3.1 Cleanup

**کارهای لازم:**

- [ ] حذف فایل‌های Test/Debug
- [ ] حذف کدهای Comment شده
- [ ] حذف Unused imports
- [ ] حذف Unused variables
- [ ] بررسی Code Comments

#### 4.3.2 Optimization

**کارهای لازم:**

- [ ] Minify JavaScript files
- [ ] Minify CSS files
- [ ] Optimize Images
- [ ] بررسی Bundle size
- [ ] بررسی Lazy Loading

### 4.4 Testing & Validation

#### 4.4.1 Manual Testing

**کارهای لازم:**

- [ ] تست تمام صفحات اصلی
- [ ] تست Authentication flows
- [ ] تست Shopping flow
- [ ] تست Payment flow
- [ ] تست User Panel flows

#### 4.4.2 Browser Compatibility

**کارهای لازم:**

- [ ] تست در Chrome
- [ ] تست در Firefox
- [ ] تست در Safari
- [ ] تست در Edge
- [ ] تست Mobile browsers

---

## 📋 Phase 5: مستندسازی

### 5.1 Technical Documentation

**کارهای لازم:**

- [ ] تکمیل API Documentation (Swagger)
- [ ] ایجاد Deployment Guide
- [ ] ایجاد Configuration Guide
- [ ] ایجاد Troubleshooting Guide
- [ ] ایجاد Architecture Documentation

### 5.2 User Documentation

**کارهای لازم:**

- [ ] ایجاد User Manual
- [ ] ایجاد Admin Guide
- [ ] ایجاد FAQ
- [ ] ایجاد Contact Information

### 5.3 Code Documentation

**کارهای لازم:**

- [ ] بررسی XML Comments
- [ ] بررسی Inline Comments
- [ ] بررسی README files
- [ ] بررسی Code Examples

---

## 📊 خلاصه اولویت‌ها

### Priority 1 (Critical - باید قبل از Production)

1. Security fixes (Environment Variables, CORS, HTTPS)
2. Frontend Integration صفحات اصلی (user-panel-order, user-panel-address, checkout)
3. Error Handling و Validation
4. Production Configuration

### Priority 2 (Important - بهتر است رفع شوند)

1. تکمیل صفحات User Panel باقیمانده
2. تکمیل Product page features
3. ایجاد forgot-password page
4. بررسی و تکمیل Controllers

### Priority 3 (Nice to Have)

1. Notification System
2. Wallet System
3. Advanced Features
4. Performance Optimization

---

## ⏱️ تخمین زمان

### Phase 1: Frontend Integration

- **زمان تخمینی:** 5-7 روز

### Phase 2: Controllers

- **زمان تخمینی:** 3-4 روز

### Phase 3: Production Preparation

- **زمان تخمینی:** 3-4 روز

### Phase 4: Error Handling & Quality

- **زمان تخمینی:** 2-3 روز

### Phase 5: Documentation

- **زمان تخمینی:** 1-2 روز

**کل زمان تخمینی:** 14-20 روز کاری

---

## ✅ Checklist نهایی قبل از Deployment

### Security

- [ ] تمام Secrets در Environment Variables
- [ ] CORS فقط برای دامنه‌های production
- [ ] HTTPS فعال و تست شده
- [ ] JWT Configuration صحیح
- [ ] Password Hashing صحیح

### Functionality

- [ ] تمام صفحات اصلی کار می‌کنند
- [ ] Authentication flows کامل
- [ ] Shopping flow کامل
- [ ] Payment flow کامل
- [ ] User Panel کامل

### Quality

- [ ] Error Handling کامل
- [ ] Validation کامل
- [ ] Code Cleanup انجام شده
- [ ] Console Logs حذف شده
- [ ] فایل‌های Test/Debug حذف شده

### Configuration

- [ ] appsettings.Production.json ایجاد شده
- [ ] Environment Variables مستندسازی شده
- [ ] Logging Configuration صحیح
- [ ] Database Configuration صحیح
- [ ] API Configuration صحیح

### Documentation

- [ ] API Documentation کامل
- [ ] Deployment Guide موجود
- [ ] Configuration Guide موجود
- [ ] README کامل

---

**تاریخ ایجاد پلن:** 2025-01-XX
**وضعیت:** آماده برای اجرا

### To-dos

- [ ] بررسی و تکمیل user-panel-order.html: search, filter, pagination, download invoice
- [ ] بررسی و تکمیل user-panel-address.html: CRUD operations, data loading
- [ ] بررسی و تکمیل user-panel-favorite.html: wishlist operations, add to cart
- [ ] تکمیل product.html: reviews, gallery, variants, related products
- [ ] ایجاد forgot-password.html و اتصال به API
- [ ] بررسی و تکمیل AuthController: forgot-password, reset-password, change-password
- [ ] بررسی UserProfileController: upload picture, user statistics
- [ ] بررسی UserOrderController: invoice generation, download, search
- [ ] حذف hardcoded secrets و استفاده از Environment Variables
- [ ] تنظیم CORS فقط برای دامنه‌های production
- [ ] فعال کردن HTTPS و تنظیمات SSL
- [ ] ایجاد appsettings.Production.json و تنظیمات production
- [ ] بهبود Global Error Handler و Error Messages
- [ ] حذف فایل‌های Test/Debug و cleanup کد
- [ ] تکمیل API Documentation و Deployment Guide