-- ============================================
-- ایجاد سفارش تست - نسخه ساده شده
-- ============================================
-- این اسکریپت فقط سفارش را ایجاد می‌کند
-- بدون کوئری verification
-- ============================================

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
    
    IF v_product_id IS NULL THEN
        RAISE EXCEPTION 'هیچ محصولی از محک پیدا نشد!';
    END IF;

    -- Get first user
    SELECT "Id" INTO v_user_id FROM "AspNetUsers" LIMIT 1;
    
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'هیچ کاربری در سیستم وجود ندارد!';
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
        'IRR', 'Test order for Mahak sync', NULL, NULL, NULL,
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
        1, v_product_price, v_product_price, 0, 'Test order item',
        NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
    );

    RAISE NOTICE '========================================';
    RAISE NOTICE '✅ سفارش با موفقیت ایجاد شد!';
    RAISE NOTICE 'شماره سفارش: %', v_order_number;
    RAISE NOTICE 'محصول: %', v_product_name;
    RAISE NOTICE 'مبلغ: % ریال', v_product_price + 50000;
    RAISE NOTICE 'OrderId: %', v_order_id;
    RAISE NOTICE '========================================';
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '❌ خطا: %', SQLERRM;
        RAISE;
END $$;
