# Ensure script is running in PowerShell Core 7.4 or later
#requires -Version 7.4

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