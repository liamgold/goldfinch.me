---
name: update-xperience
description: Update Xperience by Kentico packages (NuGet + npm) including database migrations and CI store
disable-model-invocation: true
---

# Update Xperience by Kentico

You are tasked with updating this Xperience by Kentico project to the latest version (or a specific target version if specified by the user).

## Prerequisites

Before starting, the PowerShell script will automatically validate:
- **Clean git state**: No uncommitted changes in the working directory
- **Feature branch**: Not running on main/master branch

If these checks fail, the script will exit with clear error messages. If not on a feature branch, create one first:
```powershell
git checkout -b "update/xperience-$(Get-Date -Format 'yyyy-MM-dd')"
```

## Process Overview

Follow these steps systematically to ensure a complete and correct update:

### 1. Run the Update Script

Execute the `Update-Xperience.ps1` PowerShell script to automate mechanical tasks. **Run from the repo root**:

```powershell
# For latest version
.\scripts\Update-Xperience.ps1

# For specific version
.\scripts\Update-Xperience.ps1 -TargetVersion "31.0.0"

# Dry run to preview changes
.\scripts\Update-Xperience.ps1 -DryRun

# Skip npm updates (if needed)
.\scripts\Update-Xperience.ps1 -SkipNpmUpdate

# Skip prerequisite checks (not recommended)
.\scripts\Update-Xperience.ps1 -SkipPrerequisites
```

