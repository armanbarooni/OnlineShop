---
description: Example deployment workflow with turbo mode
---

# Example Deployment Workflow

This workflow demonstrates how to use turbo mode for automated deployments.

## Steps:

1. **Check current directory**
   ```bash
   pwd
   ```

// turbo
2. **List files** (This step will auto-run because of // turbo above it)
   ```bash
   ls
   ```

3. **Build the project** (This will ask for permission - no turbo annotation)
   ```bash
   dotnet build
   ```

// turbo
4. **Check build output** (This step will auto-run)
   ```bash
   ls bin/Debug
   ```

5. **Run tests**
   ```bash
   dotnet test
   ```
