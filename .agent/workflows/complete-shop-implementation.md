---
description: Complete Shop Implementation - Turbo Plan
---

# 🚀 Turbo Plan: پیاده‌سازی کامل فروشگاه آنلاین

## 📋 Phase 1: Sync و اعتبارسنجی داده‌های محک

### ✅ 1.1 تکمیل Sync ورودی (Incoming)
- [ ] **ProductDetails Parsing**
  - پارس کردن Size, Color, SKU از محک
  - ذخیره در `ProductVariant`
  - Validation: Size و Color نباید خالی باشد
  
- [ ] **Product Validation**
  - چک کردن Category موجود باشد
  - چک کردن Brand موجود باشد
  - قیمت باید > 0
  - نام نباید خالی باشد

- [ ] **Picture Validation**
  - URL معتبر باشد
  - فرمت تصویر صحیح باشد (jpg, png, webp)
  - حداقل یک عکس برای هر محصول

- [ ] **Inventory Validation**
  - موجودی >= 0
  - StoreId معتبر باشد
  - ProductDetailId موجود باشد

### ✅ 1.2 بهبود Sync خروجی (Outgoing)

- [ ] **Customer Sync**
  - ارسال فقط موقع خرید (✅ انجام شده)
  - اضافه کردن نام کامل
  - اضافه کردن شماره تماس
  - Validation قبل از ارسال

- [ ] **Order Sync**
  - ارسال کامل اطلاعات مشتری
  - ارسال جزئیات محصول (نام، کد، سایز، رنگ)
  - ارسال آدرس کامل
  - Validation: PersonId باید موجود باشد
  - Validation: ProductDetailId باید موجود باشد
  - Validation: StoreId باید معتبر باشد

---

## 📊 Phase 2: ذخیره صحیح داده‌ها در دیتابیس

### ✅ 2.1 Product & Variants
```csharp
Product
├── Name ✅
├── Description ✅
├── Price ✅
├── CategoryId ✅
├── BrandId ✅
├── MahakId ✅
└── ProductVariants
    ├── Size ✅
    ├── Color ✅
    ├── SKU ✅
    ├── StockQuantity ✅
    └── MahakProductDetailId ✅
```

### ✅ 2.2 Images
```csharp
ProductImage
├── ImageUrl ✅
├── IsPrimary ✅
├── DisplayOrder ✅
└── ProductId ✅
```

### ✅ 2.3 Inventory
```csharp
ProductInventory
├── ProductId ✅
├── Quantity ✅
├── LastUpdated ✅
└── MahakStoreId ✅
```

---

## 🛍️ Phase 3: API های فروشگاه

### ✅ 3.1 لیست محصولات
- [x] **GET /api/Product** - لیست با فیلتر و سرچ ✅
  - SearchTerm ✅
  - CategoryId ✅
  - BrandId ✅
  - Color, Size ✅
  - Price Range ✅
  - Sorting ✅
  - Pagination ✅

- [ ] **GET /api/Product/{id}** - جزئیات محصول
  - اطلاعات کامل محصول
  - لیست Variants (سایز/رنگ)
  - عکس‌ها
  - موجودی
  - محصولات مرتبط

- [ ] **GET /api/Category** - لیست دسته‌بندی‌ها
  - Tree structure
  - تعداد محصولات هر دسته
  - Thumbnail

### ✅ 3.2 سبد خرید
- [ ] **POST /api/Cart/add** - اضافه کردن به سبد
  - ProductId
  - VariantId (سایز/رنگ)
  - Quantity
  - Validation: موجودی کافی باشد

- [ ] **GET /api/Cart** - مشاهده سبد خرید
  - لیست آیتم‌ها
  - قیمت کل
  - تخفیف
  - هزینه ارسال

- [ ] **PUT /api/Cart/update** - به‌روزرسانی تعداد
  - CartItemId
  - NewQuantity
  - Validation: موجودی

- [ ] **DELETE /api/Cart/{itemId}** - حذف از سبد

### ✅ 3.3 سفارش (Checkout)
- [ ] **POST /api/Order/create** - ثبت سفارش
  - ShippingAddressId
  - PaymentMethod
  - DiscountCode (optional)
  - Validation: سبد خالی نباشد
  - Validation: آدرس معتبر باشد
  - Validation: موجودی کافی باشد

- [ ] **GET /api/Order/{id}** - جزئیات سفارش
  - اطلاعات سفارش
  - آیتم‌ها
  - وضعیت پرداخت
  - وضعیت ارسال

- [ ] **GET /api/Order/my-orders** - لیست سفارشات کاربر
  - Pagination
  - فیلتر بر اساس وضعیت

### ✅ 3.4 پرداخت
- [ ] **POST /api/Payment/initiate** - شروع پرداخت
  - OrderId
  - PaymentGateway (Sadad)
  - Return URL

- [ ] **POST /api/Payment/callback** - Callback درگاه
  - Verify payment
  - Update order status
  - Send to Mahak if successful

