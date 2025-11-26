# Deployment Guide

## 1. Local development

### 1.1. Configure Development Settings

**Edit `src/WebAPI/appsettings.Development.json`** if needed (already configured for localhost).

### 1.2. Sync Frontend

Edit ONLY the files under `presentation/`, then sync static assets into `wwwroot`:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/sync-frontend.ps1 -Environment development
```

### 1.3. Run the API

You can run the API in three ways:

**Option 1: Visual Studio**
- Select the "http" or "https" profile and press F5

**Option 2: Command Line**
```powershell
cd src/WebAPI
dotnet run
```

**Option 3: Production Profile (for testing)**
- In Visual Studio, select the "Production" profile to test production settings locally

## 2. Production deployment

### 2.1. Configure Backend Settings

**Edit `src/WebAPI/appsettings.Production.json`** and replace the following placeholders:

- **`ConnectionStrings.DefaultConnection`**: Replace `REPLACE_ME` with your actual database credentials
  ```json
  "DefaultConnection": "Host=YOUR_DB_HOST;Port=5432;Database=OnlineShop;Username=YOUR_USER;Password=YOUR_PASSWORD;"
  ```

- **`Jwt.Secret`**: Replace with a strong secret key (minimum 32 characters)
  ```json
  "Secret": "YOUR_STRONG_SECRET_KEY_MIN_32_CHARACTERS"
  ```

- **`Cors.AllowFrontend`**: Already configured with production IPs, adjust if needed
  ```json
  "AllowFrontend": [
    "http://130.185.73.167:8080",
    "http://172.24.32.1:8080"
  ]
  ```

- **`SmsIr.ApiKey`**: Add your SMS.ir API key if SMS functionality is needed
- **`SadadGateway`**: Configure payment gateway credentials (MerchantId, TerminalId, Key)

### 2.2. Web.config (IIS Deployment)

The `web.config` file is already configured to:
- Set `ASPNETCORE_ENVIRONMENT=Production`
- Read settings from `appsettings.Production.json`
- Support environment variable overrides if needed

**Note**: If you need to override settings via environment variables in IIS, use the format:
- `ConnectionStrings__DefaultConnection`
- `JWT__Secret`
- `CORS__AllowFrontend__0`, `CORS__AllowFrontend__1`, etc.

### 2.3. Sync Frontend

Sync the presentation layer for production (creates `config.runtime.json` with prod URLs):
```powershell
powershell -ExecutionPolicy Bypass -File scripts/sync-frontend.ps1 -Environment production -CleanDest
```

### 2.4. Publish and Deploy

Publish the API:
```powershell
dotnet publish src/WebAPI/OnlineShop.WebAPI.csproj -c Release -o publish
```

Copy the published output to the server and:
- **For IIS**: Configure the application pool and site pointing to the publish folder
- **For Kestrel**: Run `dotnet OnlineShop.WebAPI.dll` with `ASPNETCORE_ENVIRONMENT=Production`

## 3. Runtime config
- `presentation/config.{env}.json` → automatically mirrored to `wwwroot/fa/config.runtime.json`.
- `presentation/config.js` loads the runtime file synchronously; no manual edits needed.
- To override temporarily, set `window.__API_BASE_URL__` before loading `config.js`.

## 4. Verification checklist
- Frontend assets exist only in `presentation/`; `wwwroot/fa` is generated.
- `config.runtime.json` matches the intended environment.
- API responds at the same origin that the frontend uses, so all requests hit a single backend URL.

Keep this guide updated as additional environments or steps are introduced.

