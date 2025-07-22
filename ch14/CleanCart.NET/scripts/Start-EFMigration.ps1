<#
.SYNOPSIS
    Applies EF Core migrations in a specified environment.

.DESCRIPTION
    This script restores .NET tools, validates EF Core configuration, and applies all pending migrations for the specified environment.
    If -AddMigration is specified and the migration does not exist, it will be created automatically before applying.
    Can be used locally or in CI/CD pipelines.

.PARAMETER Environment
    The name of the environment (e.g., dev, qa, ppe, prod). Used to set ASPNETCORE_ENVIRONMENT for EF operations. Defaults to "Development".
    Certain environments are protected from destructive operations like database resets.

.PARAMETER MigrationName
    (Optional) The specific migration to apply or create. If omitted, all pending migrations will be applied.

.PARAMETER WorkingDirectory
    The root directory of the solution or repository where EF projects can be discovered. Defaults to "./".

.PARAMETER ConnectionString
    (Optional) A SQL Server connection string used to check if the __EFMigrationsHistory table exists, which helps determine
    whether the database is new or partially initialized.

.PARAMETER AddMigration
    (Optional) If specified, the script will add the migration if it doesn't already exist before applying it.

.PARAMETER ResetDatabase
    (Optional) If specified, all tables in the database will be dropped before applying migrations.
    This is only permitted in safe environments (dev, qa, ppe). Requires -ConnectionString.

.EXAMPLE
    # Run this in CI or locally to apply all pending migrations in Development.
    ./scripts/Start-EFMigration.ps1 -Environment Development

.EXAMPLE
    # Create a new migration if it doesn't exist and apply it
    ./scripts/Start-EFMigration.ps1 -Environment Development -MigrationName "NewFeatureMigration" -AddMigration

.EXAMPLE
    # Run this in a controlled environment to apply up to a specific migration
    ./scripts/Start-EFMigration.ps1 -Environment qa -MigrationName "AddProductTable"

.EXAMPLE
    # Reset the dev database and re-apply all existing migrations
    ./scripts/Start-EFMigration.ps1 -Environment dev -ResetDatabase -ConnectionString "Server=...;Database=..." -WorkingDirectory "./"
#>
[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$Environment = "Development",
    [string]$MigrationName = "",
    [string]$WorkingDirectory = "./",
    [string]$ConnectionString = "",
    [switch]$AddMigration,
    [Switch]$ResetDatabase
)

$existingEnv = $env:ASPNETCORE_ENVIRONMENT
Set-StrictMode -Version Latest
$MigrationName = $MigrationName.Trim()

function ExitOnError {
    param ([string]$Message)
    Write-Host "##[error] $Message" -ForegroundColor Red
    Exit 1
}

function Write-Success {
    param ([string]$Message)
    Write-Host "##[section] [SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param ([string]$Message)
    Write-Host "##[warning] [WARNING] $Message" -ForegroundColor Yellow
}

function Write-Info {
    param ([string]$Message)
    Write-Host "##[command] [INFO] $Message" -ForegroundColor Cyan
}

function Test-MigrationHistoryExists {
    param ([string]$ConnectionString)

    Add-Type -AssemblyName "System.Data"
    $query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'"

    try {
        $conn = New-Object System.Data.SqlClient.SqlConnection
        $conn.ConnectionString = $ConnectionString
        $conn.Open()

        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $query
        $result = $cmd.ExecuteScalar()
        $conn.Close()

        return ($result -eq 1)
    } catch {
        Write-Warning "Failed to check migration history: $_"
        return $false
    }
}

