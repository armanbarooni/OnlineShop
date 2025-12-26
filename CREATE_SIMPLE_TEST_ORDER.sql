-- Simple test order - no PersonId required
-- Just creates an order ready for Mahak sync

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
    -- Find ANY user
    SELECT "Id" INTO v_user_id
    FROM "AspNetUsers"
    WHERE "EmailConfirmed" = true
    LIMIT 1;

    IF v_user_id IS NULL THEN
        -- Create a test user
        v_user_id := gen_random_uuid();
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PhoneNumber", "PhoneNumberConfirmed",
            "FirstName", "LastName",
            "LockoutEnabled", "AccessFailedCount",
            "CreatedAt", "UpdatedAt", "Deleted"
        )
        VALUES (
            v_user_id, 'mahaktest@test.com', 'MAHAKTEST@TEST.COM',
            'mahaktest@test.com', 'MAHAKTEST@TEST.COM',
            true, '09123456789', true,
            'مشتری', 'تست',
            false, 0,
            NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', false
        );
        RAISE NOTICE '✅ Created test user';
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
            "RowVersion", "CreatedAt", "UpdatedAt", "Deleted"
        )
        VALUES (
            v_address_id, v_user_id, 'منزل', 'مشتری', 'تست',
            'خیابان ولیعصر، پلاک 123', 'تهران', 'تهران', '1234567890', 'ایران',
            '09123456789', true, true, true,
            0, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', false
        );
        RAISE NOTICE '✅ Created address';
    ELSE
        RAISE NOTICE '✅ Using existing address';
    END IF;

    -- Find a product with Mahak mapping
    SELECT p."Id", p."Name", p."Price"
    INTO v_product_id, v_product_name, v_unit_price
    FROM "Products" p
    INNER JOIN "MahakMappings" mm ON mm."LocalEntityId" = p."Id"
    WHERE p."Deleted" = false
      AND p."StockQuantity" > 0
      AND mm."EntityType" = 'Product'
      AND mm."MahakEntityId" IS NOT NULL
    LIMIT 1;

    IF v_product_id IS NULL THEN
        RAISE NOTICE '⚠️  No product with Mahak mapping found!';
        RETURN;
    END IF;

    RAISE NOTICE '✅ Product: % (Price: %)', v_product_name, v_unit_price;

    -- Calculate amounts
    v_sub_total := v_unit_price * v_quantity;
    v_total_amount := v_sub_total + v_shipping_amount;

    -- Create order
    v_order_id := gen_random_uuid();
    v_order_item_id := gen_random_uuid();
    v_order_number := 'MAHAK-' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');

    INSERT INTO "UserOrders" (
        "Id", "UserId", "OrderNumber", "OrderStatus",
        "SubTotal", "TaxAmount", "ShippingAmount", "DiscountAmount", "TotalAmount",
        "Currency", "ShippingAddressId", "BillingAddressId",
        "SyncedToMahak", "MahakSyncedAt", "MahakOrderId",
        "RowVersion", "CreatedAt", "UpdatedAt", "Deleted"
    )
    VALUES (
        v_order_id, v_user_id, v_order_number, 'Processing',
        v_sub_total, 0, v_shipping_amount, 0, v_total_amount,
        'IRR', v_address_id, v_address_id,
        false, NULL, NULL,
        0, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', false
    );

    -- Create order item
    INSERT INTO "UserOrderItems" (
        "Id", "OrderId", "ProductId",
        "ProductName", "Quantity", "UnitPrice", "TotalPrice", "DiscountAmount",
        "RowVersion", "CreatedAt", "UpdatedAt", "Deleted"
    )
    VALUES (
        v_order_item_id, v_order_id, v_product_id,
        v_product_name, v_quantity, v_unit_price, v_sub_total, 0,
        0, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', false
    );

    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE '✅ ORDER CREATED!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Order: %', v_order_number;
    RAISE NOTICE 'Total: % Toman', v_total_amount;
    RAISE NOTICE '';
    RAISE NOTICE '⏰ Wait 1 minute for sync';
    RAISE NOTICE '📋 Check backend logs for:';
    RAISE NOTICE '   - "storeId": 1';
    RAISE NOTICE '   - "shippingAddress": "{...JSON...}"';
    RAISE NOTICE '========================================';
END $$;
