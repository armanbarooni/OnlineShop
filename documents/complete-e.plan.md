<!-- 6f0da9a4-1bab-4b0e-b258-cbf690db7a47 e7c6031e-e97c-4131-96df-59db9d5d24e3 -->
# Complete Fashion Ecommerce System - Implementation Plan

## Overview

This plan covers the complete implementation of a modern fashion ecommerce website including SMS/OTP authentication, enhanced product management with size/color variants, advanced search and filtering, discount system, order tracking, and comprehensive unit and integration tests.

---

## Phase 1: SMS Service & OTP Authentication (3-4 days)

### Objectives

- Implement SMS service integration (Kavenegar or similar Iranian SMS provider)
- Build OTP-based registration and login system
- Replace/extend current email-based auth with phone number authentication

### Tasks

- Create ISmsService interface and implementation in Infrastructure
- Add SmsSettings to appsettings.json
- Create OTP entity for storing verification codes
- Implement SendOtpCommand and VerifyOtpCommand
- Update ApplicationUser to include PhoneNumber (already has via Identity)
- Create RegisterWithPhoneDto and LoginWithPhoneDto
- Implement OTP generation, storage, and expiration logic
- Add endpoints: POST /api/auth/send-otp, POST /api/auth/verify-otp
- Update AuthController with phone-based auth endpoints
- Add OTP validators (phone format, code length, expiration)

### Deliverables

- Fully functional SMS service
- OTP registration and login working
- Unit tests for OTP logic
- API documentation for new endpoints

**Estimated Time**: 24-30 hours

---

## Phase 2: Enhanced Product Model - Entities & Migrations (3-4 days)

### Objectives

- Add Brand, ProductVariant, Material, Season entities
- Extend Product entity with new fields (Gender, Brand relationship)
- Update ProductImage with ordering and type
- Create database migrations

### Tasks

- Create Brand entity (Name, LogoUrl, Description)
- Create ProductVariant entity (ProductId, Size, Color, SKU, StockQuantity, AdditionalPrice)
- Create Material entity (Name, Description)
- Create Season entity (Name)
- Add Brand navigation to Product
- Add Gender field to Product (enum: Male, Female, Kids, Unisex)
- Add ProductMaterial many-to-many relationship
- Add ProductSeason many-to-many relationship
- Update ProductImage: add IsPrimary, DisplayOrder, ImageType fields
- Create DbConfigurations for all new entities
- Create and test database migrations
- Add repositories for new entities

### Deliverables

- All new entities created and configured
- Database migrations applied successfully
- Repositories with basic CRUD operations
- Entity relationship tests

**Estimated Time**: 24-28 hours

---

## Phase 3: Product Feature Layer - CRUD & DTOs (3-4 days)

### Objectives

- Complete CRUD operations for Brand, ProductVariant, Material, Season
- Create comprehensive DTOs for all product-related entities
- Implement AutoMapper profiles

### Tasks

- Create DTOs: BrandDto, ProductVariantDto, MaterialDto, SeasonDto
- Create Commands/Queries for Brand (Create, Update, Delete, GetAll, GetById)
- Create Commands/Queries for ProductVariant (Create, Update, Delete, GetAll, GetByProductId)
- Create Commands/Queries for Material (Create, Update, Delete, GetAll)
- Create Commands/Queries for Season (Create, Update, Delete, GetAll)
- Enhance ProductDto to include:
- Category (nested object)
- Brand (nested object)
- Unit (nested object)
- Images (list with ordering)
- Variants (list of size/color combinations)
- Materials (list)
- Seasons (list)
- Reviews summary (count, average rating)
- StockQuantity, Description, all other fields
- Update ProductProfile (AutoMapper) for comprehensive mapping
- Create validators for all new DTOs
- Add controllers: BrandController, ProductVariantController, MaterialController, SeasonController

### Deliverables

- Complete CRUD for Brand, ProductVariant, Material, Season
- Enhanced ProductDto with all nested data
- API endpoints for all new entities
- Validation working correctly

**Estimated Time**: 28-32 hours

---

## Phase 4: Advanced Product Search & Filtering (3-4 days)

### Objectives

