-- Update test order status to Completed so it can be synced
UPDATE "UserOrders"
SET "OrderStatus" = 'Completed',
    "UpdatedAt" = NOW() AT TIME ZONE 'UTC'
WHERE "OrderNumber" LIKE 'MAHAK-%'
  AND "SyncedToMahak" = false
  AND "Deleted" = false;

-- Check the updated order
SELECT 
    "OrderNumber",
    "OrderStatus",
    "SyncedToMahak",
    "TotalAmount"
FROM "UserOrders"
WHERE "OrderNumber" LIKE 'MAHAK-%'
ORDER BY "CreatedAt" DESC
LIMIT 1;
