-- Mahak Test Order Creation Script (PostgreSQL)
-- This will create a test order that will sync to Mahak

\c OnlineShop

-- Step 1: Get a product from Mahak and a user
DO $$
DECLARE
    v_product_id UUID;
    v_product_name VARCHAR(500);
    v_product_price DECIMAL(18,2);
    v_user_id UUID;
    v_order_id UUID := gen_random_uuid();
    v_order_number VARCHAR(50) := 'MAHAK' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');
    v_order_item_id UUID := gen_random_uuid();
BEGIN
    -- Get product from Mahak
    SELECT "Id", "Name", "Price" 
    INTO v_product_id, v_product_name, v_product_price
    FROM "Products" 
    WHERE "MahakId" IS NOT NULL 
    ORDER BY "CreatedAt" DESC 
    LIMIT 1;

    -- Get first user
    SELECT "Id" INTO v_user_id FROM "AspNetUsers" LIMIT 1;

    -- Create the order
    INSERT INTO "UserOrders" (
        "Id", 
        "UserId", 
        "OrderNumber", 
        "OrderStatus", 
        "SubTotal", 
        "TaxAmount", 
        "ShippingAmount", 
        "DiscountAmount", 
        "TotalAmount",
        "Currency",
        "SyncedToMahak",
        "CreatedAt", 
        "UpdatedAt", 
        "Deleted"
    )
    VALUES (
        v_order_id, 
        v_user_id, 
        v_order_number, 
        'Completed',
        v_product_price, 
        0, 
        50000,
        0, 
        v_product_price + 50000,
        'IRR',
        false,
        NOW(), 
        NOW(), 
        false
    );

    -- Add order item
    INSERT INTO "UserOrderItems" (
        "Id", 
        "UserOrderId", 
        "ProductId", 
        "ProductName", 
        "Quantity", 
        "UnitPrice", 
        "TotalPrice",
        "CreatedAt", 
        "UpdatedAt", 
        "Deleted"
    )
    VALUES (
        v_order_item_id, 
        v_order_id, 
        v_product_id, 
        v_product_name, 
        1,
        v_product_price, 
        v_product_price,
        NOW(), 
        NOW(), 
        false
    );

    -- Display results
    RAISE NOTICE '✅ Test order created successfully!';
    RAISE NOTICE 'Order Number: %', v_order_number;
    RAISE NOTICE 'Product: %', v_product_name;
    RAISE NOTICE 'Price: % IRR', v_product_price;
    RAISE NOTICE 'Total: % IRR', v_product_price + 50000;
    RAISE NOTICE '';
    RAISE NOTICE '⏳ Waiting for outgoing sync (runs every 1 minute)...';
    RAISE NOTICE 'Check backend logs for: "Order sent to Mahak successfully"';
END $$;

-- Verify the order
SELECT 
    o."OrderNumber",
    o."OrderStatus",
    o."TotalAmount",
    o."SyncedToMahak",
    o."MahakSyncedAt",
    o."CreatedAt",
    i."ProductName",
    i."Quantity",
    i."UnitPrice"
FROM "UserOrders" o
INNER JOIN "UserOrderItems" i ON o."Id" = i."UserOrderId"
WHERE o."OrderNumber" LIKE 'MAHAK%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
