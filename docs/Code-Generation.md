# Code Generation

When updating Xperience by Kentico models, you can use the `kxp-codegen` tool to [generate code](https://docs.kentico.com/developers-and-admins/api/generate-code-files-for-system-objects) for your project.

Here are some examples of how you can use the `kxp-codegen` tool:

### Reusable Field Schemas

```powershell
dotnet run --no-build -- --kxp-codegen --type "ReusableFieldSchemas" --location "../Goldfinch.Core/ContentTypes/{type}/{dataClassNamespace}/{name}/" --namespace "Goldfinch.Core.ContentTypes"
```

### Page Content Types

```powershell
dotnet run --no-build -- --kxp-codegen --type "PageContentTypes" --location "../Goldfinch.Core/ContentTypes/{type}/{dataClassNamespace}/{name}/" --namespace "Goldfinch.Core.ContentTypes"
```

### Reusable Content Types - Content Hub

```powershell
dotnet run --no-build -- --kxp-codegen --type "ReusableContentTypes" --location "../Goldfinch.Core/ContentTypes/{type}/{dataClassNamespace}/{name}/" --namespace "Goldfinch.Core.ContentTypes"
```

## Classes

```powershell
dotnet run --no-build -- --kxp-codegen --type "Classes" --location "../Goldfinch.Core/ContentTypes/{type}/{dataClassNamespace}/{name}/" --namespace "Goldfinch.Core.ContentTypes"
```