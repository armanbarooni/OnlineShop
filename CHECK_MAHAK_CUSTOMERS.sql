-- ============================================
-- بررسی مشتریان دریافتی از محک
-- ============================================

-- 1. تعداد کل مشتریان در MahakMapping
SELECT 
    'تعداد مشتریان از محک' as "نوع",
    COUNT(*) as "تعداد"
FROM "MahakMappings"
WHERE "EntityType" = 'Person';

-- 2. لیست مشتریان دریافتی از محک
SELECT 
    "MahakEntityId" as "Person ID در محک",
    "MahakEntityCode" as "کد شخص",
    "LocalEntityId" as "User ID در سایت",
    "CreatedAt" as "تاریخ دریافت"
FROM "MahakMappings"
WHERE "EntityType" = 'Person'
ORDER BY "CreatedAt" DESC
LIMIT 20;

-- 3. آخرین RowVersion برای Person
SELECT 
    "EntityType",
    MAX("MahakRowVersion") as "آخرین RowVersion",
    COUNT(*) as "تعداد رکوردها"
FROM "MahakSyncLogs"
WHERE "EntityType" = 'Person'
GROUP BY "EntityType";

-- 4. لاگ‌های sync مشتریان
SELECT 
    "SyncType",
    "Status",
    "RecordsProcessed",
    "SuccessCount",
    "FailureCount",
    "StartedAt",
    "CompletedAt"
FROM "MahakSyncLogs"
WHERE "EntityType" = 'Person'
ORDER BY "StartedAt" DESC
LIMIT 10;

-- 5. مشتریانی که هم در محک هستند هم در سایت ثبت‌نام کرده‌اند
SELECT 
    u."Email",
    u."FirstName",
    u."LastName",
    u."PhoneNumber",
    u."MahakPersonId",
    m."MahakEntityCode" as "کد در محک",
    m."CreatedAt" as "تاریخ sync"
FROM "AspNetUsers" u
INNER JOIN "MahakMappings" m ON u."MahakPersonId" = m."MahakEntityId"
WHERE m."EntityType" = 'Person'
ORDER BY m."CreatedAt" DESC;
