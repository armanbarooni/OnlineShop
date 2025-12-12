-- Check for orders that need to be synced to Mahak
-- These are orders that are paid but not yet synced

SELECT 
    o."Id",
    o."OrderNumber",
    o."OrderStatus",
    o."TotalAmount",
    o."SyncedToMahak",
    o."MahakSyncedAt",
    o."CreatedAt",
    u."Email" AS "CustomerEmail",
    u."FirstName" || ' ' || u."LastName" AS "CustomerName"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON u."Id" = o."UserId"
WHERE o."SyncedToMahak" = false
  AND o."Deleted" = false
ORDER BY o."CreatedAt" DESC
LIMIT 10;

-- Check if there are any orders at all
SELECT 
    COUNT(*) AS "TotalOrders",
    COUNT(CASE WHEN "SyncedToMahak" = true THEN 1 END) AS "SyncedOrders",
    COUNT(CASE WHEN "SyncedToMahak" = false THEN 1 END) AS "UnsyncedOrders"
FROM "UserOrders"
WHERE "Deleted" = false;
