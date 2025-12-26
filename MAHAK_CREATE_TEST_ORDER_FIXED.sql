-- ============================================
-- MAHAK TEST ORDER - FIXED VERSION
-- ============================================
-- این نسخه اصلاح شده با تمام فیلدهای لازم
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

    -- Create the order with ALL required fields
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
        "Notes",
        "ShippedAt",
        "DeliveredAt",
        "CancelledAt",
        "CancellationReason",
        "TrackingNumber",
        "EstimatedDeliveryDate",
        "ActualDeliveryDate",
        "ShippingAddressId",
        "BillingAddressId",
        "SyncedToMahak",
        "MahakSyncedAt",
        "MahakOrderId",
        "CreatedAt", 
        "UpdatedAt", 
        "Deleted",
        "CreatedBy",
        "UpdatedBy"
    )
    VALUES (
        v_order_id,                    -- Id
        v_user_id,                     -- UserId
        v_order_number,                -- OrderNumber
        'Completed',                   -- OrderStatus (must be Completed to sync)
        v_product_price,               -- SubTotal
        0,                             -- TaxAmount
        50000,                         -- ShippingAmount
        0,                             -- DiscountAmount
        v_product_price + 50000,       -- TotalAmount
        'IRR',                         -- Currency
        'Test order for Mahak sync',   -- Notes
        NULL,                          -- ShippedAt
        NULL,                          -- DeliveredAt
        NULL,                          -- CancelledAt
        NULL,                          -- CancellationReason
        NULL,                          -- TrackingNumber
        NOW() + INTERVAL '3 days',     -- EstimatedDeliveryDate
        NULL,                          -- ActualDeliveryDate
        NULL,                          -- ShippingAddressId
        NULL,                          -- BillingAddressId
        false,                         -- SyncedToMahak
        NULL,                          -- MahakSyncedAt
        NULL,                          -- MahakOrderId
        NOW(),                         -- CreatedAt
        NOW(),                         -- UpdatedAt
        false,                         -- Deleted
        'SYSTEM',                      -- CreatedBy
        'SYSTEM'                       -- UpdatedBy
    );

    -- Add order item with ALL required fields
    INSERT INTO "UserOrderItems" (
        "Id", 
        "UserOrderId", 
        "ProductId", 
        "ProductName", 
        "Quantity", 
        "UnitPrice", 
        "TotalPrice",
        "DiscountAmount",
        "TaxAmount",
        "CreatedAt", 
        "UpdatedAt", 
        "Deleted",
        "CreatedBy",
        "UpdatedBy"
    )
    VALUES (
        v_order_item_id,               -- Id
        v_order_id,                    -- UserOrderId
        v_product_id,                  -- ProductId
        v_product_name,                -- ProductName
        1,                             -- Quantity
        v_product_price,               -- UnitPrice
        v_product_price,               -- TotalPrice
        0,                             -- DiscountAmount
        0,                             -- TaxAmount
        NOW(),                         -- CreatedAt
        NOW(),                         -- UpdatedAt
        false,                         -- Deleted
        'SYSTEM',                      -- CreatedBy
        'SYSTEM'                       -- UpdatedBy
    );

    -- Show success message
    RAISE NOTICE '========================================';
    RAISE NOTICE '✅ سفارش تست با موفقیت ایجاد شد!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'شماره سفارش: %', v_order_number;
    RAISE NOTICE 'محصول: %', v_product_name;
    RAISE NOTICE 'قیمت: % ریال', v_product_price;
    RAISE NOTICE 'جمع کل: % ریال', v_product_price + 50000;
    RAISE NOTICE '========================================';
    RAISE NOTICE '⏳ مراحل بعدی:';
    RAISE NOTICE '1. یک دقیقه صبر کنید تا sync اجرا شود';
    RAISE NOTICE '2. در لاگ backend دنبال این پیام بگردید:';
    RAISE NOTICE '   "Order sent to Mahak successfully"';
    RAISE NOTICE '3. کوئری زیر را اجرا کنید تا وضعیت را ببینید';
    RAISE NOTICE '========================================';
END $$;

-- Verify the order was created
SELECT 
    '✅ جزئیات سفارش' as "وضعیت",
    o."OrderNumber" as "شماره سفارش",
    o."OrderStatus" as "وضعیت",
    o."TotalAmount" as "مبلغ کل (ریال)",
    o."SyncedToMahak" as "ارسال به محک",
    o."MahakSyncedAt" as "زمان ارسال",
    o."CreatedAt" as "تاریخ ایجاد",
    i."ProductName" as "محصول",
    i."Quantity" as "تعداد",
    i."UnitPrice" as "قیمت واحد"
FROM "UserOrders" o
INNER JOIN "UserOrderItems" i ON o."Id" = i."UserOrderId"
WHERE o."OrderNumber" LIKE 'MAHAK%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
