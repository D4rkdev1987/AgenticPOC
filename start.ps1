$root = Split-Path -Parent $MyInvocation.MyCommand.Path

function Clear-Port {
    param([int]$Port)
    $portPids = (netstat -ano | Select-String ":$Port\s" | ForEach-Object { ($_ -split '\s+')[-1] } | Sort-Object -Unique)
    foreach ($p in $portPids) {
        if ($p -match '^\d+$' -and [int]$p -ne 0) {
            Stop-Process -Id ([int]$p) -Force -ErrorAction SilentlyContinue
            Write-Host "  Freed port $Port (PID $p)" -ForegroundColor DarkGray
        }
    }
}

Write-Host "Starting Agentic Control Plane..." -ForegroundColor Cyan
Write-Host "Clearing ports 5175 and 5173/5174..." -ForegroundColor DarkGray
Clear-Port 5175
Clear-Port 5173
Clear-Port 5174

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\src\AgenticControlPlane.Api'; dotnet restore --nologo -q; dotnet run --urls http://localhost:5175" -WindowStyle Normal

Start-Sleep -Milliseconds 3000

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\src\AgenticControlPlane.Web'; npm install --silent; npm run dev" -WindowStyle Normal

Write-Host ""
Write-Host "Both services are starting in separate windows." -ForegroundColor Green
Write-Host "API  -> http://localhost:5175" -ForegroundColor Yellow
Write-Host "App  -> http://localhost:5174 (or 5173 if port is free)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Open the App URL in your browser once both windows say 'ready'." -ForegroundColor Cyan
