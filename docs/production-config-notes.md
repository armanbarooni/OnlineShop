# نکات مهم برای تنظیمات Production

## ⚠️ توجه

فایل `appsettings.Production.json` برای **تست لوکال** تنظیم شده است و از `localhost` استفاده می‌کند.

## قبل از Deploy به سرور واقعی

قبل از deploy به Production واقعی، این مقادیر را در `appsettings.Production.json` تغییر دهید:

### 1. ConnectionString (خط 10-12)
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=YOUR_DB_HOST;Port=5432;Database=OnlineShop;Username=YOUR_USER;Password=YOUR_PASSWORD;Include Error Detail=false"
}
```

**تغییرات:**
- `Host=localhost` → `Host=YOUR_DB_HOST` (آدرس واقعی سرور دیتابیس)
- `Username=postgres` → `Username=YOUR_USER` (نام کاربری واقعی)
- `Password=1234` → `Password=YOUR_PASSWORD` (رمز عبور واقعی)

### 2. CORS Origins (خط 18-25)
```json
"Cors": {
  "AllowFrontend": [
    "http://130.185.73.167:8080",
    "http://172.24.32.1:8080"
  ]
}
```
فقط IP های Production را نگه دارید و `localhost` را حذف کنید.

### 3. BaseUrl (خط 26)
```json
"BaseUrl": "http://130.185.73.167:8080"
```
به آدرس Production واقعی تغییر دهید.

### 4. ZarinPal configuration
```json
"ZarinPal": {
  "IsSandbox": false,
  "BaseUrl": "https://api.zarinpal.com/pg/v4/payment",
  "PaymentPageBaseUrl": "https://www.zarinpal.com/pg/StartPay",
  "CallbackUrl": "https://yourdomain.com/api/Payment/callback",
  "FrontendResultUrl": "https://yourdomain.com/fa/cart.html"
}
```
این مقادیر را با دامنه‌ی واقعی خودتان یا از طریق environment variables تنظیم کنید.

### 5. JWT Secret (خط 13-17)
```json
"Jwt": {
  "Secret": "YOUR_STRONG_SECRET_KEY_MIN_32_CHARACTERS"
}
```
یک کلید مخفی قوی (حداقل 32 کاراکتر) تنظیم کنید.

## Profiles در launchSettings.json

- **`Production`**: برای تست Production روی لوکال (از localhost استفاده می‌کند)
- **`Production-Server`**: برای deploy واقعی روی سرور (از IP واقعی استفاده می‌کند)

## توصیه

برای توسعه روزانه، از profile **`http`** یا **`https`** (Development) استفاده کنید.

