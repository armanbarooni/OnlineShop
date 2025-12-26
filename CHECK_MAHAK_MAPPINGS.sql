-- Check MahakMappings structure
SELECT 
    "EntityType",
    COUNT(*) as "Count"
FROM "MahakMappings"
GROUP BY "EntityType"
ORDER BY "EntityType";

-- Show sample mappings
SELECT 
    "EntityType",
    "LocalEntityId",
    "MahakEntityId"
FROM "MahakMappings"
LIMIT 10;
