# راهنمای راه‌اندازی پروژه OnlineShop

## پیش‌نیازها (Prerequisites)

### 1. نصب Node.js
- از [nodejs.org](https://nodejs.org) دانلود و نصب کنید
- نسخه LTS توصیه می‌شود
- بررسی نصب: `node --version`

### 2. نصب .NET SDK
- از [dotnet.microsoft.com](https://dotnet.microsoft.com/download) دانلود و نصب کنید
- نسخه 8.0 یا بالاتر
- بررسی نصب: `dotnet --version`

## راه‌اندازی سریع (Quick Setup)

### روش 1: اجرای خودکار (توصیه می‌شود)

#### Windows PowerShell:
```powershell
.\start-all.ps1
```

#### Windows Command Prompt:
```cmd
start-all.bat
```

### روش 2: اجرای دستی

#### 1. اجرای فرانت‌اند:
```bash
cd presentation
npm install
npm start
```

#### 2. اجرای بک‌اند (در ترمینال جدید):
```bash
cd src/WebAPI
dotnet run --urls "https://localhost:7025"
```

## آدرس‌ها (URLs)

- **فرانت‌اند**: http://localhost:8080
- **بک‌اند API**: https://localhost:7025/api
- **Swagger UI**: https://localhost:7025/swagger

## صفحات فرانت‌اند

- `register.html` - صفحه ثبت‌نام
- `login.html` - صفحه ورود
- `product.html` - صفحه محصول
- `user-panel-*.html` - صفحات پنل کاربری

## عیب‌یابی (Troubleshooting)

### مشکل CORS
اگر خطای CORS دریافت کردید:
1. مرورگر را با CORS غیرفعال باز کنید:
   - Chrome: `chrome.exe --disable-web-security --user-data-dir="C:/temp"`
   - Edge: `msedge.exe --disable-web-security --user-data-dir="C:/temp"`

### مشکل SSL Certificate
اگر خطای SSL دریافت کردید:
1. در مرورگر به `https://localhost:7025` بروید
2. "Advanced" کلیک کنید
3. "Proceed to localhost" کلیک کنید

### مشکل پورت
اگر پورت‌ها در حال استفاده هستند:
1. فرانت‌اند: پورت 8080
2. بک‌اند: پورت 7025
3. برای تغییر پورت‌ها، فایل‌های `package.json` و `Program.cs` را ویرایش کنید

## ساختار پروژه

```
OnlineShop/
├── src/
│   └── WebAPI/          # بک‌اند ASP.NET Core
├── presentation/         # فرانت‌اند HTML/CSS/JS
├── tests/               # تست‌ها
├── start-all.ps1        # اسکریپت PowerShell
├── start-all.bat        # اسکریپت Batch
└── README-SETUP.md      # این فایل
```

## دستورات مفید

### اجرای تست‌ها:
```bash
dotnet test
```

### ساخت پروژه:
```bash
dotnet build
```

### پاک کردن cache:
```bash
dotnet clean
dotnet restore
```

## پشتیبانی

اگر مشکلی داشتید:
1. ابتدا این راهنما را کامل بخوانید
2. لاگ‌ها را بررسی کنید
3. پورت‌ها را چک کنید
4. مرورگر را با CORS غیرفعال امتحان کنید
