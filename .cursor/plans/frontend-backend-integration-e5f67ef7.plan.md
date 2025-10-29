<!-- e5f67ef7-6d2e-4025-8af6-ae04b92e5414 a2dca767-df0a-444f-baa3-47e85ed121c6 -->
# پلن رفع مشکلات Authentication و Validation

## مشکلات شناسایی شده

### 1. مشکل مدیریت خطا در api-client.js
- **مشکل**: فقط `data.message` چک می‌شود، در حالی که backend گاهی string یا array برمی‌گرداند
- **تأثیر**: خطاهای validation به درستی نمایش داده نمی‌شوند
- **موقعیت**: `presentation/assets/js/api-client.js` خط 65-88

### 2. مشکل Validation Response در Backend
- **مشکل**: `BadRequest()` در AuthController response structure یکپارچه ندارد
- **تأثیر**: ایمیل تکراری شناسایی می‌شود ولی پیام خطا درست نمایش داده نمی‌شود
- **موقعیت**: `src/WebAPI/Controllers/AuthController.cs` خط 105-145

### 3. مشکل ساخت ایمیل موقت در Frontend
- **مشکل**: وقتی email خالی است، یک ایمیل موقت ساخته می‌شود که validation را دور می‌زند
- **تأثیر**: کاربران می‌توانند بدون ایمیل واقعی ثبت‌نام کنند
- **موقعیت**: `presentation/assets/js/auth-service.js` خط 112-144

### 4. مشکل handleError در api-client.js
- **مشکل**: `handleError()` برای axios نوشته شده، در حالی که از fetch استفاده می‌شود
- **تأثیر**: خطاها به درستی مدیریت نمی‌شوند
- **موقعیت**: `presentation/assets/js/api-client.js` خط 232-244

### 5. مشکل بررسی JWT در Headers
- **وضعیت**: ✅ JWT به درستی در header قرار داده می‌شود
- **نیازمند**: بررسی اطمینان از بروزرسانی token پس از refresh

## فاز 1: اصلاح Backend (Backend Fixes)

### 1.1 استانداردسازی BadRequest Responses
**فایل**: `src/WebAPI/Controllers/AuthController.cs`

- **خط 109-113**: اصلاح validation اولیه
  - تغییر از string به object با property `message`
  
- **خط 115-120**: اصلاح چک ایمیل تکراری
  - بازگرداندن response یکپارچه با structure: `{ message: "...", code: "EMAIL_EXISTS" }`
  
- **خط 130-136**: اصلاح Identity validation errors
  - تبدیل `IEnumerable<string>` به response یکپارچه
  - ساختار: `{ message: "...", errors: [...] }`

### 1.2 ایجاد Response Model برای Errors
**فایل جدید**: `src/Application/DTOs/Common/ErrorResponseDto.cs`
- ایجاد DTO یکپارچه برای error responses
- Properties: `message`, `errors[]`, `code`

## فاز 2: اصلاح Frontend Error Handling

### 2.1 بهبود api-client.js
**فایل**: `presentation/assets/js/api-client.js`

- **خط 65-88**: اصلاح مدیریت response errors
  - پشتیبانی از string, object, array
  - استخراج پیام خطا از ساختارهای مختلف
  
- **خط 232-244**: اصلاح handleError
  - تطابق با fetch API
  - مدیریت Error objects صحیح

- **خط 198-211**: بهبود setToken/setTokens
  - اطمینان از بروزرسانی instance variable پس از refresh

### 2.2 اصلاح auth-service.js
**فایل**: `presentation/assets/js/auth-service.js`

- **خط 112-144**: اصلاح register function
  - حذف ساخت ایمیل موقت
  - افزودن validation در frontend برای email
  - مدیریت بهتر خطاها

- **خط 137-144**: بهبود error handling در register
  - استفاده صحیح از error message از api-client

## فاز 3: بهبود Validation

### 3.1 Frontend Validation
**فایل**: `presentation/register.html`

- **خط 600-649**: بهبود submitForm
  - اضافه کردن client-side validation برای email
  - بررسی وجود email قبل از ارسال

### 3.2 Backend Validation
**فایل**: `src/Application/DTOs/Auth/RegisterDto.cs`
- بررسی اینکه آیا FluentValidation validator وجود دارد
- در صورت نیاز، ایجاد/بهبود validator

## فاز 4: تست و اعتبارسنجی

### 4.1 تست Scenarios
1. **ثبت‌نام با ایمیل تکراری**
   - باید خطای واضح نمایش داده شود
   - باید 400 BadRequest برگردد
   
2. **ثبت‌نام بدون ایمیل**
   - باید validation error در frontend نشان داده شود
   - نباید درخواست به backend ارسال شود

