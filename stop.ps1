Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Stopping all Microservices (dotnet watch run)..." -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Cyan

# Find all dotnet, pwsh, or cmd processes that have "watch run" in their command line
$processes = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe' or Name = 'pwsh.exe' or Name = 'cmd.exe'" | 
             Where-Object { $_.CommandLine -match "watch run" }

$count = 0
foreach ($p in $processes) {
    try {
        # Check if process still exists before trying to kill it
        if (Get-Process -Id $p.ProcessId -ErrorAction SilentlyContinue) {
            Stop-Process -Id $p.ProcessId -Force -ErrorAction Stop
            Write-Host "Stopped $($p.Name) (PID: $($p.ProcessId))" -ForegroundColor Green
            $count++
        }
    } catch {
        Write-Host "Could not stop PID $($p.ProcessId): $_" -ForegroundColor Red
    }
}

if ($count -eq 0) {
    Write-Host "No running microservices found." -ForegroundColor Yellow
} else {
    Write-Host "Successfully stopped $count processes!" -ForegroundColor Green
}
