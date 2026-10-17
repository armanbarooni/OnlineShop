# نکات اتصال به API محک

مرجع اصلی endpointها، فیلدها و مدل‌ها:

https://mahakacc.mahaksoft.com/API/v3/swagger/index.html

اول Swagger را کامل بخوانید. این فایل همان Swagger نیست. فقط نکته‌هایی است که در Swagger مبهم بوده یا از تست واقعی و صحبت با پشتیبانی مشخص شده است.

## Login

Base URL:

```text
https://mahakacc.mahaksoft.com/API/v3/Sync/
```

```http
POST /API/v3/Sync/Login
Content-Type: application/json-patch+json
```

```json
{
  "userName": "نام کاربری محک",
  "password": "md5 پسورد"
}
```

پسورد خام ارسال نمی‌شود. MD5 متن پسورد با UTF-8 است؛ خروجی hex، lowercase و بدون `-`.

```text
123456 -> e10adc3949ba59abbe56e057f20f883e
```

پاسخ موفق:

```json
{
  "result": true,
  "data": {
    "userToken": "...",
    "visitorId": 12345,
    "databaseId": 123,
    "syncId": 0
  }
}
```

| فیلد | کاربرد |
|---|---|
| `UserToken` | برای تمام درخواست‌های بعدی |
| `VisitorId` | برای رابطه مشتری با ویزیتور، فاکتور و رسید |

بعد از Login:

```http
Authorization: Bearer <UserToken>
```

اگر `result` برابر false بود، فقط HTTP status را نگاه نکنید. `message` و `errorCode` را هم بخوانید.

## گرفتن اطلاعات

```http
POST /API/v3/Sync/GetAllData
Authorization: Bearer <UserToken>
Content-Type: application/json
```

```json
{
  "fromProductVersion": 0,
  "fromProductDetailVersion": 0,
  "fromProductCategoryVersion": 0,
  "fromPictureVersion": 0,
  "fromPhotoGalleryVersion": 0,
  "fromProductDetailStoreAssetVersion": 0,
  "fromPersonVersion": 0,
  "fromPersonAddressVersion": 0,
  "fromVisitorPersonVersion": 0,
  "fromRegionVersion": 0
}
```

`0` یعنی تمام داده‌های آن بخش. دفعات بعد آخرین `RowVersion` همان بخش را بفرستید.

`RowVersion` تاریخ نیست. فقط شماره تغییر است. version هر بخش جداست؛ version محصول را به جای عکس یا موجودی نفرستید.

داده‌ها داخل `data.objects` می‌آیند. نبودن یک لیست یا خالی بودن آن الزاماً خطا نیست.

ترتیب درستِ معنی دادن به داده‌ها:

```text
ProductCategories
Products
ProductDetails
ProductDetailStoreAssets
Pictures + PhotoGalleries
People + VisitorPeople
Regions
```

## شناسه‌ها

این‌ها را با هم قاطی نکنید:

| مورد | شناسه | کاربرد |
|---|---|---|
| کالای مادر | `ProductId` | نام، دسته، توضیح؛ مثلاً تیشرت 2003 |
| رنگ/سایز قابل فروش | `ProductDetailId` | چیزی که در سطر فاکتور فروش می‌رود |
| انبار | `StoreId` | موجودی همان رنگ/سایز در یک انبار |
| کد کالا | `ProductCode` | کد عملیاتی/نمایشی؛ برای اتصال قطعی کافی نیست |
| مشتری محک | `PersonId` | در فاکتور و رسید استفاده می‌شود |
| شناسه‌ای که خودتان برای مشتری می‌فرستید | `PersonClientId` | برای پیگیری/تکرار درخواست، نه جای PersonId |

`ProductId` برای فروش کافی نیست. در `OrderDetails` باید `ProductDetailId` بفرستید.

خطای زیر یعنی detail ندارید:

```text
ProductDetailId or IncomeId or ExpenseId is required
```

مثال:

```text
ProductId: 8262620
Name: تیشرت 268

ProductDetailId: 9000001
ProductId: 8262620
Properties: رنگ مشکی، سایز L
Price1: قیمت اصلی
Price2: قیمت دوم
```

برای فروش مشکی/L باید `9000001` ارسال شود، نه `8262620`.

