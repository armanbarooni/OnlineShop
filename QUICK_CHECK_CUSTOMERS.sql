-- بررسی سریع مشتریان از محک

-- 1. تعداد مشتریان
SELECT COUNT(*) as "تعداد مشتریان از محک"
FROM "MahakMappings"
WHERE "EntityType" = 'Person';

-- 2. لیست مشتریان (اگر وجود دارند)
SELECT 
    "MahakEntityId" as "Person ID",
    "MahakEntityCode" as "کد",
    "CreatedAt" as "تاریخ"
FROM "MahakMappings"
WHERE "EntityType" = 'Person'
ORDER BY "CreatedAt" DESC
LIMIT 10;
