-- Script to assign products to categories
-- First, let's see what categories exist
SELECT "Id", "Name" FROM "ProductCategories" WHERE "Deleted" = false;

-- Example: Assign all products to a specific category
-- Replace 'CATEGORY_ID_HERE' with actual category ID
-- UPDATE "Products" 
-- SET "CategoryId" = 'CATEGORY_ID_HERE' 
-- WHERE "CategoryId" IS NULL;

-- Example: Assign specific products to specific categories
-- Product 1 -> Category 1
-- UPDATE "Products" 
-- SET "CategoryId" = '2efcba3b-0ca7-4f5c-ad89-1e9f58a86d26' 
-- WHERE "Id" = 'a3c7a7c1-85ae-42be-ada3-2b363cd097d5';

-- Product 2 -> Category 2
-- UPDATE "Products" 
-- SET "CategoryId" = 'ca9f5f52-61f2-4cc8-8848-bc748f95b537' 
-- WHERE "Id" = '74c42be2-f8ac-40bf-ae12-63a3101cd4f5';

-- Product 3 -> Category 3
-- UPDATE "Products" 
-- SET "CategoryId" = '6ac6cec8-80f5-455e-8248-52ab5e6188df' 
-- WHERE "Id" = '249cee39-5696-456a-ae72-b9765df1eb29';

-- Assign remaining products to first category
-- UPDATE "Products" 
-- SET "CategoryId" = '2efcba3b-0ca7-4f5c-ad89-1e9f58a86d26' 
-- WHERE "CategoryId" IS NULL;

