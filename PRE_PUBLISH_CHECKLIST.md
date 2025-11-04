# چک‌لیست بررسی قبل از پابلیش 🚀

تاریخ بررسی: $(Get-Date -Format "yyyy-MM-dd")

---

## 🔴 موارد حیاتی (Critical) - باید قبل از پابلیش رفع شوند

### 1. امنیت (Security) ⚠️

#### ✅ مشکل: اطلاعات حساس در فایل‌های کانفیگ
- [ ] **appsettings.json** - Password دیتابیس hardcode شده (`Password=1234`)
- [ ] **appsettings.Development.json** - JWT Secret hardcode شده (`dev-secret-change-me-please`)
- [ ] **appsettings.json** - SmsIr ApiKey hardcode شده

**راه‌حل:**
```json
// باید از Environment Variables استفاده شود
"ConnectionStrings": {
  "DefaultConnection": "${DATABASE_CONNECTION_STRING}"
},
"Jwt": {
  "Secret": "${JWT_SECRET}"
}
```

#### ✅ مشکل: عدم وجود appsettings.Production.json
- [ ] ایجاد فایل `src/WebAPI/appsettings.Production.json` با تنظیمات production

**مثال محتوا:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "از Environment Variable استفاده شود"
  },
  "Jwt": {
    "Issuer": "OnlineShop",
    "Audience": "OnlineShopClient",
    "Secret": "از Environment Variable استفاده شود - باید حداقل 32 کاراکتر باشد",
    "ExpiryMinutes": 60,
    "RefreshExpiryDays": 14
  },
  "SmsIr": {
    "ApiKey": "از Environment Variable استفاده شود",
    "TemplateId": 325822,
    "UseSandbox": false,
    "OtpParamName": "Code"
  }
}
```

### 2. تنظیمات Production (Configuration)

#### ✅ مشکل: CORS فقط برای localhost
**فایل:** `src/WebAPI/Program.cs` (خطوط 70-79)
- [ ] اضافه کردن دامنه‌های production به CORS policy
- [ ] حذف `DefaultCors` policy که همه origins را allow می‌کند

#### ✅ مشکل: API URL در Frontend
**فایل:** `presentation/config.js` (خط 5)
- [ ] تغییر `baseURL` از `localhost` به URL واقعی production

**راه‌حل:**
```javascript
const isDevelopment = window.location.hostname === 'localhost' || 
                      window.location.hostname === '127.0.0.1';
