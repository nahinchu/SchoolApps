#!/bin/bash

# =====================================================
# Setup VPS 2 (DATABASE + MINIO) - Automation Script
# =====================================================
# Usage: bash setup-vps2.sh
# Run này trên VPS 2 sau khi cài Docker

set -e

echo "🚀 Setting up VPS 2 (DATABASE + MINIO)..."

# Create directories
mkdir -p /opt/schoolapp/{data/minio,data/mssql,backups}
cd /opt/schoolapp

# Check if docker-compose-database.yml exists
# Assume it's downloaded/copied somehow
if [ ! -f "docker-compose-database.yml" ]; then
    echo "❌ docker-compose-database.yml not found!"
    echo "Make sure you copied it from the guide to /opt/schoolapp/"
    exit 1
fi

# Check if .env exists
if [ ! -f ".env" ]; then
    echo "⚠️  .env file not found!"
    echo "Creating from template..."

    # Create a basic .env with defaults
    cat > .env << 'EOF'
# SQL Server
ACCEPT_EULA=Y
SA_PASSWORD=YourStrongPassword123!@#

# MinIO
MINIO_ROOT_USER=minioadmin
MINIO_ROOT_PASSWORD=YourStrongPassword123!@#
EOF

    echo "✏️  Generated default .env file"
    echo "⚠️  PLEASE CHANGE PASSWORDS!"
    echo "   nano .env"
    read -p "Press Enter after updating .env..."
fi

echo "✅ Configuration ready!"
echo ""
echo "Starting services..."
docker-compose -f docker-compose-database.yml up -d

echo ""
echo "⏳ Waiting for services to be healthy..."
sleep 15

echo ""
echo "Checking services..."
docker-compose -f docker-compose-database.yml ps

echo ""
echo "📊 Service Health:"
echo "SQL Server:"
docker-compose -f docker-compose-database.yml exec -T sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$(grep SA_PASSWORD .env | cut -d'=' -f2)" -Q "SELECT @@version" || echo "⚠️  Waiting for SQL Server to start..."

echo ""
echo "✅ VPS 2 Setup Complete!"
echo ""
echo "Next steps:"
echo "1. Create MinIO bucket:"
echo "   docker exec -it minio-server /bin/bash"
echo "   apt update && apt install -y mc"
echo "   mc alias set minio http://localhost:9000 minioadmin \$(grep MINIO_ROOT_PASSWORD .env | cut -d'=' -f2)"
echo "   mc mb minio/school-app"
echo "   exit"
echo ""
echo "2. View MinIO UI:"
echo "   http://your-vps2-ip:9001"
echo ""
echo "3. Create database migration on VPS 1 after it's running"
