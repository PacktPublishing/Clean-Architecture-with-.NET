# Define the base directory
$baseDir = "$($PSScriptRoot)\.."

# Get all solution files in the repository
$solutionFiles = Get-ChildItem -Path $baseDir -Filter "*.sln" -Recurse

# Loop through each solution file
foreach ($solutionFile in $solutionFiles) {
    # Get the chapter folder name (e.g., ch1, ch2, etc.)
    $chapterFolder = Split-Path -Path (Split-Path -Path $solutionFile.DirectoryName -Parent) -Leaf

    Write-Host "Building and testing solution for $chapterFolder..."

    # Navigate to the solution directory
    Set-Location -Path $solutionFile.DirectoryName

    # Build the solution
    dotnet build $solutionFile.FullName

    # Check if the build was successful
    if ($?) {
        Write-Host "Build successful for $chapterFolder."

        # Run tests
        dotnet test $solutionFile.FullName

        # Check if the tests were successful
        if ($?) {
            Write-Host "Tests passed for $chapterFolder."
        } else {
            Write-Host "Tests failed for $chapterFolder."
        }
    } else {
        Write-Host "Build failed for $chapterFolder."
    }

    # Return to the base directory
    Set-Location -Path $baseDir
}
