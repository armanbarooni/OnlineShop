-- بررسی سفارش FRESH
SELECT 
    "OrderNumber",
    "OrderStatus",
    "SyncedToMahak",
    "MahakOrderId",
    "CreatedAt"
FROM "UserOrders"
WHERE "OrderNumber" LIKE 'FRESH_%'
ORDER BY "CreatedAt" DESC;
