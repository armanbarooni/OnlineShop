-- ============================================
-- بررسی عکس‌های محصولات
-- ============================================

-- تعداد کل عکس‌ها
SELECT COUNT(*) as "تعداد کل عکس‌ها"
FROM "ProductImages"
WHERE "Deleted" = false;

-- عکس‌های محصولات
SELECT 
    p."Name" as "نام محصول",
    pi."ImageUrl" as "آدرس عکس",
    pi."IsPrimary" as "عکس اصلی؟",
    pi."DisplayOrder" as "ترتیب نمایش",
    pi."CreatedAt" as "تاریخ ایجاد"
FROM "ProductImages" pi
INNER JOIN "Products" p ON pi."ProductId" = p."Id"
WHERE pi."Deleted" = false
ORDER BY p."Name", pi."DisplayOrder"
LIMIT 20;

-- محصولاتی که عکس دارند
SELECT 
    p."Name" as "نام محصول",
    p."MahakId",
    COUNT(pi."Id") as "تعداد عکس‌ها"
FROM "Products" p
LEFT JOIN "ProductImages" pi ON p."Id" = pi."ProductId" AND pi."Deleted" = false
WHERE p."Deleted" = false
GROUP BY p."Id", p."Name", p."MahakId"
HAVING COUNT(pi."Id") > 0
ORDER BY COUNT(pi."Id") DESC
LIMIT 10;

-- محصولاتی که عکس ندارند (از محک)
SELECT 
    p."Name" as "نام محصول",
    p."MahakId",
    p."Price" as "قیمت"
FROM "Products" p
LEFT JOIN "ProductImages" pi ON p."Id" = pi."ProductId" AND pi."Deleted" = false
WHERE p."Deleted" = false 
  AND p."MahakId" IS NOT NULL
  AND pi."Id" IS NULL
ORDER BY p."CreatedAt" DESC
LIMIT 10;
