-- ============================================
-- فاکتور فروش برای کاربر جدید
-- ============================================

DO $$
DECLARE
    v_new_user_id UUID := gen_random_uuid();
    v_phone VARCHAR(20) := '09' || LPAD(FLOOR(RANDOM() * 1000000000)::TEXT, 9, '0');
    v_product_id UUID;
    v_product_name VARCHAR(500);
    v_product_price DECIMAL(18,2);
    v_order_id UUID := gen_random_uuid();
    v_order_number VARCHAR(50) := 'NEW_SALE_' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');
    v_order_item_id UUID := gen_random_uuid();
BEGIN
    -- ایجاد کاربر جدید
    INSERT INTO "AspNetUsers" (
        "Id", "UserName", "NormalizedUserName",
        "Email", "NormalizedEmail", "EmailConfirmed",
        "PhoneNumber", "PhoneNumberConfirmed",
        "FirstName", "LastName",
        "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
        "CreatedAt"
    )
    VALUES (
        v_new_user_id, v_phone, UPPER(v_phone),
        v_phone || '@shop.local', UPPER(v_phone || '@shop.local'), false,
        v_phone, true,
        'مشتری', 'جدید',
        false, false, 0,
        NOW()
    );
    
    RAISE NOTICE '✅ کاربر جدید ایجاد شد: % (ID: %)', v_phone, v_new_user_id;
    
    -- انتخاب محصول
    SELECT "Id", "Name", "Price"
    INTO v_product_id, v_product_name, v_product_price
    FROM "Products"
    WHERE "MahakId" IS NOT NULL
    ORDER BY RANDOM()
    LIMIT 1;
    
    IF v_product_id IS NULL THEN
        RAISE EXCEPTION 'هیچ محصولی از محک پیدا نشد!';
    END IF;
    
    -- ایجاد فاکتور فروش
    INSERT INTO "UserOrders" (
        "Id", "UserId", "OrderNumber", "OrderStatus",
        "SubTotal", "TaxAmount", "ShippingAmount", "DiscountAmount", "TotalAmount",
        "Currency", "Notes",
        "SyncedToMahak", "MahakSyncedAt", "MahakOrderId",
        "CreatedAt", "UpdatedAt", "Deleted", "CreatedBy", "UpdatedBy", "RowVersion"
    )
    VALUES (
        v_order_id, v_new_user_id, v_order_number, 'Completed',
        v_product_price, 0, 50000, 0, v_product_price + 50000,
        'IRR', '🎯 فاکتور فروش - مشتری جدید',
        false, NULL, NULL,
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );
    
    -- افزودن آیتم به فاکتور
    INSERT INTO "UserOrderItems" (
        "Id", "OrderId", "ProductId", "ProductName",
        "Quantity", "UnitPrice", "TotalPrice", "DiscountAmount",
        "CreatedAt", "UpdatedAt", "Deleted", "CreatedBy", "UpdatedBy", "RowVersion"
    )
    VALUES (
        v_order_item_id, v_order_id, v_product_id, v_product_name,
        2, v_product_price, v_product_price * 2, 0,
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );
    
    RAISE NOTICE '========================================';
    RAISE NOTICE '🎯 فاکتور فروش ایجاد شد';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'شماره فاکتور: %', v_order_number;
    RAISE NOTICE 'کاربر: مشتری جدید (%)' , v_phone;
    RAISE NOTICE 'محصول: %', v_product_name;
    RAISE NOTICE 'تعداد: 2';
    RAISE NOTICE 'مبلغ کل: % ریال', (v_product_price * 2) + 50000;
    RAISE NOTICE '========================================';
    RAISE NOTICE '⏳ منتظر 1-2 دقیقه بمانید تا:';
    RAISE NOTICE '   1. مشتری به محک ارسال شود';
    RAISE NOTICE '   2. فاکتور با لینک به مشتری ارسال شود';
    RAISE NOTICE '========================================';
    RAISE NOTICE '📋 در لاگ backend دنبال این بگردید:';
    RAISE NOTICE '   [INF] Found 1 orders to sync';
    RAISE NOTICE '   [INF] Syncing customer ... (مشتری جدید) to Mahak';
    RAISE NOTICE '   [INF] Customer synced successfully';
    RAISE NOTICE '   [INF] Order sent to Mahak successfully';
    RAISE NOTICE '========================================';
END $$;

-- بررسی قبل از sync
SELECT 
    '⏳ قبل از Sync' as "وضعیت",
    o."OrderNumber" as "شماره فاکتور",
    u."FirstName" || ' ' || u."LastName" as "مشتری",
    u."PhoneNumber" as "شماره تماس",
    u."MahakPersonClientId" as "Person ID (قبل)",
    o."TotalAmount" as "مبلغ کل",
    o."SyncedToMahak" as "Synced?"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON o."UserId" = u."Id"
WHERE o."OrderNumber" LIKE 'NEW_SALE_%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
