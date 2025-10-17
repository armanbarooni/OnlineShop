# OnlineShop Architecture Documentation

This folder contains comprehensive architecture documentation for the OnlineShop system.

## 📁 Structure

```
Architecture/
├── Persian/
│   └── Complete-Architecture-FA.md          # Complete Persian documentation
├── English/
│   └── Complete-Architecture-EN.md          # Complete English documentation
├── Diagrams/
│   ├── system-architecture.mmd              # Overall architecture diagram
│   ├── cqrs-flow.mmd                        # CQRS pattern flow
│   ├── project-dependencies.mmd             # Project dependencies
│   ├── entity-relationships.mmd             # ER diagram
│   ├── authentication-flow.mmd              # Authentication sequence
│   └── shopping-flow.mmd                    # Shopping flow sequence
└── README.md                                 # This file
```

## 📖 Documentation Files

### Persian Documentation (مستندات فارسی)
**File:** `Persian/Complete-Architecture-FA.md`

**Contents:**
- نمای کلی سیستم (System Overview)
- معماری Clean Architecture
- ساختار پروژه (Project Structure)
- لایه Domain با 36 Entity
- لایه Application با 27 Feature
- لایه Infrastructure
- لایه WebAPI با 28 Controller
- جریان‌های اصلی (Authentication, Shopping, Orders)
- طراحی دیتابیس (36 جدول)
- مستندات API (~140 Endpoint)
- استراتژی تست
- آمار کامل سیستم

**Size:** ~1000+ lines of detailed documentation

### English Documentation
**File:** `English/Complete-Architecture-EN.md`

**Contents:**
- System Overview
- Clean Architecture
- Project Structure
- Domain Layer (36 Entities)
- Application Layer (27 Features)
- Infrastructure Layer
- WebAPI Layer (28 Controllers)
- System Flows
- Database Design (36 tables)
- API Documentation (~140 Endpoints)
- Testing Strategy
- Complete system statistics

**Size:** ~900+ lines of detailed documentation

## 🎨 Diagrams

All diagrams are in Mermaid format (.mmd) which can be rendered in:
- GitHub
- VS Code (with Mermaid extension)
- Online viewers (https://mermaid.live/)

### Available Diagrams:
1. **system-architecture.mmd** - Clean Architecture layers
2. **cqrs-flow.mmd** - CQRS pattern implementation
3. **project-dependencies.mmd** - Project reference graph
4. **entity-relationships.mmd** - Complete ER diagram
5. **authentication-flow.mmd** - OTP authentication sequence
6. **shopping-flow.mmd** - End-to-end shopping sequence

## 📊 System Statistics

- **Entities:** 36
- **Features:** 27
- **Commands:** ~95
- **Queries:** ~70
- **DTOs:** ~90
- **Validators:** ~55
- **AutoMapper Profiles:** 28
- **Repositories:** 32
- **Controllers:** 28
- **API Endpoints:** ~140
- **Database Tables:** 36
- **Migrations:** 23
- **Unit Tests:** 158

## 🔧 Technology Stack

- **.NET:** 8.0
- **EF Core:** 8.0.21
- **Database:** PostgreSQL
- **Authentication:** ASP.NET Core Identity + JWT
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **Mediator:** MediatR
- **Logging:** Serilog
- **SMS:** Kavenegar
- **API Docs:** Swagger/OpenAPI

## 📝 How to Use

### View Documentation
1. Open the Markdown files in any Markdown viewer
2. Use VS Code with Markdown preview
3. Push to GitHub for automatic rendering

### View Diagrams
1. Install Mermaid preview extension in VS Code
2. Use https://mermaid.live/ to view and edit
3. Diagrams are embedded in documentation files

### Update Documentation
1. Edit the respective Markdown file
2. Update diagrams in `Diagrams/` folder
3. Keep version number and date updated

## 🎯 Coverage

### Complete Coverage Includes:
✅ All 36 entities with properties and methods  
✅ All 27 features with Commands/Queries  
✅ All 90 DTOs with examples  
✅ All 55 validators with rules  
✅ All 28 controllers with endpoints  
✅ All 32 repositories with implementations  
✅ Complete request/response flow  
✅ Database schema with relationships  
✅ Authentication & Authorization  
✅ Error handling strategy  
✅ Testing strategy  
✅ Deployment checklist  

## 📅 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Oct 2024 | Initial complete documentation |

---

**Maintained by:** OnlineShop Architecture Team  
**Last Updated:** October 2024

