<!-- 5eadbe51-9f47-4c9b-a7d5-93bc9da14b02 50bc9fb2-4b18-4694-b563-dfd9ccb98325 -->
# Phase 1 & 2: Validation, Business Logic & Testing

## Overview

Complete all validators, implement core business logic, write comprehensive unit tests, and add integration tests. Mahak sync (Phase 3) will be deferred until after UI/UX completion.

---

## PHASE 1: Validators & Core Business Logic

### Step 1.1: Complete All Missing Validators (Priority: HIGH)

**Missing Update Validators to create:**

- `UpdateProductDetailDtoValidator.cs`
- `UpdateProductImageDtoValidator.cs`
- `UpdateProductInventoryDtoValidator.cs`
- `UpdateProductReviewDtoValidator.cs`
- `UpdateSavedCartDtoValidator.cs`
- `UpdateUserAddressDtoValidator.cs`
- `UpdateUserOrderDtoValidator.cs`
- `UpdateUserPaymentDtoValidator.cs`
- `UpdateUserProfileDtoValidator.cs`
- `UpdateUserReturnRequestDtoValidator.cs`
- `UpdateWishlistDtoValidator.cs`
- `UpdateCartDtoValidator.cs`

**Missing Specialized Validators:**

- `ApproveProductReviewDtoValidator.cs` (for approval)
- `RejectProductReviewDtoValidator.cs` (for rejection)
- `ApproveUserReturnRequestDtoValidator.cs`
- `RejectUserReturnRequestDtoValidator.cs`
- `UpdateCartItemDtoValidator.cs`

**Location:** `src/Application/Validators/{EntityName}/`

**Validation Rules to implement:**

- Required fields (NotEmpty, NotNull)
- String length limits (MaximumLength)
- Numeric ranges (GreaterThan, GreaterThanOrEqualTo, LessThan)
- Email format (EmailAddress)
- Phone format (Matches with regex)
- Business rules (custom validators)

### Step 1.2: Implement Search & Filter Features

**Create new Query Handlers:**

1. **ProductSearchQuery** (`src/Application/Features/Product/Queries/Search/`)

      - Search by: Name, Description, SKU, Barcode
      - Filter by: CategoryId, PriceRange, IsActive, IsFeatured
      - Sort by: Name, Price, ViewCount, CreatedAt
      - Pagination support

2. **ProductCategorySearchQuery** (with hierarchical support)

3. **UserOrderSearchQuery** (for user dashboard)

      - Filter by: Status, DateRange, TotalAmount
      - Sort by: OrderDate, TotalAmount

**Files to create:**

```
src/Application/Features/Product/Queries/Search/
 - ProductSearchQuery.cs
 - ProductSearchQueryHandler.cs
  
src/Application/DTOs/Product/
 - ProductSearchDto.cs (criteria)
 - PagedResultDto.cs (generic paging result)
```

### Step 1.3: Implement Checkout Business Logic

**Create CheckoutUseCase** (`src/Application/Features/Checkout/`)

**Workflow:**

1. Validate cart items exist and have stock
2. Calculate totals (subtotal, tax, shipping, discount)
3. Create UserOrder + UserOrderItems
4. Reserve inventory (update ProductInventory)
5. Clear cart after successful order
6. Return order summary

**Files to create:**

```
src/Application/Features/Checkout/
 - Commands/
  - ProcessCheckout/
   - ProcessCheckoutCommand.cs
   - ProcessCheckoutCommandHandler.cs
   - ProcessCheckoutCommandValidator.cs
  
src/Application/DTOs/Checkout/
 - CheckoutRequestDto.cs
 - CheckoutResultDto.cs
 - OrderSummaryDto.cs
```

**Business Rules:**

- Check inventory before creating order
- Lock inventory during checkout (prevent overselling)
- Validate user addresses exist
- Calculate tax and shipping (configurable rules)

### Step 1.4: Implement Inventory Management Logic

**Add methods to ProductInventory:**

- `ReserveQuantity(int quantity)` - Reserve stock during checkout
- `ReleaseReservedQuantity(int quantity)` - Cancel reservation
- `CommitSale(int quantity)` - Move from reserved to sold
- `AddStock(int quantity)` - Increase available stock
- `GetAvailableStock()` - Calculate: Available - Reserved

**Files to modify:**

```
src/Domain/Entities/ProductInventory.cs
```

**Create InventoryService** (`src/Application/Services/`)

