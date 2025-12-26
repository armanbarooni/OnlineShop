-- ============================================
-- Script to assign existing products to categories
-- ============================================

-- Step 1: View all available categories
SELECT "Id", "Name", "Description" 
FROM "ProductCategories" 
WHERE "Deleted" = false 
ORDER BY "Name";

-- Step 2: View all products without category
SELECT "Id", "Name", "Price", "StockQuantity", "CategoryId"
FROM "Products" 
WHERE "CategoryId" IS NULL AND "Deleted" = false
ORDER BY "Name";

-- Step 3: View all products with their current category (if any)
SELECT 
    p."Id" as "ProductId",
    p."Name" as "ProductName",
    p."CategoryId",
    pc."Name" as "CategoryName"
FROM "Products" p
LEFT JOIN "ProductCategories" pc ON p."CategoryId" = pc."Id"
WHERE p."Deleted" = false
ORDER BY p."Name";

-- ============================================
-- OPTION 1: Assign ALL products to ONE category
-- ============================================
-- Replace 'CATEGORY_ID_HERE' with actual category ID from Step 1
/*
UPDATE "Products" 
SET "CategoryId" = 'CATEGORY_ID_HERE',
    "UpdatedAt" = NOW()
WHERE "CategoryId" IS NULL 
  AND "Deleted" = false;
*/

-- ============================================
-- OPTION 2: Assign products evenly to categories
-- ============================================
-- This will distribute products evenly across all categories
/*
WITH CategoryList AS (
    SELECT "Id", ROW_NUMBER() OVER (ORDER BY "Name") as rn
    FROM "ProductCategories"
    WHERE "Deleted" = false
),
ProductList AS (
    SELECT "Id", ROW_NUMBER() OVER (ORDER BY "Name") as rn
    FROM "Products"
    WHERE "CategoryId" IS NULL AND "Deleted" = false
),
CategoryCount AS (
    SELECT COUNT(*) as cnt FROM CategoryList
)
UPDATE "Products" p
SET "CategoryId" = (
    SELECT cl."Id"
    FROM CategoryList cl
    WHERE cl.rn = ((pl.rn - 1) % (SELECT cnt FROM CategoryCount)) + 1
),
"UpdatedAt" = NOW()
FROM ProductList pl
WHERE p."Id" = pl."Id";
*/

-- ============================================
-- OPTION 3: Assign specific products to specific categories
-- ============================================
-- Replace the IDs below with actual Product and Category IDs
/*
-- Example 1: Assign product to category
UPDATE "Products" 
SET "CategoryId" = '2efcba3b-0ca7-4f5c-ad89-1e9f58a86d26',  -- Category ID
    "UpdatedAt" = NOW()
WHERE "Id" = 'a3c7a7c1-85ae-42be-ada3-2b363cd097d5';  -- Product ID

-- Example 2: Assign multiple products to same category
UPDATE "Products" 
SET "CategoryId" = '2efcba3b-0ca7-4f5c-ad89-1e9f58a86d26',  -- Category ID
    "UpdatedAt" = NOW()
WHERE "Id" IN (
    'a3c7a7c1-85ae-42be-ada3-2b363cd097d5',
    '74c42be2-f8ac-40bf-ae12-63a3101cd4f5',
    '249cee39-5696-456a-ae72-b9765df1eb29'
);
*/

-- ============================================
-- OPTION 4: Assign products based on name pattern
-- ============================================
-- Example: Assign products containing "تیشرت" to a specific category
/*
UPDATE "Products" 
SET "CategoryId" = '2efcba3b-0ca7-4f5c-ad89-1e9f58a86d26',  -- Category ID
    "UpdatedAt" = NOW()
WHERE "Name" LIKE '%تیشرت%'
  AND "CategoryId" IS NULL
  AND "Deleted" = false;
*/

-- ============================================
-- VERIFICATION: Check results after update
-- ============================================
-- Run this after updating to verify the changes
/*
SELECT 
    pc."Name" as "CategoryName",
    COUNT(p."Id") as "ProductCount"
FROM "ProductCategories" pc
LEFT JOIN "Products" p ON pc."Id" = p."CategoryId" AND p."Deleted" = false
WHERE pc."Deleted" = false
GROUP BY pc."Id", pc."Name"
ORDER BY pc."Name";

-- Check if any products are still without category
SELECT COUNT(*) as "UncategorizedProducts"
FROM "Products"
WHERE "CategoryId" IS NULL AND "Deleted" = false;
*/


