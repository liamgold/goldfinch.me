# Update-Xperience.ps1
# Automates the process of updating Xperience by Kentico packages:
# - Kentico.Xperience.* NuGet packages via Directory.Packages.props
# - @kentico/* npm packages in Admin Client only
# - Handles CMSEnableCI toggle during update
# - Runs CI store after update

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$TargetVersion = "latest",

    [Parameter(Mandatory=$false)]
    [switch]$SkipNpmUpdate,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [switch]$SkipPrerequisites
)

$ErrorActionPreference = "Stop"
$script:hasErrors = $false
$script:updatedVersion = $null
$script:ciWasEnabled = $false

#region Helper Functions

function Write-Section {
    param([string]$Title)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Title -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
    $script:hasErrors = $true
}

function Get-ConnectionString {
    $connectionStringsPath = "src/Goldfinch.Web/connectionstrings.json"

    if (-not (Test-Path $connectionStringsPath)) {
        throw "Connection strings file not found: $connectionStringsPath"
    }

    $connectionStringsJson = Get-Content $connectionStringsPath -Raw | ConvertFrom-Json
    return $connectionStringsJson.ConnectionStrings.CMSConnectionString
}

function Execute-SqlCommand {
    param(
        [string]$ConnectionString,
        [string]$CommandText
    )

    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)

    try {
        $connection.Open()
        $command = New-Object System.Data.SqlClient.SqlCommand($CommandText, $connection)
        $transaction = $connection.BeginTransaction()
        $command.Transaction = $transaction

        try {
            $rowsAffected = $command.ExecuteNonQuery()
            Write-Host "  SQL Command: $CommandText" -ForegroundColor Gray
            Write-Host "  Rows affected: $rowsAffected" -ForegroundColor Gray

            if ($rowsAffected -ne 1) {
                throw "Expected 1 row to be affected, but $rowsAffected rows were affected"
            }

            $transaction.Commit()
            return $true
        }
        catch {
            $transaction.Rollback()
            Write-ErrorMessage "SQL command failed: $($_.Exception.Message)"
            return $false
        }
    }
    finally {
        $connection.Close()
    }
}

function Execute-SqlQuery {
    param(
        [string]$ConnectionString,
        [string]$CommandText
    )

    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)

    try {
        $connection.Open()
        $command = New-Object System.Data.SqlClient.SqlCommand($CommandText, $connection)
        $dataAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
        $dataset = New-Object System.Data.Dataset
        $dataAdapter.Fill($dataset)

        return $dataset
    }
    finally {
        $connection.Close()
    }
}

#endregion

#region Prerequisite Checks

function Test-Prerequisites {
    Write-Section "Validating Prerequisites"

    if ($SkipPrerequisites) {
        Write-Warning "Skipping prerequisite checks"
        return $true
    }

    $isValid = $true

    # Check 1: Directory.Packages.props exists
    Write-Host "Checking for Directory.Packages.props..." -ForegroundColor Gray
    if (-not (Test-Path "Directory.Packages.props")) {
        Write-ErrorMessage "Directory.Packages.props not found. This script requires central package management."
        $isValid = $false
    } else {
        Write-Success "Directory.Packages.props found"
    }

    # Check 2: Clean git state
    Write-Host "Checking git status..." -ForegroundColor Gray
    $gitStatus = & git status --porcelain

    if ($gitStatus) {
        Write-ErrorMessage "Git working directory has uncommitted changes. Please commit or stash changes before updating."
        Write-Host "`nUncommitted changes:" -ForegroundColor Yellow
        $gitStatus | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
        $isValid = $false
    } else {
        Write-Success "Git working directory is clean"
    }

    # Check 3: Not on main branch
    Write-Host "Checking current branch..." -ForegroundColor Gray
    $currentBranch = & git rev-parse --abbrev-ref HEAD

    if ($currentBranch -eq "main" -or $currentBranch -eq "master") {
        Write-ErrorMessage "Cannot update on main/master branch. Please create a feature branch first."
        Write-Host "`nSuggested command: git checkout -b update/xperience-$(Get-Date -Format 'yyyy-MM-dd')" -ForegroundColor Yellow
        $isValid = $false
    } else {
        Write-Success "Working on branch: $currentBranch"
    }

    if (-not $isValid) {
        Write-Host "`nPrerequisite checks failed. Cannot proceed with update." -ForegroundColor Red
        exit 1
    }

    return $isValid
}

#endregion

#region Package Management

