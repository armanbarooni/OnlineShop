<!-- 84886206-dc95-433f-978f-ddc4a6852be0 c8c1c13a-1903-4868-8614-886fbc0341c8 -->
# پلن بررسی کامل تمام دکمه‌ها و کامپوننت‌های صفحات

## 🎯 هدف
بررسی **خیلی خیلی دقیق** تمام دکمه‌ها، فرم‌ها، لینک‌ها، و کامپوننت‌های تمام صفحات در `presentation` و mapping آن‌ها با API endpoints موجود.

---

## 📋 **صفحه 1: login.html**

### دکمه‌ها و عملکردها:

#### **Tab Buttons:**
1. **دکمه "ورود با رمز عبور"** 
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `switchTab('password')`
   - نیاز به API: ❌

2. **دکمه "ورود با پیامک"**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `switchTab('sms')`
   - نیاز به API: ❌

#### **Form Buttons (Password Login):**
3. **دکمه "ورود"** (Password Form)
   - وضعیت: ✅ متصل به API
   - API: `POST /api/auth/login`
   - عملکرد: ورود با username/password
   - نیاز: ✅ API موجود

4. **Toggle نمایش پسورد**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `togglePassword('password')`
   - نیاز به API: ❌

5. **لینک "فراموشی رمز عبور"**
   - وضعیت: ❌ کار نمی‌کنه
   - API مورد نیاز: `POST /api/auth/forgot-password`
   - نیاز: ⚠️ صفحه forgot-password.html وجود ندارد

#### **Form Buttons (SMS Login):**
6. **دکمه "ارسال کد"**
   - وضعیت: ✅ متصل به API
   - API: `POST /api/auth/send-otp`
   - عملکرد: ارسال OTP
   - نیاز: ✅ API موجود

7. **دکمه "ورود"** (SMS Form)
   - وضعیت: ✅ متصل به API
   - API: `POST /api/auth/verify-otp`
   - عملکرد: تایید OTP
   - نیاز: ✅ API موجود

#### **Navigation Links:**
8. **لینک "ثبت نام کنید"**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: redirect به `register.html`
   - نیاز به API: ❌

---

## 📋 **صفحه 2: register.html**

### دکمه‌ها و عملکردها:

#### **Step Navigation:**
1. **دکمه "مرحله بعد"** (Step 1 → 2)
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `nextStep(1, 2)`
   - نیاز به API: ❌

2. **دکمه "مرحله بعد"** (Step 2 → 3)
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `nextStep(2, 3)`
   - نیاز به API: ❌

3. **دکمه "مرحله قبل"**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `previousStep()`
   - نیاز به API: ❌

#### **Form Buttons:**
4. **دکمه "ثبت نام"**
   - وضعیت: ✅ متصل به API
   - API: `POST /api/auth/register`
   - عملکرد: ثبت نام کاربر
   - نیاز: ✅ API موجود

5. **Toggle نمایش پسورد**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `togglePassword()`
   - نیاز به API: ❌

#### **Navigation Links:**
6. **لینک "وارد شوید"**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: redirect به `login.html`
   - نیاز به API: ❌

---

## 📋 **صفحه 3: user-panel-index.html (Dashboard)**

### دکمه‌ها و عملکردها:

#### **Sidebar Navigation:**
1. **لینک "پیشخوان"**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `user-panel-index.html`
   - نیاز به API: ❌

2. **لینک "پروفایل"**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `user-panel-profile.html`
   - نیاز به API: ❌

3. **لینک "علاقه‌مندی‌ها"**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `user-panel-favorite.html`
   - نیاز به API: ❌

4. **Toggle منوی سفارش‌ها**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `toggleMenu('order-menu','order-arrow')`
   - نیاز به API: ❌

5. **Toggle منوی آدرس‌ها**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: `toggleMenu('address-menu','address-arrow')`
   - نیاز به API: ❌

#### **Dashboard Data Loading:**
6. **Load User Profile**
   - وضعیت: ✅ متصل به API
   - API: `GET /api/auth/me`
   - عملکرد: `window.userProfileService.getUserProfile()`
   - نیاز: ✅ API موجود

7. **Load Recent Orders**
   - وضعیت: ✅ متصل به API
   - API: `GET /api/userorder` (با pagination)
   - عملکرد: `window.orderService.getRecentOrders(5)`
   - نیاز: ✅ API موجود