window.config = {
    api: {
        baseURL: isDevelopment 
            ? 'http://localhost:5000/api' 
            : 'https://api.yourdomain.com/api',
        // ...
    }
}
```

#### ✅ مشکل: HTTPS غیرفعال
**فایل:** `src/WebAPI/Program.cs` (خط 109)
- [ ] در production: `options.RequireHttpsMetadata = true;`
- [ ] فعال کردن HTTPS redirection

#### ✅ مشکل: Logging Level در Debug
**فایل:** `src/WebAPI/Program.cs` (خط 14, 37)
- [ ] در production: `MinimumLevel.Information()` یا `MinimumLevel.Warning()`

---

## 🟡 موارد مهم (Important) - بهتر است رفع شوند

### 3. کد Production (Code Quality)

#### ⚠️ مشکل: Console.log/error زیاد در کد
**تعداد:** بیش از 200 مورد `console.error` در فایل‌های JS

**فایل‌های اصلی:**
- `presentation/assets/js/services/*.js`
- `presentation/assets/js/components/*.js`
- `presentation/assets/js/pages/*.js`

**راه‌حل:**
- [ ] ایجاد یک Logger service
- [ ] در production، فقط Error level را log کنید
- [ ] یا کامنت کردن console.log ها در production build

#### ⚠️ مشکل: فایل‌های Test/Debug باقی مانده
**فایل‌های قابل حذف:**
- [ ] `presentation/debug-*.html` (3 فایل)
- [ ] `presentation/test-*.html` (7 فایل)
- [ ] `presentation/final-test.html`
- [ ] `test-backend-connection.html`
- [ ] فایل‌های `.txt` تست در ریشه (4 فایل)
- [ ] `debug-auth.txt`

**نکته:** این فایل‌ها می‌توانند به یک پوشه `dev-tools/` منتقل شوند یا در `.gitignore` اضافه شوند.

### 4. مستندات (Documentation)

#### ⚠️ مشکل: عدم وجود README اصلی
- [ ] ایجاد `README.md` در ریشه پروژه با:
  - معرفی پروژه
  - راهنمای نصب و راه‌اندازی
  - لینک به مستندات کامل
  - اطلاعات کانتکت

### 5. Git & Version Control

#### ⚠️ مشکل: تغییرات Commit نشده
**فایل‌های تغییر یافته:**
- [ ] `presentation/assets/js/components/footer-component.js`
- [ ] `presentation/assets/js/services/order-service.js`
- [ ] `presentation/config.js`
- [ ] `presentation/product.html`

**اقدام:**
- [ ] Review تغییرات
- [ ] Commit کردن یا Revert کردن

---

## 🟢 موارد اختیاری (Optional) - بهبود کیفیت

### 6. بهینه‌سازی (Optimization)

- [ ] Minify کردن فایل‌های JavaScript در production
- [ ] Minify کردن فایل‌های CSS در production
- [ ] بهینه‌سازی تصاویر
- [ ] فعال کردن Compression در WebAPI

### 7. Monitoring & Logging

- [ ] تنظیم Logging به یک سرویس خارجی (مثل Serilog Sinks)
- [ ] اضافه کردن Health Check endpoints
- [ ] تنظیم Application Insights یا مشابه

### 8. تست‌ها (Testing)

**وضعیت فعلی:**
- ✅ Application Tests: 205/205 (100%)
- ⚠️ Integration Tests: 73/160 (45.6%)

**اقدامات:**
- [ ] بررسی و رفع مشکل Authentication در Integration Tests
- [ ] هدف: رسیدن به 95%+ coverage

### 9. Performance

- [ ] بررسی Query Performance در دیتابیس
- [ ] اضافه کردن Caching برای API های پرکاربرد
- [ ] بررسی و بهینه‌سازی Database Indexes

---

## 📋 چک‌لیست نهایی قبل از Deploy

### قبل از Deploy:

- [ ] تمام موارد حیاتی (Critical) رفع شده
- [ ] تمام موارد مهم (Important) بررسی شده
- [ ] تست‌های اصلی اجرا شده و Pass شده
- [ ] فایل `appsettings.Production.json` ایجاد و تنظیم شده
- [ ] Environment Variables تنظیم شده
- [ ] CORS برای دامنه‌های production تنظیم شده
- [ ] HTTPS فعال شده
- [ ] Logging Level مناسب تنظیم شده
- [ ] فایل‌های Test/Debug حذف یا جدا شده
- [ ] README.md ایجاد شده
- [ ] تغییرات Git Commit شده
- [ ] Backup از دیتابیس گرفته شده
- [ ] Rollback Plan آماده است

### بعد از Deploy:

- [ ] بررسی Health Check endpoints
- [ ] بررسی Logs برای خطا
- [ ] تست عملکرد اصلی (Login, Register, Order, etc.)
- [ ] بررسی Performance اولیه
- [ ] تست در مرورگرهای مختلف

---

## 🔗 لینک‌های مفید

- [QUICK_START.md](./QUICK_START.md) - راهنمای سریع
- [COMPLETE_PLAN_STATUS.md](./COMPLETE_PLAN_STATUS.md) - وضعیت پیاده‌سازی
- [HOW_TO_RUN_TESTS.md](./HOW_TO_RUN_TESTS.md) - راهنمای تست

---

## 📝 یادداشت‌ها

- **امنیت اولویت اول است** - هرگز credentials را در کد commit نکنید
- **Environment Variables** بهترین روش برای مدیریت secrets است
- **CORS** باید فقط برای دامنه‌های مورد نیاز باز باشد
- **Logging** در production باید در سطح Warning باشد

---

**آخرین به‌روزرسانی:** $(Get-Date -Format "yyyy-MM-dd HH:mm")

