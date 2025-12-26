-- ============================================
-- بررسی وضعیت سفارش تست
-- ============================================

-- 1. بررسی سفارش ایجاد شده
SELECT 
    '✅ جزئیات سفارش' as "وضعیت",
    o."OrderNumber" as "شماره سفارش",
    o."OrderStatus" as "وضعیت سفارش",
    o."TotalAmount" as "مبلغ کل (ریال)",
    o."SyncedToMahak" as "ارسال به محک؟",
    o."MahakSyncedAt" as "زمان ارسال به محک",
    o."MahakOrderId" as "شناسه سفارش در محک",
    o."CreatedAt" as "تاریخ ایجاد",
    i."ProductName" as "محصول",
    i."Quantity" as "تعداد",
    i."UnitPrice" as "قیمت واحد"
FROM "UserOrders" o
INNER JOIN "UserOrderItems" i ON o."Id" = i."OrderId"
WHERE o."OrderNumber" LIKE 'MAHAK%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;

-- 2. بررسی تمام سفارش‌های در انتظار sync
SELECT 
    '⏳ سفارش‌های در انتظار ارسال' as "وضعیت",
    "OrderNumber" as "شماره سفارش",
    "TotalAmount" as "مبلغ",
    "CreatedAt" as "تاریخ ایجاد"
FROM "UserOrders"
WHERE "SyncedToMahak" = false 
  AND "OrderStatus" = 'Completed'
  AND "Deleted" = false
ORDER BY "CreatedAt" DESC;

-- 3. بررسی سفارش‌های ارسال شده به محک
SELECT 
    '✅ سفارش‌های ارسال شده' as "وضعیت",
    "OrderNumber" as "شماره سفارش",
    "MahakOrderId" as "شناسه محک",
    "MahakSyncedAt" as "زمان ارسال",
    "TotalAmount" as "مبلغ"
FROM "UserOrders"
WHERE "SyncedToMahak" = true
ORDER BY "MahakSyncedAt" DESC;
