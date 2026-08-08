$services = @('Formal', 'ShortTerm', 'Driving', 'Customer')
foreach ($srv in $services) {
    $path = "src/services/$srv/$srv.API/appsettings.Development.json"
    
    # Check if file exists, if so read it and modify, else create new
    if (Test-Path $path) {
        $json = Get-Content $path -Raw | ConvertFrom-Json
    } else {
        $json = @{
            Logging = @{
                LogLevel = @{
                    Default = "Information"
                    "Microsoft.AspNetCore" = "Warning"
                }
            }
        }
    }

    # Add or update RabbitMQ
    $json | Add-Member -Type NoteProperty -Name "RabbitMQ" -Value @{
        Host = "127.0.0.1"
        Username = "guest"
        Password = "guest"
    } -Force

    # Add or update ConnectionStrings
    if ($srv -eq "Customer") {
        $json | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{
            CustomerDatabase = "Server=127.0.0.1;Database=$srvDb;User Id=sa;Password=Your_Strong_Passw0rd!;TrustServerCertificate=True;"
        } -Force
    } else {
        $json | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{
            CrmDatabase = "Server=127.0.0.1;Database=$srvDb;User Id=sa;Password=Your_Strong_Passw0rd!;TrustServerCertificate=True;"
        } -Force
    }

    $json | ConvertTo-Json -Depth 10 | Set-Content $path
    Write-Host "Updated $path"
}
