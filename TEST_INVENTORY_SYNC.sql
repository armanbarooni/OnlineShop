-- Force inventory resync by deleting sync log
DELETE FROM "MahakSyncLogs" WHERE "EntityType" = 'ProductDetailStoreAsset';

-- Check how many ProductDetail mappings we have now
SELECT 
    "EntityType",
    COUNT(*) as "تعداد Mappings"
FROM "MahakMappings"
WHERE "EntityType" IN ('Product', 'ProductDetail')
GROUP BY "EntityType";
