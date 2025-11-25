# Deployment Guide

## 1. Local development
1. Edit ONLY the files under `presentation/`.
2. Update backend settings in `src/WebAPI/appsettings.Development.json`.
3. Sync static assets into `wwwroot`:
   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts/sync-frontend.ps1 -Environment development
   ```
4. Run the API:
   ```powershell
   cd src/WebAPI
   dotnet run
   ```

## 2. Production deployment
1. Configure `appsettings.Production.json` with:
   - `ConnectionStrings.DefaultConnection`
   - `Jwt.Secret`
   - `Cors.AllowFrontend` (list of allowed domains/IPs)
   - `BaseUrl` & `SadadGateway.CallbackUrl`
2. Sync the presentation layer for production (creates `config.runtime.json` with prod URLs):
   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts/sync-frontend.ps1 -Environment production -CleanDest
   ```
3. Publish the API (example):
   ```powershell
   dotnet publish src/WebAPI/WebAPI.csproj -c Release -o publish
   ```
4. Copy the published output to the server and run the service (`dotnet WebAPI.dll` or configure IIS/Kestrel).

## 3. Runtime config
- `presentation/config.{env}.json` → automatically mirrored to `wwwroot/fa/config.runtime.json`.
- `presentation/config.js` loads the runtime file synchronously; no manual edits needed.
- To override temporarily, set `window.__API_BASE_URL__` before loading `config.js`.

## 4. Verification checklist
- Frontend assets exist only in `presentation/`; `wwwroot/fa` is generated.
- `config.runtime.json` matches the intended environment.
- API responds at the same origin that the frontend uses, so all requests hit a single backend URL.

Keep this guide updated as additional environments or steps are introduced.

