# Ensure script is running in PowerShell Core 7.4 or later
#requires -Version 7.4

# Define and set the connection string environment variable
Write-Output "Setting environment variable SqlServer__ConnectionString..."
$expectedConnectionString = "Server=localhost,4000; Database=CleanCart.NET; User Id=sa; Password=SqlSecret!; TrustServerCertificate=True"
$env:SqlServer__ConnectionString = $expectedConnectionString

# Validate that the environment variable was set correctly without printing the actual connection string
if ($env:SqlServer__ConnectionString -eq $expectedConnectionString) {
    Write-Host "Environment variable SqlServer__ConnectionString is set successfully." -ForegroundColor Green
    Write-Host "Remember to restart Visual Studio if you have it open." -ForegroundColor Yellow
} else {
    Write-Error "Failed to set environment variable SqlServer__ConnectionString correctly."
}

# Always start a new SQL Server container
docker rm odyssey_sqlserver -f
docker run --name odyssey_sqlserver -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=SqlSecret!' -p 4000:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# Make sure Sql Server is available
while((Test-Connection localhost -TcpPort 4000 -Detailed).Status -ne 'Success')
{
    Write-Verbose "SqlServer still not started, waiting 5 seconds..."
    Start-Sleep -Seconds 5
}

& "$PSScriptRoot/Start-Migrations.ps1"