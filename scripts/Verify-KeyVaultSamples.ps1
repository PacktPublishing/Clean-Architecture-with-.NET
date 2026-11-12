<#
.SYNOPSIS
Runs the book's Key Vault integration checks using a temporary Azure resource group.

.DESCRIPTION
Sign in to Azure CLI and Visual Studio with accounts in the subscription's tenant.
Supply the CLI user's Entra object ID and, if different, the Visual Studio user's
object ID. The script creates a billable Key Vault with one fixed test fixture
and requests deletion of its resource group unless KeepResources is specified.
Use a subscription intended for testing.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Subscription,

    [Parameter(Mandatory)]
    [guid] $AzureUserObjectId,

    [string] $Location = 'westus2',
    [string] $Dotnet = 'dotnet',
    [string] $NuGetConfig,
    [Nullable[guid]] $VisualStudioObjectId,
    [switch] $KeepResources
)

$ErrorActionPreference = 'Stop'

function Invoke-Azure {
    $Arguments = $args
    $output = & az @Arguments --subscription $Subscription --only-show-errors
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI failed for '$($Arguments[0..1] -join ' ')'."
    }
    $output
}

function Invoke-Dotnet {
    $Arguments = $args
    & $Dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet failed for '$($Arguments[0..1] -join ' ')'."
    }
}

$repo = Split-Path $PSScriptRoot
$suffix = [guid]::NewGuid().ToString('N').Substring(0, 12)
$group = "clean-cart-verify-$suffix"
$vault = "ccverify-$suffix"
$previousVaultUrl = $env:KeyVault__Url
$previousDotnetRoot = $env:DOTNET_ROOT
$previousPath = $env:PATH
$createdGroup = $false
$failures = [Collections.Generic.List[string]]::new()

try {
    if (Test-Path -LiteralPath $Dotnet -PathType Leaf) {
        $Dotnet = (Resolve-Path -LiteralPath $Dotnet).Path
        $env:DOTNET_ROOT = Split-Path $Dotnet
        $env:PATH = "$env:DOTNET_ROOT$([IO.Path]::PathSeparator)$env:PATH"
    }
    if ($NuGetConfig) { $NuGetConfig = (Resolve-Path -LiteralPath $NuGetConfig).Path }

    Invoke-Azure account get-access-token --resource-type arm --query expires_on -o none
    Invoke-Azure account get-access-token --resource https://vault.azure.net --query expires_on -o none

    Invoke-Azure group create --name $group --location $Location -o none
    $createdGroup = $true
    Write-Host "Temporary resource group $group"

    $env:KeyVault__Url = Invoke-Azure keyvault create --name $vault --resource-group $group --location $Location --enable-rbac-authorization false --no-self-perms --retention-days 7 --query properties.vaultUri -o tsv
    Invoke-Azure keyvault set-policy --name $vault --resource-group $group --object-id $AzureUserObjectId.ToString() --secret-permissions get list set -o none
    if ($VisualStudioObjectId -and $VisualStudioObjectId -ne $AzureUserObjectId) {
        Invoke-Azure keyvault set-policy --name $vault --resource-group $group --object-id $VisualStudioObjectId.ToString() --secret-permissions get list -o none
    }
    # This fixed value is asserted by the chapter tests. Use it only as test data.
    Invoke-Azure keyvault secret set --vault-name $vault --name IntegrationTest--KeyVaultOverride --value keyvault-value -o none

    foreach ($chapter in 11..14) {
        Write-Host "Testing Chapter $chapter"
        $project = Join-Path $repo "ch$chapter/CleanCart.NET/tst/Presentation.IntegrationTests/Presentation.IntegrationTests.csproj"
        try {
            if ($NuGetConfig) {
                Invoke-Dotnet restore $project --configfile $NuGetConfig
                Invoke-Dotnet test $project --no-restore --nologo
            }
            else {
                Invoke-Dotnet test $project --nologo
            }
        }
        catch {
            $failures.Add("Chapter $chapter presentation integration tests")
            Write-Warning $_
        }
    }

    Push-Location (Join-Path $repo 'ch14/CleanCart.NET')
    try {
        if ($NuGetConfig) {
            Invoke-Dotnet tool restore --configfile $NuGetConfig
        }
        else {
            Invoke-Dotnet tool restore
        }
        Invoke-Dotnet ef migrations has-pending-model-changes --no-build --project src/Infrastructure --startup-project src/Presentation.BSA '--' --environment Development
    }
    catch {
        $failures.Add('Chapter 14 EF migration snapshot')
        Write-Warning $_
    }
    finally {
        Pop-Location
    }

    if ($failures.Count) { throw "Verification failed for $($failures -join ', ')." }
}
finally {
    $env:KeyVault__Url = $previousVaultUrl
    $env:DOTNET_ROOT = $previousDotnetRoot
    $env:PATH = $previousPath
    if ($createdGroup) {
        if ($KeepResources) {
            Write-Host "Kept resource group $group and vault $vault."
        }
        else {
            Invoke-Azure group delete --name $group --yes --no-wait -o none
            Write-Host "Requested deletion of $group. Key Vault soft delete retains the vault for seven days."
        }
    }
}
