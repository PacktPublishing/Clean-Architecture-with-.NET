# Ensure script is running in PowerShell Core 7.4 or later
#requires -Version 7.4

# ------------------------------------------------------------
# Guard: Must be run from the chapter solution root
# ------------------------------------------------------------
if (-not (Test-Path "./CleanCart.NET.sln")) {
    Write-Error "This script must be run from the CleanCart.NET solution root."
    Write-Error "Example:"
    Write-Error "  cd ch08/CleanCart.NET"
    Write-Error "  .\scripts\Initialize-Database.ps1"
    Exit 1
}

# ------------------------------------------------------------
# User Secrets Configuration
# ------------------------------------------------------------
$connStringVarName = "SqlServer:ConnectionString"
$expectedConnectionString = "Server=localhost,4000; Database=CleanCart.NET; User Id=sa; Password=SqlSecret!; TrustServerCertificate=True"

Write-Output "Ensuring $connStringVarName is configured as a user secret..."

$infraProjectPath = Resolve-Path "./src/Infrastructure/Infrastructure.csproj"

if (-not (Test-Path $infraProjectPath)) {
    Write-Error "Could not find Infrastructure.csproj at expected path:"
    Write-Error "  ./src/Infrastructure/Infrastructure.csproj"
    Exit 1
}

# Ensure user-secrets is initialized (idempotent)
dotnet user-secrets init --project "$infraProjectPath" | Out-Null

# Check for existing secret
$existingSecret = dotnet user-secrets list --project "$infraProjectPath" `
    | Where-Object { $_ -like "$connStringVarName*" }

if ($existingSecret) {
    Write-Host "User secret $connStringVarName already exists. Skipping update." -ForegroundColor Green
}
else {
    dotnet user-secrets set "$connStringVarName" "$expectedConnectionString" `
        --project "$infraProjectPath"

    Write-Host "User secret $connStringVarName configured successfully." -ForegroundColor Green
}

Write-Host "Restart the application if it is currently running for the change to take effect." -ForegroundColor Yellow

# ------------------------------------------------------------
# SQL Server Container Setup
# ------------------------------------------------------------
docker rm odyssey_sqlserver -f
docker run `
    --name odyssey_sqlserver `
    -e 'ACCEPT_EULA=Y' `
    -e 'SA_PASSWORD=SqlSecret!' `
    -p 4000:1433 `
    -d mcr.microsoft.com/mssql/server:2022-latest

# Wait for SQL Server to be available
while ((Test-Connection localhost -TcpPort 4000 -Detailed).Status -ne 'Success') {
    Write-Verbose "SQL Server not ready yet. Waiting 5 seconds..."
    Start-Sleep -Seconds 5
}

# ------------------------------------------------------------
# Apply Migrations
# ------------------------------------------------------------
& "$PSScriptRoot/Start-Migrations.ps1"
