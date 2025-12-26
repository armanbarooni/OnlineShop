<!-- 4004bade-aa30-420a-9984-2287219be638 78ff3941-d1d8-40e1-a8ff-b30f35ecfd6a -->
# برنامه Dockerfile و استقرار روی VPS

## هدف

ایجاد Dockerfile بهینه با کش لایه‌ای، GitHub Actions برای CI/CD، و راه‌اندازی کامل روی VPS لینوکس

## ساختار پروژه

- **Backend**: .NET 8.0 Web API (src/WebAPI)
- **Frontend**: فایل‌های استاتیک HTML/JS (presentation/ و src/WebAPI/wwwroot/fa/)
- **Database**: PostgreSQL
- **Deployment**: VPS لینوکس با Docker

## فایل‌های مورد نیاز

### 1. Dockerfile (ریشه پروژه)

- Multi-stage build برای کاهش سایز نهایی
- Stage 1: Build .NET application
- Stage 2: Runtime image
- لایه‌بندی بهینه: dependency restore → copy csproj → restore → copy