# Deployment Guide

This directory contains deployment scripts and configuration files for deploying OnlineShop on a VPS.

## Files

- `deploy.sh` - Main deployment script
- `.env.example` - Environment variables template
- `docker-compose.prod.yml` - Production Docker Compose configuration (in root directory)

## Prerequisites

1. **VPS with Linux** (Ubuntu 20.04+ recommended)
2. **Docker** installed
3. **Docker Compose** installed
4. **SSH access** to the VPS
5. **GitHub Container Registry** access (for pulling images)

## Setup

### 1. Initial Server Setup

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Add user to docker group (optional)
sudo usermod -aG docker $USER
```

### 2. Create Application Directory

```bash
sudo mkdir -p /opt/onlineshop
sudo chown $USER:$USER /opt/onlineshop
cd /opt/onlineshop
```

### 3. Copy Files to VPS

Copy the following files to `/opt/onlineshop`:
- `docker-compose.prod.yml` (from root)
- `deploy/deploy.sh`
- Create `.env` file from `deploy/.env.example`

### 4. Configure Environment Variables

```bash
cd /opt/onlineshop
cp deploy/.env.example .env
nano .env  # Edit with your actual values
```

**Important**: Change all placeholder values, especially:
- `POSTGRES_PASSWORD` - Strong database password
- `JWT__SECRET` - At least 32 characters, strong secret
- `SMSSETTINGS__APIKEY` - Your SMS provider API key
- `DOCKER_IMAGE` - Your GitHub Container Registry image path

### 5. Make Deploy Script Executable

```bash
chmod +x deploy/deploy.sh
```

## Deployment

### Manual Deployment

```bash
cd /opt/onlineshop
./deploy/deploy.sh
```

### Automatic Deployment via GitHub Actions

The GitHub Actions workflow (`.github/workflows/deploy.yml`) will automatically deploy when you push to the `main` branch.

**Required GitHub Secrets**:
- `VPS_HOST` - Your VPS IP address or domain
- `VPS_USER` - SSH username
- `VPS_SSH_KEY` - Private SSH key for authentication
- `VPS_PORT` - SSH port (optional, default: 22)
- `VPS_APP_DIR` - Application directory on VPS (optional, default: /opt/onlineshop)

## Database Migrations

Database migrations run automatically in Development mode. For Production:

### Option 1: Manual Migration

```bash
# Run migrations manually
docker-compose -f docker-compose.prod.yml exec api dotnet ef database update --project /app
```

### Option 2: Migration Script

Create a migration script and run it as part of deployment.

## Monitoring

### Check Container Status

```bash
docker-compose -f docker-compose.prod.yml ps
```

### View Logs

```bash
# All services
docker-compose -f docker-compose.prod.yml logs -f

# Specific service
docker-compose -f docker-compose.prod.yml logs -f api
docker-compose -f docker-compose.prod.yml logs -f db
```

### Health Check

```bash
curl http://localhost:8080/api/health
```

## Backup and Restore

### Backup Database

```bash
docker-compose -f docker-compose.prod.yml exec db pg_dump -U postgres OnlineShop > backup.sql
```

### Restore Database

```bash
docker-compose -f docker-compose.prod.yml exec -T db psql -U postgres OnlineShop < backup.sql
```

## Troubleshooting

### Container Won't Start

1. Check logs: `docker-compose -f docker-compose.prod.yml logs`
2. Check environment variables: `docker-compose -f docker-compose.prod.yml config`
3. Verify Docker image: `docker images`

### Database Connection Issues

1. Check database container is running: `docker-compose -f docker-compose.prod.yml ps db`
2. Verify connection string in `.env`
3. Check database logs: `docker-compose -f docker-compose.prod.yml logs db`

### API Not Responding

1. Check API logs: `docker-compose -f docker-compose.prod.yml logs api`
2. Verify health endpoint: `curl http://localhost:8080/api/health`
3. Check port mapping: `docker-compose -f docker-compose.prod.yml ps`

## Security Considerations

1. **Change default passwords** in `.env`
2. **Use strong JWT secret** (at least 32 characters)
3. **Restrict database port** (only accessible from localhost)
4. **Use HTTPS** in production (configure reverse proxy like Nginx)
5. **Regular backups** of database
6. **Keep Docker images updated**
7. **Monitor logs** for suspicious activity

## Reverse Proxy Setup (Nginx)

For production, it's recommended to use Nginx as a reverse proxy:

```nginx
server {
    listen 80;
    server_name yourdomain.com;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## Updates

To update the application:

1. Pull latest changes from GitHub
2. Run deployment script: `./deploy/deploy.sh`
3. Or let GitHub Actions handle it automatically

## Rollback

If something goes wrong:

```bash
# Stop containers
docker-compose -f docker-compose.prod.yml down

# Pull previous image
docker pull ghcr.io/yourusername/onlineshop:previous-tag

# Update docker-compose.prod.yml to use previous tag
# Then restart
docker-compose -f docker-compose.prod.yml up -d
```

