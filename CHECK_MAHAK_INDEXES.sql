-- بررسی indexهای MahakMappings
SELECT 
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = 'MahakMappings';
