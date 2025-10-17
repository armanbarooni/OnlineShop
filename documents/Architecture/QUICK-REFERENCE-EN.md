# OnlineShop Architecture Quick Reference

## 🎯 Quick Access

### 📂 Documentation Files

- **Complete Persian Docs**: `Persian/Complete-Architecture-FA.md` (~3850 lines)
- **Complete English Docs**: `English/Complete-Architecture-EN.md` (~900 lines)
- **Entities & Features Reference**: `Persian/Complete-Entities-Features-Reference-FA.md` (~600 lines)
- **Diagrams**: `Diagrams/*.mmd` (6 files)

---

## 📊 Quick Stats

| Item | Count |
|------|-------|
| **Entities** | 36 |
| **Features** | 27 |
| **Commands** | ~95 |
| **Queries** | ~70 |
| **DTOs** | ~90 |
| **Validators** | ~55 |
| **AutoMapper Profiles** | 28 |
| **Repositories** | 32 |
| **Controllers** | 28 |
| **API Endpoints** | ~140 |
| **Database Tables** | 36 |
| **Migrations** | 23 |
| **Unit Tests** | 158 |

---

## 🗂️ Layer Structure

```
OnlineShop/
├── Domain/           36 Entities, 32 Interfaces
├── Application/      27 Features, 90 DTOs, 55 Validators
├── Infrastructure/   32 Repositories, External Services
└── WebAPI/          28 Controllers, 2 Middlewares
```

---

## 🔑 Key Entities

### Products (14 Entities)
Product, ProductCategory, Brand, Material, Season, Unit, ProductVariant, ProductImage, ProductDetail, ProductInventory, ProductReview, ProductRelation, ProductMaterial, ProductSeason

### Users (5 Entities)
ApplicationUser, UserProfile, UserAddress, Otp, RefreshToken

### Shopping & Orders (8 Entities)
Cart, CartItem, SavedCart, UserOrder, UserOrderItem, UserPayment, OrderStatusHistory, UserReturnRequest

### Others (9 Entities)
Wishlist, Coupon, UserCouponUsage, StockAlert, UserProductView, MahakMapping, MahakQueue, MahakSyncLog, SyncErrorLog

---

## 🚀 Core Features

### Authentication
- Traditional registration & login
- OTP-based authentication
- Refresh tokens

### Product Management
- Full CRUD operations
- Advanced search with multiple filters
- Related and recommended products

### Shopping
- Shopping cart
- Coupons & discounts
- Multi-step checkout

### Orders
- Order management
- Status tracking
- Returns

---

## 📡 Important Endpoints

### Authentication
```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/send-otp
POST /api/auth/login-phone
```

### Products
```
GET /api/product
POST /api/product/search
GET /api/product/{id}
POST /api/product (Admin)
```

### Cart
```
GET /api/cart
POST /api/cart/add
DELETE /api/cart/remove/{productId}
```

### Orders
```
GET /api/order
POST /api/checkout/complete
GET /api/order/{id}/timeline
```

---

## 🔧 Technology Stack

- .NET 8.0
- EF Core 8.0.21
- PostgreSQL
- ASP.NET Identity + JWT
- MediatR
- AutoMapper
- FluentValidation
- Serilog

---

## 📖 How to Use Documentation

1. **For Overview**: Read beginning of `Complete-Architecture-FA.md`
2. **For Entity Details**: See Domain Layer section
3. **For Feature Implementation**: See Application Layer
4. **For Flows**: Check Sequence diagrams
5. **For API**: See API Documentation section

---

## 🎨 Available Diagrams

1. `system-architecture.mmd` - Overall architecture
2. `cqrs-flow.mmd` - CQRS pattern
3. `project-dependencies.mmd` - Project dependencies
4. `entity-relationships.mmd` - Entity relationships
5. `authentication-flow.mmd` - Authentication flow
6. `shopping-flow.mmd` - Shopping flow

**How to View:**
- VS Code: Install Mermaid Preview Extension
- Online: https://mermaid.live/
- GitHub: Automatic rendering

---

**Last Updated:** October 2024  
**Version:** 1.0

