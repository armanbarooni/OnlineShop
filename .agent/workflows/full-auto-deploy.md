---
description: Fully automated deployment (all commands auto-run)
---

// turbo-all

# Fully Automated Deployment

This workflow will run ALL commands automatically without asking for permission.

## Steps:

1. **Clean previous builds**
   ```bash
   dotnet clean
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build --configuration Release
   ```

4. **Run all tests**
   ```bash
   dotnet test
   ```

5. **Publish the application**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

All of these commands will run automatically because of the `// turbo-all` annotation at the top!
