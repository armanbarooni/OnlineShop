# 🔧 راهنمای رفع مشکل CSS و JavaScript در IIS

## 🚨 مشکل
وقتی سایت از لحاظ بصری "داغون و بهم ریخته" است و CSS و JavaScript لود نمی‌شوند، مشکل معمولاً از مسیرهای نسبی (Relative Paths) است.

## ✅ راه‌حل‌های اعمال شده

### 1. اضافه کردن `<base href="/fa/">` به HTML
در فایل `src/WebAPI/wwwroot/fa/index.html`، tag زیر اضافه شده است:
```html
<base href="/fa/">
```
این باعث می‌شود که تمام مسیرهای نسبی نسبت به `/fa/` resolve شوند.

### 2. اصلاح ترتیب Middleware
در `Program.cs`، ترتیب middleware به این شکل تغییر کرد:
```csharp
app.UseDefaultFiles();  // باید قبل از UseStaticFiles باشد
app.UseStaticFiles();
```

### 3. بهبود MIME Types در web.config
MIME types برای فایل‌های CSS، JS، و Fonts به `web.config` اضافه شد.

## 📋 کارهای لازم روی سرور

### 1. Publish کردن پروژه
```powershell
cd C:\Users\arman\source\repos\onlintest
dotnet publish src/WebAPI/OnlineShop.WebAPI.csproj -c Release -o C:\site
```

### 2. بررسی فایل‌های استاتیک
مطمئن شوید که فایل‌های زیر در `C:\site\wwwroot\fa\` وجود دارند:
- `assets/css/app.css`
- `assets/js/app.js`
- `assets/images/...`
- `assets/fonts/...`

### 3. بررسی دسترسی‌ها
```powershell
# بررسی دسترسی IIS_IUSRS به فایل‌ها
$acl = Get-Acl C:\site\wwwroot
$acl.Access | Where-Object { $_.IdentityReference -like "*IIS*" }

# اگر دسترسی نبود، اضافه کنید:
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($accessRule)
Set-Acl C:\site\wwwroot $acl
```

### 4. Restart IIS
```powershell
iisreset
```

## 🔍 بررسی و دیباگ

### 1. بررسی در مرورگر (DevTools)
1. باز کردن `http://YOUR_IP:8080/fa/index.html`
2. F12 → تب **Network**
3. Refresh صفحه
4. بررسی Status Code فایل‌های CSS/JS:
   - ✅ **200 OK**: فایل لود شده
   - ❌ **404 Not Found**: مسیر فایل اشتباه است
   - ❌ **403 Forbidden**: مشکل دسترسی

### 2. بررسی URL فایل‌های 404
اگر فایلی 404 گرفته:
- URL کامل را کپی کنید (مثلاً: `http://YOUR_IP:8080/fa/assets/css/app.css`)
- بررسی کنید که فایل در مسیر `C:\site\wwwroot\fa\assets\css\app.css` وجود دارد

### 3. بررسی Logs
```powershell
# بررسی stdout logs
Get-Content C:\site\logs\stdout*.log -Tail 50

# بررسی Event Logs
Get-EventLog -LogName Application -Source "IIS*" -Newest 10 | Format-List TimeGenerated, Message
```

## 🛠️ راه‌حل‌های اضافی (در صورت نیاز)

### اگر هنوز مشکل دارید:

#### 1. تغییر مسیرها به Absolute
اگر `<base href>` کار نکرد، می‌توانید مسیرها را در HTML به absolute تغییر دهید:
```html
<!-- قبل -->
<link rel="stylesheet" href="assets/css/app.css">

<!-- بعد -->
<link rel="stylesheet" href="/fa/assets/css/app.css">
```

#### 2. فعال کردن URL Rewrite Module
اگر URL Rewrite Module نصب است، می‌توانید بخش rewrite را در `web.config` فعال کنید:
```xml
<rewrite>
  <rules>
    <rule name="SPA Routes" stopProcessing="true">
      <match url=".*" />
      <conditions logicalGrouping="MatchAll">
        <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
        <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
        <add input="{REQUEST_URI}" pattern="^/api/" negate="true" />
      </conditions>
      <action type="Rewrite" url="/fa/index.html" />
    </rule>
  </rules>
</rewrite>
```

#### 3. استفاده از UseStaticFiles با Options
اگر مشکل ادامه دارد، می‌توانید StaticFilesOptions را تنظیم کنید:
```csharp
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 year
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});
```

## 📝 چک‌لیست نهایی

- [ ] `<base href="/fa/">` به `index.html` اضافه شده
- [ ] پروژه publish شده و فایل‌ها در `C:\site\wwwroot\fa\` هستند
- [ ] دسترسی IIS_IUSRS به فایل‌ها تنظیم شده
- [ ] IIS restart شده
- [ ] در DevTools بررسی شده که فایل‌های CSS/JS با Status 200 لود می‌شوند
- [ ] سایت به درستی نمایش داده می‌شود

## 🆘 در صورت مشکل
اگر هنوز مشکل دارید، لطفاً:
1. Screenshot از تب Network در DevTools بفرستید
2. URL کامل یکی از فایل‌های 404 را بفرستید
3. محتوای stdout logs را بررسی کنید

