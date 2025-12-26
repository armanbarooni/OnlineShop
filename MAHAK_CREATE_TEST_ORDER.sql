-- ============================================
-- MAHAK TEST ORDER - SIMPLE VERSION
-- ============================================
-- Instructions:
-- 1. Open pgAdmin or any PostgreSQL client
-- 2. Connect to "OnlineShop" database
-- 3. Copy and paste this entire script
-- 4. Execute it
-- 5. Wait 1 minute and check backend logs
-- ============================================

-- Create test order
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
        "Id", "UserId", "OrderNumber", "OrderStatus", 
        "SubTotal", "TaxAmount", "ShippingAmount", "DiscountAmount", "TotalAmount",
        "Currency", "SyncedToMahak",
        "CreatedAt", "UpdatedAt", "Deleted"
    )
    VALUES (
        v_order_id, v_user_id, v_order_number, 'Completed',
        v_product_price, 0, 50000, 0, v_product_price + 50000,
        'IRR', false,
        NOW(), NOW(), false
    );

    -- Add order item
    INSERT INTO "UserOrderItems" (
        "Id", "UserOrderId", "ProductId", "ProductName", 
        "Quantity", "UnitPrice", "TotalPrice",
        "CreatedAt", "UpdatedAt", "Deleted"
    )
    VALUES (
        v_order_item_id, v_order_id, v_product_id, v_product_name, 
        1, v_product_price, v_product_price,
        NOW(), NOW(), false
    );

    -- Show success message
    RAISE NOTICE '========================================';
    RAISE NOTICE '✅ TEST ORDER CREATED SUCCESSFULLY!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Order Number: %', v_order_number;
    RAISE NOTICE 'Product: %', v_product_name;
    RAISE NOTICE 'Price: % IRR', v_product_price;
    RAISE NOTICE 'Total: % IRR', v_product_price + 50000;
    RAISE NOTICE '========================================';
    RAISE NOTICE '⏳ NEXT STEPS:';
    RAISE NOTICE '1. Wait 1 minute for outgoing sync';
    RAISE NOTICE '2. Check backend logs for:';
    RAISE NOTICE '   "Order sent to Mahak successfully"';
    RAISE NOTICE '3. Run the verification query below';
    RAISE NOTICE '========================================';
END $$;

-- Verify the order was created
SELECT 
    '✅ ORDER DETAILS' as "Status",
    o."OrderNumber" as "Order Number",
    o."OrderStatus" as "Status",
    o."TotalAmount" as "Total (IRR)",
    o."SyncedToMahak" as "Synced to Mahak",
    o."MahakSyncedAt" as "Synced At",
    o."CreatedAt" as "Created At",
    i."ProductName" as "Product",
    i."Quantity" as "Qty",
    i."UnitPrice" as "Price"
FROM "UserOrders" o
INNER JOIN "UserOrderItems" i ON o."Id" = i."UserOrderId"
WHERE o."OrderNumber" LIKE 'MAHAK%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
