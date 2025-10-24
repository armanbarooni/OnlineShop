# PowerShell script to start HTTP server
Write-Host "🚀 Starting HTTP Server..." -ForegroundColor Green
Write-Host "📁 Serving files from: $(Get-Location)" -ForegroundColor Yellow
Write-Host "🌐 Open your browser and go to: http://localhost:8000" -ForegroundColor Cyan
Write-Host "🛑 Press Ctrl+C to stop the server" -ForegroundColor Red
Write-Host ""

try {
    python -m http.server 8000
} catch {
    Write-Host "❌ Python not found! Please install Python first." -ForegroundColor Red
    Write-Host "💡 Download from: https://www.python.org/downloads/" -ForegroundColor Yellow
    Write-Host "💡 Or install from Microsoft Store" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
}
