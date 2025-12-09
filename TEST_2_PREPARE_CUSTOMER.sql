-- ============================================
-- تست 2: ارسال مشتری به محک
-- ============================================

DO $$
DECLARE
    v_user_id UUID;
    v_user_name VARCHAR(500);
    v_user_phone VARCHAR(20);
BEGIN
    -- پیدا کردن کاربری که MahakPersonId ندارد
    SELECT "Id", "FirstName" || ' ' || "LastName", "PhoneNumber"
    INTO v_user_id, v_user_name, v_user_phone
    FROM "AspNetUsers" 
    WHERE "MahakPersonId" IS NULL
    LIMIT 1;
    
    IF v_user_id IS NULL THEN
        RAISE NOTICE '⚠️ همه کاربران MahakPersonId دارند!';
        RAISE NOTICE 'یک کاربر جدید ایجاد می‌کنیم...';
        
        -- ایجاد کاربر جدید
        v_user_id := gen_random_uuid();
        v_user_name := 'تست محک';
        v_user_phone := '09' || LPAD(FLOOR(RANDOM() * 1000000000)::TEXT, 9, '0');
        
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName",
            "Email", "NormalizedEmail", "EmailConfirmed",
            "PhoneNumber", "PhoneNumberConfirmed",
            "FirstName", "LastName",
            "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
            "CreatedAt", "UpdatedAt", "Deleted", "CreatedBy", "UpdatedBy", "RowVersion"
        )
        VALUES (
            v_user_id, v_user_phone, UPPER(v_user_phone),
            v_user_phone || '@phone.local', UPPER(v_user_phone || '@phone.local'), false,
            v_user_phone, true,
            'تست', 'محک',
            false, false, 0,
            NOW(), NOW(), false, 'SYSTEM', 'SYSTEM', 1
        );
        
        RAISE NOTICE '✅ کاربر جدید ایجاد شد: % (%)', v_user_name, v_user_phone;
    END IF;

    RAISE NOTICE '========================================';
    RAISE NOTICE '🟡 تست 2: آماده برای ارسال مشتری';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'کاربر: %', v_user_name;
    RAISE NOTICE 'شماره: %', v_user_phone;
    RAISE NOTICE 'User ID: %', v_user_id;
    RAISE NOTICE '========================================';
    RAISE NOTICE '📋 حالا یک سفارش برای این کاربر ایجاد کنید';
    RAISE NOTICE '   تا مشتری به محک ارسال شود';
    RAISE NOTICE '========================================';
    
END $$;

-- نمایش کاربران بدون MahakPersonId
SELECT 
    '🟡 کاربران بدون Mahak Person ID' as "نوع",
    "Id",
    "FirstName" || ' ' || "LastName" as "نام",
    "PhoneNumber",
    "Email",
    "MahakPersonId"
FROM "AspNetUsers"
WHERE "MahakPersonId" IS NULL
ORDER BY "CreatedAt" DESC
LIMIT 5;
