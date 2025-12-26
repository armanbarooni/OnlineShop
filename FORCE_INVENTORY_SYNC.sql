-- Force inventory sync by resetting version
DELETE FROM "MahakSyncLogs" 
WHERE "EntityType" = 'ProductDetailStoreAsset';

-- بعد از اجرای این query، backend را restart کنید تا موجودی دوباره sync شود