- Extend ProductSearchQuery to support size, color, brand, material, season, gender filtering
- Implement search on related entity names
- Optimize query performance with proper includes
- Add sorting by multiple criteria

### Tasks

- Update ProductSearchCriteriaDto to include:
- BrandId, Sizes (list), Colors (list), MaterialIds (list), SeasonIds (list)
- Gender filter, NewArrivals (last 30 days), OnSale
- Update ProductSearchQueryHandler to:
- Include all related entities (Category, Brand, Unit, Images, Variants, Materials, Seasons)
- Filter by Size (search in ProductVariant)
- Filter by Color (search in ProductVariant)
- Filter by Brand, Material, Season
- Filter by Gender
- Search in Unit.Name, Brand.Name, Material.Name
- Update GetProductByIdQueryHandler to include all related data
- Create GetProductFullDetailsQuery (alias for enhanced GetById)
- Add ProductSearchResultDto with facets/aggregations (available sizes, colors, price ranges)
- Optimize database queries with AsNoTracking where appropriate
- Update ProductController endpoints

### Deliverables

- Advanced filtering working on all criteria
- Search returns complete product data
- Faceted search results (available filters)
- Performance optimized queries

**Estimated Time**: 26-30 hours

---

## Phase 5: Related Products & Recommendations (2-3 days)

### Objectives

- Implement "Related Products" functionality
- Add "Recently Viewed" tracking
- Create recommendation queries

### Tasks

- Create ProductRelation entity (ProductId, RelatedProductId, RelationType: Similar/Complement)
- Create UserProductView entity (UserId, ProductId, ViewedAt)
- Implement GetRelatedProductsQuery (by ProductId)
- Implement GetFrequentlyBoughtTogetherQuery (analyze OrderItems)
- Implement GetRecentlyViewedQuery (by UserId, limit 20)
- Create TrackProductViewCommand (called when user views product detail)
- Add endpoints to ProductController:
- GET /api/product/{id}/related
- GET /api/product/{id}/frequently-bought-together
- GET /api/product/recently-viewed
- POST /api/product/{id}/track-view
- Create admin endpoints for managing product relations
- Update GetProductByIdQueryHandler to auto-track view

### Deliverables

- Related products working
- Recently viewed tracking functional
- Recommendation endpoints ready
- Admin can manage product relations

**Estimated Time**: 18-22 hours

---

## Phase 6: Coupons & Discount System (3 days)

### Objectives

- Create comprehensive coupon/discount code system
- Track coupon usage per user
- Integrate with checkout process

### Tasks

- Create Coupon entity (Code, DiscountPercentage, DiscountAmount, MinimumPurchase, StartDate, EndDate, UsageLimit, UsedCount, IsActive)
- Create UserCouponUsage entity (UserId, CouponId, OrderId, UsedAt)
- Create CRUD operations for Coupon (admin only)
- Create ValidateCouponQuery (check code validity, usage limit, dates, minimum purchase)
- Create ApplyCouponCommand (apply to cart/order)
- Update CheckoutCommand to accept CouponCode
- Update ProcessCheckoutCommandHandler to validate and apply coupon
- Add UserCouponUsage tracking on successful order
- Create GetUserCouponHistoryQuery
- Add CouponController with endpoints:
- POST /api/coupon/validate (public - validate code)
- GET /api/coupon/my-usage (user - get usage history)
- Admin endpoints: CRUD operations
- Create validators for coupon codes (format, expiration)

### Deliverables

- Complete coupon system
- Validation and application working
- Usage tracking functional
- Integration with checkout completed

**Estimated Time**: 22-26 hours

---

## Phase 7: Order Tracking & Timeline (2-3 days)

### Objectives

- Enhanced order tracking with detailed status timeline
- Add tracking number and delivery estimates
- Create order status history

### Tasks

