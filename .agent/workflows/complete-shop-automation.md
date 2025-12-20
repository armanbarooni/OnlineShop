---
description: Complete shop frontend automation with dynamic data
---

// turbo-all

# Complete Online Shop Frontend Automation

This workflow will automatically transform the static shop into a fully dynamic e-commerce platform.

## Phase 1: Project Setup & Infrastructure

### 1. Kill Existing Processes
Kill any running dotnet processes to avoid port conflicts:
```powershell
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
```

### 2. Navigate to Project Directory
```powershell
cd c:\Users\arman\source\repos\OnlineShop
```

### 3. Clean Previous Builds
```powershell
dotnet clean
```

### 4. Restore Dependencies (with timeout protection)
Start restore in background and monitor:
```powershell
$job = Start-Job -ScriptBlock { dotnet restore }
$timeout = 60
$elapsed = 0
while ($job.State -eq 'Running' -and $elapsed -lt $timeout) {
    Start-Sleep -Seconds 5
    $elapsed += 5
    Write-Host "Restoring... $elapsed seconds elapsed"
}
if ($job.State -eq 'Running') {
    Stop-Job $job
    Remove-Job $job
    Write-Host "Restore timed out after $timeout seconds - continuing anyway"
} else {
    Receive-Job $job
    Remove-Job $job
}
```

### 5. Build the Project
```powershell
dotnet build --no-restore
```

### 6. Start Development Server (Background)
```powershell
cd src\WebAPI
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run" -WindowStyle Minimized
cd ..\..
```

### 7. Wait for Server to Start
```powershell
Start-Sleep -Seconds 10
Write-Host "Server should be running at http://localhost:5000"
```

### 8. Test Server Health
```powershell
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/health" -Method GET -TimeoutSec 5
    Write-Host "Server is healthy: $($response.StatusCode)"
} catch {
    Write-Host "Server health check failed - but continuing anyway"
}
```

## Phase 2: Backend API Verification

### 9. Check Product API
```powershell
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/product" -Method GET -TimeoutSec 5
    Write-Host "Product API is working"
} catch {
    Write-Host "Product API check failed: $_"
}
```

### 10. Check Category API
```powershell
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/productcategory" -Method GET -TimeoutSec 5
    Write-Host "Category API is working"
} catch {
    Write-Host "Category API check failed: $_"
}
```

## Phase 3: Generate Placeholder Images

### 11. Create Images Directory
```powershell
New-Item -ItemType Directory -Force -Path "src\WebAPI\wwwroot\fa\assets\images\products\placeholders"
Write-Host "Created placeholders directory"
```

## Phase 4: Frontend Development

### 12. Open Browser for Testing
```powershell
Start-Process "http://localhost:5000/fa/index.html"
```

### 13. Create JavaScript Modules Directory
```powershell
New-Item -ItemType Directory -Force -Path "src\WebAPI\wwwroot\fa\assets\js\modules"
Write-Host "Created modules directory"
```

## Phase 5: Testing

### 14. List All HTML Pages
```powershell
Get-ChildItem -Path "src\WebAPI\wwwroot\fa" -Filter "*.html" | Select-Object Name
```

### 15. Check for JavaScript Errors in Console
Open browser developer console and check for errors

## Phase 6: Cleanup & Optimization

### 16. Remove Unused Static Files
```powershell
Write-Host "Manual cleanup required - review static files"
```

### 17. Final Server Restart
```powershell
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
cd src\WebAPI
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run" -WindowStyle Minimized
cd ..\..
```

### 18. Open Final Testing Page
```powershell
Start-Sleep -Seconds 10
Start-Process "http://localhost:5000/fa/index.html"
```

## Notes

- All commands will run automatically (turbo-all mode)
- Server will be started in background and kept running
- Restore has 60-second timeout protection
- Browser will open automatically for testing
- Check console output for any errors

## Manual Steps After Workflow

After this workflow completes, I will:
1. Generate 5 placeholder product images using AI
2. Create dynamic category loader JavaScript module
3. Create dynamic product loader JavaScript module
4. Update index.html to use dynamic data
5. Create shop.html for category listings
6. Update product.html for product details
7. Update cart.html for shopping basket
8. Update checkout.html for payment
9. Implement search functionality
10. Implement wishlist functionality
11. Update user-panel-order.html for purchase history
12. Link all pages with proper navigation
13. Remove story section and static content
14. Final testing and cleanup
