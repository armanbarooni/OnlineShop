-- Delete previous test order and create a new one
DELETE FROM "UserOrderItems" WHERE "OrderId" IN (
    SELECT "Id" FROM "UserOrders" WHERE "OrderNumber" LIKE 'TEST-%'
);

DELETE FROM "UserOrders" WHERE "OrderNumber" LIKE 'TEST-%';

-- Create new test order with ProductDetail fix
DO $$
DECLARE
    v_user_id UUID := '3e638506-8819-4a11-a003-8b00d95c9086';
    v_order_id UUID;
    v_product_id UUID;
    v_product_name VARCHAR;
BEGIN
    -- Get product
    SELECT "Id", "Name" INTO v_product_id, v_product_name
    FROM "Products"
    WHERE "Name" LIKE '%شلوار بالوني 204%'
    LIMIT 1;
    
    -- Create order
    v_order_id := gen_random_uuid();
    
    INSERT INTO "UserOrders" (
        "Id", "UserId", "OrderNumber", "OrderStatus", 
        "SubTotal", "TaxAmount", "ShippingAmount", "DiscountAmount", "TotalAmount",
        "Currency", "SyncedToMahak", "RowVersion",
        "CreatedAt", "UpdatedAt", "Deleted"
    ) VALUES (
        v_order_id, v_user_id, 'TEST-' || EXTRACT(EPOCH FROM NOW())::TEXT, 'Completed',
        280000, 0, 0, 0, 280000,
        'IRR', false, 0,
        NOW(), NOW(), false
    );
    
    -- Add order item (quantity 2)
    INSERT INTO "UserOrderItems" (
        "Id", "OrderId", "ProductId", "ProductName", "Quantity", "UnitPrice", 
        "TotalPrice", "DiscountAmount", "RowVersion",
        "CreatedAt", "UpdatedAt", "Deleted"
    ) VALUES (
        gen_random_uuid(), v_order_id, v_product_id, v_product_name, 2, 140000,
        280000, 0, 0,
        NOW(), NOW(), false
    );
    
    RAISE NOTICE 'New test order created: %', v_order_id;
END $$;

-- Verify
SELECT "OrderNumber", "OrderStatus", "TotalAmount", "SyncedToMahak"
FROM "UserOrders"
WHERE "OrderNumber" LIKE 'TEST-%'
ORDER BY "CreatedAt" DESC
LIMIT 1;
