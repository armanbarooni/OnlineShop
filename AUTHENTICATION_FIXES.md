# تغییرات برای رفع مشکل Authentication

## 🔍 مشکل اصلی
`AuthHelper` نمی‌توانست توکن را از response دریافت کند چون:
- به دنبال `data.accessToken` می‌گشت
- اما `AuthController` در login با email/password مستقیماً `AuthResponseDto` برمی‌گرداند (بدون wrapper)

## ✅ تغییرات انجام شده

### 1. در `tests/OnlineShop.IntegrationTests/Helpers/AuthHelper.cs`:

#### تغییر در `TryHardcodedAdminLoginAsync`:
```csharp
// قبل:
var token = JsonHelper.GetNestedProperty(content, "data", "accessToken") 
            ?? JsonHelper.GetNestedProperty(content, "accessToken")
            ?? JsonHelper.GetNestedProperty(content, "access_token");

// بعد:
// AuthController returns AuthResponseDto directly (not wrapped in Result)
var token = JsonHelper.GetNestedProperty(content, "accessToken")
            ?? JsonHelper.GetNestedProperty(content, "access_token")
            ?? JsonHelper.GetNestedProperty(content, "data", "accessToken");
```

**توضیح**: اول `accessToken` مستقیم را چک می‌کند، بعد nested را.

#### تغییر در `TryLoginAsync`:
```csharp
// قبل:
var token = JsonHelper.GetNestedProperty(content, "data", "accessToken");

// بعد:
// Phone login returns AuthResponseDto directly
var token = JsonHelper.GetNestedProperty(content, "accessToken")
            ?? JsonHelper.GetNestedProperty(content, "data", "accessToken");
```

#### تغییر در `TryRegisterAsync`:
```csharp
// قبل:
var token = JsonHelper.GetNestedProperty(content, "data", "accessToken");

// بعد:
// Phone registration might return Result<AuthResponseDto> or AuthResponseDto
var token = JsonHelper.GetNestedProperty(content, "data", "accessToken")
            ?? JsonHelper.GetNestedProperty(content, "accessToken");
```

### 2. در `tests/OnlineShop.IntegrationTests/Infrastructure/CustomWebApplicationFactory.cs`:

#### اضافه شده:
- متد `SeedTestData` برای ایجاد کاربر Admin
- کاربر Admin با مشخصات:
  - Username: `09123456789`
  - Email: `admin@test.com`
  - Password: `AdminPassword123!`
  - Role: `Admin`

#### حذف شده:
- پراپرتی `IsActive` که در `ApplicationUser` وجود نداشت

## 📊 نتیجه مورد انتظار

با این تغییرات:
1. ✅ `AuthHelper` می‌تواند از هر دو نوع response (wrapped و unwrapped) توکن را بگیرد
2. ✅ کاربر Admin پیش‌فرض در دیتابیس تست وجود دارد
3. ✅ Login با email/password کار می‌کند
4. ✅ تست‌ها می‌توانند authentication کنند

## 🎯 تست کردن

برای تست کردن تغییرات:

```bash
# بیلد کردن پروژه
dotnet build

# اجرای تست‌ها
dotnet test

# یا فقط Integration Tests
dotnet test tests/OnlineShop.IntegrationTests/OnlineShop.IntegrationTests.csproj
```

## 📈 پیش‌بینی نتایج

اگر این تغییرات کار کنند:
- **قبل**: 70/160 تست Integration موفق (43.75%)
- **بعد**: ~115/160 تست Integration موفق (~72%)
- **بهبود کلی**: از 75% به ~85% ⬆️ +10%

## 🔍 نکات مهم

### Response Structure در AuthController:

1. **Login با email/password** (line 74):
   ```csharp
   return Ok(tokens); // مستقیماً AuthResponseDto
   ```
   Response:
   ```json
   {
     "accessToken": "...",
     "refreshToken": "...",
     "email": "...",
     "roles": [...]
   }
   ```

2. **Login با phone** (line 240):
   ```csharp
   return Ok(result); // Result<AuthResponseDto>
   ```
   Response:
   ```json
   {
     "isSuccess": true,
     "data": {
       "accessToken": "...",
       "refreshToken": "...",
       ...
     }
   }
   ```

3. **Register با phone** (line 220):
   ```csharp
   return CreatedAtAction(nameof(Login), result); // Result<AuthResponseDto>
   ```
   Response:
   ```json
   {
     "isSuccess": true,
     "data": {
       "accessToken": "...",
       ...
     }
   }
   ```

## ✅ Checklist تغییرات

- [x] اصلاح `AuthHelper.TryHardcodedAdminLoginAsync`
- [x] اصلاح `AuthHelper.TryLoginAsync`
- [x] اصلاح `AuthHelper.TryRegisterAsync`
- [x] ایجاد `SeedTestData` در `CustomWebApplicationFactory`
- [x] حذف `IsActive` از کاربر Admin تست
- [x] اضافه کردن logging برای debug

## 🐛 اگر هنوز کار نکرد

چک کنید:
1. آیا JWT configuration در `appsettings.json` صحیح است؟
2. آیا کاربر Admin با موفقیت ایجاد می‌شود؟
3. آیا password hash صحیح است؟
4. لاگ‌های `AuthHelper` را بررسی کنید (Console output)




