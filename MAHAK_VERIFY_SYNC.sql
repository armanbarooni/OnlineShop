-- ============================================
-- MAHAK SYNC - VERIFICATION QUERIES
-- ============================================
-- Run these queries to check sync status
-- ============================================

-- 1. Check all orders waiting to sync to Mahak
SELECT 
    '🔄 ORDERS WAITING TO SYNC' as "Status",
    "OrderNumber",
    "OrderStatus",
    "TotalAmount",
    "SyncedToMahak",
    "CreatedAt"
FROM "UserOrders"
WHERE "SyncedToMahak" = false 
  AND "OrderStatus" = 'Completed'
  AND "Deleted" = false
ORDER BY "CreatedAt" DESC;

-- 2. Check orders already synced to Mahak
SELECT 
    '✅ ORDERS SYNCED TO MAHAK' as "Status",
    "OrderNumber",
    "MahakOrderId",
    "MahakSyncedAt",
    "TotalAmount",
    "CreatedAt"
FROM "UserOrders"
WHERE "SyncedToMahak" = true
ORDER BY "MahakSyncedAt" DESC;

-- 3. Check products from Mahak
SELECT 
    '📦 PRODUCTS FROM MAHAK' as "Status",
    "Name",
    "Price",
    "StockQuantity",
    "MahakId",
    "CreatedAt"
FROM "Products"
WHERE "MahakId" IS NOT NULL
ORDER BY "CreatedAt" DESC;

-- 4. Check latest test order details
SELECT 
    '📋 LATEST TEST ORDER' as "Status",
    o."OrderNumber",
    o."OrderStatus",
    o."TotalAmount",
    o."SyncedToMahak",
    o."MahakSyncedAt",
    i."ProductName",
    i."Quantity",
    i."UnitPrice"
FROM "UserOrders" o
LEFT JOIN "UserOrderItems" i ON o."Id" = i."UserOrderId"
WHERE o."OrderNumber" LIKE 'MAHAK%'
ORDER BY o."CreatedAt" DESC
LIMIT 5;

-- 5. Count sync statistics
SELECT 
    '📊 SYNC STATISTICS' as "Report",
    (SELECT COUNT(*) FROM "Products" WHERE "MahakId" IS NOT NULL) as "Products from Mahak",
    (SELECT COUNT(*) FROM "UserOrders" WHERE "SyncedToMahak" = false AND "OrderStatus" = 'Completed') as "Orders Waiting",
    (SELECT COUNT(*) FROM "UserOrders" WHERE "SyncedToMahak" = true) as "Orders Synced";
