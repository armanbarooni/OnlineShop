# مرجع کامل Entities و Features - OnlineShop

این سند شامل جزئیات کامل تمام Entities، Features، DTOs، Validators، و Endpoints سیستم است.

---

## بخش 1: لیست کامل Entities با جزئیات

### 1. ApplicationUser
**مسیر:** `src/Domain/Entities/ApplicationUser.cs`  
**ارث‌بری:** `IdentityUser<Guid>`  
**توضیح:** کاربر سیستم که از Identity Framework استفاده می‌کند

**Properties:**
- `Id` (Guid): شناسه منحصر به فرد
- `UserName` (string): نام کاربری
- `Email` (string): ایمیل
- `PhoneNumber` (string): شماره تلفن
- `FirstName` (string): نام
- `LastName` (string): نام خانوادگی
- `EmailConfirmed` (bool): تایید ایمیل
- `PhoneNumberConfirmed` (bool): تایید شماره تلفن
- + Properties از IdentityUser

**Navigation Properties:**
- `UserProfile` (1:1): پروفایل کاربر
- `UserAddresses` (1:N): آدرس‌ها
- `UserOrders` (1:N): سفارشات
- `Carts` (1:N): سبدهای خرید
- `Wishlists` (1:N): لیست علاقه‌مندی‌ها
- `ProductReviews` (1:N): نظرات
- `UserReturnRequests` (1:N): درخواست‌های مرجوعی

**Related Features:** Auth, UserProfile

---

### 2. UserProfile
**مسیر:** `src/Domain/Entities/UserProfile.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** پروفایل کامل کاربر با اطلاعات تکمیلی

**Properties:**
- `UserId` (Guid): شناسه کاربر
- `FirstName` (string): نام
- `LastName` (string): نام خانوادگی
- `PhoneNumber` (string?): تلفن
- `BirthDate` (DateTime?): تاریخ تولد
- `Gender` (string?): جنسیت
- `AvatarUrl` (string?): لینک تصویر
- `Bio` (string?): بیوگرافی
- `IsEmailVerified` (bool): وضعیت تایید ایمیل
- `IsPhoneVerified` (bool): وضعیت تایید تلفن
- `EmailVerifiedAt` (DateTime?): زمان تایید ایمیل
- `PhoneVerifiedAt` (DateTime?): زمان تایید تلفن
- + Properties از BaseEntity

**Methods:**
- `Create(userId, firstName, lastName)`: ایجاد پروفایل
- `Update(...)`: ویرایش اطلاعات
- `VerifyEmail()`: تایید ایمیل
- `VerifyPhone()`: تایید تلفن

**Related Features:** UserProfile

---

### 3. UserAddress
**مسیر:** `src/Domain/Entities/UserAddress.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** آدرس‌های کاربر برای ارسال و صورتحساب

**Properties:**
- `UserId` (Guid): شناسه کاربر
- `Title` (string): عنوان (منزل، محل کار)
- `FirstName` (string): نام گیرنده
- `LastName` (string): نام خانوادگی گیرنده
- `AddressLine1` (string): خط آدرس 1
- `AddressLine2` (string?): خط آدرس 2
- `City` (string): شهر
- `State` (string): استان
- `PostalCode` (string): کد پستی
- `Country` (string): کشور
- `PhoneNumber` (string): تلفن تماس
- `IsDefault` (bool): آدرس پیش‌فرض
- + Properties از BaseEntity

**Methods:**
- `Create(userId, title, ...)`: ایجاد آدرس
- `Update(...)`: ویرایش آدرس
- `SetAsDefault()`: تنظیم به عنوان پیش‌فرض
- `UnsetAsDefault()`: حذف از پیش‌فرض

**Related Features:** UserAddress, UserOrder (ShippingAddress, BillingAddress)

---

### 4. Otp
**مسیر:** `src/Domain/Entities/Otp.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** کدهای یکبار مصرف برای احراز هویت

**Properties:**
- `PhoneNumber` (string): شماره تلفن
- `Code` (string): کد 6 رقمی
- `ExpiresAt` (DateTime): زمان انقضا
- `IsUsed` (bool): استفاده شده
- `Purpose` (string): هدف (Login, Register)
- + Properties از BaseEntity

**Methods:**
- `Create(phoneNumber, purpose)`: ایجاد OTP
- `MarkAsUsed()`: علامت‌گذاری به عنوان استفاده شده
- `IsValid()`: بررسی اعتبار

**Related Features:** Auth (SendOtp, VerifyOtp, LoginWithPhone, RegisterWithPhone)

---

### 5. RefreshToken
**مسیر:** `src/Domain/Entities/RefreshToken.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** توکن‌های تازه‌سازی JWT

