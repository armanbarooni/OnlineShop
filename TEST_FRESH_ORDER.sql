-- ============================================
-- تست مجدد: سفارش با مشتری (Fresh)
-- ============================================

DO $$
DECLARE
    v_product_id UUID;
    v_product_name VARCHAR(500);
    v_product_price DECIMAL(18,2);
    v_user_id UUID;
    v_user_name VARCHAR(500);
    v_order_id UUID := gen_random_uuid();
    v_order_number VARCHAR(50) := 'FRESH_' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');
    v_order_item_id UUID := gen_random_uuid();
BEGIN
    -- پیدا کردن محصول
    SELECT "Id", "Name", "Price" 
    INTO v_product_id, v_product_name, v_product_price
    FROM "Products" 
    WHERE "MahakId" IS NOT NULL 
    ORDER BY "CreatedAt" DESC 
    LIMIT 1;
    
    -- پیدا کردن یکی از کاربران "ارمان بارونی" که MahakPersonId ندارد
    SELECT "Id", "FirstName" || ' ' || "LastName"
    INTO v_user_id, v_user_name
    FROM "AspNetUsers" 
    WHERE "FirstName" = 'ارمان' 
      AND "LastName" = 'بارونی'
      AND "MahakPersonId" IS NULL
    LIMIT 1;
    
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'کاربر پیدا نشد!';
    END IF;

    -- ایجاد سفارش FRESH
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
        'IRR', '🟢 تست FRESH: سفارش با Customer Sync',
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
    RAISE NOTICE '✅ سفارش FRESH ایجاد شد';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'شماره سفارش: %', v_order_number;
    RAISE NOTICE 'کاربر: % (ID: %)', v_user_name, v_user_id;
    RAISE NOTICE 'محصول: %', v_product_name;
    RAISE NOTICE '========================================';
    RAISE NOTICE '⏳ منتظر بمانید تا worker اجرا شود';
    RAISE NOTICE '📋 در لاگ دنبال این بگردید:';
    RAISE NOTICE '   [INF] Found 1 orders to sync';
    RAISE NOTICE '   [INF] Syncing customer ... to Mahak  ← مهم!';
    RAISE NOTICE '   [INF] Customer synced successfully';
    RAISE NOTICE '========================================';
    
END $$;

-- بررسی قبل از sync
SELECT 
    '🟢 قبل از Sync' as "وضعیت",
    o."OrderNumber",
    u."FirstName" || ' ' || u."LastName" as "کاربر",
    u."MahakPersonId" as "Person ID (قبل)",
    o."SyncedToMahak",
    o."MahakOrderId"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON o."UserId" = u."Id"
WHERE o."OrderNumber" LIKE 'FRESH_%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