- Check stock availability
- Reserve stock for orders
- Release stock on order cancellation
- Update stock from Mahak (placeholder for Phase 3)

### Step 1.5: Enhance Order Management

**Add Order Status Management:**

- Status transitions: Pending → Processing → Shipped → Delivered
- Status: Cancelled, Returned
- Prevent invalid state transitions
- Track status change history (optional entity)

**Add methods to UserOrder entity:**

```csharp
public void StartProcessing(string updatedBy)
public void MarkAsShipped(string trackingNumber, string updatedBy)
public void MarkAsDelivered(string updatedBy)
public void Cancel(string reason, string updatedBy)
```

**Files to modify:**

```
src/Domain/Entities/UserOrder.cs
```

**Create Order Management Commands:**

```
src/Application/Features/UserOrder/Commands/
 - UpdateStatus/
  - UpdateOrderStatusCommand.cs
  - UpdateOrderStatusCommandHandler.cs
 - CancelOrder/
  - CancelOrderCommand.cs
  - CancelOrderCommandHandler.cs
```

---

## PHASE 2: Comprehensive Testing

### Step 2.1: Unit Tests for All Entities (Domain Tests)

**Create domain logic tests for each entity:**

Priority entities to test:

1. **Product** - pricing, activation, view counting
2. **ProductInventory** - reserve, release, commit stock
3. **Cart/CartItem** - add, update, calculate totals
4. **UserOrder** - status transitions, calculations
5. **UserAddress** - set default logic
6. **ProductReview** - approve/reject workflow
7. **UserReturnRequest** - approve/reject workflow

**Test file pattern:**

```
tests/OnlineShop.Application.Tests/Domain/
 - ProductDomainTests.cs
 - ProductInventoryDomainTests.cs
 - CartDomainTests.cs
 - UserOrderDomainTests.cs
 - UserAddressDomainTests.cs
 - ProductReviewDomainTests.cs
 - UserReturnRequestDomainTests.cs
```

**Test scenarios for each:**

- Create with valid data
- Create with invalid data (should throw)
- Update operations
- Business rule validations
- State transitions

### Step 2.2: Unit Tests for All Command Handlers

**Pattern:** Test each Command Handler with:

- Success scenario (happy path)
- Validation failures
- Not found scenarios
- Business rule violations
- Repository interactions (mocked)

**Entities needing complete command tests:**

- ProductDetail (Create, Update, Delete)
- ProductImage (Create, Update, Delete)
- ProductInventory (Create, Update, Delete)
- ProductReview (Create, Update, Delete, Approve, Reject)
- Cart (Create, Update, Delete, AddItem, RemoveItem, UpdateItem)
- SavedCart (Create, Update, Delete)
- Wishlist (Create, Update, Delete)
- UserAddress (Create, Update, Delete, SetDefault)
- UserOrder (Create, Update, Delete, UpdateStatus, Cancel)
- UserPayment (Create, Update, Delete)
- UserProfile (Create, Update, Delete)
- UserReturnRequest (Create, Update, Delete, Approve, Reject)

**Test file pattern:**

```
tests/OnlineShop.Application.Tests/Features/{Entity}/Commands/{Action}/
 - {Action}{Entity}CommandHandlerTests.cs
```

### Step 2.3: Unit Tests for All Query Handlers

**Test all queries with:**

- Empty results
- Single result
- Multiple results
- Pagination
- Filtering/searching
- Not found scenarios

**Entities needing query tests:**

- All entities with GetAll, GetById
- Specialized queries (GetByUserId, GetByProductId, etc.)
- Search queries (ProductSearch, etc.)

### Step 2.4: Unit Tests for All Validators

**Test each validator:**

- Valid data passes
- Required fields validation
- Length validations
- Range validations
- Format validations (email, phone)
- Custom business rules

**Test file pattern:**

```
tests/OnlineShop.Application.Tests/Validators/{Entity}/
 - Create{Entity}DtoValidatorTests.cs
 - Update{Entity}DtoValidatorTests.cs
```

### Step 2.5: Integration Tests

**Setup Integration Test Project:**

```
tests/OnlineShop.Integration.Tests/
 - WebApplicationFactory setup
 - In-memory database or test database
 - Test data seeders
```

**Integration test scenarios:**

1. **Authentication Flow:**

      - Register → Login → Access protected endpoint → Refresh token