- Create OrderStatusHistory entity (OrderId, Status, Note, ChangedAt, ChangedBy)
- Add fields to UserOrder: TrackingNumber, EstimatedDeliveryDate, ActualDeliveryDate
- Create OrderStatus enum (Pending, Processing, Packed, Shipped, OutForDelivery, Delivered, Cancelled)
- Create UpdateOrderStatusCommand (admin only)
- Automatically create OrderStatusHistory entry on status change
- Create GetOrderTimelineQuery (returns full status history)
- Create SetTrackingNumberCommand
- Update UserOrderDto to include timeline and tracking info
- Add endpoints:
- GET /api/order/{id}/timeline
- PUT /api/order/{id}/tracking (admin)
- PUT /api/order/{id}/status (admin)
- Create email/SMS notification on status change (use ISmsService)
- Add estimated delivery calculation logic

### Deliverables

- Order status timeline working
- Tracking number management
- Status update notifications
- Customer can track order progress

**Estimated Time**: 18-22 hours

---

## Phase 8: Stock Alerts & User Engagement Features (2 days)

### Objectives

- Notify users when out-of-stock items are available
- Track recently viewed products more comprehensively

### Tasks

- Create StockAlert entity (ProductId, ProductVariantId, UserId, Email, PhoneNumber, Notified, CreatedAt)
- Create CreateStockAlertCommand
- Create ProcessStockAlertsCommand (background job simulation)
- When ProductInventory or ProductVariant stock increases, check and send alerts
- Implement GetStockAlertsQuery (admin - see pending alerts)
- Add endpoint: POST /api/product/{id}/stock-alert
- Add "Notify Me" button logic in product detail
- Send SMS/Email when item back in stock
- Mark alert as notified
- Auto-cleanup old alerts (90 days)
- Enhance UserProductView tracking (integrate with search results click tracking)

### Deliverables

- Stock alert subscription working
- Notifications sent when stock available
- User can manage their alerts
- Recently viewed enhanced

**Estimated Time**: 14-18 hours

---

## Phase 9: Comprehensive Unit Tests (5-6 days)

### Objectives

- Achieve 80%+ code coverage with unit tests
- Test all commands, queries, validators, and domain logic

### Tasks Breakdown by Entity/Feature:

**Part A (Days 1-2): Core Product Tests**

- Brand: CRUD commands/queries tests, validators (15 tests)
- ProductVariant: CRUD + GetByProductId tests, validators (15 tests)
- Material: CRUD tests, validators (10 tests)
- Season: CRUD tests, validators (10 tests)
- Enhanced Product: Update tests for new fields, domain logic (15 tests)
- ProductImage: Test ordering, primary image logic (10 tests)

**Part B (Days 3-4): Shopping & Orders Tests**

- Cart: All cart operations, validators (20 tests)
- Checkout: With coupon integration, stock validation (15 tests)
- Coupon: Validation logic, usage tracking, expiration (15 tests)
- UserOrder: CRUD, status updates, timeline (15 tests)
- OrderStatusHistory: Auto-creation tests (8 tests)

**Part C (Day 5): User Features Tests**

- OTP: Generation, validation, expiration tests (12 tests)
- SMS Service: Mock tests for sending (8 tests)
- StockAlert: Creation, notification logic (10 tests)
- UserProductView: Tracking tests (8 tests)
- ProductRelation: Related products logic (10 tests)

**Part D (Day 6): Search & Recommendations**

- ProductSearch: All filter combinations (20 tests)
- GetRelatedProducts: Logic tests (8 tests)
- GetRecentlyViewed: Ordering, limit tests (8 tests)
- Remaining entities: UserAddress, UserPayment, UserProfile, SavedCart, ProductReview, Wishlist, UserReturnRequest (40 tests)

### Deliverables

- 250+ comprehensive unit tests
- 80%+ code coverage
- All validators tested
- Domain logic thoroughly tested
- Mock repositories and services used correctly

**Estimated Time**: 40-48 hours

---

## Phase 10: Integration Tests & E2E Scenarios (4-5 days)

### Objectives

- Test complete user journeys end-to-end
- Verify all integrations work together
- Test API contracts and error handling

### Setup Tasks

- Configure WebApplicationFactory
- Setup In-Memory SQLite database for tests
- Create test data seeders (products, users, categories, etc.)
- Create helper methods for authentication in tests
- Create base test classes

### Test Scenarios

**Day 1: Authentication & User Setup**

