-- Test: Create a sale order and check if inventory decreases
-- Step 1: Check current inventory
SELECT "Name", "StockQuantity" 
FROM "Products" 
WHERE "Name" LIKE '%شلوار بالوني 204%';

-- Step 2: Create a test order (use existing customer)
DO $$
DECLARE
    v_user_id UUID;
    v_order_id UUID;
    v_product_id UUID;
BEGIN
    -- Get test user
    SELECT "Id" INTO v_user_id 
    FROM "AspNetUsers" 
    WHERE "Email" = 'test@example.com'
    LIMIT 1;
    
    -- Get product
    SELECT "Id" INTO v_product_id
    FROM "Products"
    WHERE "Name" LIKE '%شلوار بالوني 204%'
    LIMIT 1;
    
    -- Create order
    v_order_id := gen_random_uuid();
    
    INSERT INTO "UserOrders" (
        "Id", "UserId", "OrderNumber", "OrderDate", "OrderStatus", 
        "TotalAmount", "ShippingCost", "TaxAmount", "DiscountAmount",
        "PaymentMethod", "PaymentStatus", "ShippingMethod",
        "CreatedAt", "UpdatedAt", "Deleted"
    ) VALUES (
        v_order_id, v_user_id, 'TEST-' || EXTRACT(EPOCH FROM NOW())::TEXT,
        NOW(), 'Pending', 280000, 0, 0, 0,
        'Online', 'Pending', 'Standard',
        NOW(), NOW(), false
    );
    
    -- Add order item (quantity 2)
    INSERT INTO "UserOrderItems" (
        "Id", "OrderId", "ProductId", "Quantity", "UnitPrice", 
        "TotalPrice", "DiscountAmount",
        "CreatedAt", "UpdatedAt", "Deleted"
    ) VALUES (
        gen_random_uuid(), v_order_id, v_product_id, 2, 140000,
        280000, 0,
        NOW(), NOW(), false
    );
    
    RAISE NOTICE 'Order created: %', v_order_id;
END $$;

-- Step 3: Wait for order to sync to Mahak (1 minute)
-- Then check inventory again
SELECT "Name", "StockQuantity" 
FROM "Products" 
WHERE "Name" LIKE '%شلوار بالوني 204%';

-- Expected: StockQuantity should decrease from 19 to 17 after Mahak sync
