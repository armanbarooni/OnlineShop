-- Mahak Test Order Creation Script
-- This will create a test order that will sync to Mahak

USE OnlineShopDb;
GO

-- Step 1: Get a product from Mahak and a user
DECLARE @ProductId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE MahakId IS NOT NULL ORDER BY CreatedAt DESC);
DECLARE @ProductName NVARCHAR(500) = (SELECT TOP 1 Name FROM Products WHERE Id = @ProductId);
DECLARE @ProductPrice DECIMAL(18,2) = (SELECT TOP 1 Price FROM Products WHERE Id = @ProductId);
DECLARE @UserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM AspNetUsers);
DECLARE @OrderId UNIQUEIDENTIFIER = NEWID();
DECLARE @OrderNumber NVARCHAR(50) = 'MAHAK' + FORMAT(GETDATE(), 'yyyyMMddHHmmss');

-- Step 2: Create the order
INSERT INTO UserOrders (
    Id, 
    UserId, 
    OrderNumber, 
    OrderStatus, 
    SubTotal, 
    TaxAmount, 
    ShippingAmount, 
    DiscountAmount, 
    TotalAmount,
    Currency,
    SyncedToMahak,
    CreatedAt, 
    UpdatedAt, 
    Deleted
)
VALUES (
    @OrderId, 
    @UserId, 
    @OrderNumber, 
    'Completed',  -- Must be 'Completed' to sync to Mahak
    @ProductPrice, 
    0, 
    50000,  -- 50,000 IRR shipping
    0, 
    @ProductPrice + 50000,
    'IRR',
    0,  -- Not synced yet
    GETDATE(), 
    GETDATE(), 
    0
);

-- Step 3: Add order item
INSERT INTO UserOrderItems (
    Id, 
    UserOrderId, 
    ProductId, 
    ProductName, 
    Quantity, 
    UnitPrice, 
    TotalPrice,
    CreatedAt, 
    UpdatedAt, 
    Deleted
)
VALUES (
    NEWID(), 
    @OrderId, 
    @ProductId, 
    @ProductName, 
    1,  -- Quantity
    @ProductPrice, 
    @ProductPrice,
    GETDATE(), 
    GETDATE(), 
    0
);

-- Step 4: Display results
PRINT '✅ Test order created successfully!';
PRINT 'Order Number: ' + @OrderNumber;
PRINT 'Product: ' + @ProductName;
PRINT 'Price: ' + CAST(@ProductPrice AS NVARCHAR(50)) + ' IRR';
PRINT 'Total: ' + CAST(@ProductPrice + 50000 AS NVARCHAR(50)) + ' IRR';
PRINT '';
PRINT '⏳ Waiting for outgoing sync (runs every 1 minute)...';
PRINT 'Check backend logs for: "Order sent to Mahak successfully"';

-- Verify the order
SELECT 
    o.OrderNumber,
    o.OrderStatus,
    o.TotalAmount,
    o.SyncedToMahak,
    o.MahakSyncedAt,
    o.CreatedAt,
    i.ProductName,
    i.Quantity,
    i.UnitPrice
FROM UserOrders o
INNER JOIN UserOrderItems i ON o.Id = i.UserOrderId
WHERE o.Id = @OrderId;

GO