- User Registration with OTP (send OTP → verify → create account)
- User Login with OTP
- User Login with password (existing)
- Token refresh flow
- Invalid OTP handling
- Expired OTP handling

**Day 2: Product Catalog & Search**

- Browse products with pagination
- Filter by category, brand, size, color
- Search by keyword across all fields
- Sort products (price, name, date)
- View product details with all nested data
- Track product view
- View recently viewed products

**Day 3: Shopping Flow**

- Add product to cart (with variant selection)
- Update cart item quantity
- Remove from cart
- Apply discount coupon (valid/invalid)
- Add to wishlist
- Stock alert subscription
- Checkout process (full flow with coupon)

**Day 4: Order Management**

- View order history
- View order details with timeline
- Admin updates order status
- User receives notification (mock)
- Track order with tracking number
- Submit review after delivery
- Request return

**Day 5: Admin Operations & Edge Cases**

- Admin creates products with variants
- Admin creates coupons
- Admin manages product relations
- Stock alert processing (when stock added)
- Out of stock prevention in checkout
- Authorization tests (user vs admin)
- Error scenarios (404, 400, 401, 403)

### Deliverables

- 40+ integration test scenarios
- All critical user journeys covered
- API contracts verified
- Error handling tested
- Test infrastructure reusable

**Estimated Time**: 32-40 hours

---

## Summary

| Phase | Focus Area | Estimated Hours | Days (8h) |
|-------|-----------|-----------------|-----------|
| 1 | SMS & OTP Authentication | 24-30 | 3-4 |
| 2 | Enhanced Product Entities | 24-28 | 3-4 |
| 3 | CRUD & DTOs for New Entities | 28-32 | 3-4 |
| 4 | Advanced Search & Filtering | 26-30 | 3-4 |
| 5 | Related Products & Recommendations | 18-22 | 2-3 |
| 6 | Coupons & Discounts | 22-26 | 3 |
| 7 | Order Tracking & Timeline | 18-22 | 2-3 |
| 8 | Stock Alerts & Engagement | 14-18 | 2 |
| 9 | Comprehensive Unit Tests | 40-48 | 5-6 |
| 10 | Integration Tests & E2E | 32-40 | 4-5 |

**Total Estimated Time**: 246-296 hours (31-37 working days at 8 hours/day)

**With 4-5 hours/day**: 50-60 days (10-12 weeks)

---

## Technical Stack

- **SMS Provider**: Kavenegar / Ghasedak / Farazsms (Iranian providers)
- **Testing**: xUnit, Moq, FluentAssertions, WebApplicationFactory
- **Database**: SQL Server with EF Core migrations
- **Architecture**: Clean Architecture with CQRS (MediatR)
- **Validation**: FluentValidation
- **Mapping**: AutoMapper

---

## Notes

- Each phase builds upon previous phases
- SMS service can use a mock implementation initially for testing
- Product variants are crucial for fashion ecommerce (size/color management)
- Coupon system is flexible for various discount strategies
- Integration tests ensure all features work together seamlessly
- Phases 1-4 should be completed before Phases 5-8 (dependencies)
- Phase 9-10 can run in parallel with feature development if team size allows

### To-dos

- [ ] Implement SMS service integration and OTP-based authentication system (send OTP, verify OTP, register/login with phone number)
- [ ] Create enhanced product entities (Brand, ProductVariant, Material, Season) with migrations and repositories
- [ ] Build complete CRUD operations, DTOs, and API endpoints for Brand, ProductVariant, Material, Season; enhance ProductDto
- [ ] Implement advanced product search and filtering (size, color, brand, material, season, gender) with optimized queries
- [ ] Build related products, recently viewed tracking, and recommendation features
- [ ] Create comprehensive coupon/discount system with validation, usage tracking, and checkout integration
- [ ] Implement order status timeline, tracking numbers, delivery estimates, and SMS/email notifications
- [ ] Build stock alert subscription and notification system; enhance product view tracking
- [ ] Write comprehensive unit tests for all features (250+ tests covering commands, queries, validators, domain logic)
- [ ] Create integration tests and E2E scenarios (40+ tests covering full user journeys from registration to order completion)