**Properties:**
- `UserId` (Guid): شناسه کاربر
- `Token` (string): رشته توکن
- `ExpiresAt` (DateTime): زمان انقضا
- `IsRevoked` (bool): لغو شده
- `RevokedAt` (DateTime?): زمان لغو
- + Properties از BaseEntity

**Methods:**
- `Create(userId)`: ایجاد توکن جدید
- `Revoke()`: لغو توکن

**Related Features:** Auth (RefreshToken, Logout)

---

### 6. Product
**مسیر:** `src/Domain/Entities/Product.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** محصول اصلی سیستم

**Properties:**
- `Name` (string): نام محصول
- `Description` (string): توضیحات
- `Price` (decimal): قیمت
- `StockQuantity` (int): موجودی
- `CategoryId` (Guid?): شناسه دسته‌بندی
- `UnitId` (Guid?): شناسه واحد
- `BrandId` (Guid?): شناسه برند
- `Gender` (string?): جنسیت (Male/Female/Kids/Unisex)
- `Sku` (string?): کد SKU
- `Barcode` (string?): بارکد
- `Weight` (decimal?): وزن
- `Dimensions` (string?): ابعاد
- `IsActive` (bool): فعال
- `IsFeatured` (bool): ویژه
- `ViewCount` (int): تعداد بازدید
- `SalePrice` (decimal?): قیمت حراج
- `SaleStartDate` (DateTime?): شروع حراج
- `SaleEndDate` (DateTime?): پایان حراج
- + Properties از BaseEntity

**Navigation Properties:**
- `Category` (N:1): دسته‌بندی
- `Unit` (N:1): واحد
- `Brand` (N:1): برند
- `ProductDetails` (1:N): جزئیات فنی
- `ProductImages` (1:N): تصاویر
- `ProductReviews` (1:N): نظرات
- `ProductInventories` (1:N): موجودی‌ها
- `ProductVariants` (1:N): تنوع‌ها
- `ProductMaterials` (N:M): جنس‌ها
- `ProductSeasons` (N:M): فصل‌ها

**Methods:**
- `Create(name, description, price, stockQuantity, ...)`: ایجاد محصول
- `Update(...)`: ویرایش
- `SetPrice(decimal)`: تنظیم قیمت
- `SetName(string)`: تنظیم نام
- `SetBrandId(Guid?)`: تنظیم برند
- `SetGender(string?)`: تنظیم جنسیت
- `SetSalePrice(decimal?)`: تنظیم قیمت حراج
- `SetSaleDates(DateTime?, DateTime?)`: تنظیم تاریخ حراج
- `Activate()`: فعال‌سازی
- `Deactivate()`: غیرفعال‌سازی
- `SetAsFeatured()`: تنظیم به عنوان ویژه
- `RemoveFromFeatured()`: حذف از ویژه
- `IncrementViewCount()`: افزایش تعداد بازدید
- `GetCurrentPrice()`: دریافت قیمت فعلی
- `IsOnSale()`: در حراج است؟

**Related Features:** Product, ProductImage, ProductDetail, ProductInventory, ProductReview

---

### 7. ProductCategory
**مسیر:** `src/Domain/Entities/ProductCategory.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** دسته‌بندی محصولات

**Properties:**
- `Name` (string): نام دسته
- `Description` (string): توضیحات
- `ParentCategoryId` (Guid?): دسته والد
- `ImageUrl` (string?): تصویر
- `DisplayOrder` (int): ترتیب نمایش
- `IsActive` (bool): فعال
- + Properties از BaseEntity

**Navigation Properties:**
- `ParentCategory` (N:1): دسته والد
- `SubCategories` (1:N): زیردسته‌ها
- `Products` (1:N): محصولات

**Methods:**
- `Create(name, description, ...)`: ایجاد دسته
- `Update(...)`: ویرایش
- `SetName(string)`: تنظیم نام
- `SetDescription(string)`: تنظیم توضیحات

**Related Features:** ProductCategory

---

