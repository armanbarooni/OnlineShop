# 🔄 برنامه یکپارچه‌سازی Checkout Flow

## 🎯 هدف
یکپارچه‌سازی دو مسیر Checkout موجود و ایجاد یک Flow واحد و کامل

---

## 📊 وضعیت فعلی

### مسیر 1: جدید (ساده)
```
Cart → OrderController.CreateOrder → PaymentController.Initiate → PaymentController.Verify
```
**مزایا:**
- ✅ ساده و مستقیم
- ✅ Stock Reduction در Verify
- ✅ Mahak Sync Ready

**کمبودها:**
- ❌ بدون Coupon Support
- ❌ بدون Shipping Calculation
- ❌ بدون Inventory Service (Atomic)

### مسیر 2: قدیمی (کامل)
```
Cart → CheckoutController.ProcessCheckout (All-in-One)
```
**مزایا:**
- ✅ Coupon Support
- ✅ Shipping Calculation
- ✅ Inventory Service (Atomic)
- ✅ Validation

**کمبودها:**
- ❌ پیچیده
- ❌ All-in-One (نه Modular)

---

## ✅ استراتژی یکپارچه‌سازی

### گام 1: اضافه کردن Coupon به CreateOrder
- اضافه کردن `CouponCode` به `CreateOrderDto`
- Validation کوپن در `CreateOrderCommandHandler`
- محاسبه Discount

### گام 2: نگه داشتن CheckoutController برای Helpers
- `GET /api/Checkout/addresses` - لیست آدرس‌ها
- `GET /api/Checkout/payment-methods` - روش‌های پرداخت
- `GET /api/Checkout/shipping-methods` - روش‌های ارسال
- `POST /api/Checkout/apply-coupon` - اعتبارسنجی کوپن
- `GET /api/Checkout/order-summary` - خلاصه سفارش

### گام 3: Deprecate ProcessCheckout
- اضافه کردن `[Obsolete]` Attribute
- Redirect به مسیر جدید در Documentation

### گام 4: استفاده از InventoryService
- Refactor `VerifyPaymentCommandHandler`
- استفاده از Atomic Stock Reduction

---

## 🔧 تغییرات مورد نیاز

### 1. CreateOrderDto
```csharp
public class CreateOrderDto
{
    public Guid? ShippingAddressId { get; set; }
    public Guid? BillingAddressId { get; set; }
    public string? CouponCode { get; set; }  // ← جدید
    public string? Notes { get; set; }
}
```

### 2. CreateOrderCommandHandler
```csharp
// اضافه کردن:
- Coupon Validation
- Discount Calculation
- UserCouponUsage Record
```

### 3. VerifyPaymentCommandHandler
```csharp
// تغییر:
- استفاده از IInventoryService بجای کسر مستقیم
```

---

## 📝 Flow نهایی

```
1. کاربر سبد خرید را پر می‌کند
   ↓
2. GET /api/Checkout/order-summary (پیش‌نمایش)
   ↓
3. POST /api/Checkout/apply-coupon (اختیاری)
   ↓
4. POST /api/Order/create (با CouponCode)
   → ایجاد Order با وضعیت Pending
   → محاسبه Discount
   → خالی کردن Cart
   ↓
5. POST /api/Payment/initiate
   → ایجاد Payment Record
   → دریافت Payment URL
   ↓
6. کاربر به درگاه می‌رود
   ↓
7. POST /api/Payment/verify
   → تایید پرداخت
   → Atomic Stock Reduction (InventoryService)
   → تغییر وضعیت Order به Confirmed
   → آماده برای Mahak Sync
```

---

## ⚡ اولویت اجرا

1. ✅ اضافه کردن Coupon به CreateOrder
2. ✅ Refactor VerifyPayment برای InventoryService
3. ✅ Deprecate ProcessCheckout
4. ✅ Update Documentation

---

## 🧪 تست‌های مورد نیاز

- [ ] ایجاد سفارش بدون کوپن
- [ ] ایجاد سفارش با کوپن معتبر
- [ ] ایجاد سفارش با کوپن نامعتبر
- [ ] پرداخت موفق با Stock Reduction
- [ ] پرداخت ناموفق (بدون Stock Reduction)
- [ ] Concurrent Orders (Atomic Test)