function Get-KenticoPackages {
    Write-Section "Checking Current Kentico Packages"

    if (-not (Test-Path "Directory.Packages.props")) {
        Write-ErrorMessage "Directory.Packages.props not found"
        return @{}
    }

    [xml]$packagesProps = Get-Content "Directory.Packages.props"
    $kenticoPackages = @{}

    $packagesProps.Project.ItemGroup.PackageVersion | Where-Object { $_.Include -like "Kentico.Xperience.*" } | ForEach-Object {
        $kenticoPackages[$_.Include] = $_.Version
    }

    Write-Host "Current Kentico packages:" -ForegroundColor White
    foreach ($package in $kenticoPackages.Keys | Sort-Object) {
        Write-Host "  $package : $($kenticoPackages[$package])" -ForegroundColor Gray
    }

    return $kenticoPackages
}

function Update-DirectoryPackagesProps {
    param(
        [string]$Version
    )

    Write-Section "Updating Directory.Packages.props"

    if ($DryRun) {
        Write-Warning "DRY RUN: Would update Kentico.Xperience.* packages to version $Version"
        Write-Host "  Preserving packages with independent versioning:" -ForegroundColor Yellow
        Write-Host "    - Kentico.Xperience.MiniProfiler (has separate version scheme)" -ForegroundColor Yellow
        return
    }

    try {
        [xml]$packagesProps = Get-Content "Directory.Packages.props"

        # Packages to preserve (have independent versioning)
        $preservePackages = @(
            "Kentico.Xperience.MiniProfiler"
        )

        $updatedCount = 0

        $packagesProps.Project.ItemGroup.PackageVersion | Where-Object {
            $_.Include -like "Kentico.Xperience.*" -and $preservePackages -notcontains $_.Include
        } | ForEach-Object {
            $packageName = $_.Include
            $oldVersion = $_.Version

            if ($Version -eq "latest") {
                # Query NuGet for latest version
                Write-Host "  Querying NuGet for latest version of $packageName..." -ForegroundColor Gray
                $nugetResponse = Invoke-RestMethod -Uri "https://api.nuget.org/v3-flatcontainer/$($packageName.ToLower())/index.json"

                if ($nugetResponse.versions.Count -gt 0) {
                    $latestVersion = $nugetResponse.versions[-1]
                    $_.Version = $latestVersion
                    $script:updatedVersion = $latestVersion
                } else {
                    Write-Warning "No versions found for $packageName on NuGet. Skipping update for this package."
                    return
                }
            } else {
                $_.Version = $Version
                $script:updatedVersion = $Version
            }

            Write-Host "  Updated $packageName : $oldVersion -> $($_.Version)" -ForegroundColor Green
            $updatedCount++
        }

        if ($updatedCount -gt 0) {
            $packagesProps.Save((Resolve-Path "Directory.Packages.props"))
            Write-Success "Updated $updatedCount Kentico packages in Directory.Packages.props"

            # Restore packages
            Write-Host "`nRunning dotnet restore..." -ForegroundColor Gray
            & dotnet restore

            if ($LASTEXITCODE -eq 0) {
                Write-Success "Package restore completed"
            } else {
                Write-ErrorMessage "Package restore failed"
            }
        } else {
            Write-Warning "No packages were updated"
        }

    } catch {
        Write-ErrorMessage "Failed to update Directory.Packages.props: $_"
    }
}