## دسته‌ها و کالاها

`ProductCategories`:

```text
ProductCategoryId
ProductCategoryCode
Name
Deleted
RowVersion
```

`Products`:

```text
ProductId
ProductClientId
ProductCode
ProductCategoryId
Name
Description
Deleted
RowVersion
```

محصول با `ProductCategoryId` به دسته وصل است. اول دسته را بگیرید، بعد محصول را به آن وصل کنید.

`ProductCode` را برای چسباندن نام، عکس یا detail به محصول مبنا قرار ندهید. شناسه پایدار محصول `ProductId` است.

اگر `Deleted = true` بود، آن رکورد را معتبر برای نمایش یا فروش حساب نکنید.

## قیمت و ویژگی‌ها

قیمت و رنگ/سایز در `ProductDetails` است، نه `Products`.

فیلدهای مهم:

| فیلد | معنی |
|---|---|
| `ProductDetailId` | شناسه تنوع قابل فروش |
| `ProductId` | کالای مادر |
| `Price1` | قیمت اصلی |
| `Price2` | قیمت دوم/ویژه، اگر صفر نباشد |
| `Barcode` | بارکد detail |
| `Properties` | رنگ، سایز و اندازه‌ها |
| `Deleted` | detail نامعتبر/حذف‌شده |

اگر `Price2 > 0` باشد، قیمت پرداختی `Price2` است و `Price1` قیمت قبل تخفیف.

رنگ و سایز در `Properties` همان detail هستند. یک ProductDetail می‌تواند مشکی/XL باشد و detail دیگر همان کالا سفید/L. قیمت، موجودی و فروش باید برای همان detail انجام شود.

در داده‌های محک معمولاً:

```text
کد 3: رنگ
کد 4: سایز
کد 10: عرض سینه
```

برای مثال، در این داده مقدار عرض سینه `57` است:

```json
{ "C": "10", "V": "57" }
```

مقدار `V` همان مقدار ویژگی است. عنوان و کد سایر اندازه‌ها ممکن است از تنظیمات همان دیتابیس محک بیاید؛ با داده‌ی خودتان بررسی کنید.

## موجودی

موجودی را از `ProductDetailStoreAssets` بخوانید:

```text
ProductDetailId
StoreId
Count1
Count2
Deleted
UpdateDate
RowVersion
```

`ProductDetail.Count1` را موجودی قطعی یک انبار حساب نکنید. موجودی واقعی detail از asset همان انبار می‌آید:

```text
ProductDetailStoreAsset.ProductDetailId = detail مورد نظر
ProductDetailStoreAsset.StoreId = انبار مورد نظر
ProductDetailStoreAsset.Count1 = موجودی
```

اگر چند انبار دارید، از اول مشخص کنید فروش آنلاین از کدام `StoreId` است. همان `StoreId` را در سطر فروش هم بفرستید.

asset با `Deleted = true` موجودی معتبر نیست.

## عکس‌ها

عکس محصول از دو لیست به دست می‌آید:

1. `Pictures`: خود عکس و URL آن.
2. `PhotoGalleries`: اتصال عکس به محصول.

فیلدهای مهم Picture:

```text
PictureId
Url
FileName
Deleted
RowVersion
```

فیلدهای مهم PhotoGallery:

```text
PhotoGalleryId
EntityType
ItemCode
PictureId
IsMain
Deleted
RowVersion
```

برای عکس محصول:

```text
PhotoGallery.EntityType = 102
PhotoGallery.ItemCode = Product.ProductId
PhotoGallery.PictureId = Pictures.PictureId
```

`IsMain = true` یعنی عکس اصلی. اگر Picture یا PhotoGallery deleted باشد، آن عکس را برای محصول فعال نگه ندارید.

رابطه معتبر فقط PhotoGallery فعال است که `ItemCode` آن با `ProductId` فعلی یکی باشد. مبنا قرار دادن تنها `PictureId` یا `ProductCode` می‌تواند عکس کالای قدیمی را روی کالای جدید بیندازد.

## حذف و تکرار شناسه‌ها

محک رکوردهای حذف‌شده را هم در sync می‌فرستد. این‌ها برای تاریخچه‌اند، نه نمایش/فروش.