8. **Load Wishlist Count**
   - وضعیت: ✅ متصل به API
   - API: `GET /api/wishlist`
   - عملکرد: `window.wishlistService.getWishlistCount()`
   - نیاز: ✅ API موجود

9. **Load User Statistics**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `GET /api/userprofile/statistics`
   - عملکرد: `window.userProfileService.getUserStatistics()`
   - نیاز: ❓ باید بررسی شود

10. **Load Notifications**
    - وضعیت: ⚠️ نیاز به بررسی
    - API: `GET /api/notifications`
    - عملکرد: `window.userProfileService.getNotifications()`
    - نیاز: ❌ API موجود نیست

#### **Header Buttons:**
11. **دکمه Notifications**
    - وضعیت: ❌ کار نمی‌کنه
    - API مورد نیاز: `GET /api/notifications`
    - نیاز: ❌ API موجود نیست

12. **دکمه Profile Dropdown**
    - وضعیت: ✅ کار می‌کنه
    - عملکرد: `toggleDropdown('profile-dropdown')`
    - نیاز به API: ❌

13. **دکمه Logout**
    - وضعیت: ⚠️ نیاز به بررسی
    - API: `POST /api/auth/logout`
    - عملکرد: `window.authService.logout()`
    - نیاز: ✅ API موجود

---

## 📋 **صفحه 4: user-panel-profile.html**

### دکمه‌ها و عملکردها:

#### **Profile Form:**
1. **دکمه "ذخیره تغییرات"**
   - وضعیت: ✅ متصل به API
   - API: `PUT /api/userprofile`
   - عملکرد: `window.userProfileService.updateProfile()`
   - نیاز: ✅ API موجود

2. **دکمه "آپلود عکس پروفایل"**
   - وضعیت: ✅ متصل به API
   - API: `POST /api/userprofile/upload-picture`
   - عملکرد: `window.userProfileService.uploadProfilePicture()`
   - نیاز: ⚠️ باید بررسی شود

#### **Password Change Form:**
3. **دکمه "تغییر رمز عبور"**
   - وضعیت: ✅ متصل به API
   - API: `POST /api/auth/change-password`
   - عملکرد: `window.userProfileService.changePassword()`
   - نیاز: ✅ API موجود

#### **Data Pre-filling:**
4. **Pre-fill Profile Data**
   - وضعیت: ✅ متصل به API
   - API: `GET /api/auth/me`
   - عملکرد: `loadUserProfile()` → `populateProfileForm()`
   - نیاز: ✅ API موجود

---

## 📋 **صفحه 5: user-panel-order.html**

### دکمه‌ها و عملکردها:

#### **Search & Filter:**
1. **دکمه "جستجو"**
   - وضعیت: ✅ متصل به API
   - API: `POST /api/userorder/search`
   - عملکرد: `window.orderService.searchOrders()`
   - نیاز: ✅ API موجود

2. **Filter by Status**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `POST /api/userorder/search` (با filter)
   - عملکرد: `filterOrders(status)`
   - نیاز: ✅ API موجود

3. **Filter by Date Range**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `POST /api/userorder/search` (با date filter)
   - عملکرد: `filterByDateRange()`
   - نیاز: ✅ API موجود

#### **Order Actions:**
4. **لینک "مشاهده جزئیات"**
   - وضعیت: ✅ کار می‌کنه
   - عملکرد: redirect به `user-panel-order-detail.html?id={orderId}`
   - نیاز به API: ❌

5. **دکمه "دانلود فاکتور"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `GET /api/userorder/{id}/invoice`
   - عملکرد: `downloadInvoice(orderId)`
   - نیاز: ⚠️ باید بررسی شود

6. **دکمه "پیگیری سفارش"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `GET /api/userorder/{id}/track`
   - عملکرد: `trackOrder(orderId)`
   - نیاز: ⚠️ باید بررسی شود

#### **Pagination:**
7. **دکمه‌های Pagination**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `POST /api/userorder/search` (با page number)
   - عملکرد: `changePage(pageNumber)`
   - نیاز: ✅ API موجود

---

## 📋 **صفحه 6: user-panel-address.html**

