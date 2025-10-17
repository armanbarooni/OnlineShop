# راهنمای سریع معماری OnlineShop

## 🎯 دسترسی سریع به اطلاعات

### 📂 فایل‌های مستندات

- **مستندات کامل فارسی**: `Persian/Complete-Architecture-FA.md` (~1200 خط)
- **مستندات کامل انگلیسی**: `English/Complete-Architecture-EN.md` (~900 خط)
- **مرجع Entities و Features**: `Persian/Complete-Entities-Features-Reference-FA.md` (~600 خط)
- **دیاگرام‌ها**: `Diagrams/*.mmd` (6 فایل)

---

## 📊 آمار سریع سیستم

| مورد | تعداد |
|------|-------|
| **Entities** | 36 |
| **Features** | 27 |
| **Commands** | ~95 |
| **Queries** | ~70 |
| **DTOs** | ~90 |
| **Validators** | ~55 |
| **AutoMapper Profiles** | 28 |
| **Repositories** | 32 |
| **Controllers** | 28 |
| **API Endpoints** | ~140 |
| **Database Tables** | 36 |
| **Migrations** | 23 |
| **Unit Tests** | 158 |

---

## 🗂️ ساختار لایه‌ها

```
OnlineShop/
├── Domain/           36 Entities, 32 Interfaces
├── Application/      27 Features, 90 DTOs, 55 Validators
├── Infrastructure/   32 Repositories, External Services
└── WebAPI/          28 Controllers, 2 Middlewares
```

---

## 🔑 موجودیت‌های کلیدی

### محصولات (14 Entity)
Product, ProductCategory, Brand, Material, Season, Unit, ProductVariant, ProductImage, ProductDetail, ProductInventory, ProductReview, ProductRelation, ProductMaterial, ProductSeason

### کاربران (5 Entity)
ApplicationUser, UserProfile, UserAddress, Otp, RefreshToken

### خرید و سفارش (8 Entity)
Cart, CartItem, SavedCart, UserOrder, UserOrderItem, UserPayment, OrderStatusHistory, UserReturnRequest

### سایر (9 Entity)
Wishlist, Coupon, UserCouponUsage, StockAlert, UserProductView, MahakMapping, MahakQueue, MahakSyncLog, SyncErrorLog

---

## 🚀 Features اصلی

### احراز هویت
- ثبت‌نام و ورود سنتی
- احراز هویت با OTP
- Refresh Token

### مدیریت محصولات
- CRUD محصولات
- جستجوی پیشرفته با فیلترهای متعدد
- محصولات مرتبط و پیشنهادی

### خرید
- سبد خرید
- کوپن و تخفیف
- Checkout چندمرحله‌ای

### سفارشات
- ثبت و مدیریت سفارش
- ردیابی وضعیت
- مرجوعی

---

## 📡 Endpoints مهم

### Authentication
```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/send-otp
POST /api/auth/login-phone
```

### Products
```
GET /api/product
POST /api/product/search
GET /api/product/{id}
POST /api/product (Admin)
```

### Cart
```
GET /api/cart
POST /api/cart/add
DELETE /api/cart/remove/{productId}
```

### Orders
```
GET /api/order
POST /api/checkout/complete
GET /api/order/{id}/timeline
```

---

## 🔧 تکنولوژی‌ها

- .NET 8.0
- EF Core 8.0.21
- PostgreSQL
- ASP.NET Identity + JWT
- MediatR
- AutoMapper
- FluentValidation
- Serilog

---

## 📖 نحوه استفاده از مستندات

1. **برای درک کلی**: ابتدای `Complete-Architecture-FA.md` را بخوانید
2. **برای جزئیات Entity**: بخش Domain Layer را ببینید
3. **برای پیاده‌سازی Feature**: بخش Application Layer
4. **برای مشاهده جریان‌ها**: دیاگرام‌های Sequence را ببینید
5. **برای API**: بخش API Documentation

---

## 🎨 دیاگرام‌های موجود

1. `system-architecture.mmd` - معماری کلی
2. `cqrs-flow.mmd` - الگوی CQRS
3. `project-dependencies.mmd` - وابستگی پروژه‌ها
4. `entity-relationships.mmd` - روابط Entity ها
5. `authentication-flow.mmd` - جریان احراز هویت
6. `shopping-flow.mmd` - جریان خرید

**نحوه مشاهده:**
- VS Code: نصب Mermaid Preview Extension
- آنلاین: https://mermaid.live/
- GitHub: رندر خودکار

---

**به‌روزرسانی:** مهر 1404  
**نسخه:** 1.0

