-- ============================================
-- بررسی نتیجه فاکتور فروش
-- ============================================

-- بررسی بعد از sync (1-2 دقیقه بعد از اجرای NEW_CUSTOMER_SALE.sql)
SELECT 
    '✅ بعد از Sync' as "وضعیت",
    o."OrderNumber" as "شماره فاکتور",
    u."FirstName" || ' ' || u."LastName" as "مشتری",
    u."PhoneNumber" as "شماره تماس",
    u."MahakPersonClientId" as "Person ID در محک",
    u."MahakSyncedAt" as "تاریخ Sync مشتری",
    o."MahakOrderId" as "Order ID در محک",
    o."MahakSyncedAt" as "تاریخ Sync فاکتور",
    o."TotalAmount" as "مبلغ کل"
FROM "UserOrders" o
INNER JOIN "AspNetUsers" u ON o."UserId" = u."Id"
WHERE o."OrderNumber" LIKE 'NEW_SALE_%'
ORDER BY o."CreatedAt" DESC
LIMIT 1;

-- جزئیات آیتم‌های فاکتور
SELECT 
    oi."ProductName" as "محصول",
    oi."Quantity" as "تعداد",
    oi."UnitPrice" as "قیمت واحد",
    oi."TotalPrice" as "مبلغ کل"
FROM "UserOrderItems" oi
INNER JOIN "UserOrders" o ON oi."OrderId" = o."Id"
WHERE o."OrderNumber" LIKE 'NEW_SALE_%'
ORDER BY o."CreatedAt" DESC;

-- انتظار:
-- ✅ MahakPersonClientId باید مقدار داشته باشد (مثلاً: 1234567890)
-- ✅ MahakSyncedAt باید تاریخ داشته باشد
-- ✅ MahakOrderId باید مقدار داشته باشد
-- ✅ در محک باید مشتری جدید و فاکتور با لینک به مشتری وجود داشته باشد