The script will:
- Validate prerequisites (Directory.Packages.props exists, clean git state, feature branch)
- Check current Kentico package versions in Directory.Packages.props
- Ensure CI is enabled via the official CLI (`--kxp-ci-status` / `--kxp-ci-enable`, self-healing a DB left disabled by a previously interrupted run), clear `CI_FileMetadata`, and run `--kxp-ci-restore` to apply CI XML files to the database **before any package updates** (assembly and DB are still on the same version at this point)
- Disable CI via `--kxp-ci-disable` for the duration of the package update
- Update Kentico.Xperience.* packages in Directory.Packages.props and run `dotnet restore` (preserves Kentico.Xperience.MiniProfiler which has independent versioning; resolves each package's version individually against NuGet with preview fallback)
- Update @kentico/* npm packages in admin client only (not custom frontend dependencies)
- Run `npm audit fix` for security vulnerabilities
- Build the solution with the new packages
- Run `dotnet run --kxp-update --skip-confirmation` for database migrations
- Re-enable CI via `--kxp-ci-enable` — the script **always leaves CI enabled** at the end, even if the update fails
- Run `dotnet run --kxp-ci-store` to serialize updated objects
- Check for CI repository changes

### 2. Review Release Notes and Flag Manual Steps

Use the Kentico docs MCP server to research the release notes for the target version. Search for:
- "Xperience by Kentico X.X.X release notes"
- "Xperience by Kentico X.X.X upgrade"
- "Xperience by Kentico X.X.X migration"

For each relevant release between the old and new version, review:
- **Breaking changes**: Deprecated/removed APIs, method signature changes, renamed types, namespace moves
- **Required code changes**: New patterns the project must adopt
- **Configuration changes**: New or changed `appsettings.json` keys
- **Manual upgrade steps**: Steps that `--kxp-update` does NOT handle automatically — these are often listed under "Manual steps", "Refresh notes", or "Upgrade guide" sections in the docs

**Important**: Explicitly call out any manual steps to the user before proceeding. For example:
- Database schema changes that need manual SQL
- New required service registrations in `Program.cs`/`ServiceConfiguration.cs`
- Content type or channel configuration changes required in the admin UI
- New required NuGet packages to add
- CI repository files that need manual editing

Present any found manual steps as a checklist for the user to action.

### 3. Fix Compilation Errors

If the build fails:
- Read the error messages carefully
- Use the Kentico docs MCP server to understand new APIs
- Update code to use new patterns/APIs
- Pay attention to:
  - Namespace changes
  - Method signature changes
  - Removed or renamed types
  - New required parameters

**Common issue — NU1605 package downgrade errors**: After a Kentico update, the build may fail with errors like:
```
error NU1605: Detected package downgrade: SomePackage from X.Y.Z to A.B.C
  Goldfinch.Web -> Kentico.Xperience.WebApp 31.5.1 -> SomePackage (>= X.Y.Z)
  Goldfinch.Web -> SomePackage (>= A.B.C)
```
This means a package is pinned in `Directory.Packages.props` at a lower version than Kentico now requires. Check whether the project actually uses that package's API in code (`Grep` for its namespace). If not used directly, **remove it from both `Directory.Packages.props` and all `.csproj` files** — it's a transitive dependency that Kentico should own. If it is used directly, bump the version in `Directory.Packages.props` to the required minimum.

### 4. Handle CI Store Asset Failures

The CI store may fail with an error like:
```
Could not find a part of the path '...\assets\contentitems\...\filename.svg'
```
This means a content item has an asset (image/file) that exists in the database but not in the local `assets/` folder — typically because an asset was uploaded to the CMS but `--kxp-ci-store` was never run and committed afterwards.

**Understanding the assets/CI relationship:**
- `assets/contentitems/` is the runtime folder for uploaded files — gitignored
- The CI repository (`App_Data/CIRepository`) stores the binary files alongside their XML metadata — tracked in git
- `--kxp-ci-restore` populates `assets/` from the CI repository; `--kxp-ci-store` reads from `assets/` to serialize into the CI repository

**Resolution options (in order of preference):**
1. **Use the real file**: Find the original file (check Downloads/recent files), copy it to the exact path shown in the error, then re-run `--kxp-ci-store`.
2. **Use an existing similar file as a placeholder**: Copy a suitable file from elsewhere in the CI repository to the missing path, re-run `--kxp-ci-store`. The correct file can be committed separately later.
3. **Don't let it muddy the update**: If the failure causes the CI store to wipe existing CI image entries, restore those entries with `git checkout HEAD -- <path>` to keep the update PR clean. Address the missing asset separately.

The root cause is always the same: an asset was uploaded without committing the CI store output. After fixing, always run `--kxp-ci-store` and commit after uploading new assets.

### 5. Manual Testing Checklist

After the build succeeds, recommend the user test these critical areas locally:
- Homepage loads correctly
- Blog listing page works
- Blog detail pages render with images
- Navigation and header work
- Admin interface is accessible (`https://localhost:52623/admin`)
- Page builder widgets function correctly

### 6. Review CI Repository Changes

The `App_Data/CIRepository/` folder contains serialized Kentico objects that must be committed:
- Check `git status src/Goldfinch.Web/App_Data/CIRepository/`
- Review what changed
- These changes are expected and should be committed with the update

### 7. Commit and Create PR

When everything is working:
- Stage all changes: Directory.Packages.props, package-lock.json files, CI repository files, and any code fixes
- Create a commit using the conventional commit format:
  ```
  build(sln): update to Xperience vX.X.X

  - Updated Kentico.Xperience.* packages in Directory.Packages.props
  - Updated @kentico/* npm packages in admin client
  - Applied database migrations via --kxp-update
  - Serialized CI objects with --kxp-ci-store
  [If applicable: - Fixed breaking changes: <description>]

  🤖 Generated with Claude Code
  ```
- Create a pull request with a summary of changes and testing performed

## Important Notes

- **Central Package Management**: Uses Directory.Packages.props for all NuGet package versions
- **Preserved Packages**: Kentico.Xperience.MiniProfiler has independent versioning and won't be automatically updated
- **CI Always Enabled**: CI is always expected to be enabled for this project. The script enables CI unconditionally at the start (self-healing a DB left disabled by a previously interrupted run) and always leaves it enabled at the end — it never skips the CI steps based on the current `CMSEnableCI` value
- **CI Restore First**: The script clears `CI_FileMetadata` and runs `--kxp-ci-restore` **before the NuGet package update**. This is critical — the assembly and DB must be on the same version for CI restore to run, and it must happen before the migration so `--kxp-ci-store` serialises the CI XML state, not stale seed-DB state (which would roll back fields on custom content types)
- **CI Toggle**: All CI state management (status/enable/disable) uses the official CLI commands (`--kxp-ci-enable` / `--kxp-ci-disable` / `--kxp-ci-status`, available from Xperience 31.6.0) — no direct SQL. The always-leave-CI-enabled safety net also uses the CLI; if the update broke badly enough that `dotnet run` cannot execute, the script reports it so CI can be re-enabled manually. The only remaining SQL is the `CI_FileMetadata` clear, which has no CLI equivalent
- **CI Repository**: Changes to `App_Data/CIRepository/` are expected and necessary after CI store operation
- **Nullable Reference Types**: This project has nullable reference types enabled - ensure any code changes respect this
- **Minimal Changes**: Keep changes focused on Kentico packages only - don't bump unrelated dependencies
- **Documentation**: Always use the Kentico docs MCP server for authoritative information about APIs and changes
- **Lucene Package**: This project uses `Kentico.Xperience.Lucene` — the `kenticolucene.luceneindexassemblyversionitem` object type is excluded under `<ExcludedObjectTypes>` in `src/Goldfinch.Web/App_Data/CIRepository/repository.config` because it is per-environment runtime state (which assembly version built the local index) and must not flow through CI. If a file for it ever appears under `App_Data/CIRepository/@global/` during an update, check the exclusion is still in place and re-run `--kxp-ci-store`

## Project-Specific Context

- **Main Projects**: Goldfinch.Core, Goldfinch.Admin, Goldfinch.Web
- **Admin UI**: React/Vite app in `src/Goldfinch.Admin/Client` (contains @kentico/* packages)
- **Web Client**: Vite bundle in `src/Goldfinch.Web/wwwroot/sitefiles` (custom frontend, no Kentico packages)
- **Local URLs**: https://localhost:52623 (web), https://localhost:52623/admin (admin: admin/Test123!)

## Troubleshooting

If you encounter issues:
1. Check the Kentico docs for migration guides specific to the version
2. Search for error messages in Kentico documentation
3. Review recent Kentico GitHub releases for known issues
4. Consider updating in smaller increments (one minor version at a time) if jumping multiple versions

---

Now, begin the update process. Start by running the PowerShell script and reporting the results.
