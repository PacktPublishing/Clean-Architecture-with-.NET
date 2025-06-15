<#
.SYNOPSIS
    Drops all SQL tables in a database using a provided script.

.DESCRIPTION
    Useful for resetting a database before applying fresh migrations. Only to be used in safe, non-production environments.

.PARAMETER ConnectionString
    The connection string to the SQL Server database.

.PARAMETER WorkingDirectory
    The root path where the `DropDatabaseTables.sql` script is located (typically in ./scripts/ci/).

.EXAMPLE
    ./scripts/Remove-SqlDatabaseTables.ps1 -ConnectionString "Server=..." -WorkingDirectory "./"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ConnectionString,

    [Parameter(Mandatory = $true)]
    [string]$WorkingDirectory
)

function Write-Info { param ([string]$msg); Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-ErrorAndExit { param ([string]$msg); Write-Host "[ERROR] $msg" -ForegroundColor Red; exit 1 }
function Write-Success { param ([string]$msg); Write-Host "[SUCCESS] $msg" -ForegroundColor Green }

Write-Info "Checking SqlServer module..."
if (-not (Get-Module -Name SqlServer -ListAvailable)) {
    Install-Module -Name SqlServer -Force -Scope CurrentUser
}

$sqlScriptPath = Join-Path $WorkingDirectory "scripts/ci/DropDatabaseTables.sql"
if (!(Test-Path -Path $sqlScriptPath)) {
    Write-ErrorAndExit "SQL script not found: $sqlScriptPath"
}

Write-Info "Dropping all tables using: $sqlScriptPath"
Invoke-SqlCmd -ConnectionString $ConnectionString -InputFile $sqlScriptPath

$result = Invoke-SqlCmd -ConnectionString $ConnectionString -Query "SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
if ($result.TableCount -eq 0) {
    Write-Success "All tables have been successfully deleted."
} else {
    Write-ErrorAndExit "Tables still exist after script ran. Check SQL script for issues."
}
