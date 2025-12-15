# Simple HTTP Server with PowerShell
$port = 8000
$root = Get-Location

Write-Host "Starting HTTP Server on port $port" -ForegroundColor Green
Write-Host "Open your browser and go to: http://localhost:$port" -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Red

# Create a simple HTTP listener
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$port/")
$listener.Start()

Write-Host "Server started successfully!" -ForegroundColor Green

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response
        
        $localPath = $request.Url.LocalPath
        if ($localPath -eq "/") {
            $localPath = "/test.html"
        }
        
        $filePath = Join-Path $root $localPath.TrimStart('/')
        
        if (Test-Path $filePath) {
            $content = Get-Content $filePath -Raw -Encoding UTF8
            $buffer = [System.Text.Encoding]::UTF8.GetBytes($content)
            
            # Set content type based on file extension
            if ($filePath.EndsWith('.html')) {
                $response.ContentType = "text/html; charset=utf-8"
            } elseif ($filePath.EndsWith('.js')) {
                $response.ContentType = "application/javascript; charset=utf-8"
            } elseif ($filePath.EndsWith('.css')) {
                $response.ContentType = "text/css; charset=utf-8"
            } elseif ($filePath.EndsWith('.png')) {
                $response.ContentType = "image/png"
            } elseif ($filePath.EndsWith('.jpg') -or $filePath.EndsWith('.jpeg')) {
                $response.ContentType = "image/jpeg"
            } else {
                $response.ContentType = "text/plain; charset=utf-8"
            }
            
            $response.ContentLength64 = $buffer.Length
            $response.OutputStream.Write($buffer, 0, $buffer.Length)
        } else {
            $response.StatusCode = 404
            $errorMessage = "File not found: $localPath"
            $buffer = [System.Text.Encoding]::UTF8.GetBytes($errorMessage)
            $response.ContentLength64 = $buffer.Length
            $response.OutputStream.Write($buffer, 0, $buffer.Length)
        }
        
        $response.Close()
    }
} catch {
    Write-Host "Server stopped." -ForegroundColor Red
} finally {
    $listener.Stop()
}