### ✅ 3.5 پنل کاربری
- [ ] **GET /api/User/profile** - پروفایل کاربر
- [ ] **PUT /api/User/profile** - ویرایش پروفایل

- [ ] **GET /api/User/addresses** - لیست آدرس‌ها
- [ ] **POST /api/User/addresses** - اضافه کردن آدرس
- [ ] **PUT /api/User/addresses/{id}** - ویرایش آدرس
- [ ] **DELETE /api/User/addresses/{id}** - حذف آدرس

- [ ] **GET /api/User/orders** - سفارشات کاربر
- [ ] **GET /api/User/orders/{id}** - جزئیات سفارش

---

## 🔄 Phase 4: Integration با محک

### ✅ 4.1 Sync Order به محک (موقع پرداخت موفق)
```csharp
Payment Success
    ↓
1. Sync Customer (if not synced)
    ├── PersonId
    ├── Name, Family
    ├── Mobile
    ├── Email
    └── VisitorPeople (اتصال به Visitor)
    ↓
2. Send Order
    ├── PersonId ✅
    ├── VisitorId ✅
    ├── OrderType = 201 (فروش)
    ├── OrderDate
    ├── ShippingAddress (JSON)
    ├── Discount
    ├── SendCost
    └── OrderDetails
        ├── ProductDetailId (MahakId) ✅
        ├── StoreId ✅
        ├── Price
        ├── Count1 (تعداد)
        ├── Description (نام محصول + سایز + رنگ)
        └── ItemType = 1
    ↓
3. Update Order Status
    ├── MahakOrderId
    └── SyncedAt
```

### ✅ 4.2 Validation قبل از ارسال
- [ ] PersonId موجود باشد
- [ ] ProductDetailId معتبر باشد
- [ ] StoreId معتبر باشد
- [ ] موجودی کافی باشد
- [ ] قیمت‌ها مطابقت داشته باشد

---

## 🎯 Phase 5: Business Logic

### ✅ 5.1 مدیریت موجودی
- [ ] کاهش موجودی موقع ثبت سفارش
- [ ] Reserve کردن موجودی تا پرداخت
- [ ] برگشت موجودی در صورت کنسلی
- [ ] Sync موجودی از محک (هر 5 دقیقه)

### ✅ 5.2 مدیریت قیمت
- [ ] قیمت پایه از محک
- [ ] قیمت اضافی برای Variant
- [ ] تخفیف‌ها
- [ ] محاسبه هزینه ارسال

### ✅ 5.3 وضعیت سفارش
```
Pending → PaymentPending → Paid → Processing → Shipped → Delivered
                ↓
            Cancelled
```

### ✅ 5.4 Notifications
- [ ] ایمیل تایید سفارش
- [ ] SMS کد رهگیری
- [ ] اطلاع‌رسانی تغییر وضعیت

---

## 📝 Phase 6: Validation Rules

### ✅ 6.1 Product
- Name: required, max 200
- Price: > 0
- CategoryId: must exist
- StockQuantity: >= 0

### ✅ 6.2 Order
- UserId: required, must exist
- ShippingAddressId: required, must exist
- Items: at least 1 item
- Each item: stock available

### ✅ 6.3 Payment
- Amount: must match order total
- Gateway response: must be valid

---

## 🔧 Phase 7: Error Handling

### ✅ 7.1 Mahak Sync Errors
- [ ] Login failed → Retry 3 times
- [ ] Network error → Queue for later
- [ ] Invalid data → Log and skip
- [ ] Duplicate → Update existing

### ✅ 7.2 Order Errors
- [ ] Out of stock → Notify user
- [ ] Payment failed → Cancel order
- [ ] Mahak sync failed → Retry later

---

## 📊 Priority Order (اولویت اجرا)

### 🔥 High Priority (Week 1)
1. ✅ Product List API با فیلتر (Done)
2. Product Details API
3. Cart APIs (Add, View, Update, Delete)
4. Order Create API
5. Payment Integration
6. Mahak Order Sync

### 🟡 Medium Priority (Week 2)
7. User Profile APIs
8. Address Management
9. Order History
10. Product Variants Sync از محک
11. Inventory Management

### 🟢 Low Priority (Week 3)
12. Notifications
13. Reviews & Ratings
14. Wishlist
15. Related Products
16. Search Optimization

---

## 🚀 Next Steps

1. **الان**: تکمیل Product Details API
2. **بعدی**: Cart Management
3. **سپس**: Order & Payment
4. **آخر**: Mahak Integration تکمیل

---

## 📌 Notes

- همه API ها باید Validation داشته باشند
- همه خطاها باید Log شوند
- Transaction برای عملیات مهم (Order, Payment)
- Unit Tests برای Business Logic
- Integration Tests برای Mahak Sync

---

**می‌خوای از کجا شروع کنیم؟** 🔥
