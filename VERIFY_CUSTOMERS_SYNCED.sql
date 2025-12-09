-- ✅ تأیید مشتریان دریافتی از محک

-- 1. تعداد کل مشتریان
SELECT 
    '✅ مشتریان از محک دریافت شدند!' as "وضعیت",
    COUNT(*) as "تعداد"
FROM "MahakMappings"
WHERE "EntityType" = 'Person';

-- 2. جزئیات مشتریان
SELECT 
    "MahakEntityId" as "Person ID در محک",
    "MahakEntityCode" as "کد شخص",
    "LocalEntityId" as "Placeholder GUID",
    "CreatedAt" as "تاریخ دریافت"
FROM "MahakMappings"
WHERE "EntityType" = 'Person'
ORDER BY "CreatedAt" DESC;

-- 3. آخرین sync log
SELECT 
    "EntityType",
    "SyncStatus",
    "RecordsProcessed",
    "RecordsSuccessful",
    "SyncCompletedAt"
FROM "MahakSyncLogs"
WHERE "EntityType" = 'Person'
ORDER BY "SyncCompletedAt" DESC
LIMIT 5;
