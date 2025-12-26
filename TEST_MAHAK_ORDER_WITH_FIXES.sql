-- Simple Test Order for Mahak Sync - PostgreSQL
-- Creates a test order with proper schema

DO $$
DECLARE
    v_user_id UUID;
    v_address_id UUID;
    v_product_id UUID;
    v_order_id UUID;
    v_order_item_id UUID;
    v_order_number VARCHAR(50);
    v_product_name VARCHAR(500);
    v_unit_price DECIMAL(18,2);
    v_quantity INT := 1;
    v_sub_total DECIMAL(18,2);
    v_shipping_amount DECIMAL(18,2) := 50000;
    v_total_amount DECIMAL(18,2);
BEGIN
    -- Find an active user
    SELECT "Id" INTO v_user_id
    FROM "AspNetUsers"
    WHERE "EmailConfirmed" = true
    LIMIT 1;

    IF v_user_id IS NULL THEN
        RAISE NOTICE '⚠️  No user found. Creating a test user...';
        
        v_user_id := gen_random_uuid();
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PhoneNumber", "PhoneNumberConfirmed",
            "FirstName", "LastName",
            "LockoutEnabled", "AccessFailedCount",
            "CreatedAt", "UpdatedAt", "Deleted"
        )
        VALUES (
            v_user_id, 'testuser@test.com', 'TESTUSER@TEST.COM',
            'testuser@test.com', 'TESTUSER@TEST.COM',
            true, '09123456789', true,
            'آرمان', 'تست',
            false, 0,
            NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', false
        );
        
        RAISE NOTICE '✅ Created test user: %', v_user_id;
    ELSE
        RAISE NOTICE '✅ Using existing user: %', v_user_id;
    END IF;

    -- Get or create address
    SELECT "Id" INTO v_address_id
    FROM "UserAddresses"
    WHERE "UserId" = v_user_id AND "Deleted" = false
    LIMIT 1;

    IF v_address_id IS NULL THEN
        v_address_id := gen_random_uuid();
        INSERT INTO "UserAddresses" (
            "Id", "UserId", "Title", "FirstName", "LastName",
            "AddressLine1", "City", "State", "PostalCode", "Country",
            "PhoneNumber", "IsDefault", "IsBillingAddress", "IsShippingAddress",
            "CreatedAt", "UpdatedAt", "Deleted"
        )
        VALUES (
            v_address_id, v_user_id, 'منزل', 'آرمان', 'تست',
            'خیابان ولیعصر، پلاک 123', 'تهران', 'تهران', '1234567890', 'ایران',
            '09123456789', true, true, true,
            NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', false
        );
        RAISE NOTICE '✅ Created address: %', v_address_id;
    ELSE
        RAISE NOTICE '✅ Using existing address: %', v_address_id;
    END IF;

    -- Find a product with Mahak mapping
    SELECT p."Id", p."Name", p."Price"
    INTO v_product_id, v_product_name, v_unit_price
    FROM "Products" p
    INNER JOIN "ProductDetailMahakMappings" pdm ON pdm."LocalEntityId" = p."Id"
    WHERE p."Deleted" = false
      AND p."StockQuantity" > 0
      AND pdm."MahakEntityId" IS NOT NULL
    LIMIT 1;

    IF v_product_id IS NULL THEN
        RAISE NOTICE '⚠️  No product with Mahak mapping found!';
        RAISE NOTICE 'Please sync products from Mahak first.';
        RETURN;
    END IF;

    RAISE NOTICE '✅ Product: % (Price: %)', v_product_name, v_unit_price;

    -- Calculate amounts
    v_sub_total := v_unit_price * v_quantity;
    v_total_amount := v_sub_total + v_shipping_amount;

    -- Create order
    v_order_id := gen_random_uuid();
    v_order_item_id := gen_random_uuid();
    v_order_number := 'TEST-MAHAK-' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');

    INSERT INTO "UserOrders" (
        "Id", "UserId", "OrderNumber", "OrderStatus",
        "SubTotal", "TaxAmount", "ShippingAmount", "DiscountAmount", "TotalAmount",
        "Currency", "ShippingAddressId", "BillingAddressId",
        "SyncedToMahak", "MahakSyncedAt", "MahakOrderId",
        "CreatedAt", "UpdatedAt", "Deleted"
    )
    VALUES (
        v_order_id, v_user_id, v_order_number, 'Processing',
        v_sub_total, 0, v_shipping_amount, 0, v_total_amount,
        'IRR', v_address_id, v_address_id,
        false, NULL, NULL,
        NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', false
    );

    -- Create order item
    INSERT INTO "UserOrderItems" (
        "Id", "OrderId", "ProductId",
        "ProductName", "Quantity", "UnitPrice", "TotalPrice", "DiscountAmount",
        "CreatedAt", "UpdatedAt", "Deleted"
    )
    VALUES (
        v_order_item_id, v_order_id, v_product_id,
        v_product_name, v_quantity, v_unit_price, v_sub_total, 0,
        NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', false
    );

    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE '✅ ORDER CREATED SUCCESSFULLY!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Order Number: %', v_order_number;
    RAISE NOTICE 'Order ID: %', v_order_id;
    RAISE NOTICE 'SubTotal: % Toman', v_sub_total;
    RAISE NOTICE 'Shipping: % Toman', v_shipping_amount;
    RAISE NOTICE 'Total: % Toman', v_total_amount;
    RAISE NOTICE 'Status: Processing (Ready for Mahak sync)';
    RAISE NOTICE '';
    RAISE NOTICE '📋 Next Steps:';
    RAISE NOTICE '1. Restart backend to apply StoreId & JSON fixes';
    RAISE NOTICE '2. Wait for MahakSyncWorker to run';
    RAISE NOTICE '3. Check logs for JSON with storeId & shippingAddress';
    RAISE NOTICE '========================================';
END $$;

-- Show created order
SELECT 
    o."OrderNumber",
    o."OrderStatus",
    o."SubTotal",
    o."ShippingAmount",
    o."TotalAmount",
    o."SyncedToMahak",
    u."Email" AS "CustomerEmail"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON u."Id" = o."UserId"
WHERE o."OrderNumber" LIKE 'TEST-MAHAK-%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;
