# Docker Deployment Checklist

Use this checklist to ensure all components are properly configured for deployment.

## Pre-Deployment Checklist

### Local Development Setup
- [ ] Docker Desktop/Engine installed
- [ ] Docker Compose installed
- [ ] Repository cloned
- [ ] `.env` file created (optional for local dev)
- [ ] Test build: `docker-compose build`
- [ ] Test run: `docker-compose up`
- [ ] Verify API: http://localhost:5000/api/health
- [ ] Verify database connection

### Production VPS Setup
- [ ] VPS purchased and accessible
- [ ] Docker installed on VPS
- [ ] Docker Compose installed on VPS
- [ ] SSH access configured
- [ ] Application directory created: `/opt/onlineshop`
- [ ] Deployment files copied to VPS:
  - [ ] `docker-compose.prod.yml`
  - [ ] `deploy/` directory
- [ ] `.env` file created from `deploy/env.template`
- [ ] Environment variables configured in `.env`:
  - [ ] `POSTGRES_PASSWORD` - Strong password
  - [ ] `JWT__SECRET` - At least 32 characters
  - [ ] `SMSIR__APIKEY` - SMS provider API key (if using)
  - [ ] `DOCKER_IMAGE` - GitHub Container Registry image path
  - [ ] Other required variables

### GitHub Actions Setup
- [ ] GitHub repository created
- [ ] Repository secrets configured:
  - [ ] `VPS_HOST` - VPS IP or domain
  - [ ] `VPS_USER` - SSH username
  - [ ] `VPS_SSH_KEY` - Private SSH key
  - [ ] `VPS_PORT` - SSH port (optional)
  - [ ] `VPS_APP_DIR` - App directory (optional)
  - [ ] `GHCR_TOKEN` - GitHub Personal Access Token (for private repos)
- [ ] GitHub Actions workflow file exists: `.github/workflows/deploy.yml`
- [ ] Repository packages permissions configured (for GitHub Container Registry)

### Docker Image Configuration
- [ ] Dockerfile exists and is optimized
- [ ] `.dockerignore` file configured
- [ ] Image builds successfully: `docker build -t test .`
- [ ] Image size is reasonable
- [ ] Health check works: `docker run --rm test curl http://localhost:8080/api/health`

### Security Checklist
- [ ] Strong passwords set in `.env`
- [ ] JWT secret is at least 32 characters
- [ ] Database port restricted to localhost in production
- [ ] Non-root user configured in Dockerfile
- [ ] `.env` file not committed to Git
- [ ] SSH keys properly secured
- [ ] Firewall rules configured on VPS

## Deployment Checklist

### Initial Deployment
- [ ] VPS setup completed
- [ ] Deployment files copied to VPS
- [ ] `.env` file configured on VPS
- [ ] Deployment script is executable: `chmod +x deploy/deploy.sh`
- [ ] First deployment successful: `./deploy/deploy.sh`
- [ ] Containers running: `docker-compose -f docker-compose.prod.yml ps`
- [ ] API accessible: `curl http://localhost:8080/api/health`
- [ ] Database accessible and migrations applied
- [ ] Logs checked: `docker-compose -f docker-compose.prod.yml logs`

### CI/CD Setup
- [ ] GitHub Actions secrets configured
- [ ] Workflow file pushed to repository
- [ ] Test push to main branch
- [ ] Build successful in GitHub Actions
- [ ] Image pushed to GitHub Container Registry
- [ ] Deployment to VPS successful
- [ ] Verify application is running after deployment

### Post-Deployment
- [ ] Application accessible from browser
- [ ] API endpoints responding correctly
- [ ] Database operations working
- [ ] Logs directory created and writable
- [ ] Backup directory created
- [ ] Health checks passing
- [ ] Monitor logs for errors

## Ongoing Maintenance

### Regular Tasks
- [ ] Monitor application logs
- [ ] Check container health
- [ ] Review error logs
- [ ] Verify backups are working
- [ ] Update dependencies regularly
- [ ] Keep Docker images updated
- [ ] Review security updates

### Backup Verification
- [ ] Automated backups configured
- [ ] Backup restoration tested
- [ ] Backup retention policy set
- [ ] Offsite backup storage (if needed)

### Monitoring Setup
- [ ] Health check monitoring configured
- [ ] Log aggregation set up (optional)
- [ ] Alerting configured (optional)
- [ ] Performance monitoring (optional)

## Troubleshooting Reference

### Common Issues
- [ ] Container won't start → Check logs: `docker-compose logs`
- [ ] Database connection error → Verify `.env` and database container
- [ ] API not responding → Check health endpoint and port mapping
- [ ] Build fails → Check Dockerfile and build context
- [ ] Deployment fails → Check GitHub Actions logs and SSH connection

### Useful Commands
```bash
# Check container status
docker-compose -f docker-compose.prod.yml ps

# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Restart services
docker-compose -f docker-compose.prod.yml restart

# Check health
curl http://localhost:8080/api/health

# Backup database
docker-compose -f docker-compose.prod.yml exec db pg_dump -U postgres OnlineShop > backup.sql
```

## Documentation

- [ ] Read [DEPLOYMENT.md](./DEPLOYMENT.md) for detailed instructions
- [ ] Read [QUICK_DEPLOY.md](./QUICK_DEPLOY.md) for quick reference
- [ ] Read [deploy/README.md](./deploy/README.md) for deployment script docs
- [ ] Read [DOCKER_SETUP_SUMMARY.md](./DOCKER_SETUP_SUMMARY.md) for overview

## Notes

- Keep this checklist updated as you configure your deployment
- Add custom items specific to your setup
- Review before each major deployment
- Document any custom configurations

