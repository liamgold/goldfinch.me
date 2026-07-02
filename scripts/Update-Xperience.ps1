# Update-Xperience.ps1
# Automates the process of updating Xperience by Kentico packages:
# - Kentico.Xperience.* NuGet packages via Directory.Packages.props
# - @kentico/* npm packages in Admin Client only
# - Toggles CI via the official CLI commands (--kxp-ci-enable/-disable/-status, requires
#   the project to start on Xperience 31.6.0+)
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
$script:resolvedVersions = @{}
$script:connectionString = $null

#region Helper Functions

function Write-Section {
    param([string]$Title)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Title -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
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

# The only remaining SQL access — used by Clear-CIFileMetadata, which has no CLI equivalent.
# All CI state management (status/enable/disable) goes through the official CLI commands.
function Execute-SqlNonQuery {
    param(
        [string]$ConnectionString,
        [string]$CommandText
    )

    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)

    try {
        $connection.Open()
        $command = New-Object System.Data.SqlClient.SqlCommand($CommandText, $connection)
        $rowsAffected = $command.ExecuteNonQuery()
        Write-Host "  SQL: $CommandText" -ForegroundColor Gray
        Write-Host "  Rows affected: $rowsAffected" -ForegroundColor Gray
        return $rowsAffected
    }
    catch {
        Write-ErrorMessage "SQL command failed: $_"
        throw
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

        $packagesToUpdate = $packagesProps.Project.ItemGroup.PackageVersion | Where-Object {
            $_.Include -like "Kentico.Xperience.*" -and $preservePackages -notcontains $_.Include
        }

        foreach ($package in $packagesToUpdate) {
            $packageName = $package.Include
            $oldVersion = $package.Version

            # Always query NuGet to resolve the actual version to use
            Write-Host "  Querying NuGet for $packageName..." -ForegroundColor Gray
            $nugetResponse = Invoke-RestMethod -Uri "https://api.nuget.org/v3-flatcontainer/$($packageName.ToLower())/index.json"

            if ($nugetResponse.versions.Count -eq 0) {
                Write-Warning "No versions found for $packageName on NuGet. Skipping."
                continue
            }

            $resolvedVersion = if ($Version -eq "latest") {
                $nugetResponse.versions[-1]
            } elseif ($nugetResponse.versions -contains $Version) {
                $Version
            } else {
                # Exact version not found — try preview variant, then fall back to latest
                $previewVariant = "$Version-preview"
                if ($nugetResponse.versions -contains $previewVariant) {
                    Write-Warning "$packageName $Version not found; using $previewVariant"
                    $previewVariant
                } else {
                    Write-Warning "$packageName $Version not found and no preview variant exists; using latest ($($nugetResponse.versions[-1]))"
                    $nugetResponse.versions[-1]
                }
            }

            $package.Version = $resolvedVersion
            $script:resolvedVersions[$packageName] = $resolvedVersion

            Write-Host "  Updated $packageName : $oldVersion -> $resolvedVersion" -ForegroundColor Green
            $updatedCount++
        }

        if ($updatedCount -gt 0) {
            # Use the requested target version for the summary/commit hint; fall back to
            # the resolved version only when the target was "latest" (no explicit request).
            $script:updatedVersion = if ($Version -eq "latest") {
                $script:resolvedVersions.Values | Select-Object -First 1
            } else {
                $Version
            }

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

# Reads the CI state via the official CLI (--kxp-ci-status, Xperience 31.6.0+).
# Must be called from the Web project directory. -AllowBuild lets the first CLI call
# of a run build the project; subsequent calls should rely on --no-build.
function Get-CIEnabledCli {
    param([switch]$AllowBuild)

    $runArgs = @("run")
    if (-not $AllowBuild) { $runArgs += "--no-build" }
    $runArgs += @("--", "--kxp-ci-status", "--format", "json")

    $output = & dotnet @runArgs 2>&1

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to read CI status via --kxp-ci-status (exit code $LASTEXITCODE). Output:`n$($output -join "`n")"
    }

    # dotnet run prints launch-settings info before the JSON result - find the JSON line
    $json = $output | Where-Object { "$_" -match '^\s*\{' } | Select-Object -Last 1

    if (-not $json) {
        throw "No JSON output found from --kxp-ci-status. Output:`n$($output -join "`n")"
    }

    return [bool](ConvertFrom-Json "$json").enabled
}

function Clear-CIFileMetadata {
    param([string]$ConnectionString)

    try {
        $rowsDeleted = Execute-SqlNonQuery -ConnectionString $ConnectionString -CommandText "DELETE [CI_FileMetadata]"
        Write-Success "CI_FileMetadata cleared ($rowsDeleted rows deleted)"
    }
    catch {
        Write-ErrorMessage "Failed to clear CI_FileMetadata: $_"
        throw
    }
}

# Runs BEFORE any package updates while the assembly version still matches the DB version.
# CI is always expected to be enabled for this project, so it is enabled unconditionally here
# (self-healing a DB left disabled by a previously interrupted run). Restores CI XML files into
# the DB so the migration runs on top of the correct object state, then disables CI so the
# package update and dotnet restore don't trigger CI side-effects.
# This is critical: skipping the restore causes --kxp-ci-store to serialise stale seed-DB state
# back into the CI XML files, rolling back fields on custom content types.
function Invoke-CIPreRestore {
    Write-Section "CI Pre-Restore (before package update)"

    if ($DryRun) {
        Write-Warning "DRY RUN: Would ensure CI is enabled, clear CI_FileMetadata, run --kxp-ci-restore, then disable CI"
        return
    }

    try {
        $script:connectionString = Get-ConnectionString
        Write-Success "Connection string retrieved"

        Push-Location "src/Goldfinch.Web"
        try {
            # CI is always enabled for this project. Enable it unconditionally so the pre-restore
            # syncs the DB from the repo CI XML before migration, even if a prior run left it off.
            # The status check is the first CLI call of the run, so it is allowed to build;
            # every dotnet run after it uses --no-build.
            Write-Host "Checking CI status (--kxp-ci-status)..." -ForegroundColor Gray
            if (Get-CIEnabledCli -AllowBuild) {
                Write-Host "CI is already enabled" -ForegroundColor Gray
            } else {
                Write-Host "Enabling CI (--kxp-ci-enable)..." -ForegroundColor Yellow
                & dotnet run --no-build -- --kxp-ci-enable

                if ($LASTEXITCODE -ne 0) {
                    throw "Failed to enable CI (exit code $LASTEXITCODE). Cannot proceed with update."
                }

                Write-Success "CI enabled"
            }

            Write-Host "`nClearing CI_FileMetadata before restore..." -ForegroundColor Yellow
            Clear-CIFileMetadata -ConnectionString $script:connectionString

            Write-Host "`nRestoring CI objects before package update..." -ForegroundColor Yellow
            & dotnet run --no-build --kxp-ci-restore

            if ($LASTEXITCODE -eq 0) {
                Write-Success "CI restore completed"
            } else {
                throw "CI restore failed with exit code $LASTEXITCODE"
            }

            Write-Host "`nDisabling CI before package update (--kxp-ci-disable)..." -ForegroundColor Yellow
            & dotnet run --no-build -- --kxp-ci-disable

            if ($LASTEXITCODE -ne 0) {
                throw "Failed to disable CI (exit code $LASTEXITCODE). Cannot proceed with update."
            }

            Write-Success "CI disabled"
        } finally {
            Pop-Location
        }
    } catch {
        Write-ErrorMessage "Failed during CI pre-restore: $_"
        throw
    }
}

#endregion

#region Kentico Update

function Invoke-KenticoUpdate {
    Write-Section "Running Kentico Database Update"

    if ($DryRun) {
        Write-Warning "DRY RUN: Would build, run --kxp-update, re-enable CI, and run --kxp-ci-store"
        return
    }

    try {
        Push-Location "src/Goldfinch.Web"
        try {
            Write-Host "Building with new packages..." -ForegroundColor Gray
            & dotnet build

            if ($LASTEXITCODE -ne 0) {
                throw "Build failed before database update"
            }

            Write-Success "Build completed"

            Write-Host "`nRunning 'dotnet run --kxp-update --skip-confirmation'..." -ForegroundColor Gray
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

        # Re-enable CI and store (CI is always enabled for this project)
        Push-Location "src/Goldfinch.Web"
        try {
            Write-Host "`nRe-enabling CI (--kxp-ci-enable)..." -ForegroundColor Yellow
            & dotnet run --no-build -- --kxp-ci-enable

            if ($LASTEXITCODE -ne 0) {
                Write-ErrorMessage "Failed to re-enable CI"
                return
            }

            Write-Success "CI re-enabled"

            Write-Host "`nStoring CI objects..." -ForegroundColor Gray
            & dotnet run --no-build --kxp-ci-store

            if ($LASTEXITCODE -eq 0) {
                Write-Success "CI objects stored successfully"
            } else {
                Write-ErrorMessage "Failed to store CI objects. Run 'dotnet run --kxp-ci-store' manually after fixing issues."
            }
        } finally {
            Pop-Location
        }

    } catch {
        Write-ErrorMessage "Failed to run Kentico update: $_"
    } finally {
        # Always leave CI enabled — runs whether the update succeeded or failed. If the update
        # broke badly enough that dotnet run cannot execute, this reports the failure so CI can
        # be re-enabled manually.
        Push-Location "src/Goldfinch.Web"
        try {
            if (-not (Get-CIEnabledCli)) {
                Write-Host "`nEnsuring CI is re-enabled (--kxp-ci-enable)..." -ForegroundColor Yellow
                & dotnet run --no-build -- --kxp-ci-enable

                if ($LASTEXITCODE -ne 0) {
                    throw "kxp-ci-enable failed with exit code $LASTEXITCODE"
                }

                Write-Success "CI re-enabled"
            }
        } catch {
            Write-ErrorMessage "Could not verify or restore CI state. Please check CI is enabled in the Kentico admin before proceeding."
        } finally {
            Pop-Location
        }
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

            # Highlight any packages that resolved to a different version than requested
            $overrides = $script:resolvedVersions.GetEnumerator() | Where-Object { $_.Value -ne $script:updatedVersion }
            if ($overrides) {
                Write-Host "`nNote: the following packages resolved to a different version:" -ForegroundColor Yellow
                foreach ($entry in $overrides) {
                    Write-Host "  $($entry.Key): $($entry.Value)" -ForegroundColor Yellow
                }
            }
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
    Invoke-CIPreRestore
    Update-DirectoryPackagesProps -Version $TargetVersion
    Update-NpmPackages
    Invoke-KenticoUpdate
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
