# Mahak Data Sync - Implementation Guide

## How Data Flows from Mahak to Database

### Step-by-Step Process:

```
1. Login to Mahak → Get Token
2. Call GetAllDataV2 → Get Products, Images, Inventory
3. For Each Product:
   - Check if exists (by MahakId or MahakMapping)
   - If exists → UPDATE
   - If not exists → CREATE
   - Save MahakMapping
4. Update RowVersion tracking
```

## Example: Syncing a Product

### Mahak Product Data (from API):
```json
{
  "productId": 12345,
  "productClientId": 67890,
  "productCode": 100,
  "productCategoryId": 5,
  "name": "تی شرت مردانه",
  "unitName": "عدد",
  "tags": "پوشاک,مردانه",
  "description": "تی شرت کتان با کیفیت عالی",
  "rowVersion": 1234567890,
  "deleted": false
}
```

### Mapping to OnlineShop Database:

```csharp
// 1. Check if product exists
var existingProduct = await _productRepository
    .GetByMahakIdAsync(mahakProduct.ProductId);

if (existingProduct != null)
{
    // UPDATE existing product
    existingProduct.Update(
        name: mahakProduct.Name,
        description: mahakProduct.Description ?? "",
        price: 0, // Get from ProductDetail
        qty: 0,   // Get from ProductDetailStoreAsset
        updatedBy: "MahakSync"
    );
}
else
{
    // CREATE new product
    var newProduct = Product.Create(
        name: mahakProduct.Name,
        description: mahakProduct.Description ?? "",
        price: 0,
        stockQuantity: 0,
        mahakClientId: mahakProduct.ProductClientId,
        mahakId: mahakProduct.ProductId
    );
    
    await _productRepository.AddAsync(newProduct);
    
    // Create mapping
    var mapping = MahakMapping.Create(
        entityType: "Product",
        localEntityId: newProduct.Id,
        mahakEntityId: mahakProduct.ProductId,
        mahakEntityCode: mahakProduct.ProductCode.ToString()
    );
    
    await _mahakMappingRepository.AddAsync(mapping);
}
```

## Database Tables Involved

### 1. Products Table
- Stores actual product data
- Has `MahakId` and `MahakClientId` columns

### 2. MahakMapping Table
- Links OnlineShop entities to Mahak entities
- Tracks sync status

### 3. MahakSyncLog Table
- Logs each sync operation
- Tracks RowVersion for incremental sync

## What Needs to be Implemented

### ✅ Already Exists:
- Login to Mahak
- GetAllData API call
- Database entities (Product, MahakMapping, MahakSyncLog)
- Repositories

### ❌ Needs Implementation:

1. **Product Sync Logic** (ProcessProductsAsync)
2. **ProductDetail Sync** (prices, variants)
3. **Inventory Sync** (stock quantities)
4. **Image Sync** (download and store images)
5. **Category Sync** (map Mahak categories)

## Next Steps

### Option 1: Simple Test (Recommended)
Just log the data to see what Mahak sends:
```csharp
_logger.LogInformation("Received {Count} products from Mahak", products.Count);
foreach(var p in products)
{
    _logger.LogInformation("Product: {Name}, ID: {Id}, RowVersion: {Version}", 
        p.Name, p.ProductId, p.RowVersion);
}
```

### Option 2: Full Implementation
Implement complete product sync with database updates.

## Which Option Do You Want?

**A) Simple Test First** - Just see what data comes from Mahak
**B) Full Implementation** - Complete product sync to database

Let me know and I'll implement it!
