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
- Sets the connection string environment variable for SQL Server.
- Waits for SQL Server to become available.
- Calls `Start-Migrations.ps1` to apply migrations.

Run this script from the root directory of the repository:

```powershell
.\scripts\Initialize-Database.ps1
```

> Note: Each time these scripts are run, the database is deleted and recreated, making this approach well-suited for active development where frequent schema changes occur. This promotes agility by allowing developers to make and test breaking changes in the database schema without concerns about preserving existing data.

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

## About dotnet-ef and Configuration Setup

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