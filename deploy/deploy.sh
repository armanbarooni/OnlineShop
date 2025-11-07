#!/bin/bash

# Deployment script for OnlineShop on VPS
# This script handles deployment of the application using Docker Compose

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
COMPOSE_FILE="docker-compose.prod.yml"
BACKUP_DIR="./backups"
LOG_FILE="./deploy.log"

# Functions
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$LOG_FILE"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "$LOG_FILE"
    exit 1
}

warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1" | tee -a "$LOG_FILE"
}

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    error "Docker is not installed. Please install Docker first."
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    error "Docker Compose is not installed. Please install Docker Compose first."
fi

# Use docker compose (v2) if available, otherwise use docker-compose (v1)
if docker compose version &> /dev/null; then
    COMPOSE_CMD="docker compose"
else
    COMPOSE_CMD="docker-compose"
fi

log "Starting deployment..."

# Check if .env file exists
if [ ! -f .env ]; then
    error ".env file not found. Please create .env file from deploy/env.template"
fi

# Load environment variables from .env file
log "Loading environment variables from .env file..."
set -a  # Automatically export all variables
source .env
set +a

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

# Backup database before deployment
log "Creating database backup..."
BACKUP_FILE="$BACKUP_DIR/db_backup_$(date +%Y%m%d_%H%M%S).sql"
if $COMPOSE_CMD -f "$COMPOSE_FILE" ps | grep -q "db"; then
    $COMPOSE_CMD -f "$COMPOSE_FILE" exec -T db pg_dump -U ${POSTGRES_USER:-postgres} ${POSTGRES_DB:-OnlineShop} > "$BACKUP_FILE" 2>/dev/null || warning "Failed to create database backup"
    if [ -f "$BACKUP_FILE" ] && [ -s "$BACKUP_FILE" ]; then
        log "Database backup created: $BACKUP_FILE"
        # Keep only last 7 backups
        ls -t "$BACKUP_DIR"/db_backup_*.sql | tail -n +8 | xargs rm -f 2>/dev/null || true
    fi
fi

# Pull latest images
log "Pulling latest Docker images..."
$COMPOSE_CMD -f "$COMPOSE_FILE" pull || warning "Failed to pull some images"

# Stop existing containers
log "Stopping existing containers..."
$COMPOSE_CMD -f "$COMPOSE_FILE" down || warning "Some containers were not running"

# Start containers
log "Starting containers..."
$COMPOSE_CMD -f "$COMPOSE_FILE" up -d

# Wait for services to be healthy
log "Waiting for services to be healthy..."
sleep 10

# Check container status
log "Checking container status..."
$COMPOSE_CMD -f "$COMPOSE_FILE" ps

# Wait for API health check
log "Waiting for API to be ready..."
MAX_ATTEMPTS=30
ATTEMPT=0
while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
    if curl -f http://localhost:8080/api/health > /dev/null 2>&1; then
        log "API is healthy!"
        break
    fi
    ATTEMPT=$((ATTEMPT + 1))
    sleep 2
done

if [ $ATTEMPT -eq $MAX_ATTEMPTS ]; then
    error "API failed to become healthy after $MAX_ATTEMPTS attempts"
fi

# Clean up old Docker images
log "Cleaning up old Docker images..."
docker image prune -f || warning "Failed to prune images"

log "Deployment completed successfully!"

# Show running containers
log "Running containers:"
$COMPOSE_CMD -f "$COMPOSE_FILE" ps

# Show logs (last 20 lines)
log "Recent logs:"
$COMPOSE_CMD -f "$COMPOSE_FILE" logs --tail=20

