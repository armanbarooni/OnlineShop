# Quick Deployment Guide

This is a quick reference guide for deploying OnlineShop. For detailed documentation, see [DEPLOYMENT.md](./DEPLOYMENT.md).

## Local Development

```bash
# 1. Clone repository
git clone <repository-url>
cd onlintest

# 2. Create .env file (optional, for custom config)
# Copy docker-compose.yml and adjust as needed

# 3. Start services
docker-compose up --build

# 4. Access application
# API: http://localhost:5000
# Health: http://localhost:5000/api/health
```

## Production Deployment

### Initial VPS Setup (One-time)

```bash
# 1. Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# 2. Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 3. Create app directory
sudo mkdir -p /opt/onlineshop
sudo chown $USER:$USER /opt/onlineshop
cd /opt/onlineshop
```

### Deploy Application

```bash
# 1. Copy files to VPS
# - docker-compose.prod.yml
# - deploy/ directory

# 2. Create .env file
cp deploy/env.template .env
nano .env  # Edit with production values

# 3. Deploy
chmod +x deploy/deploy.sh
./deploy/deploy.sh
```

### GitHub Actions Setup

1. **Add GitHub Secrets** (Repository Settings → Secrets):
   - `VPS_HOST`: Your VPS IP or domain
   - `VPS_USER`: SSH username
   - `VPS_SSH_KEY`: Private SSH key
   - `VPS_PORT`: SSH port (optional, default: 22)
   - `VPS_APP_DIR`: App directory (optional, default: /opt/onlineshop)
   - `GHCR_TOKEN`: GitHub Personal Access Token (for private repos)

2. **Push to main branch** - Deployment happens automatically!

## Environment Variables

Required variables in `.env`:

```env
# Database
POSTGRES_PASSWORD=strong_password_here

# JWT
JWT__SECRET=at_least_32_character_secret_here

# SMS (if using)
SMSIR__APIKEY=your_api_key
```

## Useful Commands

```bash
# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Check status
docker-compose -f docker-compose.prod.yml ps

# Stop services
docker-compose -f docker-compose.prod.yml down

# Start services
docker-compose -f docker-compose.prod.yml up -d

# Restart services
docker-compose -f docker-compose.prod.yml restart

# Backup database
docker-compose -f docker-compose.prod.yml exec db pg_dump -U postgres OnlineShop > backup.sql
```

## Troubleshooting

**Container won't start:**
```bash
docker-compose -f docker-compose.prod.yml logs
```

**Database connection error:**
- Check `.env` file has correct credentials
- Verify database container is running: `docker-compose ps db`

**API not responding:**
- Check health endpoint: `curl http://localhost:8080/api/health`
- Check API logs: `docker-compose logs api`

## Next Steps

- Configure reverse proxy (Nginx/Caddy) for HTTPS
- Set up SSL certificates (Let's Encrypt)
- Configure firewall rules
- Set up automated backups
- Configure monitoring

For more details, see [DEPLOYMENT.md](./DEPLOYMENT.md).

