# Stage 1: Base SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /src

# Stage 2: Restore dependencies
# This stage caches NuGet package restore
# Only reruns when .csproj files change
FROM base AS restore

# Copy project files first (for better layer caching)
# Copy Domain project (no dependencies)
COPY ["src/Domain/OnlineShop.Domain.csproj", "src/Domain/"]

# Copy Application project (depends on Domain)
COPY ["src/Application/OnlineShop.Application.csproj", "src/Application/"]

# Copy Infrastructure project (depends on Application and Domain)
COPY ["src/Infrastructure/OnlineShop.Infrastructure.csproj", "src/Infrastructure/"]

# Copy WebAPI project (depends on Application and Infrastructure)
COPY ["src/WebAPI/OnlineShop.WebAPI.csproj", "src/WebAPI/"]

# Restore dependencies for all projects
# This layer will be cached unless .csproj files change
RUN dotnet restore "src/WebAPI/OnlineShop.WebAPI.csproj" \
    --runtime linux-x64 \
    --verbosity quiet

# Stage 3: Build
# This stage builds the application
# Only reruns when source code changes (not dependencies)
FROM restore AS build

# Copy all source code
COPY src/ src/

# Copy solution file (if needed for build context)
COPY ["OnlineShop.sln", "./"]
COPY ["NuGet.Config", "./"]

# Build the solution
RUN dotnet build "src/WebAPI/OnlineShop.WebAPI.csproj" \
    --configuration Release \
    --no-restore \
    --runtime linux-x64

# Stage 4: Publish
# This stage publishes the application for production
FROM build AS publish

RUN dotnet publish "src/WebAPI/OnlineShop.WebAPI.csproj" \
    --configuration Release \
    --no-restore \
    --no-build \
    --runtime linux-x64 \
    --self-contained false \
    --output /app/publish \
    /p:UseAppHost=false

# Stage 5: Runtime
# This is the final, minimal image for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Install curl for health checks (before switching user)
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Create logs directory and set permissions
RUN mkdir -p /app/logs && \
    chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port (can be overridden via environment variable)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl --fail http://localhost:8080/api/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "OnlineShop.WebAPI.dll"]