3. **ثبت‌نام با validation errors از Identity**
   - باید تمام خطاها نمایش داده شوند
   - response structure یکپارچه باشد

4. **Login با credentials نامعتبر**
   - باید پیام خطای مناسب نمایش داده شود

5. **JWT در Headers**
   - بررسی اینکه JWT در تمام requests protected اضافه می‌شود
   - بررسی refresh token mechanism

## فاز 5: مستندسازی

### 5.1 مستندات Error Handling
- **فایل**: `docs/ERROR_HANDLING.md`
  - ساختار Error Responses
  - لیست Error Codes
  - راهنمای مدیریت خطاها در Frontend

### 5.2 به‌روزرسانی مستندات موجود
- به‌روزرسانی `COMPLETE_IMPLEMENTATION_GUIDE.md`
- به‌روزرسانی `TROUBLESHOOTING.md`

## جزئیات تغییرات

### تغییر 1: api-client.js - بهبود Error Parsing
```javascript
// قبل از خط 65
const data = await response.json();

if (!response.ok) {
    // بهبود: پشتیبانی از ساختارهای مختلف
    let errorMessage = '';
    
    if (typeof data === 'string') {
        errorMessage = data;
    } else if (data?.message) {
        errorMessage = data.message;
    } else if (Array.isArray(data)) {
        errorMessage = data.join(', ');
    } else if (data?.errors && Array.isArray(data.errors)) {
        errorMessage = data.errors.join(', ');
    } else if (data?.title) {
        errorMessage = data.title;
    } else {
        errorMessage = `HTTP error! status: ${response.status}`;
    }
    
    throw new Error(errorMessage);
}
```

### تغییر 2: AuthController.cs - استانداردسازی Responses
```csharp
// تغییر خط 112
return BadRequest(new { message = "ایمیل و رمز عبور الزامی است" });

// تغییر خط 119
return BadRequest(new { 
    message = "کاربری با این ایمیل قبلاً ثبت‌نام کرده است",
    code = "EMAIL_EXISTS"
});

// تغییر خط 135
var errorMessages = result.Errors.Select(e => e.Description).ToList();
return BadRequest(new { 
    message = string.Join(". ", errorMessages),
    errors = errorMessages
});
```

### تغییر 3: auth-service.js - حذف ایمیل موقت
```javascript
async register(userData) {
    try {
        // افزودن validation
        if (!userData.email || !userData.email.trim()) {
            return {
                success: false,
                error: 'ایمیل الزامی است'
            };
        }
        
        const response = await this.apiClient.post('/auth/register', {
            firstName: userData.firstName,
            lastName: userData.lastName,
            phoneNumber: userData.phone,
            email: userData.email.trim(), // حذف ساخت ایمیل موقت
            password: userData.password,
            confirmPassword: userData.password
        });
        // ...
    }
}
```

## اولویت اجرا

1. **بالا**: اصلاح api-client.js (فاز 2.1)
2. **بالا**: اصلاح AuthController.cs (فاز 1.1)
3. **متوسط**: اصلاح auth-service.js (فاز 2.2)
4. **متوسط**: بهبود validation (فاز 3)
5. **پایین**: مستندسازی (فاز 5)

## نتیجه نهایی

پس از اجرای این پلن:
- ✅ خطاهای validation به درستی نمایش داده می‌شوند
- ✅ ایمیل تکراری شناسایی و پیام مناسب نمایش داده می‌شود
- ✅ ساخت ایمیل موقت غیرممکن می‌شود
- ✅ Error handling یکپارچه و قابل اعتماد می‌شود
- ✅ JWT در همه requests محافظت‌شده موجود است


### To-dos

- [ ] اصلاح مدیریت خطا در api-client.js برای پشتیبانی از ساختارهای مختلف response
- [ ] اصلاح handleError در api-client.js برای سازگاری با fetch API
- [ ] استانداردسازی BadRequest responses در AuthController
- [ ] بهبود پاسخ خطای ایمیل تکراری در AuthController
- [ ] بهبود پاسخ خطاهای Identity validation در AuthController
- [ ] حذف ساخت ایمیل موقت در auth-service.js register function
- [ ] افزودن validation ایمیل در frontend قبل از ارسال درخواست
- [ ] بهبود error handling در auth-service.js register
- [ ] تست ثبت‌نام با ایمیل تکراری
- [ ] تست ثبت‌نام بدون ایمیل
- [ ] تست validation errors از Identity
- [ ] تست JWT در headers تمام requests
- [ ] ایجاد مستندات ERROR_HANDLING.md
- [ ] به‌روزرسانی COMPLETE_IMPLEMENTATION_GUIDE.md