try {
    Write-Info "Restoring .NET tools..."
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { ExitOnError "Tool restore failed." }
    Write-Success "Tools restored."

    Write-Info "Checking EF CLI..."
    $efVersion = dotnet ef --version
    if ($LASTEXITCODE -ne 0) { ExitOnError "EF CLI not installed or configured." }
    Write-Success "EF CLI Version: $efVersion"

    Write-Info "Setting ASPNETCORE_ENVIRONMENT to $Environment"
    $env:ASPNETCORE_ENVIRONMENT = $Environment

    $sqlProjectFilter = "*Infrastructure.csproj"
    $startupProjectFilter = "*Presentation.BSA.csproj"

    Write-Info "Locating EF Core projects..."
    $sqlProject = Get-ChildItem -Path $WorkingDirectory -Recurse -Filter $sqlProjectFilter -File | Select-Object -First 1
    if (-not $sqlProject) { ExitOnError "Could not find project: $sqlProjectFilter" }
    $startupProject = Get-ChildItem -Path $WorkingDirectory -Recurse -Filter $startupProjectFilter -File | Select-Object -First 1
    if (-not $startupProject) { ExitOnError "Could not find startup project: $startupProjectFilter" }

    Write-Success "EF Project: $($sqlProject.FullName)"
    Write-Success "Startup Project: $($startupProject.FullName)"

    $migrationHistoryExists = $false
    if (-not [string]::IsNullOrWhiteSpace($ConnectionString)) {
        Write-Info "Checking __EFMigrationsHistory via ADO.NET..."
        $migrationHistoryExists = Test-MigrationHistoryExists -ConnectionString $ConnectionString
        if ($migrationHistoryExists) {
            Write-Success "__EFMigrationsHistory table found."
        } else {
            Write-Warning "__EFMigrationsHistory table not found — assuming fresh database."
        }
    }

    Write-Info "Getting EF migration list..."
    $migrationOutput = dotnet ef migrations list --project "$($sqlProject.FullName)" --startup-project "$($startupProject.FullName)" -- --environment $Environment 2>&1
    if ($migrationOutput -match "Build failed" -or $migrationOutput -match "No DbContext was found") {
        ExitOnError "Migration list check failed: $migrationOutput"
    }

    $migrationLines = $migrationOutput | Where-Object { $_ -match '^\s*\d{14}_[A-Za-z0-9_]+' }
    $appliedMigrations = $migrationLines | Where-Object { $_ -notmatch "\(Pending\)" } | ForEach-Object { $_.Trim() }
    $pendingMigrations = $migrationLines | Where-Object { $_ -match "\(Pending\)" } | ForEach-Object { $_ -replace "\(Pending\)", "" -replace "=", "" -replace "\s+$", "" }

    if (-not $appliedMigrations) {
        Write-Warning "No migrations currently applied."
    } else {
        Write-Success "Applied: $($appliedMigrations -join ', ')"
    }

    if ($pendingMigrations) {
        Write-Info "Pending: $($pendingMigrations -join ', ')"
    } else {
        Write-Success "No pending migrations."
    }

    # Migration creation if requested
    if (-not [string]::IsNullOrWhiteSpace($MigrationName)) {
        $normalizedTarget = $MigrationName.ToLowerInvariant()

        $normalizedMigrations = ($appliedMigrations + $pendingMigrations) |
            ForEach-Object { ($_ -replace '^\d{14}_', '').ToLowerInvariant() }

        if ($normalizedMigrations -contains $normalizedTarget) {
            Write-Info "Migration '$MigrationName' already exists. Skipping creation."
        }
        elseif ($AddMigration) {
            Write-Warning "Migration '$MigrationName' not found. Creating it now..."

            $addCmd = "dotnet ef migrations add $MigrationName --project `"$($sqlProject.FullName)`" --startup-project `"$($startupProject.FullName)`" -- --environment $Environment"
            Write-Info "Executing: $addCmd"
            $addOutput = Invoke-Expression $addCmd 2>&1

            if ($LASTEXITCODE -ne 0) {
                if ($addOutput -join "`n" -match "A migration named '$MigrationName' already exists") {
                    Write-Warning "EF CLI reports that migration '$MigrationName' already exists. Skipping creation."
                } else {
                    $relevant = ($addOutput | Where-Object { $_ -match "error|System\.|Exception|Build failed" }) -join "`n"
                    ExitOnError "Migration creation failed:`n$relevant"
                }
            } else {
                Write-Success "Migration '$MigrationName' created successfully."
            }
        }
        else {
            ExitOnError "Migration '$MigrationName' not found. Use -AddMigration to create it."
        }
    }

    # Build EF update command
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        if (-not $pendingMigrations -and -not $migrationHistoryExists) {
            Write-Warning "No migrations pending, but migration history table not found. Proceeding anyway."
        } elseif (-not $pendingMigrations) {
            Write-Success "All migrations already applied."
            Write-Info "Optional: Add database seeding logic here if needed using application code or raw SQL."
            exit 0
        }

        $cmd = "dotnet ef database update --project `"$($sqlProject.FullName)`" --startup-project `"$($startupProject.FullName)`" -- --environment $Environment"
    } else {
        Write-Info "Applying migration '$MigrationName'..."
        $cmd = "dotnet ef database update $MigrationName --project `"$($sqlProject.FullName)`" --startup-project `"$($startupProject.FullName)`" -- --environment $Environment"
    }

    # Reset database logic (before migration execution)
    $safeResetEnvironments = @("development", "test", "dev", "qa", "ppe", "staging", "stage", "local")

    if ($ResetDatabase) {
        $envLower = $Environment.ToLowerInvariant()

        if (-not $safeResetEnvironments -contains $envLower) {
            ExitOnError "Resetting the database is not allowed in environment '$Environment'. Allowed: $($safeResetEnvironments -join ', ')"
        }

        if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
            ExitOnError "ConnectionString is required when using -ResetDatabase."
        }

        # Extract Server and Database for a better confirmation message
        $server = ""
        $database = ""

        if ($ConnectionString -match "(?i)(Server|Data Source)\s*=\s*([^;]+)") {
            $server = $matches[2].Trim()
        }

        if ($ConnectionString -match "(?i)(Database|Initial Catalog)\s*=\s*([^;]+)") {
            $database = $matches[2].Trim()
        }

        if (-not $server -or -not $database) {
            ExitOnError "Could not determine server or database name from ConnectionString. Please ensure both 'Server=' and 'Database=' are present."
        }

        $target = "[$database] on server [$server]"
        $resetScript = Join-Path $PSScriptRoot "Remove-SqlDatabaseTables.ps1"

        # Warn if environment is Development but the database does not look local
        if ($envLower -eq "development") {
            $isLikelyLocal = $server -match "localhost|127\.0\.0\.1|0\.0\.0\.0|\.\\SQLEXPRESS" -or $server -like "*localdb*"

            if (-not $isLikelyLocal) {
                Write-Warning "The environment is 'Development', but the database server '$server' does not appear to be local."
                $confirmation = Read-Host "Are you sure you want to reset a non-local development database? Type 'YES' to continue"
                if ($confirmation -ne "YES") {
                    ExitOnError "Database reset aborted by user."
                }
            }
        }

        if ($PSCmdlet.ShouldProcess("Database $target", "Reset all tables using $resetScript")) {
            Write-Warning "You are about to delete all tables in the database $target. This action is IRREVERSIBLE."
            Write-Info "Running: $resetScript"

            & "$resetScript" -ConnectionString $ConnectionString -WorkingDirectory $WorkingDirectory

            if ($LASTEXITCODE -ne 0) {
                ExitOnError "Database reset failed. See error above."
            }

            Write-Success "Database reset completed."
        } else {
            ExitOnError "Database reset canceled by user."
        }
    }

    Write-Info "Executing EF migration update..."
    $output = Invoke-Expression $cmd 2>&1
    if ($LASTEXITCODE -ne 0) {
        $relevantLines = $output | Where-Object { $_ -match "error|System\.|Exception|Build failed" }
        $relevant = $relevantLines -join "`n"

        if ($relevant -match "There is already an object named '.*' in the database") {
            Write-Warning "EF Core reported a schema conflict: an object already exists in the database."
            Write-Warning "This often means you're trying to apply a migration to a database that wasn't reset."
            Write-Warning "Double-check the following before retrying:"
            Write-Host ""
            Write-Host "   Did you forget to include the -ResetDatabase flag?" -ForegroundColor Yellow
            Write-Host "     Example:" -ForegroundColor Gray
            Write-Host "     ./Start-EFMigration.ps1 -Environment $Environment -MigrationName $MigrationName -ResetDatabase -ConnectionString `"{your-connection}`"" -ForegroundColor Gray
            Write-Host ""
            Write-Host "   Did you accidentally target the wrong environment (e.g., pointing at prod or another DB)?" -ForegroundColor Yellow
            Write-Host "     Current environment: '$Environment'" -ForegroundColor Gray
            Write-Host ""

            ExitOnError "Migration failed due to database schema conflict.`n$relevant"
        } else {
            ExitOnError "Migration failed:`n$relevant"
        }
    }

    Write-Success "Migrations applied."

    # Final verification
    $finalOutput = dotnet ef migrations list --project "$($sqlProject.FullName)" --startup-project "$($startupProject.FullName)" -- --environment $Environment 2>&1
    $finalLines = $finalOutput | Where-Object { $_ -match '^\s*\d{14}_[A-Za-z0-9_]+' }
    $finalApplied = $finalLines | Where-Object { $_ -notmatch "\(Pending\)" } | ForEach-Object { $_.Trim() } | ForEach-Object { $_ -replace '^\d{14}_', '' }

    if ($MigrationName -and ($finalApplied -notcontains $MigrationName)) {
        ExitOnError "Verification failed. '$MigrationName' not found in final applied list."
    }

    if (-not [string]::IsNullOrWhiteSpace($MigrationName)) {
        Write-Success "Database updated to: $MigrationName"
    } else {
        Write-Success "Database fully migrated."
    }

    Write-Info "Optional: Add database seeding logic here if needed using application code or raw SQL."
}
catch {
    Write-Error "Unexpected error: $_"
    exit 1
}
finally {
    if ($existingEnv) {
        $env:ASPNETCORE_ENVIRONMENT = $existingEnv
    }
}