2. **Product Catalog Flow:**

      - Create category → Create product → Add images → Add details → Get product

3. **Shopping Flow:**

      - Search products → Add to cart → Update cart → Checkout → Create order

4. **User Profile Flow:**

      - Register → Create profile → Add address → Set default address

5. **Review Flow:**

      - Create review → Admin approve → Review visible

6. **Return Request Flow:**

      - Create order → Create return request → Admin approve/reject

**Test files:**

```
tests/OnlineShop.Integration.Tests/
 - AuthenticationFlowTests.cs
 - ProductCatalogFlowTests.cs
 - ShoppingFlowTests.cs
 - UserProfileFlowTests.cs
 - ReviewWorkflowTests.cs
 - ReturnRequestWorkflowTests.cs
```

### Step 2.6: Test Coverage & Quality Assurance

**Goals:**

- Minimum 80% code coverage for business logic
- All validators have tests
- All command/query handlers have tests
- All domain entities have tests
- Integration tests cover main workflows

**Run and verify:**

```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Review and fix:**

- All tests passing
- No flaky tests
- Code coverage reports
- Fix any bugs found during testing

---

## PHASE 1-2 VALIDATION CHECKLIST

Before moving to Phase 3 (Mahak Sync) or Phase 4 (UI/UX):

### Validators

- [ ] All Create validators exist and tested
- [ ] All Update validators exist and tested
- [ ] All specialized validators (Approve/Reject) exist and tested
- [ ] Validation rules comprehensive and correct

### Business Logic

- [ ] Search & Filter queries implemented
- [ ] Checkout process complete and tested
- [ ] Inventory management logic working
- [ ] Order status management complete
- [ ] All business rules enforced

### Tests

- [ ] All domain entities have unit tests
- [ ] All command handlers have unit tests
- [ ] All query handlers have unit tests
- [ ] All validators have unit tests
- [ ] Integration tests cover main workflows
- [ ] Code coverage ≥ 80%
- [ ] All tests passing consistently

### Code Quality

- [ ] No compiler warnings
- [ ] Code follows clean architecture
- [ ] DRY principle followed
- [ ] SOLID principles followed
- [ ] Error handling comprehensive

### Documentation

- [ ] API endpoints documented (Swagger)
- [ ] Business rules documented
- [ ] Test scenarios documented

---

## NEXT STEPS AFTER PHASE 1-2

Once Phase 1-2 is complete and validated:

**Option A: Payment Gateway Integration (Recommended next)**

- Integrate Zarinpal or other gateway
- Test payment flow end-to-end
- Keep Mahak sync for later

**Option B: UI/UX Development**

- Build frontend
- Test with mock data
- Integration with backend APIs

**Option C: Mahak Sync (Phase 3)**

- Only after Options A & B are stable
- Implement as additional feature
- Test in isolation

---

## ESTIMATION

**Phase 1 (Validators & Business Logic):** 3-5 days

- Validators: 1 day
- Search/Filter: 1 day  
- Checkout Logic: 1-2 days
- Inventory & Order Management: 1 day

**Phase 2 (Testing):** 5-7 days

- Domain Tests: 1 day
- Command Handler Tests: 2 days
- Query Handler Tests: 1 day
- Validator Tests: 1 day
- Integration Tests: 2-3 days

**Total:** 8-12 days for complete Phase 1-2

### To-dos

- [ ] Create all missing UserReturnRequest features (Commands & Queries) and mapping profile
- [ ] Complete ProductReview CRUD - Add Delete command, GetAll and GetById queries, missing DTOs
- [ ] Complete Wishlist CRUD - Add Update command, GetAll and GetById queries, missing DTOs
- [ ] Complete UserAddress CRUD - Add Delete command, GetAll and GetById queries, missing DTOs
- [ ] Complete UserProfile CRUD - Add Delete command, GetAll and GetById queries
- [ ] Complete Cart CRUD - Add Update and Delete commands, GetAll and GetById queries
- [ ] Create complete CRUD for MahakMapping entity (DTOs, Features, Controller, Mapping)
- [ ] Create complete CRUD for MahakQueue entity (DTOs, Features, Controller, Mapping)
- [ ] Create complete CRUD for MahakSyncLog entity (DTOs, Features, Controller, Mapping)
- [ ] Create complete CRUD for SyncErrorLog entity (DTOs, Features, Controller, Mapping)
- [ ] Verify all implementations, check for linter errors, ensure all mappings are registered