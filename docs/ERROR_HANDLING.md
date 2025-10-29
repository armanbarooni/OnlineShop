# مستندات مدیریت خطا (Error Handling Documentation)

## مقدمه

این مستندات ساختار مدیریت خطا در پروژه OnlineShop را شرح می‌دهد. شامل ساختار Error Responses، کدهای خطا و راهنمای مدیریت خطاها در Frontend است.

## ساختار Error Responses

### Backend Error Response Structure

تمام liveم BadRequest responses از backend به صورت یکپارچه برمی‌گردند:

```json
{
  "message": "پیام خطای اصلی",
  "errors": ["خطای 1", "خطای 2"],  // اختیاری - برای خطاهای چندتایی
  "code": "ERROR_CODE"              // اختیاری - کد خطا برای شناسایی نوع خطا
}
```

### نمونه Response های مختلف

#### 1. Validation Error (خطای اعتبارسنجی)
```json
{
  "message": "ایمیل و رمز عبور الزامی است"
}
```

#### 2. Duplicate Email Error (ایمیل تکراری)
```json
{
  "message": "کاربری با این ایمیل قبلاً ثبت‌نام کرده است",
  "code": "EMAIL_EXISTS"
}
```

#### 3. Identity Validation Errors (خطاهای اعتبارسنجی Identity)
```json
{
  "message": "رمز عبور باید حداقل 6 کاراکتر باشد. رمز عبور باید شامل حروف بزرگ باشد.",
  "errors": [
    "رمز عبور باید حداقل 6 کاراکتر باشد",
    "رمز عبور باید شامل حروف بزرگ باشد"
  ],
  "code": "VALIDATION_ERROR"
}
```

## کدهای خطا (Error Codes)

| کد خطا | توضیحات | HTTP Status |
|--------|---------|-------------|
| `EMAIL_EXISTS` | ایمیل قبلاً ثبت شده است | 400 |
| `VALIDATION_ERROR` | خطاهای اعتبارسنجی | 400 |
| `UNAUTHORIZED` | احراز هویت ناموفق | 401 |
| `NOT_FOUND` | منبع یافت نشد | 404 |
| `INTERNAL_ERROR` | خطای داخلی سرور | 500 |

## مدیریت خطا در Frontend

### api-client.js

کلاس `ApiClient` به صورت خودکار خطاها را مدیریت می‌کند و از ساختارهای مختلف response پشتیبانی می‌کند:

```javascript
// پشتیبانی از:
// 1. String responses
// 2. Object responses با property message
// 3. Array responses
// 4. Object responses با property errors
// 5. Object responses با property title
```

### auth-service.js

سرویس احراز هویت خطاها را handle می‌کند و به صورت یکپارچه برمی‌گرداند:

```javascript
// همه متدها این ساختار را برمی‌گردانند:
{
  success: true/false,
  error: "پیام خطا" // در صورت وجود خطا
}
```

### مثال استفاده در Frontend

```javascript
const result = await authService.register(formData);

if (!result.success) {
  // نمایش پیام خطا به کاربر
  showError(result.error);
} else {
  // انجام عملیات موفق
  redirectToDashboard();
}
```

## Validation در Frontend

### Email Validation

```javascript
// در register.html و auth-service.js
const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
if (!emailRegex.test(email)) {
  // خطای فرمت ایمیل
}
```

### Password Validation

```javascript
// در register.html
function checkPasswordStrength() {
  // بررسی طول
  // بررسی حروف بزرگ/کوچک
  // بررسی اعداد
  // بررسی کاراکترهای خاص
}
```

## خطاهای رایج و راه‌حل‌ها

### 1. خطای "ایمیل الزامی است"

**علت**: ایمیل وارد نشده یا خالی است.

**راه‌حل**: 
- اطمینان از پر بودن فیلد ایمیل
- بررسی validation در frontend

### 2. خطای "کاربری با این ایمیل قبلاً ثبت‌نام کرده است"

**علت**: ایمیل قبلاً در سیستم ثبت شده است.

**راه‌حل**: 
- استفاده از ایمیل دیگر
- استفاده از فراموشی رمز عبور

### 3. خطاهای Validation Identity

**علت**: رمز عبور یا اطلاعات دیگر معیارهای امنیتی را برآورده نمی‌کند.

**راه‌حل**: 
- بررسی پیام‌های خطا در `errors` array
- اصلاح اطلاعات ورودی بر اساس خطاها

## تست Error Handling

### تست با curl

```bash
# تست ثبت‌نام با ایمیل تکراری
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"existing@test.com","password":"Test123!"}'
```

### تست در Browser Console

```javascript
// تست در Console مرورگر
const result = await window.authService.register({
  firstName: "Test",
  lastName: "User",
  email: "test@test.com",
  phone: "09123456789",
  password: "Test123!"
});

console.log(result);
```

## Best Practices

1. **همیشه success را چک کنید**: قبل از استفاده از نتیجه، `result.success` را بررسی کنید.

2. **نمایش خطا به کاربر**: خطاها را به صورت user-friendly به کاربر نمایش دهید.

3. **Logging**: خطاها را در console لاگ کنید برای debugging.

4. **Validation دو طرفه**: هم در frontend و هم در backend validation انجام دهید.

5. **پیام‌های خطا واضح**: پیام‌های خطا باید واضح و قابل فهم باشند.

## تغییرات اخیر

### تاریخ: 2025-10-25

1. **استانداردسازی Error Responses**: تمام BadRequest responses اکنون ساختار یکپارچه دارند.

2. **بهبود Error Parsing**: api-client.js اکنون از ساختارهای مختلف response پشتیبانی می‌کند.

3. **حذف ساخت ایمیل موقت**: امکان ساخت ایمیل موقت در frontend حذف شد.

4. **افزودن Email Validation**: validation ایمیل در frontend اضافه شد.

## منابع مرتبط

- [API Endpoints Documentation](./API_ENDPOINTS.md)
- [Frontend Routes](./FRONTEND_ROUTES.md)
- [Setup Guide](./SETUP_GUIDE.md)
- [Troubleshooting](./TROUBLESHOOTING.md)

