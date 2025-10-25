# PowerShell script to start both frontend and backend at once

Write-Host "Starting OnlineShop project..."

# Check Node.js
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "Node.js is not installed! Skipping frontend startup." -ForegroundColor Yellow
    $skipFrontend = $true
} else {
    $skipFrontend = $false
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

# Frontend dependencies (only if Node.js is available)
if (-not $skipFrontend) {
    Write-Host "Checking frontend dependencies..."
    if (-not (Test-Path "presentation/node_modules")) {
        Write-Host "Installing frontend dependencies..."
        Set-Location "presentation"
        npm install
        Set-Location ".."
    }
}

# Database migration
Write-Host "Running database migration..."
Set-Location "src/WebAPI"

# First, ensure the project builds successfully
Write-Host "Build started..."
$buildResult = dotnet build --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Cannot continue with migration." -ForegroundColor Red
    Write-Host "Please fix build errors before running migrations." -ForegroundColor Red
    Set-Location "../.."
    exit 1
}
Write-Host "Build succeeded."

# Now run migration
try {
    $migrationResult = dotnet ef database update
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Migration failed!" -ForegroundColor Red
        Set-Location "../.."
        exit 1
    }
    Write-Host "Database migration successful."
} catch {
    Write-Host "Database migration error. Database might already be up to date." -ForegroundColor Yellow
}
Set-Location "../.."

# Start frontend & backend
Write-Host "Starting backend..."

if (-not $skipFrontend) {
    Write-Host "Starting frontend and backend..."
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd presentation; npm run start" -WindowStyle Normal
    Start-Sleep -Seconds 3
    Write-Host "Frontend: http://localhost:8080"
} else {
    Write-Host "Skipping frontend startup (Node.js not available)"
}

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/WebAPI; dotnet run --urls 'http://localhost:5000'" -WindowStyle Normal

Write-Host "Project started!"
Write-Host "Backend: http://localhost:5000"
Write-Host "Swagger: http://localhost:5000/swagger"
Write-Host "API: http://localhost:5000/api"
Write-Host ""
Write-Host "To stop the project, close the PowerShell windows."