| entity | وقتی `Deleted = true` |
|---|---|
| Product | نمایش و فروش ندهید |
| ProductDetail | رنگ/سایز را نمایش و فروش ندهید |
| ProductDetailStoreAsset | موجودی آن asset را حساب نکنید |
| Picture / PhotoGallery | عکس را به محصول وصل نکنید |
| Person / VisitorPeople | رابطه فعال حساب نکنید |

ممکن است یک شناسه بیش از یک بار در یک پاسخ باشد یا نسخه قدیمی و جدید یک رکورد برگردد. برای یک شناسه، `RowVersion` بزرگ‌تر معتبرتر است.

## مشتری

برای ساخت مشتری از `SaveAllDataV2` استفاده می‌شود:

```http
POST /API/v3/Sync/SaveAllDataV2
Authorization: Bearer <UserToken>
Content-Type: application/json-patch+json
```

```json
{
  "people": [
    {
      "personClientId": 1000001,
      "personGroupId": 999,
      "firstName": "نام",
      "lastName": "نام خانوادگی",
      "mobile": "0912...",
      "email": "...",
      "personType": 0,
      "deleted": false
    }
  ]
}
```

`PersonGroupId` برای ساخت مشتری لازم است. مقدارش به دیتابیس محک شما بستگی دارد.

بعد از ثبت موفق، `entityId` این مسیر همان `PersonId` واقعی است:

```text
data.objects.people.results[0].entityId
```

بعد از ساخت Person، رابطه مشتری و ویزیتور را هم ثبت کنید:

```json
{
  "visitorPeople": [
    {
      "visitorPersonClientId": 10000011,
      "visitorId": 12345,
      "personId": 56789,
      "personClientId": 1000001,
      "deleted": false
    }
  ]
}
```

`visitorId` از Login می‌آید. برای فروش واقعی و کم شدن موجودی، این رابطه را جدی بگیرید.

## فروش و تخفیف

```http
POST /API/v3/Sync/SaveAllDataV2
Authorization: Bearer <UserToken>
Content-Type: application/json-patch+json
```

فاکتور فروش `OrderType = 201` است.

نمونه فروش یک کالا با تخفیف:

```json
{
  "orders": [
    {
      "orderClientId": 10001,
      "visitorId": 12345,
      "personId": 56789,
      "receiptClientId": 10001,
      "orderType": 201,
      "orderDate": "2026-09-23T10:00:00",
      "deliveryDate": "2026-09-23T10:00:00",
      "discount": 800000,
      "discountType": 0,
      "sendCost": 0,
      "otherCost": 0,
      "settlementType": 1,
      "description": "توضیحات سفارش",
      "deleted": false
    }
  ],
  "orderDetails": [
    {
      "orderDetailClientId": 10002,
      "itemType": 1,
      "orderClientId": 10001,
      "productDetailId": 9000001,
      "storeId": 31940,
      "price": 998000,
      "count1": 1,
      "count2": 0,
      "discount": 800000,
      "discountType": 0,
      "taxPercent": 0,
      "chargePercent": 0,
      "description": "نام کالا - رنگ - سایز",
      "gift": 0,
      "deleted": false
    }
  ],
  "receipts": [
    {
      "receiptClientId": 10001,
      "personId": 56789,
      "visitorId": 12345,
      "cashAmount": 198000,
      "description": "پرداخت سفارش",
      "date": "2026-09-23T10:05:00",
      "orderClientId": 10001,
      "orderType": 201,
      "deleted": false
    }
  ]
}
```

| فیلد | نکته |
|---|---|
| `OrderClientId` | عدد یکتایی که خودتان می‌سازید؛ برای retry همان را نگه دارید |
| `OrderDetailClientId` | برای هر سطر یکتا |
| `PersonId` | شناسه واقعی Person در محک |
| `VisitorId` | از Login |
| `ProductDetailId` | رنگ/سایز واقعی فروخته‌شده |
| `StoreId` | انباری که موجودی از آن کم می‌شود |
| `Price` | قیمت قبل از تخفیف هر واحد |
| `Discount` | مبلغ تخفیف، نه درصد |
| `DiscountType = 0` | تخفیف مبلغی |
| `SettlementType = 1` | فروش تسویه‌شده/نقدی |
| `CashAmount` | مبلغ نهایی بعد از همه تخفیف‌ها |

