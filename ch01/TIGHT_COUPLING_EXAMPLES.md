# Tight Coupling Examples - Chapter 1

This document outlines the intentionally bad practices and tight coupling issues demonstrated in the TightlyCoupled.WebShop project to illustrate problems that Clean Architecture solves.

## 🚨 WARNING: These are BAD PRACTICES for Educational Purposes Only!

The code in this chapter demonstrates **what NOT to do** in a real application. These examples are intentionally poor to highlight the problems with tightly coupled architecture.

## Major Tight Coupling Issues Demonstrated

### 1. **Hard-Coded Dependencies and Configuration**

#### In `ShoppingCartController.cs`:
- Hard-coded file paths (`C:\Logs\WebShop\`, `C:\Data\inventory.xml`)
- Hard-coded database connection strings
- Hard-coded external service URLs
- Hard-coded email credentials and SMTP settings
- Hard-coded business rules (max 10 items, premium items restrictions)

```csharp
// BAD: Hard-coded paths
private const string LOG_FILE_PATH = @"C:\Logs\WebShop\cart_operations.log";
private const string INVENTORY_FILE_PATH = @"C:\Data\inventory.xml";
private const string EMAIL_PASSWORD = "hardcoded_password_123";
```

### 2. **Mixed Concerns and Single Responsibility Violations**

#### The `ShoppingCartController` does WAY too much:
- HTTP request handling (its actual job)
- Direct database access with raw SQL
- File system operations (logging, inventory management)
- Email sending
- External HTTP service calls
- Business rule validation
- Analytics data collection

#### The `OrderSummary` model contains:
- Data properties (correct)
- Business logic methods
- Data access code with SQL queries
- External service calls
- File system operations
- Email functionality

### 3. **Direct Database Access with Security Issues**

#### SQL Injection Vulnerabilities:
```csharp
// TERRIBLE: SQL injection vulnerability
var command = new SqlCommand($"SELECT * FROM CartItems WHERE UserId = '{userId}'", connection);
```

#### Bypassing Entity Framework:
- Controllers make direct SQL calls instead of using the repository pattern
- String concatenation for SQL queries
- No parameterized queries
- No transaction management

### 4. **Static Dependencies and Global State**

#### `GlobalUtilities` class demonstrates:
- Static methods that can't be mocked for testing
- Global state variables
- Hard-coded configuration access
- File system dependencies in static methods
- Direct database access in utility methods

```csharp
// BAD: Global state
public static string CurrentUser { get; set; } = "";
public static int ErrorCount { get; set; } = 0;
```

### 5. **File System Coupling**

#### Multiple file system dependencies:
- Custom logging to specific file paths
- XML-based inventory management
- Configuration files in hard-coded locations
- User greeting personalization files
- Order summary exports
- Email backups to files

### 6. **External Service Coupling**

#### Synchronous, blocking calls to external services:
- Tax calculation service
- Analytics service
- Recommendation service
- Shipping tracking service
- No circuit breakers or fallback mechanisms
- No timeout handling
- Services called directly from controllers and models

### 7. **Environment-Specific Code**

#### Platform-specific assumptions:
- Windows file paths (`C:\`)
- SQL Server-specific connection strings
- SMTP server configurations
- Directory creation in constructors

### 8. **Poor Error Handling**

#### Swallowing exceptions:
```csharp
// BAD: Hiding errors
catch
{
    // Swallow exceptions - another bad practice
}
```

#### No proper logging strategy
- Mix of `ILogger` and custom file logging
- Inconsistent error reporting
- No structured logging

### 9. **Presentation Logic in Wrong Places**

#### `UserStatsViewComponent`:
- Makes direct database calls
- Calls external services
- Contains business logic
- File system operations
- Updates global state

### 10. **Startup Configuration Issues**

#### `Program.cs` demonstrates:
- Environment-specific setup mixed with startup
- File system operations during application startup
- Hard-coded configuration validation
- Infrastructure concerns in startup code

## Testing Challenges Created

These tight coupling issues make the code extremely difficult to test:

1. **Cannot unit test** controllers without database, file system, and external services
2. **Cannot mock dependencies** due to static calls and hard-coded instantiation
3. **Cannot isolate components** because they directly depend on infrastructure
4. **Cannot run tests in parallel** due to shared file system resources
5. **Cannot test error scenarios** because exceptions are swallowed
6. **Cannot verify behavior** because of mixed concerns and side effects

## Deployment and Maintenance Issues

1. **Environment-specific** - won't work on Linux/Mac due to Windows paths
2. **Not scalable** - file-based inventory won't work with multiple instances
3. **Security vulnerabilities** - SQL injection, hard-coded passwords
4. **Performance problems** - synchronous external service calls
5. **Difficult to configure** - hard-coded values throughout
6. **Hard to debug** - inconsistent logging and error handling

## What Clean Architecture Solves

The subsequent chapters will show how Clean Architecture addresses these issues through:

- **Dependency Inversion** - depend on abstractions, not concrete implementations
- **Separation of Concerns** - each class has one responsibility
- **Proper layering** - UI → Application → Domain → Infrastructure
- **Dependency Injection** - externally provided dependencies
- **Configuration management** - externalized configuration
- **Repository pattern** - abstracted data access
- **Service abstractions** - mockable external service calls
- **Proper error handling** - structured error management
- **Testability** - isolated, unit-testable components

---

**Remember**: This chapter shows what NOT to do! The rest of the book demonstrates proper Clean Architecture principles.
