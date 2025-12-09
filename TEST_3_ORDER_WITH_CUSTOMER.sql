-- ============================================
-- تست 3: سفارش با مشتری
-- ============================================

DO $$
DECLARE
    v_product_id UUID;
    v_product_name VARCHAR(500);
    v_product_price DECIMAL(18,2);
    v_user_id UUID;
    v_user_name VARCHAR(500);
    v_order_id UUID := gen_random_uuid();
    v_order_number VARCHAR(50) := 'WITH_CUST_' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');
    v_order_item_id UUID := gen_random_uuid();
BEGIN
    -- پیدا کردن محصول
    SELECT "Id", "Name", "Price" 
    INTO v_product_id, v_product_name, v_product_price
    FROM "Products" 
    WHERE "MahakId" IS NOT NULL 
    ORDER BY "CreatedAt" DESC 
    LIMIT 1;
    
    IF v_product_id IS NULL THEN
        RAISE EXCEPTION 'هیچ محصولی از محک پیدا نشد!';
    END IF;

    -- پیدا کردن کاربر بدون MahakPersonId
    SELECT "Id", "FirstName" || ' ' || "LastName"
    INTO v_user_id, v_user_name
    FROM "AspNetUsers" 
    WHERE "MahakPersonId" IS NULL
    ORDER BY "CreatedAt" DESC
    LIMIT 1;
    
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'هیچ کاربری بدون MahakPersonId وجود ندارد! ابتدا TEST_2 را اجرا کنید.';
    END IF;

    -- ایجاد سفارش
    INSERT INTO "UserOrders" (
        "Id", "UserId", "OrderNumber", "OrderStatus", 
        "SubTotal", "TaxAmount", "ShippingAmount", "DiscountAmount", "TotalAmount",
        "Currency", "Notes",
        "SyncedToMahak", "MahakSyncedAt", "MahakOrderId",
        "CreatedAt", "UpdatedAt", "Deleted", "CreatedBy", "UpdatedBy", "RowVersion"
    )
    VALUES (
        v_order_id, v_user_id, v_order_number, 'Completed',
        v_product_price, 0, 50000, 0, v_product_price + 50000,
        'IRR', '🟢 تست: سفارش با مشتری',
        false, NULL, NULL,
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );

    -- افزودن آیتم
    INSERT INTO "UserOrderItems" (
        "Id", "OrderId", "ProductId", "ProductName", 
        "Quantity", "UnitPrice", "TotalPrice", "DiscountAmount",
        "CreatedAt", "UpdatedAt", "Deleted", "CreatedBy", "UpdatedBy", "RowVersion"
    )
    VALUES (
        v_order_item_id, v_order_id, v_product_id, v_product_name, 
        1, v_product_price, v_product_price, 0,
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );

    RAISE NOTICE '========================================';
    RAISE NOTICE '✅ تست 3: سفارش با مشتری ایجاد شد';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'شماره سفارش: %', v_order_number;
    RAISE NOTICE 'کاربر: % (ID: %)', v_user_name, v_user_id;
    RAISE NOTICE 'محصول: %', v_product_name;
    RAISE NOTICE 'مبلغ: % ریال', v_product_price + 50000;
    RAISE NOTICE '========================================';
    RAISE NOTICE '⏳ منتظر بمانید تا worker اجرا شود (1 دقیقه)';
    RAISE NOTICE '📋 در لاگ دنبال این بگردید:';
    RAISE NOTICE '   "Syncing customer ... to Mahak"';
    RAISE NOTICE '   "Customer ... synced successfully"';
    RAISE NOTICE '   "Sending order ... to Mahak"';
    RAISE NOTICE '   "Order ... sent with PersonId: XXXXX"';
    RAISE NOTICE '========================================';
    
END $$;

-- بررسی سفارش
SELECT 
    '🟢 سفارش با مشتری' as "نوع تست",
    o."OrderNumber",
    u."FirstName" || ' ' || u."LastName" as "کاربر",
    u."MahakPersonId" as "Person ID (قبل)",
    o."SyncedToMahak",
    o."MahakOrderId"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON o."UserId" = u."Id"
WHERE o."OrderNumber" LIKE 'WITH_CUST_%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