### 8. Brand
**مسیر:** `src/Domain/Entities/Brand.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** برندهای محصولات

**Properties:**
- `Name` (string): نام برند
- `LogoUrl` (string?): لوگو
- `Description` (string?): توضیحات
- `IsActive` (bool): فعال
- + Properties از BaseEntity

**Navigation Properties:**
- `Products` (1:N): محصولات

**Methods:**
- `Create(name, logoUrl, description)`: ایجاد برند
- `Update(...)`: ویرایش
- `Activate()`: فعال‌سازی
- `Deactivate()`: غیرفعال‌سازی

**Related Features:** Brand

---

### 9. Material
**مسیر:** `src/Domain/Entities/Material.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** جنس و مواد محصولات (پنبه، پشم، ...)

**Properties:**
- `Name` (string): نام جنس
- `Description` (string?): توضیحات
- `IsActive` (bool): فعال
- + Properties از BaseEntity

**Navigation Properties:**
- `ProductMaterials` (N:M): محصولات

**Methods:**
- `Create(name, description)`: ایجاد
- `Update(...)`: ویرایش

**Related Features:** Material

---

### 10. Season
**مسیر:** `src/Domain/Entities/Season.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** فصل‌های مناسب محصولات

**Properties:**
- `Name` (string): نام فصل
- `IsActive` (bool): فعال
- + Properties از BaseEntity

**Navigation Properties:**
- `ProductSeasons` (N:M): محصولات

**Methods:**
- `Create(name)`: ایجاد
- `Update(name)`: ویرایش

**Related Features:** Season

---

### 11. Unit
**مسیر:** `src/Domain/Entities/Unit.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** واحدهای اندازه‌گیری (عدد، کیلوگرم، متر)

**Properties:**
- `Name` (string): نام واحد
- `Symbol` (string?): نماد
- `IsActive` (bool): فعال
- + Properties از BaseEntity

**Navigation Properties:**
- `Products` (1:N): محصولات

**Methods:**
- `Create(name, symbol)`: ایجاد
- `Update(...)`: ویرایش

**Related Features:** Unit

---

### 12. ProductVariant
**مسیر:** `src/Domain/Entities/ProductVariant.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** تنوع محصولات (سایز، رنگ، مدل)

**Properties:**
- `ProductId` (Guid): شناسه محصول
- `Size` (string?): سایز
- `Color` (string?): رنگ
- `SKU` (string): کد SKU
- `StockQuantity` (int): موجودی
- `AdditionalPrice` (decimal): قیمت اضافی
- `IsAvailable` (bool): موجود
- `DisplayOrder` (int): ترتیب نمایش
- + Properties از BaseEntity

**Navigation Properties:**
- `Product` (N:1): محصول اصلی

**Methods:**
- `Create(productId, size, color, sku, ...)`: ایجاد
- `Update(...)`: ویرایش
- `SetStockQuantity(int)`: تنظیم موجودی
- `SetAvailability(bool)`: تنظیم موجود بودن

**Related Features:** ProductVariant

---

### 13. ProductImage
**مسیر:** `src/Domain/Entities/ProductImage.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** تصاویر محصولات

**Properties:**
- `ProductId` (Guid): شناسه محصول
- `ImageUrl` (string): لینک تصویر
- `AltText` (string?): متن جایگزین
- `Title` (string?): عنوان
- `DisplayOrder` (int): ترتیب نمایش
- `IsPrimary` (bool): تصویر اصلی
- `ImageType` (string): نوع (Main, Hover, Gallery, 360, Video)
- `FileSize` (long?): حجم فایل
- `MimeType` (string?): نوع فایل
- + Properties از BaseEntity

**Navigation Properties:**
- `Product` (N:1): محصول

**Methods:**
- `Create(productId, imageUrl, ...)`: ایجاد
- `Update(...)`: ویرایش
- `SetAsPrimary()`: تنظیم به عنوان اصلی
- `SetDisplayOrder(int)`: تنظیم ترتیب

**Related Features:** ProductImage

---

### 14. ProductDetail
**مسیر:** `src/Domain/Entities/ProductDetail.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** جزئیات فنی و مشخصات محصولات

**Properties:**
- `ProductId` (Guid): شناسه محصول
- `Key` (string): کلید (مثلاً "جنس")
- `Value` (string): مقدار (مثلاً "پنبه 100%")
- `Description` (string?): توضیحات
- `DisplayOrder` (int): ترتیب نمایش
- + Properties از BaseEntity

**Navigation Properties:**
- `Product` (N:1): محصول

**Methods:**
- `Create(productId, key, value, ...)`: ایجاد
- `Update(...)`: ویرایش

**Related Features:** ProductDetail

---

### 15. ProductInventory
**مسیر:** `src/Domain/Entities/ProductInventory.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** مدیریت موجودی محصولات