### دکمه‌ها و عملکردها:

#### **Address List:**
1. **دکمه "افزودن آدرس جدید"**
   - وضعیت: ⚠️ نیاز به بررسی
   - عملکرد: نمایش modal یا redirect
   - نیاز به API: ❌

2. **دکمه "ویرایش"**
   - وضعیت: ⚠️ نیاز به بررسی
   - عملکرد: redirect به `user-panel-edit-address.html?id={addressId}`
   - نیاز به API: ❌

3. **دکمه "حذف"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `DELETE /api/useraddress/{id}`
   - عملکرد: `deleteAddress(addressId)`
   - نیاز: ✅ API موجود

#### **Address Form:**
4. **دکمه "ذخیره آدرس"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `POST /api/useraddress`
   - عملکرد: `createAddress()`
   - نیاز: ✅ API موجود

#### **Data Loading:**
5. **Load Addresses**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `GET /api/useraddress`
   - عملکرد: `loadAddresses()`
   - نیاز: ✅ API موجود

---

## 📋 **صفحه 7: user-panel-favorite.html**

### دکمه‌ها و عملکردها:

#### **Wishlist Actions:**
1. **دکمه "حذف از علاقه‌مندی‌ها"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `DELETE /api/wishlist/{id}`
   - عملکرد: `removeFromWishlist(wishlistId)`
   - نیاز: ✅ API موجود

2. **دکمه "افزودن به سبد خرید"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `POST /api/cart/items`
   - عملکرد: `addToCart(productId)`
   - نیاز: ✅ API موجود

3. **دکمه "مشاهده محصول"**
   - وضعیت: ⚠️ نیاز به بررسی
   - عملکرد: redirect به `product.html?id={productId}`
   - نیاز به API: ❌

#### **Data Loading:**
4. **Load Wishlist**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `GET /api/wishlist`
   - عملکرد: `loadWishlist()`
   - نیاز: ✅ API موجود

---

## 📋 **صفحه 8: user-panel-wallet.html**

### دکمه‌ها و عملکردها:

#### **Wallet Actions:**
1. **دکمه "افزایش موجودی"**
   - وضعیت: ⚠️ نیاز به بررسی
   - عملکرد: redirect به `user-panel-increase-money.html`
   - نیاز به API: ❌

2. **دکمه "انتقال وجه"**
   - وضعیت: ⚠️ نیاز به بررسی
   - عملکرد: redirect به `user-panel-transfer-money.html`
   - نیاز به API: ❌

3. **دکمه "مشاهده جزئیات تراکنش"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `GET /api/userpayment/{id}`
   - عملکرد: `viewTransactionDetails(paymentId)`
   - نیاز: ✅ API موجود

#### **Data Loading:**
4. **Load Wallet Balance**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `GET /api/userpayment/balance`
   - عملکرد: `loadWalletBalance()`
   - نیاز: ⚠️ باید بررسی شود

5. **Load Transaction History**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `GET /api/userpayment`
   - عملکرد: `loadTransactionHistory()`
   - نیاز: ✅ API موجود

---

## 📋 **صفحه 9: product.html**

### دکمه‌ها و عملکردها:

#### **Product Actions:**
1. **دکمه "افزودن به سبد خرید"**
   - وضعیت: ✅ متصل به API
   - API: `POST /api/cart/items`
   - عملکرد: `addToCart(productId, quantity)`
   - نیاز: ✅ API موجود

2. **دکمه "افزودن به علاقه‌مندی‌ها"**
   - وضعیت: ✅ متصل به API
   - API: `POST /api/wishlist`
   - عملکرد: `addToWishlist(productId)`
   - نیاز: ✅ API موجود

3. **دکمه "مقایسه"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `POST /api/productcomparison`
   - عملکرد: `addToComparison(productId)`
   - نیاز: ✅ API موجود

4. **دکمه "اشتراک‌گذاری"**
   - وضعیت: ⚠️ نیاز به بررسی
   - عملکرد: `shareProduct()`
   - نیاز به API: ❌

#### **Product Reviews:**
5. **دکمه "ثبت نظر"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `POST /api/productreview`
   - عملکرد: `submitReview()`
   - نیاز: ✅ API موجود

