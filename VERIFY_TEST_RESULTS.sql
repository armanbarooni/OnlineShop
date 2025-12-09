-- ============================================
-- بررسی نتایج تست‌ها
-- ============================================

-- 1. همه سفارشات تست
SELECT 
    CASE 
        WHEN o."OrderNumber" LIKE 'NO_CUST_%' THEN '🔴 بدون مشتری'
        WHEN o."OrderNumber" LIKE 'WITH_CUST_%' THEN '🟢 با مشتری'
        ELSE '⚪ دیگر'
    END as "نوع",
    o."OrderNumber",
    u."FirstName" || ' ' || u."LastName" as "کاربر",
    u."MahakPersonId" as "Person ID",
    o."SyncedToMahak" as "Synced?",
    o."MahakOrderId" as "Order ID در محک",
    o."MahakSyncedAt" as "تاریخ Sync",
    o."CreatedAt" as "تاریخ ایجاد"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON o."UserId" = u."Id"
WHERE o."OrderNumber" LIKE 'NO_CUST_%' 
   OR o."OrderNumber" LIKE 'WITH_CUST_%'
ORDER BY o."CreatedAt" DESC;

-- 2. وضعیت کاربران
SELECT 
    "FirstName" || ' ' || "LastName" as "نام",
    "PhoneNumber",
    "MahakPersonId",
    "MahakSyncedAt",
    CASE 
        WHEN "MahakPersonId" IS NOT NULL THEN '✅ Synced'
        ELSE '❌ Not Synced'
    END as "وضعیت"
FROM "AspNetUsers"
WHERE "Id" IN (
    SELECT DISTINCT "UserId" 
    FROM "UserOrders" 
    WHERE "OrderNumber" LIKE 'NO_CUST_%' 
       OR "OrderNumber" LIKE 'WITH_CUST_%'
)
ORDER BY "MahakSyncedAt" DESC NULLS LAST;

-- 3. آمار کلی
SELECT 
    COUNT(*) FILTER (WHERE "OrderNumber" LIKE 'NO_CUST_%') as "سفارشات بدون مشتری",
    COUNT(*) FILTER (WHERE "OrderNumber" LIKE 'WITH_CUST_%') as "سفارشات با مشتری",
    COUNT(*) FILTER (WHERE "SyncedToMahak" = true) as "Synced شده",
    COUNT(*) FILTER (WHERE "SyncedToMahak" = false) as "منتظر Sync"
FROM "UserOrders"
WHERE "OrderNumber" LIKE 'NO_CUST_%' 
   OR "OrderNumber" LIKE 'WITH_CUST_%';
