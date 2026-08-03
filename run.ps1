Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Setting environment variables to prevent MSBuild crashes..." -ForegroundColor Yellow
$env:MSBUILDDISABLENODEREUSE = "1"
$env:MSBUILDMAXNODECOUNT = "1"

Write-Host "Pre-building the solution to prevent file lock issues..." -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Cyan

dotnet build

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Starting all Microservices with Hot Reload (dotnet watch)" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Cyan

$services = @(
    @{ Name = "Auth.API"; Path = "src\services\Auth\Auth.API"; Port = 5001 },
    @{ Name = "Formal.API"; Path = "src\services\Formal\Formal.API"; Port = 5002 },
    @{ Name = "ShortTerm.API"; Path = "src\services\ShortTerm\ShortTerm.API"; Port = 5003 },
    @{ Name = "Driving.API"; Path = "src\services\Driving\Driving.API"; Port = 5004 },
    @{ Name = "LeadAssignment.API"; Path = "src\services\LeadAssignment\LeadAssignment.API"; Port = 5005 },
    @{ Name = "Customer.API"; Path = "src\services\Customer\Customer.API"; Port = 5006 }
)

foreach ($service in $services) {
    Write-Host "Starting $($service.Name) (Expected Port: $($service.Port))..." -ForegroundColor Yellow
    $titleCmd = "`$Host.UI.RawUI.WindowTitle = '$($service.Name)'"
    
    # Use HTTP to avoid SSL/Certificate ERR_SSL_PROTOCOL_ERROR issues on localhost
    $envCmd = "`$env:MSBUILDDISABLENODEREUSE=1; `$env:MSBUILDMAXNODECOUNT=1; `$env:ASPNETCORE_ENVIRONMENT='Development'; `$env:ASPNETCORE_URLS='http://localhost:$($service.Port)'"
    
    Start-Process pwsh -WorkingDirectory $service.Path -ArgumentList @("-NoExit", "-Command", "$envCmd; $titleCmd; dotnet watch run --no-launch-profile")
}

Write-Host "Waiting for all APIs to accept connections..." -ForegroundColor Magenta

foreach ($service in $services) {
    Write-Host "Waiting for $($service.Name) to open port $($service.Port) " -NoNewline -ForegroundColor Yellow
    
    $retryCount = 0
    while ($retryCount -lt 120) {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        try {
            $asyncResult = $tcpClient.BeginConnect("127.0.0.1", $service.Port, $null, $null)
            $success = $asyncResult.AsyncWaitHandle.WaitOne(500, $false)
            if ($success) {
                $tcpClient.EndConnect($asyncResult)
                break
            }
        } catch { 
        } finally {
            $tcpClient.Close()
        }
        Write-Host "." -NoNewline -ForegroundColor Yellow
        $retryCount++
        Start-Sleep -Milliseconds 500
    }
    if ($retryCount -eq 120) {
        Write-Host " TIMEOUT!" -ForegroundColor Red
        Write-Host "$($service.Name) failed to start on port $($service.Port) within 60 seconds. Check its terminal window for errors!" -ForegroundColor Red
    } else {
        Write-Host " READY!" -ForegroundColor Green
    }
}

Write-Host "All APIs are up! Starting ApiGateway..." -ForegroundColor Yellow
$gatewayTitle = "`$Host.UI.RawUI.WindowTitle = 'ApiGateway'"
$envCmd = "`$env:MSBUILDDISABLENODEREUSE=1; `$env:MSBUILDMAXNODECOUNT=1; `$env:ASPNETCORE_ENVIRONMENT='Development'; `$env:ASPNETCORE_URLS='http://localhost:5000'"
Start-Process pwsh -WorkingDirectory "src\gateway\ApiGateway" -ArgumentList @("-NoExit", "-Command", "$envCmd; $gatewayTitle; dotnet watch run --no-launch-profile")

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "ALL SERVICES STARTED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "Open: http://localhost:5000/swagger/index.html to view Swagger" -ForegroundColor Magenta
Write-Host "========================================================" -ForegroundColor Cyan
