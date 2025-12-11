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

Execute the `Update-Xperience.ps1` PowerShell script to automate mechanical tasks:

```powershell
# For latest version
.\Update-Xperience.ps1

# For specific version
.\Update-Xperience.ps1 -TargetVersion "30.0.0"

# Dry run to preview changes
.\Update-Xperience.ps1 -DryRun

# Skip npm updates (if needed)
.\Update-Xperience.ps1 -SkipNpmUpdate

# Skip prerequisite checks (not recommended)
.\Update-Xperience.ps1 -SkipPrerequisites
```

The script will:
- Validate prerequisites (Directory.Packages.props exists, clean git state, feature branch)
- Check current Kentico package versions in Directory.Packages.props
- Update Kentico.Xperience.* packages (preserves Kentico.Xperience.MiniProfiler which has independent versioning)
- Update @kentico/* npm packages in admin client only (not custom frontend dependencies)
- Run `npm audit fix` for security vulnerabilities
- Disable CI (CMSEnableCI) if enabled
- Run `dotnet run --kxp-update --skip-confirmation` for database migrations
- Re-enable CI if it was previously enabled
- Run `dotnet run --kxp-ci-store` to serialize updated objects
- Build the solution
- Check for CI repository changes

### 2. Review Breaking Changes

Use the Kentico docs MCP server to research any breaking changes:

- Search for release notes for the target version
- Look for migration guides and breaking changes documentation
- Pay special attention to:
  - Deprecated APIs and their replacements
  - New recommended patterns
  - Required code changes
  - Configuration changes

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

### 4. Manual Testing Checklist

After the build succeeds, recommend the user test these critical areas locally:
- Homepage loads correctly
- Blog listing page works
- Blog detail pages render with images
- Navigation and header work
- Admin interface is accessible
- Page builder widgets function correctly

### 5. Review CI Repository Changes

The `App_Data/CIRepository/` folder contains serialized Kentico objects that must be committed:
- Check `git status src/Goldfinch.Web/App_Data/CIRepository/`
- Review what changed
- These changes are expected and should be committed with the update

### 6. Commit and Create PR

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
- **CI Toggle**: The script automatically disables CI before update and re-enables it afterward (per Kentico best practices)
- **CI Repository**: Changes to `App_Data/CIRepository/` are expected and necessary after CI store operation
- **Nullable Reference Types**: This project has nullable reference types enabled - ensure any code changes respect this
- **Minimal Changes**: Keep changes focused on Kentico packages only - don't bump unrelated dependencies
- **Documentation**: Always use the Kentico docs MCP server for authoritative information about APIs and changes

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
