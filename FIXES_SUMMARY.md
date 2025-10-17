# خلاصه اصلاحات Integration Tests

## تاریخ: 17 اکتبر 2025

### مشکلات شناسایی شده:
1. ✅ **خطای JSON Parsing**: استفاده از `dynamic` در برخی تست‌ها
2. ✅ **خطای Authentication**: `AuthHelper` نمی‌توانست به درستی token دریافت کند
3. ⚠️ **خطای MethodNotAllowed (405)**: برخی endpoint های API پیاده‌سازی نشده‌اند
4. ⚠️ **خطای Unauthorized (401)**: بسیاری از تست‌ها به دلیل مشکلات احراز هویت fail می‌شوند

---

## اصلاحات انجام شده:

### 1. AuthHelper.cs ✅
**مسیر**: `tests/OnlineShop.IntegrationTests/Helpers/AuthHelper.cs`

**تغییرات**:
- ساختار کامل AuthHelper بازنویسی شد
- سه روش مختلف برای احراز هویت اضافه شد:
  - `TryLoginAsync()`: تلاش برای ورود با شماره تلفن
  - `TryRegisterAsync()`: تلاش برای ثبت‌نام کاربر جدید
  - `TryHardcodedAdminLoginAsync()`: تلاش برای ورود با اکانت ادمین
- لاگ‌های بهتر برای debug اضافه شد
- به جای پرتاب exception، رشته خالی برمی‌گرداند

**قبل**:
```csharp
public static async Task<string> GetAdminTokenAsync(HttpClient client)
{
    // تلاش یک‌باره برای ثبت‌نام
    // اگر ناموفق بود، رشته خالی برمی‌گرداند
}
```

**بعد**:
```csharp
public static async Task<string> GetAdminTokenAsync(HttpClient client)
{
    // 1. تلاش برای login
    // 2. تلاش برای register
    // 3. تلاش برای hardcoded admin
    // 4. اگر همه ناموفق بودند، رشته خالی برمی‌گرداند
}
```

---

### 2. ProductVariantTests.cs ✅
**مسیر**: `tests/OnlineShop.IntegrationTests/Scenarios/ProductVariantTests.cs`

**تغییرات**:
- تبدیل `ReadFromJsonAsync<dynamic>()` به `JsonHelper.GetNestedProperty()`

**قبل**:
```csharp
var result = await response.Content.ReadFromJsonAsync<dynamic>();
result?.isSuccess.Should().Be(true);
```

**بعد**:
```csharp
var content = await response.Content.ReadAsStringAsync();
var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
isSuccess.Should().Be("true");
```

---

### 3. ProductReviewTests.cs ✅
**مسیر**: `tests/OnlineShop.IntegrationTests/Scenarios/ProductReviewTests.cs`

**تغییرات**:
- اصلاح JSON parsing در متد `GetReviewsByProduct_ShouldReturnReviews`

---

### 4. TestSmsService.cs ✅ (جدید)
**مسیر**: `tests/OnlineShop.IntegrationTests/Infrastructure/TestSmsService.cs`

**توضیحات**:
- سرویس test برای SMS که OTP کدها را capture می‌کند
- OTP های ارسال شده در یک Dictionary static ذخیره می‌شوند
- متد `GetLastOtpCode(phoneNumber)` برای دریافت آخرین OTP ارسال شده
- متد `ClearOtpCodes()` برای پاک کردن تمام OTP ها

**ویژگی‌ها**:
```csharp
public static string? GetLastOtpCode(string phoneNumber)
{
    // دریافت آخرین OTP code برای یک شماره تلفن
}

public static void ClearOtpCodes()
{
    // پاک کردن تمام OTP های ذخیره شده
}
```

**نحوه استفاده**:
```csharp
// 1. ارسال OTP
await client.PostAsJsonAsync("/api/auth/send-otp", new { PhoneNumber = "09123456789" });

// 2. دریافت کد
var otpCode = TestSmsService.GetLastOtpCode("09123456789");

// 3. استفاده از کد در login/register
await client.PostAsJsonAsync("/api/auth/login-phone", new { PhoneNumber = "09123456789", Code = otpCode });
```

---

