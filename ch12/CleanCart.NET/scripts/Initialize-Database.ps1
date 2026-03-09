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
    exit 1
}

# ------------------------------------------------------------
# Azure Key Vault Configuration
# ------------------------------------------------------------
$keyVaultName = "podyssey-local"
$secretName = "SqlServer--ConnectionString"

Write-Host "Preparing Azure authentication..."

# Ensure Azure CLI exists
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI is required but not installed."
    Write-Error "Install it from: https://learn.microsoft.com/cli/azure/install-azure-cli"
    exit 1
}

# ------------------------------------------------------------
# Tenant Configuration (cached locally)
# ------------------------------------------------------------
$configDir = Join-Path $PSScriptRoot ".config"
$tenantFile = Join-Path $configDir "tenant-id"

if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Path $configDir | Out-Null
}

if (Test-Path $tenantFile) {
    $tenantId = Get-Content $tenantFile
    Write-Host "Using cached Azure tenant: $tenantId" -ForegroundColor Green
}
else {

    Write-Host ""
    Write-Host "------------------------------------------------------------"
    Write-Host "Azure Authentication"
    Write-Host "------------------------------------------------------------"
    Write-Host "Enter the Azure Tenant ID that contains your Key Vault."
    Write-Host "You can find this in the Azure Portal under:"
    Write-Host "Azure Active Directory → Overview → Directory ID"
    Write-Host ""

    $tenantId = Read-Host "Tenant ID (Directory ID)"

    if (-not $tenantId) {
        Write-Error "Tenant ID is required."
        exit 1
    }

    $tenantId | Out-File $tenantFile
    Write-Host "Tenant ID saved for future runs." -ForegroundColor Yellow
}

# ------------------------------------------------------------
# Azure Authentication
# ------------------------------------------------------------
Write-Host ""
Write-Host "Signing into Azure tenant $tenantId..."

az login --tenant $tenantId | Out-Null

# ------------------------------------------------------------
# Locate the correct subscription automatically
# ------------------------------------------------------------
Write-Host "Searching for Key Vault '$keyVaultName'..."

$subscriptions = az account list --query "[].id" --output tsv

$keyVaultSubscription = $null

foreach ($sub in $subscriptions) {

    az account set --subscription $sub | Out-Null

    $exists = az keyvault show `
        --name $keyVaultName `
        --query name `
        --output tsv 2>$null

    if ($exists) {
        $keyVaultSubscription = $sub
        break
    }
}

if (-not $keyVaultSubscription) {
    Write-Error "Could not locate Key Vault '$keyVaultName' in any accessible subscription."
    Write-Error "Ensure the vault exists and your Azure account has access."
    exit 1
}

az account set --subscription $keyVaultSubscription

$subscriptionName = az account show --query name --output tsv

Write-Host "Using subscription: $subscriptionName" -ForegroundColor Green

# ------------------------------------------------------------
# Retrieve connection string
# ------------------------------------------------------------
Write-Host "Retrieving connection string from Azure Key Vault..."

$connString = az keyvault secret show `
    --vault-name $keyVaultName `
    --name $secretName `
    --query value `
    --output tsv

if (-not $connString) {
    Write-Error "Failed to retrieve secret '$secretName' from Key Vault."
    exit 1
}

Write-Host "Connection string retrieved successfully." -ForegroundColor Green

# ------------------------------------------------------------
# Parse Connection String
# ------------------------------------------------------------
$connectionParts = @{}
$connString.Split(";") | ForEach-Object {
    if ($_ -match "=") {
        $k,$v = $_.Split("=",2)
        $connectionParts[$k.Trim()] = $v.Trim()
    }
}

$saPassword = $connectionParts["Password"]
$server = $connectionParts["Server"]

if (-not $saPassword) {
    Write-Error "Could not determine SQL Server password from connection string."
    exit 1
}

# Extract port (example: localhost,4000)
if ($server -match ",(\d+)$") {
    $port = $Matches[1]
}
else {
    $port = "1433"
}

Write-Host "Using SQL Server port: $port"

# ------------------------------------------------------------
# SQL Server Container Setup
# ------------------------------------------------------------
Write-Host "Starting SQL Server Docker container..."

docker rm odyssey_sqlserver -f 2>$null | Out-Null

docker run `
    --name odyssey_sqlserver `
    -e "ACCEPT_EULA=Y" `
    -e "SA_PASSWORD=$saPassword" `
    -p "$port`:1433" `
    -d mcr.microsoft.com/mssql/server:2022-latest | Out-Null

# ------------------------------------------------------------
# Wait for SQL Server to be available
# ------------------------------------------------------------
Write-Host "Waiting for SQL Server to start..."

while ((Test-Connection localhost -TcpPort $port -Detailed).Status -ne 'Success') {
    Write-Host "SQL Server not ready yet. Waiting 5 seconds..."
    Start-Sleep -Seconds 5
}

Write-Host "SQL Server is ready." -ForegroundColor Green

# ------------------------------------------------------------
# Apply Migrations
# ------------------------------------------------------------
Write-Host "Applying EF Core migrations..."

& "$PSScriptRoot/Start-Migrations.ps1"