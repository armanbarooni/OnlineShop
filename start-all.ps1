# اسکریپت PowerShell برای اجرای همزمان فرانت‌اند و بک‌اند
# PowerShell Script to run Frontend and Backend simultaneously

Write-Host "🚀 شروع پروژه OnlineShop..." -ForegroundColor Green
Write-Host "Starting OnlineShop project..." -ForegroundColor Green

# بررسی وجود Node.js
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Node.js نصب نیست! لطفاً Node.js را نصب کنید." -ForegroundColor Red
    Write-Host "❌ Node.js is not installed! Please install Node.js." -ForegroundColor Red
    exit 1
}

# بررسی وجود .NET
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "❌ .NET نصب نیست! لطفاً .NET SDK را نصب کنید." -ForegroundColor Red
    Write-Host "❌ .NET is not installed! Please install .NET SDK." -ForegroundColor Red
    exit 1
}

# بررسی وجود PostgreSQL
try {
    $pgTest = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
    if (-not $pgTest) {
        Write-Host "⚠️ PostgreSQL سرویس یافت نشد. اطمینان حاصل کنید که PostgreSQL نصب و اجرا شده است." -ForegroundColor Yellow
        Write-Host "⚠️ PostgreSQL service not found. Make sure PostgreSQL is installed and running." -ForegroundColor Yellow
    } else {
        Write-Host "✅ PostgreSQL سرویس در حال اجرا است" -ForegroundColor Green
        Write-Host "✅ PostgreSQL service is running" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️ نتوانستیم وضعیت PostgreSQL را بررسی کنیم" -ForegroundColor Yellow
    Write-Host "⚠️ Could not check PostgreSQL status" -ForegroundColor Yellow
}

Write-Host "✅ Node.js و .NET نصب هستند" -ForegroundColor Green
Write-Host "✅ Node.js and .NET are installed" -ForegroundColor Green

# نصب dependencies فرانت‌اند در صورت نیاز
Write-Host "🔄 بررسی dependencies فرانت‌اند..." -ForegroundColor Yellow
Write-Host "🔄 Checking frontend dependencies..." -ForegroundColor Yellow

if (-not (Test-Path "presentation/node_modules")) {
    Write-Host "📦 نصب dependencies فرانت‌اند..." -ForegroundColor Yellow
    Write-Host "📦 Installing frontend dependencies..." -ForegroundColor Yellow
    Set-Location "presentation"
    npm install
    Set-Location ".."
}

# اجرای database migration
Write-Host "🔄 اجرای database migration..." -ForegroundColor Yellow
Write-Host "🔄 Running database migration..." -ForegroundColor Yellow

Set-Location "src/WebAPI"
try {
    dotnet ef database update
    Write-Host "✅ Database migration موفق بود" -ForegroundColor Green
    Write-Host "✅ Database migration successful" -ForegroundColor Green
} catch {
    Write-Host "⚠️ خطا در database migration. ممکن است دیتابیس قبلاً به‌روزرسانی شده باشد." -ForegroundColor Yellow
    Write-Host "⚠️ Database migration error. Database might already be up to date." -ForegroundColor Yellow
}
Set-Location "../.."

# اجرای فرانت‌اند و بک‌اند به صورت همزمان
Write-Host "🔄 اجرای فرانت‌اند و بک‌اند..." -ForegroundColor Yellow
Write-Host "🔄 Starting Frontend and Backend..." -ForegroundColor Yellow

# اجرای فرانت‌اند در پورت 8080
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd presentation; npm run start" -WindowStyle Normal

# کمی صبر کنید تا فرانت‌اند شروع شود
Start-Sleep -Seconds 3

# اجرای بک‌اند در پورت 5000 (HTTP)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/WebAPI; dotnet run --urls 'http://localhost:5000'" -WindowStyle Normal

Write-Host "✅ پروژه شروع شد!" -ForegroundColor Green
Write-Host "✅ Project started!" -ForegroundColor Green
Write-Host "🌐 فرانت‌اند: http://localhost:8080" -ForegroundColor Cyan
Write-Host "🌐 Frontend: http://localhost:8080" -ForegroundColor Cyan
Write-Host "🔧 بک‌اند: http://localhost:5000" -ForegroundColor Cyan
Write-Host "🔧 Backend: http://localhost:5000" -ForegroundColor Cyan
Write-Host "📚 Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "🔗 API: http://localhost:5000/api" -ForegroundColor Cyan

Write-Host "`nبرای توقف پروژه، پنجره‌های PowerShell را ببندید" -ForegroundColor Yellow
Write-Host "To stop the project, close the PowerShell windows" -ForegroundColor Yellow
