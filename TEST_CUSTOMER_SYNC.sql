-- ============================================
-- تست Customer Sync با سفارش جدید
-- ============================================

DO $$
DECLARE
    v_product_id UUID;
    v_product_name VARCHAR(500);
    v_product_price DECIMAL(18,2);
    v_user_id UUID;
    v_order_id UUID := gen_random_uuid();
    v_order_number VARCHAR(50) := 'CUST' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');
    v_order_item_id UUID := gen_random_uuid();
BEGIN
    -- Get product from Mahak
    SELECT "Id", "Name", "Price" 
    INTO v_product_id, v_product_name, v_product_price
    FROM "Products" 
    WHERE "MahakId" IS NOT NULL 
    ORDER BY "CreatedAt" DESC 
    LIMIT 1;
    
    IF v_product_id IS NULL THEN
        RAISE EXCEPTION 'هیچ محصولی از محک پیدا نشد!';
    END IF;

    -- Get first user (should NOT have MahakPersonId yet)
    SELECT "Id" INTO v_user_id 
    FROM "AspNetUsers" 
    WHERE "MahakPersonId" IS NULL
    LIMIT 1;
    
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'هیچ کاربری بدون MahakPersonId وجود ندارد!';
    END IF;

    -- Create the order
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
        'IRR', 'Test order for Customer Sync', NULL, NULL, NULL,
        NULL, NULL, NOW() + INTERVAL '3 days',
        NULL, NULL, NULL,
        false, NULL, NULL,
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );

    -- Add order item
    INSERT INTO "UserOrderItems" (
        "Id", "OrderId", "ProductId", "ProductName", 
        "ProductDescription", "ProductSku",
        "Quantity", "UnitPrice", "TotalPrice", "DiscountAmount", "Notes",
        "CreatedAt", "UpdatedAt", "Deleted", "CreatedBy", "UpdatedBy", "RowVersion"
    )
    VALUES (
        v_order_item_id, v_order_id, v_product_id, v_product_name, 
        NULL, NULL,
        1, v_product_price, v_product_price, 0, 'Test item',
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );

    RAISE NOTICE '========================================';
    RAISE NOTICE '✅ سفارش تست Customer Sync ایجاد شد!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'شماره سفارش: %', v_order_number;
    RAISE NOTICE 'کاربر: %', v_user_id;
    RAISE NOTICE 'محصول: %', v_product_name;
    RAISE NOTICE 'مبلغ: % ریال', v_product_price + 50000;
    RAISE NOTICE '========================================';
    RAISE NOTICE '⏳ منتظر بمانید:';
    RAISE NOTICE '1. Worker در 1 دقیقه اجرا می‌شود';
    RAISE NOTICE '2. ابتدا Customer به محک ارسال می‌شود';
    RAISE NOTICE '3. سپس Order ارسال می‌شود';
    RAISE NOTICE '========================================';
    RAISE NOTICE '📊 در لاگ backend دنبال این پیام‌ها بگردید:';
    RAISE NOTICE '   "Syncing customer ... to Mahak"';
    RAISE NOTICE '   "Customer ... synced to Mahak successfully"';
    RAISE NOTICE '   "Sending order ... to Mahak"';
    RAISE NOTICE '========================================';
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '❌ خطا: %', SQLERRM;
        RAISE;
END $$;

-- بررسی سفارش ایجاد شده
SELECT 
    '✅ سفارش ایجاد شده' as "وضعیت",
    o."OrderNumber",
    u."Email" as "کاربر",
    u."MahakPersonId" as "Mahak Person ID (قبل از sync)",
    o."SyncedToMahak" as "Synced?",
    o."CreatedAt"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON o."UserId" = u."Id"
WHERE o."OrderNumber" LIKE 'CUST%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
