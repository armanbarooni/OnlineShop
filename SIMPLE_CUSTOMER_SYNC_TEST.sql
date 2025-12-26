-- ============================================
-- تست ساده: سفارش جدید برای Customer Sync
-- ============================================

DO $$
DECLARE
    v_user_id UUID;
    v_user_name VARCHAR(500);
    v_product_id UUID;
    v_product_name VARCHAR(500);
    v_product_price DECIMAL(18,2);
    v_order_id UUID := gen_random_uuid();
    v_order_number VARCHAR(50) := 'SYNC_TEST_' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');
    v_order_item_id UUID := gen_random_uuid();
BEGIN
    -- یک کاربر موجود بدون MahakPersonId
    SELECT "Id", "FirstName" || ' ' || "LastName"
    INTO v_user_id, v_user_name
    FROM "AspNetUsers"
    WHERE "MahakPersonId" IS NULL
    ORDER BY "Id"
    LIMIT 1;
    
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'هیچ کاربری بدون MahakPersonId پیدا نشد!';
    END IF;
    
    -- یک محصول
    SELECT "Id", "Name", "Price"
    INTO v_product_id, v_product_name, v_product_price
    FROM "Products"
    WHERE "MahakId" IS NOT NULL
    LIMIT 1;
    
    IF v_product_id IS NULL THEN
        RAISE EXCEPTION 'هیچ محصولی از محک پیدا نشد!';
    END IF;
    
    -- سفارش
    INSERT INTO "UserOrders" (
        "Id", "UserId", "OrderNumber", "OrderStatus",
        "SubTotal", "TaxAmount", "ShippingAmount", "DiscountAmount", "TotalAmount",
        "Currency", "Notes",
        "SyncedToMahak", "MahakSyncedAt", "MahakOrderId",
        "CreatedAt", "UpdatedAt", "Deleted", "CreatedBy", "UpdatedBy", "RowVersion"
    )
    VALUES (
        v_order_id, v_user_id, v_order_number, 'Completed',
        v_product_price, 0, 0, 0, v_product_price,
        'IRR', 'تست Customer Sync',
        false, NULL, NULL,
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );
    
    -- آیتم
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
    RAISE NOTICE '✅ سفارش تست ایجاد شد';
    RAISE NOTICE 'شماره: %', v_order_number;
    RAISE NOTICE 'کاربر: % (ID: %)', v_user_name, v_user_id;
    RAISE NOTICE 'محصول: %', v_product_name;
    RAISE NOTICE '========================================';
    RAISE NOTICE '⏳ منتظر 1 دقیقه بمانید تا worker اجرا شود';
    RAISE NOTICE '📋 در لاگ backend دنبال این بگردید:';
    RAISE NOTICE '   [INF] Found 1 orders to sync';
    RAISE NOTICE '   [INF] Syncing customer ... to Mahak';
    RAISE NOTICE '   [INF] Customer synced successfully';
    RAISE NOTICE '========================================';
END $$;

-- بررسی قبل از sync
SELECT 
    '⏳ قبل از Sync' as "وضعیت",
    o."OrderNumber",
    u."FirstName" || ' ' || u."LastName" as "کاربر",
    u."MahakPersonId" as "Person ID (قبل)",
    o."SyncedToMahak",
    o."MahakOrderId"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON o."UserId" = u."Id"
WHERE o."OrderNumber" LIKE 'SYNC_TEST_%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
