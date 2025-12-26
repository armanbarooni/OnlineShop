-- Check if the test order was created and is ready for sync
SELECT 
    "Id",
    "OrderNumber",
    "OrderStatus",
    "TotalAmount",
    "SyncedToMahak",
    "CreatedAt"
FROM "UserOrders"
WHERE "OrderNumber" LIKE 'MAHAK-%'
  AND "Deleted" = false
ORDER BY "CreatedAt" DESC
LIMIT 5;
