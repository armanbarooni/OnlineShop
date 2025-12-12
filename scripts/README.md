# Scripts راهنما

این پوشه شامل اسکریپت‌های PowerShell برای مدیریت پروژه است.

## اسکریپت‌های موجود

### 1. `start-all.ps1` (در روت پروژه)
شروع پروژه برای توسعه (Development)

**استفاده:**
```powershell
.\start-all.ps1
```

**کارهایی که انجام می‌دهد:**
- بررسی نصب Node.js و .NET SDK
- نصب dependencies در صورت نیاز
- اجرای migration های دیتابیس
- شروع frontend (http://localhost:8080)
- شروع backend (http://localhost:5000)

**نکته:** این اسکریپت فایل‌های `presentation` را به `wwwroot` کپی **نمی‌کند**. برای development، frontend از پوشه `presentation` اجرا می‌شود.

---

### 2. `prepare-publish.ps1`
آماده‌سازی فایل‌های frontend برای publish

**استفاده:**
```powershell
.\scripts\prepare-publish.ps1
# یا با environment مشخص:
.\scripts\prepare-publish.ps1 -Environment production
```

**کارهایی که انجام می‌دهد:**
- کپی فایل‌های `presentation` به `src/WebAPI/wwwroot/fa`
- حذف فایل‌های غیرضروری (node_modules، package.json و ...)
- کپی config مناسب برای environment

**نکته:** این اسکریپت را قبل از publish اجرا کنید.

---

### 3. `publish.ps1`
Build و Publish کامل پروژه

**استفاده:**
```powershell
.\scripts\publish.ps1
# یا با تنظیمات سفارشی:
.\scripts\publish.ps1 -Configuration Release -Environment production -OutputPath "./publish"
```

**کارهایی که انجام می‌دهد:**
1. آماده‌سازی فایل‌های frontend (با استفاده از `prepare-publish.ps1`)
2. Build پروژه
3. Publish پروژه به پوشه مشخص شده

**پارامترها:**
- `-Configuration`: Configuration برای build (پیش‌فرض: `Release`)
- `-Environment`: Environment برای frontend config (پیش‌فرض: `production`)
- `-OutputPath`: مسیر خروجی publish (پیش‌فرض: `./publish`)

---

### 4. `sync-frontend.ps1`
همگام‌سازی فایل‌های frontend (استفاده داخلی)

این اسکریپت توسط `prepare-publish.ps1` استفاده می‌شود.

---

## Workflow پیشنهادی

### برای Development:
```powershell
# فقط این را اجرا کنید:
.\start-all.ps1
```

### برای Publish:
```powershell
# گزینه 1: استفاده از publish script (پیشنهادی)
.\scripts\publish.ps1

# گزینه 2: دستی
.\scripts\prepare-publish.ps1
dotnet publish src/WebAPI/OnlineShop.WebAPI.csproj -c Release -o ./publish
```

---

## نکات مهم

1. **در Development:** فایل‌های `presentation` به `wwwroot` کپی نمی‌شوند. Frontend از پوشه `presentation` اجرا می‌شود.

2. **قبل از Publish:** حتماً `prepare-publish.ps1` را اجرا کنید تا فایل‌های frontend به `wwwroot` کپی شوند.

3. **Environment Variables:** بعد از publish، environment variables و connection strings را در سرور تنظیم کنید.

4. **Database Migration:** Migration ها به صورت خودکار در `start-all.ps1` اجرا می‌شوند. برای production، migration را دستی اجرا کنید:
   ```powershell
   dotnet ef database update --project src/WebAPI/OnlineShop.WebAPI.csproj
   ```