function Update-NpmPackages {
    Write-Section "Updating Kentico npm Packages (Admin Client)"

    if ($SkipNpmUpdate) {
        Write-Warning "Skipping npm updates"
        return
    }

    $adminClientPath = "src/Goldfinch.Admin/Client"

    if (-not (Test-Path "$adminClientPath/package.json")) {
        Write-Warning "No package.json found in $adminClientPath"
        return
    }

    Push-Location $adminClientPath
    try {
        if ($DryRun) {
            Write-Warning "DRY RUN: Would update @kentico/* packages and run npm audit fix"
            Write-Host "  Checking outdated @kentico/* packages..." -ForegroundColor Gray

            $outdatedOutput = & npm outdated --json
            if ($outdatedOutput) {
                try {
                    $outdated = $outdatedOutput | ConvertFrom-Json
                    $outdated.PSObject.Properties | Where-Object { $_.Name -like "@kentico/*" } | ForEach-Object {
                        Write-Host "    $($_.Name): $($_.Value.current) -> $($_.Value.latest)" -ForegroundColor Yellow
                    }
                } catch {
                    Write-Warning "Could not parse npm outdated output as JSON. Output was:`n$outdatedOutput"
                }
            } else {
                Write-Host "    No outdated @kentico/* packages found." -ForegroundColor Gray
            }
        } else {
            Write-Host "  Getting list of @kentico/* packages..." -ForegroundColor Gray
            $packageJson = Get-Content "package.json" -Raw | ConvertFrom-Json

            $kenticoPackages = @()
            if ($packageJson.dependencies) {
                $kenticoPackages += $packageJson.dependencies.PSObject.Properties | Where-Object { $_.Name -like "@kentico/*" } | Select-Object -ExpandProperty Name
            }
            if ($packageJson.devDependencies) {
                $kenticoPackages += $packageJson.devDependencies.PSObject.Properties | Where-Object { $_.Name -like "@kentico/*" } | Select-Object -ExpandProperty Name
            }

            if ($kenticoPackages.Count -eq 0) {
                Write-Warning "No @kentico/* packages found in package.json"
            } else {
                Write-Host "  Found $($kenticoPackages.Count) @kentico/* packages" -ForegroundColor Gray

                foreach ($package in $kenticoPackages) {
                    Write-Host "  Updating $package..." -ForegroundColor Gray
                    & npm install "$package@latest"
                }

                Write-Success "Updated Kentico npm packages"

                # Run npm audit fix
                Write-Host "`n  Running npm audit fix..." -ForegroundColor Gray
                & npm audit fix

                Write-Success "npm audit fix completed"
            }
        }
    } catch {
        Write-ErrorMessage "Failed to update Kentico npm packages: $_"
    } finally {
        Pop-Location
    }
}

#endregion

#region CI Management

function Get-CIEnabled {
    param([string]$ConnectionString)

    try {
        $query = "SELECT KeyValue FROM CMS_SettingsKey WHERE KeyName = N'CMSEnableCI'"
        $dataset = Execute-SqlQuery -ConnectionString $ConnectionString -CommandText $query

        $value = $dataset.Tables[0].Rows[0][0]
        return ($value -eq 'True')
    }
    catch {
        Write-ErrorMessage "Failed to check CI status: $_"
        return $false
    }
}

function Set-CIEnabled {
    param(
        [string]$ConnectionString,
        [bool]$Enabled
    )

    $value = if ($Enabled) { "True" } else { "False" }
    $command = "UPDATE CMS_SettingsKey SET KeyValue = N'$value' WHERE KeyName = N'CMSEnableCI'"

    return Execute-SqlCommand -ConnectionString $ConnectionString -CommandText $command
}

#endregion

#region Kentico Update

function Invoke-KenticoUpdate {
    Write-Section "Running Kentico Database Update"

    if ($DryRun) {
        Write-Warning "DRY RUN: Would disable CI, run 'dotnet run --kxp-update --skip-confirmation', re-enable CI, and run CI store"
        return
    }

    try {
        $connectionString = Get-ConnectionString
        Write-Success "Connection string retrieved"

        # Check if CI is enabled
        Write-Host "Checking CI status..." -ForegroundColor Gray
        $script:ciWasEnabled = Get-CIEnabled -ConnectionString $connectionString

        if ($script:ciWasEnabled) {
            Write-Host "CI is currently enabled. Disabling CI before update..." -ForegroundColor Yellow
            $success = Set-CIEnabled -ConnectionString $connectionString -Enabled $false

            if (-not $success) {
                Write-ErrorMessage "Failed to disable CI. Cannot proceed with update."
                return
            }

            Write-Success "CI disabled"
        } else {
            Write-Host "CI is not enabled" -ForegroundColor Gray
        }

        # Run the update
        Push-Location "src/Goldfinch.Web"
        try {
            Write-Host "Running 'dotnet run --kxp-update --skip-confirmation'..." -ForegroundColor Gray
            & dotnet run --no-build --kxp-update --skip-confirmation

            if ($LASTEXITCODE -eq 0) {
                Write-Success "Kentico update completed successfully"
            } else {
                Write-ErrorMessage "Kentico update failed with exit code $LASTEXITCODE"
                return
            }
        } finally {
            Pop-Location
        }

        # Re-enable CI if it was enabled
        if ($script:ciWasEnabled) {
            Write-Host "`nRe-enabling CI..." -ForegroundColor Yellow
            $success = Set-CIEnabled -ConnectionString $connectionString -Enabled $true

            if (-not $success) {
                Write-ErrorMessage "Failed to re-enable CI"
                return
            }

            Write-Success "CI re-enabled"

            # Store CI objects
            Write-Host "`nStoring CI objects..." -ForegroundColor Gray
            Push-Location "src/Goldfinch.Web"
            try {
                & dotnet run --no-build --kxp-ci-store

                if ($LASTEXITCODE -eq 0) {
                    Write-Success "CI objects stored successfully"
                } else {
                    Write-ErrorMessage "Failed to store CI objects. Run 'dotnet run --kxp-ci-store' manually after fixing issues."
                }
            } finally {
                Pop-Location
            }
        }

    } catch {
        Write-ErrorMessage "Failed to run Kentico update: $_"

        # Attempt to re-enable CI if it was disabled
        if ($script:ciWasEnabled) {
            Write-Host "`nAttempting to re-enable CI after error..." -ForegroundColor Yellow
            try {
                Set-CIEnabled -ConnectionString $connectionString -Enabled $true | Out-Null
                Write-Success "CI re-enabled after error"
            } catch {
                Write-ErrorMessage "Failed to re-enable CI after error. Please re-enable manually in the admin."
            }
        }
    }
}

