# 🧪 MAHAK SYNC - TESTING GUIDE

## 📋 Step-by-Step Instructions

### Step 1: Create Test Order

1. **Open pgAdmin** (or any PostgreSQL client)
2. **Connect to database**: `OnlineShop`
3. **Open file**: `MAHAK_CREATE_TEST_ORDER.sql`
4. **Execute the script** (F5 or click Execute)
5. **Check the output** - you should see:
   ```
   ✅ TEST ORDER CREATED SUCCESSFULLY!
   Order Number: MAHAK20251208065151
   Product: شلوار لويي وي 508
   Price: 17000000 IRR
   Total: 17050000 IRR
   ```

### Step 2: Wait for Sync (1 minute)

The **MahakOutgoingSyncWorker** runs every 1 minute and will:
1. Find your test order
2. Login to Mahak
3. Send the order
4. Mark it as synced

### Step 3: Check Backend Logs

Look for these messages in your backend console:

```
✅ SUCCESS:
[INF] Starting outgoing sync to Mahak...
[INF] Mahak outgoing sync login successful. VisitorId: 41874
[INF] Found 1 orders to sync to Mahak
[INF] Sending order {OrderId} to Mahak
[INF] Order {OrderId} sent to Mahak successfully. Response: {...}
[INF] Outgoing sync completed: 1 success, 0 failed
```

```
❌ IF YOU SEE ERRORS:
[ERR] Failed to sync order {OrderId} to Mahak
[ERR] Error: {error message}
```

### Step 4: Verify in Database

Run the verification script:

1. **Open file**: `MAHAK_VERIFY_SYNC.sql`
2. **Execute it**
3. **Check results**:
   - Orders waiting to sync: Should be 0
   - Orders synced: Should be 1
   - MahakSyncedAt: Should have timestamp

### Step 5: Verify in Mahak System

1. Login to Mahak accounting system
2. Go to Orders/Invoices section
3. Look for order with:
   - Order number starting with "MAHAK"
   - Product: شلوار لويي وي 508
   - Amount: 17,050,000 IRR

---

## 🎯 Expected Timeline

| Time | Event |
|------|-------|
| 00:00 | You execute SQL script |
| 00:01 | Order created in database |
| 00:30 | Outgoing sync worker wakes up |
| 01:00 | Order sent to Mahak ✅ |
| 01:01 | Database updated (SyncedToMahak = true) |

---

## 📊 What to Check

### In Database (Before Sync):
```sql
SyncedToMahak: false
MahakSyncedAt: NULL
MahakOrderId: NULL
OrderStatus: Completed
```

### In Database (After Sync):
```sql
SyncedToMahak: true
MahakSyncedAt: 2025-12-08 06:52:00
MahakOrderId: 123456789
OrderStatus: Completed
```

### In Backend Logs:
```
[INF] Order sent to Mahak successfully
```

### In Mahak System:
- New order appears
- Inventory updated
- Invoice created

---

## 🔧 Troubleshooting

### Problem: No order created
**Solution**: Check if you have users in AspNetUsers table
```sql
SELECT COUNT(*) FROM "AspNetUsers";
```

### Problem: Order created but not syncing
**Check**:
1. OrderStatus must be "Completed"
2. SyncedToMahak must be false
3. Backend must be running
4. Check backend logs for errors

### Problem: Sync fails with error
**Common causes**:
1. Mahak login failed - check credentials
2. Product not found in Mahak - check MahakMapping
3. Network issue - check internet connection

---

## 📁 Files Created

1. **MAHAK_CREATE_TEST_ORDER.sql** - Creates test order
2. **MAHAK_VERIFY_SYNC.sql** - Verifies sync status
3. **This file** - Testing guide

---

## ✅ Success Criteria

- [x] Backend running
- [x] Products synced from Mahak (8 products)
- [ ] Test order created
- [ ] Order synced to Mahak within 1 minute
- [ ] Database updated with sync status
- [ ] Order appears in Mahak system

---

## 🎉 Next Steps After Success

Once the test order syncs successfully:

1. **Test with real orders** from your website
2. **Monitor sync logs** for any issues
3. **Verify inventory** updates in Mahak
4. **Test edge cases** (multiple items, discounts, etc.)
5. **Deploy to production** 🚀

---

**Ready to test? Run MAHAK_CREATE_TEST_ORDER.sql now!** 🎯