**Properties:**
- `ProductId` (Guid): شناسه محصول
- `AvailableQuantity` (int): موجود
- `ReservedQuantity` (int): رزرو شده
- `SoldQuantity` (int): فروخته شده
- `Location` (string?): محل انبار
- `LastSyncAt` (DateTime?): آخرین همگام‌سازی
- `SyncStatus` (string?): وضعیت همگام‌سازی
- `SyncError` (string?): خطای همگام‌سازی
- + Properties از BaseEntity

**Navigation Properties:**
- `Product` (N:1): محصول

**Methods:**
- `Create(productId, availableQuantity, ...)`: ایجاد
- `SetAvailableQuantity(int)`: تنظیم موجودی
- `AddStock(int)`: افزودن موجودی
- `ReserveQuantity(int)`: رزرو موجودی
- `ReleaseReservedQuantity(int)`: آزادسازی رزرو
- `CommitSale(int)`: تایید فروش
- `GetAvailableStock()`: دریافت موجودی در دسترس
- `GetTotalQuantity()`: دریافت کل موجودی

**Related Features:** ProductInventory, Checkout (برای رزرو موجودی)

---

### 16. ProductReview
**مسیر:** `src/Domain/Entities/ProductReview.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** نظرات و امتیازدهی کاربران

**Properties:**
- `ProductId` (Guid): شناسه محصول
- `UserId` (Guid): شناسه کاربر
- `Title` (string): عنوان نظر
- `Comment` (string): متن نظر
- `Rating` (int): امتیاز (1-5)
- `IsVerified` (bool): خرید تایید شده
- `IsApproved` (bool): تایید ادمین
- `ApprovedAt` (DateTime?): زمان تایید
- `ApprovedBy` (string?): تایید توسط
- `AdminNotes` (string?): یادداشت ادمین
- `RejectedAt` (DateTime?): زمان رد
- `RejectedBy` (string?): رد توسط
- `RejectionReason` (string?): دلیل رد
- + Properties از BaseEntity

**Navigation Properties:**
- `Product` (N:1): محصول
- `User` (N:1): کاربر

**Methods:**
- `Create(productId, userId, title, comment, rating)`: ایجاد
- `Update(...)`: ویرایش
- `MarkAsVerified()`: تایید خرید
- `Approve(adminNotes?)`: تایید نظر
- `Reject(reason, adminNotes?)`: رد نظر

**Related Features:** ProductReview

---

### 17. ProductRelation
**مسیر:** `src/Domain/Entities/ProductRelation.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** محصولات مرتبط (پیشنهادی، مشابه، جایگزین)

**Properties:**
- `ProductId` (Guid): محصول اصلی
- `RelatedProductId` (Guid): محصول مرتبط
- `RelationType` (string): نوع رابطه (Related, Similar, Alternative, Accessory)
- `DisplayOrder` (int): ترتیب نمایش
- `IsActive` (bool): فعال
- + Properties از BaseEntity

**Navigation Properties:**
- `Product` (N:1): محصول اصلی
- `RelatedProduct` (N:1): محصول مرتبط

**Methods:**
- `Create(productId, relatedProductId, relationType)`: ایجاد
- `Update(...)`: ویرایش

**Related Features:** Product (GetRelatedProducts)

---

### 18. Cart
**مسیر:** `src/Domain/Entities/Cart.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** سبد خرید کاربر

**Properties:**
- `UserId` (Guid): شناسه کاربر
- `SessionId` (string): شناسه نشست
- `CartName` (string): نام سبد
- `IsActive` (bool): فعال
- `ExpiresAt` (DateTime?): زمان انقضا
- + Properties از BaseEntity

**Navigation Properties:**
- `User` (N:1): کاربر
- `CartItems` (1:N): آیتم‌های سبد

**Methods:**
- `Create(userId, sessionId, cartName, ...)`: ایجاد
- `Activate()`: فعال‌سازی
- `Deactivate()`: غیرفعال‌سازی
- `SetExpiresAt(DateTime?)`: تنظیم انقضا
- `IsExpired()`: منقضی شده؟

**Related Features:** Cart, Checkout

---

### 19. CartItem
**مسیر:** `src/Domain/Entities/CartItem.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** آیتم‌های داخل سبد خرید

