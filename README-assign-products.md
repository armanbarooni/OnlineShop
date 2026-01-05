# راهنمای Assign کردن محصولات به کتگوری‌ها

## مشکل
محصولات موجود در دیتابیس که قبل از اضافه شدن قابلیت `CategoryId` ایجاد شده‌اند، `CategoryId` آنها `NULL` است.

## راه‌حل‌ها

### راه 1: استفاده از SQL (سریع‌ترین روش)

1. فایل `assign-products-to-categories.sql` را باز کنید
2. ابتدا کتگوری‌های موجود را ببینید:
   ```sql
   SELECT "Id", "Name" FROM "ProductCategories" WHERE "Deleted" = false;
   ```
3. محصولات بدون کتگوری را ببینید:
   ```sql
   SELECT "Id", "Name" FROM "Products" WHERE "CategoryId" IS NULL AND "Deleted" = false;
   ```
4. یکی از گزینه‌های زیر را انتخاب کنید:

   **گزینه A: همه محصولات را به یک کتگوری assign کنید:**
   ```sql
   UPDATE "Products" 
   SET "CategoryId" = 'YOUR_CATEGORY_ID_HERE',
       "UpdatedAt" = NOW()
   WHERE "CategoryId" IS NULL AND "Deleted" = false;
   ```

   **گزینه B: محصولات خاص را به کتگوری‌های خاص assign کنید:**
   ```sql
   UPDATE "Products" 
   SET "CategoryId" = 'CATEGORY_ID',
       "UpdatedAt" = NOW()
   WHERE "Id" = 'PRODUCT_ID';
   ```

   **گزینه C: بر اساس الگوی نام:**
   ```sql
   UPDATE "Products" 
   SET "CategoryId" = 'CATEGORY_ID',
       "UpdatedAt" = NOW()
   WHERE "Name" LIKE '%تیشرت%'
     AND "CategoryId" IS NULL
     AND "Deleted" = false;
   ```

5. نتیجه را بررسی کنید:
   ```sql
   SELECT 
       pc."Name" as "CategoryName",
       COUNT(p."Id") as "ProductCount"
   FROM "ProductCategories" pc
   LEFT JOIN "Products" p ON pc."Id" = p."CategoryId" AND p."Deleted" = false
   WHERE pc."Deleted" = false
   GROUP BY pc."Id", pc."Name"
   ORDER BY pc."Name";
   ```

### راه 2: استفاده از API (برای تعداد کم)

اگر تعداد محصولات کم است، می‌توانید از API استفاده کنید:

```http
PUT /api/Product/{productId}
Authorization: Bearer YOUR_ADMIN_TOKEN
Content-Type: application/json

{
  "id": "product-id",
  "name": "نام محصول",
  "description": "توضیحات",
  "price": 1000000,
  "stockQuantity": 10,
  "categoryId": "category-id"
}
```

### راه 3: استفاده از PowerShell Script (برای تعداد زیاد)

1. ابتدا یک Admin Token دریافت کنید
2. فایل `bulk-assign-products-api.ps1` را اجرا کنید:

```powershell
.\bulk-assign-products-api.ps1 `
    -AuthToken "your-admin-token" `
    -CategoryId "category-id" `
    -ProductIds @("product-id-1", "product-id-2", "product-id-3")
```

## نکات مهم

1. **Backup**: قبل از اجرای UPDATE، از دیتابیس backup بگیرید
2. **تست**: ابتدا روی یک محصول تست کنید
3. **بررسی**: بعد از UPDATE، نتیجه را بررسی کنید
4. **Transaction**: می‌توانید UPDATE را در یک transaction قرار دهید:

```sql
BEGIN;
UPDATE "Products" SET "CategoryId" = '...' WHERE ...;
-- بررسی کنید
SELECT * FROM "Products" WHERE ...;
-- اگر درست بود:
COMMIT;
-- اگر اشتباه بود:
ROLLBACK;
```

## محصولات جدید

محصولات جدید که از طریق API ایجاد می‌شوند، می‌توانند مستقیماً `CategoryId` داشته باشند:

```http
POST /api/Product
{
  "name": "محصول جدید",
  "description": "...",
  "price": 1000000,
  "stockQuantity": 10,
  "categoryId": "category-id"  // ✅ این فیلد حالا پشتیبانی می‌شود
}
```

