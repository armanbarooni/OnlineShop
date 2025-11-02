# OnlineShop

A modular e-commerce sample that combines an ASP.NET Core Web API with a static front-end.

## Project Structure

- src/ – backend services (ASP.NET Core Web API).
- presentation/ – static front-end assets and pages.
- 	ests/ – automated tests.
- docs/ – additional design and feature documentation.

## Prerequisites

- .NET 8 SDK or newer
- Node.js 18+ (for front-end tooling if required)
- PostgreSQL 14+

## Getting Started

1. Copy .env.example to .env and update the values to match your environment. See ENV_VARIABLES.md for the full list of required keys.
2. Restore dependencies and build the Web API:
   `powershell
   dotnet restore src/WebAPI/OnlineShop.WebAPI.csproj
   dotnet build src/WebAPI/OnlineShop.WebAPI.csproj
   `
3. Apply database migrations:
   `powershell
   dotnet ef database update --project src/WebAPI/OnlineShop.WebAPI.csproj
   `
4. Run the backend:
   `powershell
   dotnet run --project src/WebAPI/OnlineShop.WebAPI.csproj
   `
5. Open presentation/index.html (or host the presentation folder via a static server) to access the front-end.

## Environment Variables

The application relies on environment variables for secrets and deployment-specific values. A curated list and guidance is available in ENV_VARIABLES.md.

## Documentation

Additional setup notes and process documents are located in the docs/ directory and the existing README-SETUP.md, QUICK_START.md, and related files.

## Support & Contact

For implementation questions or production readiness reviews, document findings in the repository issue tracker or reach out to the project maintainer.
