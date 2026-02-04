# Database Initialization Scripts

This folder contains PowerShell scripts to set up and initialize the database for the application. Follow these steps to ensure the database is properly configured with the required schema and initial data.

## Prerequisites

- **Docker**: Ensure Docker is installed and running on your machine.
- **.NET Core**: The `.config/dotnet-tools.json` file is included to manage `dotnet-ef`, the tool used to apply migrations. Run `dotnet tool restore` if this tool is not installed locally.
- **PowerShell Core**: These scripts require PowerShell Core to execute. Follow the instructions below to install PowerShell Core if it is not already available on your system.

### Installing PowerShell

If PowerShell is not already installed on your system, you can install it by following the official Microsoft documentation for your operating system:

- **Windows**: [Installing PowerShell on Windows](https://learn.microsoft.com/powershell/scripting/install/installing-powershell-on-windows)
- **Linux**: [Installing PowerShell on Linux](https://learn.microsoft.com/powershell/scripting/install/installing-powershell-on-linux)
- **macOS**: [Installing PowerShell on macOS](https://learn.microsoft.com/powershell/scripting/install/installing-powershell-on-macos)

After installation, ensure that PowerShell is accessible from your terminal by running:

```sh
pwsh --version
```

---

## Scripts Overview

### 1. Initialize-Database.ps1

This script:

- Removes any existing SQL Server Docker container with the name `odyssey_sqlserver`.
- Runs a new SQL Server container in Docker on port 4000.
- Sets the connection string for SQL Server as a User Secret on the `Infrastructure` project.
- Waits for SQL Server to become available.
- Calls `Start-Migrations.ps1` to apply migrations.

Run this script from the **solution root for the chapter** or **solution root for your project if copied**:

```powershell
.\scripts\Initialize-Database.ps1
```

> ⚠️ Warning: Each chapter contains its own solution and Infrastructure project. Running scripts from the repository root may cause the wrong project to be resolved.

> Note: Each time these scripts are run, the database is deleted and recreated, making this approach well-suited for active development where frequent schema changes occur. This promotes agility by allowing developers to make and test breaking changes in the database schema without concerns about preserving existing data.

#### ⚠️ Important Note on Connection Strings in This Repository

The database connection string used by these scripts is **intentionally included in source control** as part of this book’s learning materials.

This is done **solely for demonstration purposes** so that:

- Readers can run the project immediately without additional setup
- Database initialization remains fully automated and repeatable
- Until Azure Key Vault is introduced, chapters can focus on Clean Architecture concepts instead of secret management

**You should NOT do this in your own projects.**

In real-world applications:

- Connection strings must **never** be checked into source control
- Secrets should be supplied via environment variables, user secrets, or a secure secrets manager (such as Azure Key Vault)
- Initialization scripts should accept configuration externally, not embed credentials

This repository trades strict security practices for approachability and clarity in a controlled, local-only learning environment. Treat it as instructional scaffolding—not a production template.

---

### 2. Start-Migrations.ps1

This script:

- Locates the `Infrastructure.csproj` and `Presentation.BSA.csproj` files in the repository.
- Cleans, restores, and builds the project to ensure up-to-date binaries.
- Applies the latest database migrations using the `dotnet-ef` tool.

Run this script automatically by executing `Initialize-Database.ps1`, or individually if required:

```powershell
.\scripts\Start-Migrations.ps1
```

### Notes

- **Port**: The SQL Server container runs on port 4000 to avoid conflicts. Adjust as needed.
- **Authentication**: After database recreation, previously saved user data will be lost, potentially leading to authentication issues. To prevent errors, log out or clear browser cookies for the application domain.
- **Migrations**: Migration files are stored in the Infrastructure project and should only be checked into source control once the database schema stabilizes.

_For further details on adding and managing migrations, see the official [Entity Framework Core documentation](https://learn.microsoft.com/ef/core/managing-schemas/migrations)._

---

## Reference: dotnet-ef and Tooling Configuration

This enhanced script is designed for real-world development and deployment pipelines. It supports:

- Creating new EF Core migrations with `-MigrationName` and `-AddMigration`
- Applying migrations to a target environment using `--environment`
- Resetting the database schema safely via `-ResetDatabase`
- Verifying migration state and status
- Ensuring idempotent execution across CI or local environments
- Optional confirmation prompts to avoid accidental destructive actions

**Example**: Add a new migration for local development and apply it to your local development database:

```powershell
.\scripts\Start-EFMigration.ps1 `
    -Environment Development `
    -MigrationName "AddUserRoles" `
    -AddMigration `
    -ConnectionString "Server=localhost;Database=MyApp.Dev;Integrated Security=true;"
```

> ⚠️ Resetting the database is a destructive action. The script will prompt for confirmation unless run in a CI/CD pipeline with the appropriate override flag.

#### Running in CI/CD Pipelines Without User Prompts

When used in automation scenarios like GitHub Actions or Azure Pipelines, you can override the interactive confirmation required for resetting databases by passing the `-Confirm:$true` flag.

```powershell
# Example usage in CI/CD:
.\scripts\Start-EFMigration.ps1 `
    -Environment dev `
    -ResetDatabase `
    -MigrationName "InitialCreate" `
    -ConnectionString "Server=localhost;Database=MyApp.Dev;Trusted_Connection=True;" `
    -Confirm:$true
```

> This is required when running in non-interactive environments where user input cannot be provided.

Make sure your environment is one of the safe reset environments (dev, qa, ppe, etc.) or the reset will be blocked automatically.

This script complements `Start-Migrations.ps1` by offering greater flexibility and diagnostics when managing real environments like QA, PPE, or production (with safety checks). It is especially useful for team-based development workflows and DevOps automation.

---


To ensure the database schema is up-to-date and populated with necessary tables and initial data, we use the `dotnet-ef` tool to apply migrations. This tool streamlines the database initialization process, making it efficient and repeatable across different environments.

### Configuring dotnet-ef for Local Development and CI/CD

Before using dotnet-ef, create a .config folder in the root of your repository with a dotnet-tools.json file. This configuration file ensures that the necessary tools are available, and the correct tool versions are used in both local development and CI/CD pipelines.

Here is an example `dotnet-tools.json` configuration for `dotnet-ef`:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "dotnet-ef": {
      "version": "10.0.0",
      "commands": ["dotnet-ef"]
    }
  }
}
```

_To learn more about configuring .NET local tools, refer to the [official documentation](https://learn.microsoft.com/dotnet/core/tools/local-tools-how-to-use)._

Once set up, run `dotnet tool restore` to install `dotnet-ef` locally. After that, the tool will be ready to apply migrations using the scripts in this folder. The `Start-Migrations.ps1` already handles the tool restoration process, so you can run it directly without manual intervention.

---

### Naming Migrations

When creating a migration, the dotnet-ef tool requires a name for the migration. The initial migration is named `InitialCreate`, but this name is arbitrary and can be customized as preferred. Consistent naming practices help identify migration purposes, such as `AddUserTable` or `ModifyProductSchema`.