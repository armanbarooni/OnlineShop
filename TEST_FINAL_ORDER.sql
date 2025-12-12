-- Delete old test orders and create new one with ItemType=1 fix
DELETE FROM "UserOrderItems" WHERE "OrderId" IN (
    SELECT "Id" FROM "UserOrders" WHERE "OrderNumber" LIKE 'TEST-%'
);
DELETE FROM "UserOrders" WHERE "OrderNumber" LIKE 'TEST-%';

-- Create new test order
DO $$
DECLARE
    v_user_id UUID := '3e638506-8819-4a11-a003-8b00d95c9086';
    v_order_id UUID;
    v_product_id UUID;
    v_product_name VARCHAR;
BEGIN
    SELECT "Id", "Name" INTO v_product_id, v_product_name
    FROM "Products"
    WHERE "Name" LIKE '%شلوار بالوني 204%'
    LIMIT 1;
    
    v_order_id := gen_random_uuid();
    
    INSERT INTO "UserOrders" (
        "Id", "UserId", "OrderNumber", "OrderStatus", 
        "SubTotal", "TaxAmount", "ShippingAmount", "DiscountAmount", "TotalAmount",
        "Currency", "SyncedToMahak", "RowVersion",
        "CreatedAt", "UpdatedAt", "Deleted"
    ) VALUES (
        v_order_id, v_user_id, 'TEST-FINAL-' || EXTRACT(EPOCH FROM NOW())::TEXT, 'Completed',
        280000, 0, 0, 0, 280000,
        'IRR', false, 0,
        NOW(), NOW(), false
    );
    
    INSERT INTO "UserOrderItems" (
        "Id", "OrderId", "ProductId", "ProductName", "Quantity", "UnitPrice", 
        "TotalPrice", "DiscountAmount", "RowVersion",
        "CreatedAt", "UpdatedAt", "Deleted"
    ) VALUES (
        gen_random_uuid(), v_order_id, v_product_id, v_product_name, 2, 140000,
        280000, 0, 0,
        NOW(), NOW(), false
    );
    
    RAISE NOTICE 'Final test order created with ItemType=1 fix: %', v_order_id;
END $$;

SELECT "OrderNumber", "OrderStatus", "TotalAmount"
FROM "UserOrders"
WHERE "OrderNumber" LIKE 'TEST-FINAL-%'
ORDER BY "CreatedAt" DESC
LIMIT 1;
