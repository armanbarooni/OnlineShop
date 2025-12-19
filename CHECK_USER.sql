-- Check if user exists with phone number
SELECT "Id", "UserName", "Email", "PhoneNumber", "FirstName", "LastName", "EmailConfirmed", "PhoneNumberConfirmed"
FROM "AspNetUsers" 
WHERE "PhoneNumber" = '09168044154';

-- If user exists, show all details
SELECT *
FROM "AspNetUsers" 
WHERE "PhoneNumber" = '09168044154';
