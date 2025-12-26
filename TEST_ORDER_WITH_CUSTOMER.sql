-- ============================================
-- تست کامل: ایجاد سفارش با لینک به مشتری
-- ============================================

DO $$
DECLARE
    v_product_id UUID;
    v_product_name VARCHAR(500);
    v_product_price DECIMAL(18,2);
    v_user_id UUID;
    v_order_id UUID := gen_random_uuid();
    v_order_number VARCHAR(50) := 'TEST' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');
    v_order_item_id UUID := gen_random_uuid();
BEGIN
    -- 1. پیدا کردن محصول از محک
    SELECT "Id", "Name", "Price" 
    INTO v_product_id, v_product_name, v_product_price
    FROM "Products" 
    WHERE "MahakId" IS NOT NULL 
    ORDER BY "CreatedAt" DESC 
    LIMIT 1;
    
    IF v_product_id IS NULL THEN
        RAISE EXCEPTION 'هیچ محصولی از محک پیدا نشد!';
    END IF;

    -- 2. پیدا کردن کاربر (ترجیحاً بدون MahakPersonId)
    SELECT "Id" INTO v_user_id 
    FROM "AspNetUsers" 
    WHERE "MahakPersonId" IS NULL
    LIMIT 1;
    
    IF v_user_id IS NULL THEN
        -- اگر همه کاربران MahakPersonId دارند، اولین کاربر را بگیر
        SELECT "Id" INTO v_user_id 
        FROM "AspNetUsers" 
        LIMIT 1;
    END IF;

    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'هیچ کاربری در سیستم وجود ندارد!';
    END IF;

    -- 3. ایجاد سفارش
    INSERT INTO "UserOrders" (
        "Id", "UserId", "OrderNumber", "OrderStatus", 
        "SubTotal", "TaxAmount", "ShippingAmount", "DiscountAmount", "TotalAmount",
        "Currency", "Notes", "ShippedAt", "DeliveredAt", "CancelledAt",
        "CancellationReason", "TrackingNumber", "EstimatedDeliveryDate",
        "ActualDeliveryDate", "ShippingAddressId", "BillingAddressId",
        "SyncedToMahak", "MahakSyncedAt", "MahakOrderId",
        "CreatedAt", "UpdatedAt", "Deleted", "CreatedBy", "UpdatedBy", "RowVersion"
    )
    VALUES (
        v_order_id, v_user_id, v_order_number, 'Completed',
        v_product_price, 0, 50000, 0, v_product_price + 50000,
        'IRR', 'تست سفارش با لینک به مشتری', NULL, NULL, NULL,
        NULL, NULL, NOW() + INTERVAL '3 days',
        NULL, NULL, NULL,
        false, NULL, NULL,
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );

    -- 4. افزودن آیتم سفارش
    INSERT INTO "UserOrderItems" (
        "Id", "OrderId", "ProductId", "ProductName", 
        "ProductDescription", "ProductSku",
        "Quantity", "UnitPrice", "TotalPrice", "DiscountAmount", "Notes",
        "CreatedAt", "UpdatedAt", "Deleted", "CreatedBy", "UpdatedBy", "RowVersion"
    )
    VALUES (
        v_order_item_id, v_order_id, v_product_id, v_product_name, 
        NULL, NULL,
        1, v_product_price, v_product_price, 0, 'تست آیتم',
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );

    RAISE NOTICE '========================================';
    RAISE NOTICE '✅ سفارش تست ایجاد شد!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'شماره سفارش: %', v_order_number;
    RAISE NOTICE 'کاربر: %', v_user_id;
    RAISE NOTICE 'محصول: %', v_product_name;
    RAISE NOTICE 'مبلغ کل: % ریال', v_product_price + 50000;
    RAISE NOTICE '========================================';
    RAISE NOTICE '⏳ جریان کار:';
    RAISE NOTICE '1. Worker در 1 دقیقه اجرا می‌شود';
    RAISE NOTICE '2. ابتدا بررسی می‌کند آیا کاربر MahakPersonId دارد';
    RAISE NOTICE '3. اگر ندارد، کاربر را به محک می‌فرستد';
    RAISE NOTICE '4. سپس سفارش را با PersonId به محک می‌فرستد';
    RAISE NOTICE '========================================';
    
END $$;

-- بررسی سفارش و کاربر
SELECT 
    '📋 جزئیات سفارش و کاربر' as "عنوان",
    o."OrderNumber" as "شماره سفارش",
    u."Email" as "ایمیل کاربر",
    u."FirstName" || ' ' || u."LastName" as "نام کاربر",
    u."MahakPersonId" as "Mahak Person ID (قبل از sync)",
    o."SyncedToMahak" as "سفارش Sync شده؟",
    o."TotalAmount" as "مبلغ کل"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON o."UserId" = u."Id"
WHERE o."OrderNumber" LIKE 'TEST%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
