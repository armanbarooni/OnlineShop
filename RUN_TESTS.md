# How to Run Tests

## Quick Commands

### Run All Tests
```powershell
dotnet test --verbosity minimal --nologo
```

### Run Only Integration Tests
```powershell
dotnet test tests/OnlineShop.IntegrationTests/OnlineShop.IntegrationTests.csproj --verbosity minimal
```

### Run Only Application Tests
```powershell
dotnet test tests/OnlineShop.Application.Tests/OnlineShop.Application.Tests.csproj --verbosity minimal
```

### Run Specific Test Class
```powershell
# Coupon tests
dotnet test --filter "FullyQualifiedName~CouponTests"

# Product Inventory tests
dotnet test --filter "FullyQualifiedName~ProductInventoryTests"

# Complete Shopping Journey tests
dotnet test --filter "FullyQualifiedName~CompleteShoppingJourneyTests"
```

### Get Summary Only
```powershell
dotnet test --verbosity quiet --nologo | Select-String "Passed!|Failed!|Test summary"
```

---

## Expected Output

You should see something like:

```
Passed!  - Failed:     0, Passed:   205, Skipped:     0, Total:   205 - OnlineShop.Application.Tests.dll
Passed!  - Failed:    10, Passed:   150, Skipped:     2, Total:   162 - OnlineShop.IntegrationTests.dll

Test summary: total: 367, failed: 10, succeeded: 355, skipped: 2
```

**Target:** 95%+ success rate (355+/365 tests passing)

---

## If Tests Fail

1. **Check the error messages** - they will show which endpoint or validation is failing

2. **Run specific failing test** to see detailed error:
   ```powershell
   dotnet test --filter "FullyQualifiedName~TestName" --verbosity normal
   ```

3. **Check authentication** - if you see many 401 errors, authentication setup might need review

4. **Check console output** - `AuthHelper` logs helpful information about token retrieval

---

## Notes

- Tests use in-memory database (fresh for each test run)
- Admin user is auto-created in tests with credentials:
  - Email: `admin@test.com`
  - Password: `AdminPassword123!`
- OTP codes are captured by `TestSmsService` and available to tests
- Some tests may be skipped intentionally




