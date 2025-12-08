$infraProjectFilter = "*Infrastructure.csproj"
$infraProjectFile = Get-ChildItem ./ -Recurse -Filter $infraProjectFilter -File

if (!(Test-Path $infraProjectFile.FullName))
{
    Write-Error "Could not find project matching: $infraProjectFilter"
    Exit
}

$startupProjectFilter = "*Presentation.BSA.csproj"
$startupProjectFile = Get-ChildItem ./ -Recurse -Filter $startupProjectFilter -File

if (!(Test-Path $startupProjectFile.FullName))
{
    Write-Error "Could not find project matching: $startupProjectFilter"
    Exit
}

Write-Verbose "Building solution..."
dotnet clean "$($infraProjectFile.FullName)"
dotnet clean "$($startupProjectFile.FullName)"
dotnet restore "$($startupProjectFile.FullName)"
dotnet build "$($startupProjectFile.FullName)" --no-restore

# Ensure that dotnet-ef is available
dotnet tool restore

Write-Verbose "Running migrations..."
dotnet ef migrations add InitialCreate --project "$($infraProjectFile.FullName)" --startup-project "$($startupProjectFile.FullName)"
dotnet ef database update --project "$($infraProjectFile.FullName)" --startup-project "$($startupProjectFile.FullName)"