$env:SqlServer__ConnectionString = "Server=localhost,4000; Database=CleanCart.NET; User Id=sa; Password=SqlSecret!; TrustServerCertificate=True"
docker rm odyssey_sqlserver -f
docker run --name odyssey_sqlserver -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=SqlSecret!' -p 4000:1433 -d mcr.microsoft.com/mssql/server:2022-latest

#Make sure Sql Server is available
while(!(Test-NetConnection localhost -port 4000).TcpTestSucceeded)
{
    Write-Verbose "SqlServer still not started, waiting 5 seconds..."
    Start-Sleep -Seconds 5
}

& "$($PSScriptRoot)\Start-Migrations.ps1"