**Properties:**
- `CartId` (Guid): شناسه سبد
- `ProductId` (Guid): شناسه محصول
- `Quantity` (int): تعداد
- `UnitPrice` (decimal): قیمت واحد
- `TotalPrice` (decimal): قیمت کل
- + Properties از BaseEntity

**Navigation Properties:**
- `Cart` (N:1): سبد
- `Product` (N:1): محصول

**Methods:**
- `Create(cartId, productId, quantity, ...)`: ایجاد
- `SetQuantity(int)`: تنظیم تعداد
- `UpdatePrice(decimal)`: به‌روزرسانی قیمت

**Related Features:** Cart

---

### 20. SavedCart
**مسیر:** `src/Domain/Entities/SavedCart.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** سبدهای خرید ذخیره شده کاربر

**Properties:**
- `UserId` (Guid): شناسه کاربر
- `CartId` (Guid): شناسه سبد
- `SavedAt` (DateTime): زمان ذخیره
- `LastAccessedAt` (DateTime?): آخرین دسترسی
- `AccessCount` (int): تعداد دسترسی
- + Properties از BaseEntity

**Navigation Properties:**
- `User` (N:1): کاربر
- `Cart` (N:1): سبد

**Methods:**
- `Create(userId, cartId, name)`: ایجاد
- `UpdateLastAccessed()`: به‌روزرسانی دسترسی

**Related Features:** SavedCart

---

### 21. UserOrder
**مسیر:** `src/Domain/Entities/UserOrder.cs`  
**ارث‌بری:** `BaseEntity`  
**توضیح:** سفارشات کاربران

**Properties:**
- `UserId` (Guid): شناسه کاربر
- `OrderNumber` (string): شماره سفارش
- `OrderStatus` (string): وضعیت
- `SubTotal` (decimal): جمع جزء
- `TaxAmount` (decimal): مالیات
- `ShippingAmount` (decimal): هزینه ارسال
- `DiscountAmount` (decimal): تخفیف
- `TotalAmount` (decimal): جمع کل
- `Currency` (string): واحد پول
- `Notes` (string?): یادداشت
- `ShippedAt` (DateTime?): زمان ارسال
- `DeliveredAt` (DateTime?): زمان تحویل
- `CancelledAt` (DateTime?): زمان لغو
- `CancellationReason` (string?): دلیل لغو
- `TrackingNumber` (string?): شماره پیگیری
- `EstimatedDeliveryDate` (DateTime?): تاریخ تخمینی
- `ActualDeliveryDate` (DateTime?): تاریخ واقعی
- `ShippingAddressId` (Guid?): آدرس ارسال
- `BillingAddressId` (Guid?): آدرس صورتحساب
- + Properties از BaseEntity

**Navigation Properties:**
- `User` (N:1): کاربر
- `ShippingAddress` (N:1): آدرس ارسال
- `BillingAddress` (N:1): آدرس صورتحساب
- `OrderItems` (1:N): آیتم‌های سفارش
- `Payments` (1:N): پرداخت‌ها

**Methods:**
- `Create(userId, orderNumber, ...)`: ایجاد
- `Update(...)`: ویرایش
- `StartProcessing()`: شروع پردازش
- `MarkAsShipped(trackingNumber, estimatedDelivery)`: ارسال شد
- `MarkAsDelivered()`: تحویل داده شد
- `Cancel(reason)`: لغو سفارش
- `SetTrackingNumber(string)`: تنظیم شماره پیگیری
- `SetShippingAddress(Guid)`: تنظیم آدرس ارسال
- `SetBillingAddress(Guid)`: تنظیم آدرس صورتحساب

**Related Features:** UserOrder, Checkout, OrderTracking

---

### 22-36. سایر Entities

به همین ترتیب برای:
- UserOrderItem
- UserPayment
- OrderStatusHistory
- UserReturnRequest
- Wishlist
- Coupon
- UserCouponUsage
- StockAlert
- UserProductView
- MahakMapping
- MahakQueue
- MahakSyncLog
- SyncErrorLog
- ProductMaterial
- ProductSeason

---

## بخش 2: لیست کامل Features با Commands/Queries

### Feature 1: Auth
**مسیر:** `src/Application/Features/Auth/`

**Commands:**
1. `Register` - ثبت‌نام با ایمیل
2. `Login` - ورود با ایمیل
3. `SendOtp` - ارسال کد OTP
4. `VerifyOtp` - تایید کد OTP
5. `RegisterWithPhone` - ثبت‌نام با تلفن
6. `LoginWithPhone` - ورود با تلفن
7. `RefreshToken` - تازه‌سازی توکن
8. `Logout` - خروج

**DTOs:**
- RegisterDto, LoginDto
- SendOtpDto, VerifyOtpDto, OtpResponseDto
- RegisterWithPhoneDto, LoginWithPhoneDto
- RefreshTokenDto, AuthResponseDto
- ChangePasswordDto, ForgotPasswordDto
- ResetPasswordDto

**Validators:**
- RegisterDtoValidator
- LoginDtoValidator
- SendOtpDtoValidator
- VerifyOtpDtoValidator
- RegisterWithPhoneDtoValidator
- LoginWithPhoneDtoValidator
- ChangePasswordDtoValidator
- ForgotPasswordDtoValidator
- ResetPasswordDtoValidator
- RefreshTokenDtoValidator

**API Endpoints:**
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/send-otp
- POST /api/auth/verify-otp
- POST /api/auth/register-phone
- POST /api/auth/login-phone
- POST /api/auth/refresh-token
- POST /api/auth/logout

---

### Feature 2: Product
**مسیر:** `src/Application/Features/Product/`

**Commands:**
1. `CreateProduct` - ایجاد محصول
2. `UpdateProduct` - ویرایش محصول
3. `DeleteProduct` - حذف محصول
4. `TrackProductView` - ثبت بازدید

**Queries:**
1. `GetProductById` - دریافت جزئیات
2. `GetAllProducts` - لیست محصولات
3. `ProductSearch` - جستجوی پیشرفته
4. `GetRelatedProducts` - محصولات مرتبط
5. `GetRecentlyViewed` - اخیراً دیده شده
6. `GetFrequentlyBoughtTogether` - خریداری شده با هم

**DTOs:**
- ProductDto (با تمام اطلاعات)
- ProductDetailsDto
- CreateProductDto
- UpdateProductDto
- ProductSearchDto
- ProductSearchResultDto

**Validators:**
- CreateProductDtoValidator
- UpdateProductDtoValidator

**API Endpoints:**
- GET /api/product
- GET /api/product/{id}
- POST /api/product
- PUT /api/product/{id}
- DELETE /api/product/{id}
- POST /api/product/search
- GET /api/product/{id}/related
- GET /api/product/recently-viewed
- GET /api/product/{id}/frequently-bought-together
- POST /api/product/track-view

---

### Features 3-27: (همین الگو برای سایر Features)

- **Brand**: 3 Commands, 2 Queries, 5 Endpoints
- **Material**: 3 Commands, 2 Queries, 5 Endpoints
- **Season**: 3 Commands, 2 Queries, 5 Endpoints
- **ProductVariant**: 3 Commands, 3 Queries, 6 Endpoints
- **ProductCategory**: 3 Commands, 3 Queries, 6 Endpoints
- **ProductImage**: 4 Commands, 2 Queries, 6 Endpoints
- **ProductDetail**: 3 Commands, 2 Queries, 5 Endpoints
- **ProductInventory**: 6 Commands, 3 Queries, 9 Endpoints
- **ProductReview**: 6 Commands, 3 Queries, 8 Endpoints
- **Cart**: 6 Commands, 3 Queries, 8 Endpoints
- **SavedCart**: 3 Commands, 3 Queries, 6 Endpoints
- **Checkout**: 3 Commands, 1 Query, 4 Endpoints
- **UserOrder**: 8 Commands, 4 Queries, 12 Endpoints
- **UserPayment**: 4 Commands, 3 Queries, 7 Endpoints
- **UserAddress**: 4 Commands, 3 Queries, 7 Endpoints
- **UserProfile**: 4 Commands, 2 Queries, 6 Endpoints
- **UserReturnRequest**: 5 Commands, 4 Queries, 9 Endpoints
- **Wishlist**: 3 Commands, 2 Queries, 5 Endpoints
- **Coupon**: 5 Commands, 4 Queries, 9 Endpoints
- **StockAlert**: 3 Commands, 2 Queries, 5 Endpoints
- **Unit**: 3 Commands, 2 Queries, 5 Endpoints
- **MahakMapping**: 3 Commands, 2 Queries, 5 Endpoints
- **MahakQueue**: 3 Commands, 2 Queries, 5 Endpoints
- **MahakSyncLog**: 3 Commands, 2 Queries, 5 Endpoints
- **SyncErrorLog**: 3 Commands, 2 Queries, 5 Endpoints

**جمع کل:**
- **Commands**: ~95
- **Queries**: ~70
- **Endpoints**: ~140

---


