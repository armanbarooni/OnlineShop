-- Create test order using existing Mahak Person
-- This ensures the order will have a valid PersonId from Mahak

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
    v_mahak_person_id INT;
BEGIN
    -- Find a user that has Mahak PersonId (synced from Mahak)
    SELECT 
        u."Id",
        pm."MahakEntityId"
    INTO v_user_id, v_mahak_person_id
    FROM "AspNetUsers" u
    INNER JOIN "PersonMahakMappings" pm ON pm."LocalEntityId" = u."Id"
    WHERE u."EmailConfirmed" = true
      AND pm."MahakEntityId" IS NOT NULL
      AND pm."MahakEntityId" > 0
    LIMIT 1;

    IF v_user_id IS NULL THEN
        RAISE NOTICE '⚠️  No user with Mahak PersonId found!';
        RAISE NOTICE 'Available Mahak Persons:';
        
        -- Show available Mahak persons
        FOR v_mahak_person_id IN 
            SELECT DISTINCT "MahakEntityId" 
            FROM "PersonMahakMappings" 
            WHERE "MahakEntityId" IS NOT NULL AND "MahakEntityId" > 0
        LOOP
            RAISE NOTICE '  - Mahak PersonId: %', v_mahak_person_id;
        END LOOP;
        
        RETURN;
    END IF;

    RAISE NOTICE '✅ Using User with Mahak PersonId: %', v_mahak_person_id;
    RAISE NOTICE '   User ID: %', v_user_id;

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
            v_address_id, v_user_id, 'منزل', 'مشتری', 'محک',
            'خیابان ولیعصر، پلاک 123', 'تهران', 'تهران', '1234567890', 'ایران',
            '09123456789', true, true, true,
            NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', false
        );
        RAISE NOTICE '✅ Created address';
    ELSE
        RAISE NOTICE '✅ Using existing address';
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
        RETURN;
    END IF;

    RAISE NOTICE '✅ Product: % (Price: %)', v_product_name, v_unit_price;

    -- Calculate amounts
    v_sub_total := v_unit_price * v_quantity;
    v_total_amount := v_sub_total + v_shipping_amount;

    -- Create order
    v_order_id := gen_random_uuid();
    v_order_item_id := gen_random_uuid();
    v_order_number := 'MAHAK-TEST-' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS');

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
    RAISE NOTICE 'Mahak PersonId: %', v_mahak_person_id;
    RAISE NOTICE 'SubTotal: % Toman', v_sub_total;
    RAISE NOTICE 'Shipping: % Toman', v_shipping_amount;
    RAISE NOTICE 'Total: % Toman', v_total_amount;
    RAISE NOTICE '';
    RAISE NOTICE '⏰ Wait up to 1 minute for MahakOutgoingSyncWorker';
    RAISE NOTICE '📋 Check backend logs for JSON payload';
    RAISE NOTICE '========================================';
END $$;
