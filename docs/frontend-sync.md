# Frontend Sync Workflow

`presentation/` is the single source of truth for every HTML/CSS/JS asset.  
`src/WebAPI/wwwroot/fa/` is **generated output** that ASP.NET serves.

## One-time setup
1. Open PowerShell.
2. Allow script execution if needed:
   ```powershell
   Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
   ```

## Sync commands
```powershell
# Default sync (updates files, preserves leftovers)
pwsh scripts/sync-frontend.ps1

# Mirror + clean destination first
pwsh scripts/sync-frontend.ps1 -CleanDest

# Optional environment label (for logging only, future hooks)
pwsh scripts/sync-frontend.ps1 -Environment production
```

The script mirrors `presentation/` (excluding `node_modules`, `.git`, `.cursor`, `.vscode`, `.idea`) into `src/WebAPI/wwwroot/fa/` using `robocopy /MIR`.  
Any manual edits inside `wwwroot/fa` will be overwritten on next sync—always edit under `presentation/`.

## Watch mode (optional)
Use a PowerShell file watcher during active development:
```powershell
pwsh -Command "while($true){ pwsh scripts/sync-frontend.ps1; Start-Sleep 3 }"
```
or rely on your editor’s “file watcher/auto copy” feature.

## Deployment checklist
1. Apply code changes under `presentation/`.
2. Run `pwsh scripts/sync-frontend.ps1 -CleanDest`.
3. Commit both `presentation` and `src/WebAPI/wwwroot/fa` (since backend project serves from there) or run the script on the server before publishing.

Update this doc if more environments or steps are introduced.

