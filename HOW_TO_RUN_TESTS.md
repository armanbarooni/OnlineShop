# 🚀 راهنمای اجرای تست‌ها (بدون گیر کردن در Cursor)

## ⚡ روش سریع (توصیه می‌شود)

### 1. باز کردن PowerShell خارج از Cursor

**Windows Key + X** → **Windows PowerShell** (یا Terminal)

### 2. رفتن به پوشه پروژه

```powershell
cd C:\Users\arman\source\repos\OnlineShop
```

### 3. اجرای اسکریپت

```powershell
.\run-tests.ps1
```

---

## 🎯 گزینه‌های مختلف

### اجرای همه تست‌ها (پیش‌فرض)
```powershell
.\run-tests.ps1
```

### فقط Integration Tests
```powershell
.\run-tests.ps1 -OnlyIntegration
```

### فقط Application Tests
```powershell
.\run-tests.ps1 -OnlyApplication
```

### فیلتر کردن تست‌های خاص
```powershell
# فقط تست‌های Coupon
.\run-tests.ps1 -Filter "CouponTests"

# فقط تست‌های Authentication
.\run-tests.ps1 -Filter "AuthenticationFlowTests"

# فقط تست‌های ProductInventory
.\run-tests.ps1 -Filter "ProductInventoryTests"
```

### خروجی با جزئیات بیشتر
```powershell
.\run-tests.ps1 -Detailed
```

### ترکیبی
```powershell
# Integration tests فقط برای Coupon با جزئیات
.\run-tests.ps1 -OnlyIntegration -Filter "CouponTests" -Detailed
```

---

## 📊 خروجی اسکریپت

اسکریپت این اطلاعات را نمایش می‌دهد:

1. **وضعیت Build** - آیا کامپایل موفق بود؟
2. **نتایج کلی** - تعداد Passed/Failed/Skipped
3. **لیست تست‌های ناموفق** - نام تست‌هایی که fail شدند
4. **آمار خطاها** - تعداد 401, 404, 400, 405
5. **درصد موفقیت** - با نوار پیشرفت بصری
6. **مسیر فایل‌های گزارش** - TRX و Summary

### نمونه خروجی:

```
========================================
  Starting Test Execution
========================================

[1/3] Building solution...
✓ Build successful

[2/3] Running All Tests...
    Output: test-results/test_results_20251017_123045.trx

[3/3] Analyzing results...

==========================================
     Test Execution Summary
==========================================
Timestamp: 2025-10-17 12:30:52
TRX File: test-results/test_results_20251017_123045.trx

Passed!  - Failed:     0, Passed:   205, Skipped:     0, Total:   205
Failed!  - Failed:    85, Passed:    73, Skipped:     2, Total:   160

Test summary: total: 365, failed: 85, succeeded: 278, skipped: 2

==========================================
     Detailed Analysis
==========================================

Error Types:
  - 401 Unauthorized: 82
  - 404 Not Found: 1
  - 400 Bad Request: 2
  - 405 Method Not Allowed: 0

Success Rate: 76.2% (278/365)
Progress: [██████████████████████████████░░░░░░░░░░] 76.2%

==========================================

⚠️  Some tests failed. Check details above.
   Full results: test-results/test_results_20251017_123045.trx
   Summary: test-results/test_summary_20251017_123045.txt
```

---

## 📁 فایل‌های خروجی

در پوشه `test-results/` این فایل‌ها ایجاد می‌شوند:

- **`test_results_TIMESTAMP.trx`** - فایل XML کامل نتایج (برای Visual Studio Test Explorer)
- **`test_summary_TIMESTAMP.txt`** - خلاصه متنی (برای Cursor AI)

---

## 🔍 تحلیل نتایج در Cursor

بعد از اجرای اسکریپت، فایل summary را در Cursor بخوانید:

```
# در Cursor Chat:
@test-results/test_summary_LATEST.txt بررسی کن و بگو چه تست‌هایی fail شدند
```

یا فقط نام فایل را بدهید به من تا بخوانم و تحلیل کنم!

---

## 🐛 Debug تست‌های خاص

اگر می‌خواهید فقط یک تست را با خروجی کامل ببینید:

```powershell
dotnet test --filter "FullyQualifiedName~DebugTests.TestAuthentication" --verbosity detailed
```

این در PowerShell عادی هم کار می‌کند.

---

## ✅ مزایای این روش

1. ✅ **هیچ وقت گیر نمی‌کند** - چون stdout به فایل می‌رود
2. ✅ **سریع‌تر** - Cursor نیازی به پردازش stream ندارد
3. ✅ **قابل تحلیل** - فایل‌های structured TRX و Summary
4. ✅ **قابل اشتراک** - می‌توانید نتایج را با تیم share کنید
5. ✅ **تاریخچه** - تمام نتایج با timestamp ذخیره می‌شوند

---

## 🎯 برای شما (الان):

**فقط این را اجرا کنید:**

```powershell
cd C:\Users\arman\source\repos\OnlineShop
.\run-tests.ps1
```

و بعد نتیجه را کپی کرده و برایم بفرستید، یا بگویید کدام فایل summary را بخوانم! 🚀