فرمول نمونه:

```text
Price1: 998,000
Price2: 198,000
Count1: 1

Price: 998,000
Discount: 800,000
DiscountType: 0
CashAmount: 198,000
```

اگر `Price` را مبلغ تخفیفی بفرستید و `Discount` را هم بفرستید، تخفیف دوبار اعمال می‌شود. اگر `CashAmount` را قیمت قبل تخفیف بفرستید، مبلغ فاکتور و مانده مشتری اشتباه می‌شود.

نوع‌ها:

```text
OrderType 201 = فروش
OrderType 299 = پیش‌فاکتور
OrderType 202 = برگشت از فروش

DiscountType 0 = مبلغ
DiscountType 1 = درصد

SettlementType 1 = نقد/تسویه‌شده
SettlementType 0 = اعتباری
```

فقط `result: true` بالای پاسخ را ملاک ندانید. این‌ها را جداگانه بررسی کنید:

```text
data.objects.orders.results
data.objects.orderDetails.results
data.objects.receipts.results
```

ممکن است Order ثبت شود ولی یک OrderDetail خطا بخورد.

## رسید

رسید داخل `Receipts` همان اعلام پرداخت به محک است.

برای فروش پرداخت‌شده، `CashAmount` باید دقیقاً مبلغی باشد که مشتری بعد از تخفیف پرداخت کرده است.

```text
فی: 998,000
تخفیف: 800,000
قابل پرداخت: 198,000

CashAmount: 198,000
```

اگر رسید ارسال نشود یا مبلغ آن قبل تخفیف باشد، گزارش فاکتور و مانده مشتری می‌تواند غلط شود.

## آدرس و شماره

برای آدرس، مطمئن‌ترین موردی که در عمل جواب داده `Description` سفارش است. نام گیرنده، شماره تماس، استان، شهر، آدرس، پلاک و کدپستی را در آن بگذارید.

اعداد فارسی و عربی را پیش از ارسال به انگلیسی تبدیل کنید:

```text
۰۱۲۳۴۵۶۷۸۹ -> 0123456789
٠١٢٣٤٥٦٧٨٩ -> 0123456789
```

این کار برای جلوگیری از نمایش `?` به جای پلاک، شماره یا کدپستی مهم است.

## مناطق

`Regions` از `GetAllData` می‌آید:

```text
CityID
CityName
ProvinceID
ProvinceName
MapCode
RowVersion
```

برای شهر/استان، `FromRegionVersion` را جدا از versionهای دیگر نگه دارید.

## خطاهای تکراری

| خطا یا نشانه | معنی |
|---|---|
| `ProductDetailId or IncomeId or ExpenseId is required` | در سطر فروش، detail نفرستاده‌اید |
| عکس یا نام محصول اشتباه است | `Deleted`، `RowVersion`، `ProductId` و PhotoGallery را چک کنید |
| محصول حذف‌شده هنوز دیده می‌شود | رکوردهای `Deleted = true` فیلتر نشده‌اند |
| قیمت فاکتور و قابل پرداخت با هم نمی‌خوانند | `Price`، `Discount` و `CashAmount` با هم سازگار نیستند |
| مشتری بدهکار دیده می‌شود | رسید با مبلغ بعد تخفیف ارسال نشده یا اصلاً ارسال نشده |
| موجودی اشتباه است | count سطح ProductDetail به جای ProductDetailStoreAsset انبار درست استفاده شده |
| یک درخواست دوبار فروش ثبت می‌کند | clientId یکتا/ثابت برای retry ندارید یا نتیجه قبلی را بررسی نکرده‌اید |
| HTTP 200 ولی اطلاعات ناقص است | `Results` داخل entityها بررسی نشده |

چند نکته آخر:

- `Deleted` را برای همه مدل‌ها جدی بگیرید، نه فقط Product.
- برای فروش detail لازم است، برای عکس gallery لازم است، برای موجودی asset لازم است.
- `PersonId`، `PersonClientId`، `ProductId`، `ProductDetailId` و `StoreId` جای هم نیستند.
- اگر یک شناسه چند بار آمد، `RowVersion` بزرگ‌تر را جدی بگیرید.