6. **دکمه "ویرایش نظر"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `PUT /api/productreview/{id}`
   - عملکرد: `editReview(reviewId)`
   - نیاز: ✅ API موجود

7. **دکمه "حذف نظر"**
   - وضعیت: ⚠️ نیاز به بررسی
   - API: `DELETE /api/productreview/{id}`
   - عملکرد: `deleteReview(reviewId)`
   - نیاز: ✅ API موجود

#### **Product Gallery:**
8. **دکمه‌های Navigation گالری**
   - وضعیت: ⚠️ نیاز به بررسی
   - عملکرد: `changeImage(index)`
   - نیاز به API: ❌

9. **دکمه Zoom**
   - وضعیت: ⚠️ نیاز به بررسی
   - عملکرد: `zoomImage()`
   - نیاز به API: ❌

#### **Product Variants:**
10. **انتخاب رنگ**
    - وضعیت: ⚠️ نیاز به بررسی
    - عملکرد: `selectColor(colorId)`
    - نیاز به API: ❌

11. **انتخاب سایز**
    - وضعیت: ⚠️ نیاز به بررسی
    - عملکرد: `selectSize(sizeId)`
    - نیاز به API: ❌

#### **Data Loading:**
12. **Load Product Details**
    - وضعیت: ✅ متصل به API
    - API: `GET /api/product/{id}`
    - عملکرد: `window.productService.getProductById(productId)`
    - نیاز: ✅ API موجود

13. **Load Related Products**
    - وضعیت: ✅ متصل به API
    - API: `GET /api/product/{id}/related`
    - عملکرد: `window.productService.getRelatedProducts(productId)`
    - نیاز: ✅ API موجود

14. **Load Product Reviews**
    - وضعیت: ⚠️ نیاز به بررسی
    - API: `GET /api/productreview/product/{productId}`
    - عملکرد: `loadProductReviews(productId)`
    - نیاز: ✅ API موجود

---

## 📊 **خلاصه وضعیت**

### ✅ **کامل و کار می‌کنند:**
- login.html - تمام دکمه‌های اصلی
- register.html - تمام دکمه‌های اصلی
- user-panel-index.html - navigation و data loading
- user-panel-profile.html - فرم‌ها و data loading
- product.html - دکمه‌های اصلی (add to cart, wishlist)

### ⚠️ **نیاز به بررسی و تست:**
- user-panel-order.html - search, filter, pagination
- user-panel-address.html - CRUD operations
- user-panel-favorite.html - wishlist operations
- user-panel-wallet.html - wallet operations
- product.html - reviews, gallery, variants

### ❌ **کار نمی‌کنند یا API ندارند:**
- Notifications system - API موجود نیست
- Forgot Password - صفحه موجود نیست
- User Statistics - API باید بررسی شود
- Upload Profile Picture - API باید بررسی شود

---

## 🎯 **اولویت‌بندی رفع مشکلات**

### Priority 1 (بحرانی):
1. بررسی و رفع مشکلات user-panel-order.html
2. بررسی و رفع مشکلات user-panel-address.html
3. بررسی و رفع مشکلات user-panel-favorite.html
4. بررسی و رفع مشکلات product.html (reviews)

### Priority 2 (مهم):
5. بررسی و رفع مشکلات user-panel-wallet.html
6. ایجاد صفحه forgot-password.html
7. ایجاد Notifications API
8. بررسی User Statistics API

### Priority 3 (متوسط):
9. بررسی Upload Profile Picture API
10. بررسی سایر صفحات user-panel
11. تست کامل تمام دکمه‌ها



### To-dos

- [ ] بررسی و رفع مشکلات user-panel-order.html: search, filter, pagination, download invoice
- [ ] بررسی و رفع مشکلات user-panel-address.html: CRUD operations, data loading
- [ ] بررسی و رفع مشکلات user-panel-favorite.html: wishlist operations, add to cart
- [ ] بررسی و رفع مشکلات product.html: reviews, gallery, variants
- [ ] بررسی و رفع مشکلات user-panel-wallet.html: wallet operations, transaction history
- [ ] ایجاد صفحه forgot-password.html و اتصال به API
- [ ] بررسی API های مفقود: Notifications, User Statistics, Upload Profile Picture
- [ ] تست کامل تمام دکمه‌ها و کامپوننت‌ها در تمام صفحات