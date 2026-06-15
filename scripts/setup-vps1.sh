#!/bin/bash

# =====================================================
# Setup VPS 1 (APP) - Automation Script
# =====================================================
# Usage: bash setup-vps1.sh
# Run này trên VPS 1 sau khi cài Docker

set -e

echo "🚀 Setting up VPS 1 (APP)..."

# Create directories
mkdir -p /opt/schoolapp
cd /opt/schoolapp

# Check if git is installed
if ! command -v git &> /dev/null; then
    echo "📦 Installing git..."
    apt install -y git
fi

# Clone repository
echo "📥 Cloning repository..."
if [ -d ".git" ]; then
    echo "Repository already exists, pulling latest..."
    git pull origin main
else
    # Replace with your actual repo
    git clone https://github.com/your-username/schoolapp.git .
fi

# Check if docker-compose-app.yml exists
if [ ! -f "docker-compose-app.yml" ]; then
    echo "❌ docker-compose-app.yml not found!"
    echo "Make sure you copied it from the guide."
    exit 1
fi

# Check if .env exists
if [ ! -f ".env" ]; then
    echo "⚠️  .env file not found!"
    echo "Creating from template..."
    cp .env.example-vps1 .env
    echo "📝 Edit .env with your configuration:"
    echo "   nano .env"
    exit 1
fi

echo "✅ Prerequisites ready!"
echo ""
echo "Next steps:"
echo "1. Edit .env file with your configuration:"
echo "   nano /opt/schoolapp/.env"
echo ""
echo "2. Build and start containers:"
echo "   cd /opt/schoolapp"
echo "   docker-compose -f docker-compose-app.yml build"
echo "   docker-compose -f docker-compose-app.yml up -d"
echo ""
echo "3. Check logs:"
echo "   docker-compose -f docker-compose-app.yml logs -f app"
