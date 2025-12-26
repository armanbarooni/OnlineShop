# Deployment Documentation

This document provides comprehensive instructions for deploying OnlineShop using Docker and Docker Compose.

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Local Development](#local-development)
4. [Production Deployment](#production-deployment)
5. [Docker Configuration](#docker-configuration)
6. [CI/CD with GitHub Actions](#cicd-with-github-actions)
7. [Troubleshooting](#troubleshooting)

## Overview

OnlineShop uses Docker for containerization and can be deployed in two modes:
- **Development**: Using `docker-compose.yml` for local development
- **Production**: Using `docker-compose.prod.yml` for VPS deployment

## Prerequisites

### Local Development
- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose
- Git

### Production VPS
- Linux VPS (Ubuntu 20.04+ recommended)
- Docker installed
- Docker Compose installed
- SSH access to VPS
- Domain name (optional, for production)

## Local Development

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd onlintest
   ```

2. **Build and run with Docker Compose**
   ```bash
   docker-compose up --build
   ```

3. **Access the application**
   - API: http://localhost:5000
   - Health Check: http://localhost:5000/api/health
   - Swagger UI: http://localhost:5000 (in Development mode)

### Environment Variables

For local development, create a `.env` file in the root directory:

```env
POSTGRES_DB=OnlineShop
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
JWT__ISSUER=OnlineShop
JWT__AUDIENCE=OnlineShopClient
JWT__SECRET=dev-secret-change-me-please-0123456789
```

### Running Migrations

Migrations run automatically in Development mode. To run manually:

```bash
docker-compose exec api dotnet ef database update --project /app
```

## Production Deployment

### VPS Setup

1. **Install Docker and Docker Compose**
   ```bash
   # Install Docker
   curl -fsSL https://get.docker.com -o get-docker.sh
   sudo sh get-docker.sh
   
   # Install Docker Compose
   sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
   sudo chmod +x /usr/local/bin/docker-compose
   ```

2. **Create application directory**
   ```bash
   sudo mkdir -p /opt/onlineshop
   sudo chown $USER:$USER /opt/onlineshop
   cd /opt/onlineshop
   ```

3. **Copy deployment files**
   - Copy `docker-compose.prod.yml` to `/opt/onlineshop/`
   - Copy `deploy/` directory to `/opt/onlineshop/deploy/`
   - Create `.env` file from `deploy/env.template`

4. **Configure environment variables**
   ```bash
   cp deploy/env.template .env
   nano .env  # Edit with production values
   ```

5. **Run deployment script**
   ```bash
   chmod +x deploy/deploy.sh
   ./deploy/deploy.sh
   ```

### Manual Deployment

If you prefer manual deployment:

```bash
# Pull latest image
docker pull ghcr.io/yourusername/onlineshop:latest

# Stop existing containers
docker-compose -f docker-compose.prod.yml down

# Start new containers
docker-compose -f docker-compose.prod.yml up -d

# Check status
docker-compose -f docker-compose.prod.yml ps
```

## Docker Configuration

### Dockerfile

The `Dockerfile` uses multi-stage build for optimization:

1. **Base Stage**: .NET SDK for building
2. **Restore Stage**: Restore NuGet packages (cached layer)
3. **Build Stage**: Build the application
4. **Publish Stage**: Publish for production
5. **Runtime Stage**: Final image with only runtime dependencies

### Layer Caching Optimization

The Dockerfile is optimized for layer caching:
- `.csproj` files are copied first, allowing dependency restore to be cached
- Source code is copied after dependencies, so code changes don't invalidate dependency cache
- Each project layer is separate for maximum cache reuse

### .dockerignore

The `.dockerignore` file excludes unnecessary files from the build context:
- Build artifacts (`bin/`, `obj/`)
- Test projects
- Documentation
- IDE files
- Git files

## CI/CD with GitHub Actions

### Setup

1. **Configure GitHub Secrets**
   - Go to Repository Settings → Secrets and variables → Actions
   - Add the following secrets:
     - `VPS_HOST`: Your VPS IP or domain
     - `VPS_USER`: SSH username
     - `VPS_SSH_KEY`: Private SSH key
     - `VPS_PORT`: SSH port (optional, default: 22)
     - `VPS_APP_DIR`: Application directory (optional, default: /opt/onlineshop)

2. **Workflow**
   - The workflow (`.github/workflows/deploy.yml`) triggers on push to `main` branch
   - Builds Docker image with caching
   - Pushes to GitHub Container Registry
   - Deploys to VPS via SSH

### Manual Trigger

You can also trigger the workflow manually:
- Go to Actions tab
- Select "Build and Deploy"
- Click "Run workflow"

## Troubleshooting

### Container Issues

**Container won't start:**
```bash
# Check logs
docker-compose -f docker-compose.prod.yml logs

# Check container status
docker-compose -f docker-compose.prod.yml ps

# Inspect container
docker inspect <container-id>
```

**Database connection errors:**
- Verify `.env` file has correct database credentials
- Check database container is running: `docker-compose ps db`
- Check database logs: `docker-compose logs db`
- Verify connection string format

**API not responding:**
- Check API logs: `docker-compose logs api`
- Verify health endpoint: `curl http://localhost:8080/api/health`
- Check port mapping in `docker-compose.prod.yml`
- Verify firewall rules

### Build Issues

**Build fails:**
- Check Dockerfile syntax
- Verify all dependencies are available
- Check build logs: `docker build -t test .`

**Image too large:**
- Use multi-stage build (already implemented)
- Remove unnecessary dependencies
- Use `.dockerignore` effectively

### Deployment Issues

**GitHub Actions fails:**
- Check workflow logs in Actions tab
- Verify GitHub Secrets are set correctly
- Check SSH key permissions
- Verify VPS is accessible

**Deployment script fails:**
- Check script permissions: `chmod +x deploy/deploy.sh`
- Verify `.env` file exists and is configured
- Check Docker and Docker Compose are installed
- Review script logs

## Security Considerations

1. **Environment Variables**
   - Never commit `.env` files
   - Use strong passwords and secrets
   - Rotate secrets regularly

2. **Network Security**
   - Database port should only be accessible from localhost
   - Use firewall rules to restrict access
   - Use HTTPS in production (reverse proxy)

3. **Container Security**
   - Run containers as non-root user (already implemented)
   - Keep Docker images updated
   - Scan images for vulnerabilities

4. **Backups**
   - Regular database backups
   - Test backup restoration
   - Store backups securely

## Monitoring

### Health Checks

The application includes health check endpoints:
- `/api/health` - Basic health check
- Docker health checks are configured in `docker-compose.prod.yml`

### Logging

Logs are stored in:
- Container logs: `docker-compose logs`
- Application logs: `./logs/` directory (mounted as volume)
- Serilog logs to file and PostgreSQL

### Monitoring Tools

Consider using:
- **Docker stats**: `docker stats`
- **Portainer**: Docker management UI
- **Prometheus + Grafana**: Metrics and monitoring
- **ELK Stack**: Log aggregation

## Backup and Restore

### Database Backup

```bash
# Manual backup
docker-compose -f docker-compose.prod.yml exec db pg_dump -U postgres OnlineShop > backup.sql

# Automated backup (via cron)
0 2 * * * cd /opt/onlineshop && docker-compose -f docker-compose.prod.yml exec -T db pg_dump -U postgres OnlineShop > backups/db_backup_$(date +\%Y\%m\%d).sql
```

### Database Restore

```bash
docker-compose -f docker-compose.prod.yml exec -T db psql -U postgres OnlineShop < backup.sql
```

## Updates and Rollback

### Updating

1. Pull latest code
2. Build new image
3. Deploy using deployment script

### Rollback

1. Identify previous working image tag
2. Update `docker-compose.prod.yml` to use previous tag
3. Redeploy: `docker-compose -f docker-compose.prod.yml up -d`

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [ASP.NET Core Docker Guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)