#endregion

#region Build

function Invoke-Build {
    Write-Section "Building Solution"

    if ($DryRun) {
        Write-Warning "DRY RUN: Would run 'dotnet build'"
        return
    }

    try {
        Write-Host "Running 'dotnet build'..." -ForegroundColor Gray
        & dotnet build

        if ($LASTEXITCODE -eq 0) {
            Write-Success "Build completed successfully"
        } else {
            Write-ErrorMessage "Build failed with exit code $LASTEXITCODE"
        }
    } catch {
        Write-ErrorMessage "Failed to build solution: $_"
    }
}

#endregion

#region Verification

function Get-CIRepositoryChanges {
    Write-Section "Checking CI Repository Changes"

    try {
        $ciPath = "src/Goldfinch.Web/App_Data/CIRepository"

        if (-not (Test-Path $ciPath)) {
            Write-Warning "CI Repository path not found: $ciPath"
            return
        }

        $gitStatus = & git status --porcelain $ciPath

        if ($gitStatus) {
            Write-Warning "CI Repository has changes:"
            $gitStatus | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
            Write-Host "`nThese changes should be committed as part of the update." -ForegroundColor Yellow
        } else {
            Write-Success "No CI Repository changes detected"
        }
    } catch {
        Write-ErrorMessage "Failed to check CI Repository changes: $_"
    }
}

#endregion

#region Summary

function Show-Summary {
    Write-Section "Update Summary"

    if ($DryRun) {
        Write-Host "DRY RUN COMPLETED - No changes were made" -ForegroundColor Cyan
        Write-Host "Run without -DryRun to apply changes" -ForegroundColor Gray
    } elseif ($script:hasErrors) {
        Write-Host "UPDATE COMPLETED WITH ERRORS" -ForegroundColor Red
        Write-Host "Please review the errors above and fix any issues" -ForegroundColor Yellow
        Write-Host "You may need to manually resolve breaking changes" -ForegroundColor Yellow
    } else {
        Write-Host "UPDATE COMPLETED SUCCESSFULLY" -ForegroundColor Green

        if ($script:updatedVersion) {
            Write-Host "`nUpdated to Xperience version: $script:updatedVersion" -ForegroundColor Cyan
        }

        Write-Host "`nNext steps:" -ForegroundColor White
        Write-Host "  1. Review changes with 'git status'" -ForegroundColor Gray
        Write-Host "  2. Test the application locally (https://localhost:52623)" -ForegroundColor Gray
        Write-Host ('  3. Commit changes using: git commit -m "build(sln): update to Xperience v' + $script:updatedVersion + '"') -ForegroundColor Gray
        Write-Host "  4. Push and create a pull request" -ForegroundColor Gray
    }
}

#endregion

#region Main Execution

try {
    Write-Host "`nXperience by Kentico Update Script" -ForegroundColor Cyan
    Write-Host "Target Version: $TargetVersion" -ForegroundColor Gray

    if ($DryRun) {
        Write-Host "Mode: DRY RUN (no changes will be made)" -ForegroundColor Yellow
    }

    Test-Prerequisites
    Get-KenticoPackages
    Update-DirectoryPackagesProps -Version $TargetVersion
    Update-NpmPackages
    Invoke-KenticoUpdate
    Invoke-Build
    Get-CIRepositoryChanges
    Show-Summary

} catch {
    Write-ErrorMessage "Unexpected error: $_"
    exit 1
}

if ($script:hasErrors) {
    exit 1
}

#endregion
