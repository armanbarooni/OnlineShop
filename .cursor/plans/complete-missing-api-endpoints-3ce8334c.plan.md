<!-- 3ce8334c-1ddd-4b52-9804-fc88ef756281 abac4ca2-bded-4946-9244-38ca1c638702 -->
# رفع مشکل Encoding متون فارسی

## مشکل:
فایل‌های HTML در `presentation/` با encoding اشتباه ذخیره شده‌اند و متون فارسی به صورت mojibake نمایش داده می‌شوند (مثل: ط³ط§ط¹طھ ظ…ع†غŒ به جای "ساعت مچی").

## راه‌حل‌ها:

### Phase 1: تنظیم API Response Encoding
**فایل:** `src/WebAPI/Program.cs`
- اضافه کردن تنظیمات UTF-8 به `AddControllers()`:
  ```csharp
  builder.Services.AddControllers()
      .AddJsonOptions(options =>
      {
          options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
          options.JsonSerializerOptions.WriteIndented = false;
      });
  ```
- اضافه کردن middleware برای تنظیم Content-Type headers:
  ```csharp
  app.Use(async (context, next) =>
  {
      if (context.Response.ContentType?.StartsWith("application/json") == true)
      {
          context.Response.ContentType = "application/json; charset=utf-8";
      }
      await next();
  });
  ```
**جای قرارگیری:** بعد از `builder.Services.AddControllers()` (خط 60) و قبل از `app.MapControllers()` (خط 198)

### Phase 2: تبدیل فایل‌های HTML به UTF-8
**اسکریپت PowerShell:** `fix-html-encoding.ps1` (ایجاد جدید)
- تبدیل همه فایل‌های `.html` در `presentation/` از encoding فعلی به UTF-8
- حفظ ساختار و محتوای فایل‌ها
- استفاده از `[System.IO.File]::WriteAllText()` با UTF-8 encoding

**روش:**
```powershell
Get-ChildItem -Path "presentation\*.html" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding Default
    [System.IO.File]::WriteAllText($_.FullName, $content, [System.Text.Encoding]::UTF8)
    Write-Host "Converted: $($_.Name)"
}
```

### Phase 3: تنظیم http-server Headers
**فایل:** `presentation/package.json`
- تغییر اسکریپت start برای اضافه کردن charset header:
  ```json
  "start": "http-server -p 8080 -c-1 --cors --proxy http://localhost:5000"
  ```
- ایجاد فایل `.htaccess` در `presentation/` (اگر http-server از آن پشتیبانی کند):
  ```
  AddDefaultCharset UTF-8
  ```

**توجه:** http-server به صورت پیش‌فرض UTF-8 را پشتیبانی می‌کند، اما باید مطمئن شویم که فایل‌ها با UTF-8 ذخیره شده‌اند.

### Phase 4: بررسی و تست
1. تست یک فایل HTML نمونه (مثلاً `index.html`) بعد از تبدیل
2. تست API response با متن فارسی
3. تست در مرورگر برای اطمینان از نمایش صحیح

## ترتیب اجرا:
1. Phase 1 (API) - اولویت بالا
2. Phase 2 (HTML files) - اولویت بالا  
3. Phase 3 (http-server) - اولویت متوسط
4. Phase 4 (تست) - پس از اجرای مراحل قبلی

## فایل‌های مورد تغییر:
- `src/WebAPI/Program.cs` - اضافه کردن UTF-8 settings
- `presentation/*.html` (46 فایل) - تبدیل encoding
- `fix-html-encoding.ps1` - اسکریپت جدید برای تبدیل
- `presentation/package.json` - اصلاح اسکریپت start (اختیاری)


### To-dos

- [ ] اضافه کردن GET /api/userorder/{id}/track که از GetOrderTimelineQuery استفاده می‌کند
- [ ] ایجاد POST /api/checkout/calculate-shipping با CalculateShippingCommand
- [ ] ایجاد POST /api/checkout/apply-coupon برای اعمال coupon در checkout
- [ ] ایجاد DELETE /api/checkout/remove-coupon
- [ ] اصلاح GET /api/checkout/order-summary برای دریافت cartId از active cart اگر ارائه نشده
- [ ] ایجاد POST /api/cart/apply-coupon برای اعمال coupon به cart
- [ ] ایجاد DELETE /api/cart/remove-coupon
- [ ] ایجاد GET /api/cart/summary (اختیاری - اگر frontend نیاز داشته باشد)