### 5. CustomWebApplicationFactory.cs ✅
**مسیر**: `tests/OnlineShop.IntegrationTests/Infrastructure/CustomWebApplicationFactory.cs`

**تغییرات**:
- اضافه شدن `using OnlineShop.Application.Contracts.Services;`
- Replace کردن `ISmsService` با `TestSmsService`

**قبل**:
```csharp
builder.ConfigureServices(services =>
{
    // فقط Database configuration
});
```

**بعد**:
```csharp
builder.ConfigureServices(services =>
{
    // Database configuration
    
    // Replace ISmsService with TestSmsService
    var smsServiceDescriptor = services.SingleOrDefault(
        d => d.ServiceType == typeof(ISmsService));

    if (smsServiceDescriptor != null)
    {
        services.Remove(smsServiceDescriptor);
    }

    services.AddScoped<ISmsService, TestSmsService>();
});
```

---

### 6. AuthenticationFlowTests.cs ✅
**مسیر**: `tests/OnlineShop.IntegrationTests/Scenarios/AuthenticationFlowTests.cs`

**تغییرات**:
- اضافه شدن `using OnlineShop.IntegrationTests.Helpers;`
- تبدیل استفاده از `AuthResponseDto` به `JsonHelper`

---

## نتایج تست قبلی:

### آمار کلی:
- **کل تست‌ها**: 160
- **موفق**: 69 (43%)
- **ناموفق**: 89 (56%)
- **Skip شده**: 2 (1%)

### دسته‌بندی خطاها:

#### 1. Unauthorized (401) - 65 تست
**علت**: `AuthHelper` نمی‌تواند token معتبر دریافت کند

**تست‌های affected**:
- UserAddressTests
- SavedCartTests
- CompleteShoppingJourneyTests
- PaymentTests
- UserProfileTests
- ProductReviewTests
- OrderManagementTests
- UserReturnRequestTests
- WishlistTests
- CategoryHierarchyTests
- StockAlertTests
- ProductInventoryTests
- ProductVariantTests
- CouponTests

**راه‌حل پیشنهادی**:
1. ✅ اصلاح `AuthHelper` (انجام شده)
2. 🔄 بررسی Mock OTP Service
3. 🔄 بررسی تنظیمات Identity در `CustomWebApplicationFactory`

---

#### 2. MethodNotAllowed (405) - 15 تست
**علت**: Endpoint های API پیاده‌سازی نشده یا HTTP method اشتباه است

**Endpoint های affected**:
- `POST /api/savedcart/save`
- `DELETE /api/wishlist/{productId}`
- `POST /api/cart` (برخی سناریوها)

**راه‌حل پیشنهادی**:
1. بررسی Controller ها
2. اضافه کردن action method های گمشده
3. بررسی Routing

---

#### 3. JSON Parsing (RuntimeBinderException) - 1 تست
**علت**: استفاده از `dynamic` برای JsonElement

**تست affected**:
- ProductVariantTests.GetProductVariants_ForProduct_ShouldReturnAllVariants

**راه‌حل**:
- ✅ اصلاح شده (استفاده از JsonHelper)

---

#### 4. Invalid Data - 2 تست
**علت**: داده‌های ورودی نامعتبر یا response خالی

**تست‌های affected**:
- DebugTests.TestAuthentication_ShouldGetToken
- CategoryHierarchyTests.CreateCategory_WithParentId_ShouldCreateSubCategory

---

## اقدامات بعدی پیشنهادی:

### اولویت بالا:
1. 🔴 **بررسی MockSmsService**: اطمینان از اینکه OTP به درستی mock می‌شود
2. 🔴 **بررسی CustomWebApplicationFactory**: تنظیمات Identity و Database
3. 🔴 **اضافه کردن Endpoint های گمشده**: اصلاح Controller ها

### اولویت متوسط:
4. 🟡 **بررسی Authorization Policies**: اطمینان از تنظیمات صحیح
5. 🟡 **اصلاح Response Format**: یکپارچه‌سازی format های JSON response

### اولویت پایین:
6. 🟢 **بهبود Test Coverage**: اضافه کردن assertion های بیشتر
7. 🟢 **Cleanup**: حذف فایل‌های debug موقت

---

