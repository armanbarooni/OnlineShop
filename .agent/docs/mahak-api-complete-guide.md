# 📘 Mahak API Complete Guide – SaveAllDataV2

> **خلاصه فنی برای کدنویس/Cursor**  
> تمام flow‌های ضروری برای کار با API محک

---

## ✅ 1️⃣ ساخت Person

**فقط این‌ها لازم است:**
- `personGroupId` ✅ (اجباری)
- `personGroupClientId` ❌ (ارسال نشود)

```json
{
  "People": [
    {
      "personClientId": 1001,
      "firstName": "علی",
      "lastName": "احمدی",
      "personType": 0,
      "personGroupId": 102479,
      "deleted": false
    }
  ]
}
```

### نکات مهم:
- `personClientId` باید یونیک باشد
- `personType`: 0 = حقیقی، 1 = حقوقی
- `personGroupId` از قبل باید در سیستم محک موجود باشد

---

## ✅ 2️⃣ اضافه کردن عکس پروفایل

> ⚠️ محک **عکس را مستقیم داخل Person نمی‌گیرد**  
> عکس‌ها فقط از طریق `Pictures` ارسال می‌شوند

### ساخت عکس
- سایز زیاد نباشد (ترجیحاً < 300KB)
- `binaryData` = Base64 خالص (بدون `data:image/...`)

```json
{
  "People": [
    {
      "personClientId": 70010,
      "firstName": "علی",
      "lastName": "احمدی",
      "personType": 0,
      "personGroupId": 102479,
      "deleted": false
    }
  ],
  "Pictures": [
    {
      "pictureClientId": 90001,
      "fileName": "person-70010.jpg",
      "binaryData": "Base64ImageDataHere",
      "deleted": false
    }
  ]
}
```

### نکات مهم:
- `pictureClientId` باید یونیک باشد
- `fileName` باید پسوند معتبر داشته باشد (.jpg, .png)
- `binaryData` باید Base64 خالص باشد (بدون prefix)

---

## ✅ 3️⃣ اتصال Person به Visitor (الزامی برای فروش واقعی)

> 📌 **بدون این:**
> - فروش ثبت می‌شود ✅
> - ولی انبار + تراکنش ساخته نمی‌شود ❌

```json
{
  "VisitorPeople": [
    {
      "visitorPersonClientId": 81001,
      "visitorId": 41874,
      "personClientId": 70010,
      "deleted": false
    }
  ],
  "People": [],
  "Orders": [],
  "OrderDetails": []
}
```

### نتیجه:
- در Sync بعدی → `PersonId` واقعی می‌گیری
- این اتصال برای ثبت فروش واقعی **الزامی** است

---

## ✅ 4️⃣ ارسال فاکتور فروش واقعی (کاهش موجودی)

**Prerequisites:**
- Person ساخته و Sync شده
- VisitorPeople ثبت شده
- visitorId معتبر

```json
{
  "Orders": [
    {
      "orderClientId": 3001,
      "orderType": 201,
      "orderDate": "2025-01-01T10:00:00",
      "personId": 123456,
      "visitorId": 41874,
      "settlementType": 1,
      "deleted": false
    }
  ],
  "OrderDetails": [
    {
      "orderDetailClientId": 3002,
      "orderClientId": 3001,
      "productDetailId": 9243274,
      "count1": 1,
      "price": 9000000,
      "storeId": 31940,
      "deleted": false
    }
  ]
}
```

### نکات مهم:
- `personId` باید **EntityId واقعی** باشد (نه ClientId)
- `orderType`: 201 = فروش واقعی
- `settlementType`: 1 = نقدی، 2 = نسیه
- `storeId` در OrderDetails الزامی است
- `productDetailId` باید معتبر باشد

---

## ✅ قوانین طلایی (TL;DR)

- ✅ `visitorId` حتماً
- ✅ `storeId` در OrderDetails
- ✅ `personId` واقعی (EntityId)
- ✅ VisitorPeople وجود داشته باشد
- ❌ بدون VisitorPeople ⇒ `Transactions = null`
- ❌ Result=true تضمین فروش واقعی نیست

---

## 🧪 Stock = 0

اگر موجودی صفر باشد:
- Order ثبت می‌شود ✅
- Transaction انبار ساخته نمی‌شود ❌  
  (Mahak: NegativeStock=false)

---

## 🎯 نتیجه نهایی

```text
فروش واقعی = Person + VisitorPeople + visitorId + orderType=201
```

---

## 📦 Payload کامل (مثال واقعی)

```json
{
  "People": [
    {
      "personClientId": 70010,
      "firstName": "علی",
      "lastName": "احمدی",
      "personType": 0,
      "personGroupId": 102479,
      "deleted": false
    }
  ],
  "Pictures": [
    {
      "pictureClientId": 90001,
      "fileName": "person-70010.jpg",
      "binaryData": "/9j/4AAQSkZJRgABAQEAYABgAAD...",
      "deleted": false
    }
  ],
  "VisitorPeople": [
    {
      "visitorPersonClientId": 81001,
      "visitorId": 41874,
      "personClientId": 70010,
      "deleted": false
    }
  ],
  "Orders": [
    {
      "orderClientId": 3001,
      "orderType": 201,
      "orderDate": "2025-12-25T10:00:00",
      "personId": 123456,
      "visitorId": 41874,
      "settlementType": 1,
      "deleted": false
    }
  ],
  "OrderDetails": [
    {
      "orderDetailClientId": 3002,
      "orderClientId": 3001,
      "productDetailId": 9243274,
      "count1": 1,
      "price": 9000000,
      "storeId": 31940,
      "deleted": false
    }
  ]
}
```

---

## 🔧 Validation Checklist

قبل از ارسال API، این‌ها را چک کن:

- [ ] `personGroupId` معتبر است؟
- [ ] `visitorId` معتبر است؟
- [ ] `storeId` معتبر است؟
- [ ] `productDetailId` معتبر است؟
- [ ] `personId` EntityId واقعی است (نه ClientId)؟
- [ ] VisitorPeople ساخته شده؟
- [ ] عکس Base64 خالص است (بدون prefix)?
- [ ] سایز عکس < 300KB است؟

---

## 🚨 Error Mapping

### Result = true ولی Transaction = null
- **علت**: VisitorPeople ثبت نشده
- **راه‌حل**: ابتدا VisitorPeople بساز

### Result = true ولی موجودی کم نشده
- **علت**: Stock = 0 یا NegativeStock=false
- **راه‌حل**: موجودی را چک کن

### PersonId not found
- **علت**: Person هنوز Sync نشده
- **راه‌حل**: صبر کن تا Sync بعدی یا دوباره Sync کن

### Invalid personGroupId
- **علت**: personGroupId در سیستم محک وجود ندارد
- **راه‌حل**: از personGroupId معتبر استفاده کن

---

## 📝 Notes

- همیشه از `deleted: false` استفاده کن مگر اینکه بخوای حذف کنی
- ClientId‌ها باید در سطح کل سیستم یونیک باشند
- تاریخ‌ها باید ISO 8601 باشند
- قیمت‌ها به ریال هستند

---

**آخرین بروزرسانی**: 2025-12-25  
**نسخه**: 1.0
