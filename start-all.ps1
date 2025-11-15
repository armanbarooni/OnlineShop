# PowerShell script to start both frontend and backend at once

Write-Host "Starting OnlineShop project..."

function Sync-PresentationAssets {
    param(
        [string]$PresentationPath = "presentation",
        [string]$DestinationPath = "src/WebAPI/wwwroot/fa"
    )

    if (-not (Test-Path $PresentationPath)) {
        Write-Host "Presentation folder not found: $PresentationPath" -ForegroundColor Yellow
        return
    }

    if (-not (Test-Path $DestinationPath)) {
        Write-Host "Destination folder not found. Creating $DestinationPath ..."
        New-Item -ItemType Directory -Path $DestinationPath | Out-Null
    }

    Write-Host "Syncing presentation assets into $DestinationPath ..." -ForegroundColor Cyan

    # Copy HTML files
    Get-ChildItem -Path $PresentationPath -Filter *.html -File | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination (Join-Path $DestinationPath $_.Name) -Force
    }

    # Copy config.js if exists
    $configFile = Join-Path $PresentationPath "config.js"
    if (Test-Path $configFile) {
        Copy-Item -Path $configFile -Destination (Join-Path $DestinationPath "config.js") -Force
    }

    # Mirror assets directory
    $sourceAssets = Join-Path $PresentationPath "assets"
    $destinationAssets = Join-Path $DestinationPath "assets"
    if (Test-Path $sourceAssets) {
        if (-not (Test-Path $destinationAssets)) {
            New-Item -ItemType Directory -Path $destinationAssets | Out-Null
        }
        $robocopyArgs = @(
            $sourceAssets,
            $destinationAssets,
            "/MIR",
            "/NFL","/NDL","/NJH","/NJS","/NP"
        )
        $robocopyResult = Start-Process -FilePath "robocopy" -ArgumentList $robocopyArgs -Wait -NoNewWindow -PassThru
        if ($robocopyResult.ExitCode -ge 8) {
            Write-Host "Robocopy failed with exit code $($robocopyResult.ExitCode)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Presentation assets folder not found: $sourceAssets" -ForegroundColor Yellow
    }
}

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

# Sync presentation assets into ASP.NET static folder
Sync-PresentationAssets

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
