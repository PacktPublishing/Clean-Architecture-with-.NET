param(
    [string]$Chapter = ""  # Optional chapter filter (e.g., "ch03" or "03"
)

# Disable built-in PowerShell progress indicators (prevents deletion spam)
$ProgressPreference = 'SilentlyContinue'

# Stop immediately on errors
$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================="
Write-Host " Validate Solutions for Clean-Architecture-with-.NET"
Write-Host "============================================="
Write-Host ""

# Expected repo root folder name
$expectedRoot = "Clean-Architecture-with-.NET"

# Determine repo root relative to script location
$baseDir = Resolve-Path "$PSScriptRoot\.."

# ======================================================
# SAFETY CHECK
# ======================================================
if ($baseDir -notmatch [regex]::Escape($expectedRoot)) {
    Write-Host "ERROR: Script is NOT running inside the expected repository ('$expectedRoot')." -ForegroundColor Red
    Write-Host "Actual path: $baseDir"
    Write-Host "Aborting to avoid accidental deletion." -ForegroundColor Red
    exit 1
}

Write-Host "Repository root validated: $baseDir" -ForegroundColor Green
Write-Host ""

# ======================================================
# LOCATE ALL SOLUTION FILES
# ======================================================
$solutionFiles = Get-ChildItem -Path $baseDir -Recurse | Where-Object { $_.Extension -in ".sln", ".slnx" }

if ($solutionFiles.Count -eq 0) {
    Write-Host "No solution files found." -ForegroundColor Yellow
    exit 1
}

# ======================================================
# OPTIONAL: FILTER BY CHAPTER
# ======================================================
if ($Chapter) {

    # Normalize: "10" --> "ch10"
    if ($Chapter -notmatch "^ch\d+$") {
        $Chapter = "ch$Chapter"
    }

    Write-Host "Targeting ONLY chapter: $Chapter" -ForegroundColor Cyan

    $solutionFiles = $solutionFiles | Where-Object {
        # Extract chapter folder from path
        $solutionDir = $_.DirectoryName
        $chapterFolder = Split-Path -Path (Split-Path -Path $solutionDir -Parent) -Leaf

        $chapterFolder -ieq $Chapter
    }

    if ($solutionFiles.Count -eq 0) {
        Write-Host "No solutions found for chapter '$Chapter'." -ForegroundColor Yellow
        exit 1
    }
}

# ======================================================
# PROCESS EACH SOLUTION
# ======================================================
foreach ($solutionFile in $solutionFiles) {

    $solutionDir = $solutionFile.DirectoryName
    $chapterFolder = Split-Path -Path (Split-Path -Path $solutionDir -Parent) -Leaf

    Write-Host ""
    Write-Host "============================================="
    Write-Host " CLEAN → BUILD → TEST for: $chapterFolder"
    Write-Host " Solution: $($solutionFile.Name)"
    Write-Host "============================================="
    Write-Host ""

    Push-Location $solutionDir

    # ------------------------------------------
    # CLEANUP ONLY THIS SOLUTION'S bin/obj
    # ------------------------------------------
    Write-Host "→ Cleaning bin/obj for $chapterFolder..."

    $localBinObj = Get-ChildItem -Path $solutionDir -Recurse -Directory -Include bin,obj

    if ($localBinObj.Count -gt 0) {
        Write-Host "   ... cleaning ..."
    }

    foreach ($folder in $localBinObj) {

        # Extra safety: ensure folder is truly inside repo root
        if (-not $folder.FullName.StartsWith($baseDir, [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        # Perform removal silently
        Remove-Item $folder.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }

    # ------------------------------------------
    # RESTORE → BUILD → TEST (with environment override)
    # ------------------------------------------
    try {
        Write-Host "→ Restoring..."
        dotnet restore $solutionFile.FullName

        Write-Host "→ Building..."
        dotnet build $solutionFile.FullName --no-restore

        # ============================
        # ENVIRONMENT OVERRIDE FOR TESTING
        # ============================

        $originalEnv = $Env:ASPNETCORE_ENVIRONMENT

        if ([string]::IsNullOrWhiteSpace($originalEnv)) {
            Write-Host "→ Current ASPNETCORE_ENVIRONMENT: <not set>" -ForegroundColor Yellow
        }
        else {
            Write-Host "→ Current ASPNETCORE_ENVIRONMENT: $originalEnv" -ForegroundColor Yellow
        }

        Write-Host "→ Setting ASPNETCORE_ENVIRONMENT=Development for test execution..." -ForegroundColor Cyan
        $Env:ASPNETCORE_ENVIRONMENT = "Development"

        # Run tests
        Write-Host "→ Running tests..."
        dotnet test $solutionFile.FullName --no-build

        # ============================
        # RESTORE ORIGINAL ENVIRONMENT
        # ============================
        if ([string]::IsNullOrWhiteSpace($originalEnv)) {
            Write-Host "→ Restoring ASPNETCORE_ENVIRONMENT: <removed>" -ForegroundColor Yellow
            Remove-Item Env:ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
        }
        else {
            Write-Host "→ Restoring ASPNETCORE_ENVIRONMENT: $originalEnv" -ForegroundColor Yellow
            $Env:ASPNETCORE_ENVIRONMENT = $originalEnv
        }

        Write-Host ""
        Write-Host "✔ SUCCESS: $chapterFolder" -ForegroundColor Green
    }
    catch {
        Write-Host ""
        Write-Host "✖ FAILURE in $chapterFolder" -ForegroundColor Red
        Write-Host $_

        # Attempt to restore env variable even after failure
        if ($originalEnv) {
            $Env:ASPNETCORE_ENVIRONMENT = $originalEnv
        }
        else {
            Remove-Item Env:ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
        }
    }
    finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "============================================="
Write-Host " Validation Complete"
Write-Host "============================================="
Write-Host ""
