# راهنمای سریع اجرای پروژه OnlineShop

## پیش‌نیازها (Prerequisites)

### 1. نصب Node.js
- از [nodejs.org](https://nodejs.org) دانلود و نصب کنید
- نسخه LTS توصیه می‌شود
- بررسی نصب: `node --version`

### 2. نصب .NET SDK
- از [dotnet.microsoft.com](https://dotnet.microsoft.com/download) دانلود و نصب کنید
- نسخه 8.0 یا بالاتر
- بررسی نصب: `dotnet --version`

### 3. نصب PostgreSQL
- از [postgresql.org](https://www.postgresql.org/download/) دانلود و نصب کنید
- اطمینان حاصل کنید که سرویس PostgreSQL در حال اجرا است
- بررسی نصب: `psql --version`

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
dotnet run --urls "http://localhost:5000"
```

## آدرس‌ها (URLs)

- **فرانت‌اند**: http://localhost:8080
- **بک‌اند API**: http://localhost:5000/api
- **Swagger UI**: http://localhost:5000/swagger

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

### مشکل Database Connection
اگر خطای اتصال به دیتابیس دریافت کردید:
1. اطمینان حاصل کنید که PostgreSQL در حال اجرا است
2. بررسی کنید که Connection String در `appsettings.json` صحیح است
3. دستور `dotnet ef database update` را در پوشه `src/WebAPI` اجرا کنید

### مشکل پورت
اگر پورت‌ها در حال استفاده هستند:
1. فرانت‌اند: پورت 8080
2. بک‌اند: پورت 5000
3. برای تغییر پورت‌ها، فایل‌های `package.json` و `Program.cs` را ویرایش کنید

### مشکل SSL Certificate
اگر خطای SSL دریافت کردید:
1. در مرورگر به `http://localhost:5000` بروید (نه https)
2. اطمینان حاصل کنید که از HTTP استفاده می‌کنید

## ساختار پروژه

```
OnlineShop/
├── src/
│   └── WebAPI/          # بک‌اند ASP.NET Core
├── presentation/         # فرانت‌اند HTML/CSS/JS
├── tests/               # تست‌ها
├── start-all.ps1        # اسکریپت PowerShell
├── start-all.bat        # اسکریپت Batch
└── QUICK_START.md       # این فایل
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

### اجرای Migration:
```bash
cd src/WebAPI
dotnet ef database update
```

## تنظیمات مهم

### Backend Configuration
- **پورت**: 5000 (HTTP)
- **HTTPS**: غیرفعال شده برای توسعه
- **CORS**: فعال برای پورت 8080

### Frontend Configuration
- **پورت**: 8080
- **API URL**: http://localhost:5000/api
- **Server**: http-server

## پشتیبانی

اگر مشکلی داشتید:
1. ابتدا این راهنما را کامل بخوانید
2. لاگ‌ها را بررسی کنید
3. پورت‌ها را چک کنید
4. مرورگر را با CORS غیرفعال امتحان کنید
5. اطمینان حاصل کنید که تمام پیش‌نیازها نصب شده‌اند

## نکات مهم

- **همیشه از HTTP استفاده کنید** (نه HTTPS) برای جلوگیری از مشکلات SSL
- **PostgreSQL باید در حال اجرا باشد** قبل از شروع پروژه
- **پورت‌ها باید آزاد باشند** (8080 برای فرانت، 5000 برای بک)
- **CORS تنظیم شده است** برای ارتباط فرانت و بک