## فایل‌های اصلاح شده:
1. ✅ `tests/OnlineShop.IntegrationTests/Helpers/AuthHelper.cs`
2. ✅ `tests/OnlineShop.IntegrationTests/Scenarios/ProductVariantTests.cs`
3. ✅ `tests/OnlineShop.IntegrationTests/Scenarios/ProductReviewTests.cs`
4. ✅ `tests/OnlineShop.IntegrationTests/Scenarios/AuthenticationFlowTests.cs`
5. ✅ `tests/OnlineShop.IntegrationTests/Infrastructure/CustomWebApplicationFactory.cs`
6. ✅ `tests/OnlineShop.IntegrationTests/Infrastructure/TestSmsService.cs` (جدید)

## فایل‌های جدید ایجاد شده:
1. ✅ `tests/OnlineShop.IntegrationTests/Infrastructure/TestSmsService.cs` - سرویس test برای capture کردن OTP
2. ✅ `FIXES_SUMMARY.md` - فایل خلاصه اصلاحات

---

## نکات مهم:
- تمام تغییرات JSON parsing با استفاده از `JsonHelper` یکپارچه شده‌اند
- `AuthHelper` حالا سه روش مختلف برای authentication دارد
- Log های بهتری برای debug اضافه شده‌اند
- همه تست‌ها به درستی compile می‌شوند (بدون خطای syntax)

---

## چک لیست قبل از تست مجدد:
- [ ] بررسی `MockSmsService` در `CustomWebApplicationFactory`
- [ ] بررسی `ApplicationDbContext` و seed data
- [ ] بررسی Controller های `SavedCart`, `Wishlist`, `Cart`
- [ ] اجرای تست‌های debug برای بررسی authentication
- [ ] اجرای تست‌های کامل

---

---

## آخرین تغییرات (Round 2):

### اصلاحات نهایی ✅

1. **TestSmsService.cs**: سرویس capture کردن OTP اضافه شد
2. **CustomWebApplicationFactory.cs**: ISmsService با TestSmsService جایگزین شد
3. **AuthHelper.cs**: بازنویسی کامل با استفاده از OTP واقعی از TestSmsService
4. **CompleteShoppingJourneyTests.cs**: استفاده از TestSmsService برای گرفتن OTP code

### اصلاحات JSON Parsing (تمام فایل‌ها) ✅

تمام موارد `ReadFromJsonAsync<dynamic>()` با `JsonHelper.GetNestedProperty()` جایگزین شدند:

- ✅ ProductInventoryTests.cs
- ✅ SavedCartTests.cs
- ✅ UserReturnRequestTests.cs
- ✅ StockAlertTests.cs
- ✅ UserAddressTests.cs (2 مورد)
- ✅ PaymentTests.cs
- ✅ OrderManagementTests.cs
- ✅ WishlistTests.cs (3 مورد)
- ✅ ProductComparisonTests.cs (3 مورد)
- ✅ ProductVariantTests.cs
- ✅ CompleteShoppingJourneyTests.cs
- ✅ UserProfileTests.cs

**نتیجه**: صفر (0) مورد `ReadFromJsonAsync<dynamic>()` باقی مانده!

---

## نتایج پیش‌بینی شده:

### قبل از اصلاحات:
- **کل تست‌ها**: 160
- **موفق**: 69 (43%)
- **ناموفق**: 89 (56%)
- **Skip شده**: 2 (1%)

### بعد از اصلاحات (پیش‌بینی):
- **Unauthorized (401)**: کاهش قابل توجه (از 65 به حدود 10-15)
- **JSON Parsing Errors**: صفر (0)
- **MethodNotAllowed (405)**: بدون تغییر (نیاز به اصلاح API)
- **تست‌های موفق پیش‌بینی شده**: حدود 110-120 (70-75%)

---

**توجه**: برای اجرای تست‌ها از دستور زیر استفاده کنید:
```powershell
dotnet test tests/OnlineShop.IntegrationTests/OnlineShop.IntegrationTests.csproj --verbosity minimal
```

**برای مشاهده جزئیات بیشتر**:
```powershell
dotnet test tests/OnlineShop.IntegrationTests/OnlineShop.IntegrationTests.csproj --verbosity detailed
```

