# PowerShell script to start both frontend and backend at once

Write-Host "Starting OnlineShop project..."

# Check Node.js
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "Node.js is not installed! Please install Node.js."
    exit 1
}

# Check .NET SDK
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ".NET SDK is not installed! Please install .NET SDK."
    exit 1
}

# Check PostgreSQL
try {
    $pgTest = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
    if (-not $pgTest) {
        Write-Host "PostgreSQL service not found. Make sure it is installed and running."
    } else {
        Write-Host "PostgreSQL service running."
    }
} catch {
    Write-Host "Could not verify PostgreSQL status."
}

Write-Host "Node.js and .NET confirmed."

# Frontend dependencies
Write-Host "Checking frontend dependencies..."
if (-not (Test-Path "presentation/node_modules")) {
    Write-Host "Installing frontend dependencies..."
    Set-Location "presentation"
    npm install
    Set-Location ".."
}

# Database migration
Write-Host "Running database migration..."
Set-Location "src/WebAPI"
try {
    dotnet ef database update
    Write-Host "Database migration successful."
} catch {
    Write-Host "Database migration error. Database might already be up to date."
}
Set-Location "../.."

# Start frontend & backend
Write-Host "Starting frontend and backend..."

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd presentation; npm run start" -WindowStyle Normal
Start-Sleep -Seconds 3
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/WebAPI; dotnet run --urls 'http://localhost:5000'" -WindowStyle Normal

Write-Host "Project started!"
Write-Host "Frontend: http://localhost:8080"
Write-Host "Backend: http://localhost:5000"
Write-Host "Swagger: http://localhost:5000/swagger"
Write-Host "API: http://localhost:5000/api"
Write-Host ""
Write-Host "To stop the project, close the PowerShell windows."
