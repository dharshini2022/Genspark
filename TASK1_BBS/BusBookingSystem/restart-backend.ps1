# restart-backend.ps1
# Kills any process holding port 5000, then starts dotnet watch run cleanly.

Write-Host "🔍 Checking port 5000..." -ForegroundColor Cyan
$pids = (netstat -ano | Select-String ":5000.*LISTENING") -replace ".*LISTENING\s+","" |
        ForEach-Object { $_.Trim() } | Select-Object -Unique

if ($pids) {
    $pids | ForEach-Object {
        Write-Host "💀 Killing PID $_" -ForegroundColor Yellow
        Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Milliseconds 800
} else {
    Write-Host "✅ Port 5000 is already free" -ForegroundColor Green
}

Write-Host "🚀 Starting backend..." -ForegroundColor Cyan
dotnet watch run
