# Mahak Synchronization Implementation Plan

## Overview
Implement bidirectional synchronization between OnlineShop and Mahak accounting system using Mahak's REST API.

## Current Status
- ✅ Basic Mahak models exist (`MahakProductModels`, `MahakSyncModels`, `MahakMediaModels`)
- ✅ `MahakSyncService` exists with basic Login and GetAllData
- ✅ Database entities: `MahakMapping`, `MahakQueue`, `MahakSyncLog`
- ❌ RowVersion tracking not fully implemented
- ❌ Outgoing sync (website → Mahak) not implemented
- ❌ Sales synchronization not implemented

## Requirements

### 1. Incoming Sync (Mahak → Website) - Every 5 minutes
**Data to receive:**
- Products (`fromProductVersion`)
- Product Details (`fromProductDetailVersion`)
- Product Categories (`fromProductCategoryVersion`)
- Pictures/Images (`fromPictureVersion`, `fromPhotoGalleryVersion`)
- Inventory (`fromProductDetailStoreAssetVersion`)
- Prices (embedded in ProductDetail)
- Orders from Mahak (`fromOrderVersion`, `fromOrderDetailVersion`)

**Implementation Strategy:**
1. Track last `RowVersion` for each entity type
2. On each sync, send last RowVersion to get only changed data
3. For each incoming record:
   - Check if exists by `MahakId` (e.g., `productId`, `productDetailId`)
   - If exists and `RowVersion` changed → UPDATE
   - If not exists → INSERT
   - Store mapping in `MahakMapping` table

### 2. Outgoing Sync (Website → Mahak) - Every 1 minute
**Data to send:**
- Website orders/sales
- Inventory updates (when products are sold)

**Implementation Strategy:**
1. Add `SyncedToMahak` flag to `Order` entity
2. After successful payment callback → mark order as pending sync
3. Every 1 minute:
   - Find orders where `SyncedToMahak = false` AND `PaymentStatus = Paid`
   - Send to Mahak via appropriate API
   - Mark as `SyncedToMahak = true` on success

### 3. Conflict Resolution
**Scenario:** Same product sold simultaneously on website and in Mahak

**Strategy (TODO - implement later):**
- Detect: When incoming Mahak inventory < website inventory
- Action: Cancel website order
- Requires: Order cancellation API (not yet implemented)
- For now: Add TODO comment and log warning

## Database Changes Needed

### Add to Order entity:
```csharp
public bool SyncedToMahak { get; set; } = false;
public DateTime? MahakSyncedAt { get; set; }
public string? MahakOrderId { get; set; }
```

### Add RowVersion tracking table:
```csharp
public class MahakSyncState
{
    public int Id { get; set; }
    public string EntityType { get; set; } // "Product", "ProductDetail", etc.
    public long LastRowVersion { get; set; }
    public DateTime LastSyncAt { get; set; }
}
```

## API Endpoints to Implement

### Mahak API Calls:
1. **Login** - `/Sync/LoginV2` ✅ (already exists)
2. **Get Data** - `/Sync/GetAllDataV2` ✅ (partially implemented)
3. **Send Order** - Need to find in documentation (TODO: research)

## Worker Service Changes

### Current: `MahakSyncWorker`
- Runs every 5 minutes
- Only does incoming sync

### Needed: Two separate workers or combined logic

**Option A: Single worker with different intervals**
```csharp
- Every 1 minute: Outgoing sync (send orders)
- Every 5 minutes: Incoming sync (receive data)
```

**Option B: Two workers**
```csharp
- MahakIncomingSyncWorker: Every 5 minutes
- MahakOutgoingSyncWorker: Every 1 minute
```

## Implementation Steps

### Phase 1: Enhance Incoming Sync
1. ✅ Review existing `MahakSyncService.SyncAsync()`
2. ⬜ Add `MahakSyncState` entity and repository
3. ⬜ Implement RowVersion tracking
4. ⬜ Enhance product sync to handle:
   - Product categories
   - Product images
   - Inventory (ProductDetailStoreAsset)
5. ⬜ Add proper error handling and logging
6. ⬜ Test with real Mahak credentials

### Phase 2: Implement Outgoing Sync
1. ⬜ Add migration for Order entity changes
2. ⬜ Research Mahak API for sending orders
3. ⬜ Implement `SendOrderToMahakAsync()` method
4. ⬜ Create outgoing sync worker (1-minute interval)
5. ⬜ Hook into payment callback to mark orders for sync
6. ⬜ Test order synchronization

### Phase 3: Conflict Resolution (Future)
1. ⬜ Implement order cancellation API
2. ⬜ Add conflict detection logic
3. ⬜ Implement automatic cancellation strategy
4. ⬜ Add admin notifications for conflicts

## Configuration Needed

### appsettings.json:
```json
{
  "Mahak": {
    "ApiUrl": "https://mahakacc.mahaksoft.com/API/v3",
    "UserName": "...",
    "Password": "...",
    "DatabaseId": 0,
    "PackageNo": "...",
    "IncomingSyncIntervalMinutes": 5,
    "OutgoingSyncIntervalMinutes": 1
  }
}
```

## Testing Strategy
1. Unit tests for sync logic
2. Integration tests with Mahak sandbox (if available)
3. Manual testing with real credentials
4. Monitor sync logs for errors

## Risks & Considerations
1. **Network failures**: Implement retry logic
2. **Data integrity**: Use transactions for database updates
3. **Performance**: Batch processing for large datasets
4. **Mahak API limits**: Respect rate limits, implement backoff
5. **Concurrent sales**: Low probability but needs handling

## Next Steps
1. Review this plan with stakeholder
2. Get Mahak credentials for testing
3. Start with Phase 1: Enhance incoming sync
4. Implement incrementally with testing at each step

## Questions to Answer
1. What Mahak API endpoint sends orders? (need to search documentation)
2. Does Mahak have a sandbox environment?
3. What is the exact conflict resolution strategy preferred?
4. Should we sync ALL orders or only certain statuses?
