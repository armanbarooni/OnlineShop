-- بررسی اینکه آیا اصلاً سفارشی در دیتابیس هست یا نه

-- 1. تمام سفارش‌ها (حتی deleted)
SELECT 
    'تمام سفارش‌ها' as "نوع",
    COUNT(*) as "تعداد"
FROM "UserOrders";

-- 2. سفارش‌های اخیر (10 تای آخر)
SELECT 
    "Id",
    "OrderNumber",
    "OrderStatus",
    "TotalAmount",
    "SyncedToMahak",
    "Deleted",
    "CreatedAt"
FROM "UserOrders"
ORDER BY "CreatedAt" DESC
LIMIT 10;

-- 3. آیا کاربری وجود دارد؟
SELECT 
    'تعداد کاربران' as "نوع",
    COUNT(*) as "تعداد"
FROM "AspNetUsers";

-- 4. آیا محصولی از محک داریم؟
SELECT 
    'محصولات از محک' as "نوع",
    COUNT(*) as "تعداد"
FROM "Products"
WHERE "MahakId" IS NOT NULL;

-- 5. جزئیات محصولات از محک
SELECT 
    "Id",
    "Name",
    "Price",
    "MahakId"
FROM "Products"
WHERE "MahakId" IS NOT NULL
LIMIT 5;
