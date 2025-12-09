-- Update test order status to Completed so it will be synced
UPDATE "UserOrders"
SET "OrderStatus" = 'Completed'
WHERE "OrderNumber" LIKE 'TEST-%'
AND "SyncedToMahak" = false;

-- Verify update
SELECT "OrderNumber", "OrderStatus", "SyncedToMahak", "TotalAmount"
FROM "UserOrders"
WHERE "OrderNumber" LIKE 'TEST-%'
ORDER BY "CreatedAt" DESC
LIMIT 1;
