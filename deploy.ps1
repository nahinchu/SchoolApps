# ============================================
# SchoolApp Deployment Script
# ============================================
# Cách dùng:
# .\deploy.ps1 -Mode "build" -Environment "Production"
# .\deploy.ps1 -Mode "publish"
# .\deploy.ps1 -Mode "migrate"
# ============================================

param(
    [string]$Mode = "full",  # full, build, publish, migrate, run, install-service, start-service, stop-service
    [string]$Environment = "Development"  # Development, Production
)

$ProjectPath = "C:\Training\SchoolApp\SchoolApp"
$PublishPath = "C:\SchoolApp\Published"
$AppName = "SchoolApp"
$Port = 5122

function Write-Success {
    param([string]$Message)
    Write-Host "[✓] $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[✗] $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "[i] $Message" -ForegroundColor Cyan
}

# ============================================
# BƯỚC 1: BUILD
# ============================================
function Build-Project {
    Write-Info "Building project in $Environment mode..."
    
    Push-Location $ProjectPath
    
    try {
        dotnet restore
        if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
        Write-Success "Restored NuGet packages"
        
        dotnet build -c Release
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
        Write-Success "Build completed"
    }
    catch {
        Write-Error-Custom "Build failed: $_"
        Pop-Location
        exit 1
    }
    
    Pop-Location
}

# ============================================
# BƯỚC 2: PUBLISH
# ============================================
function Publish-Project {
    Write-Info "Publishing to $PublishPath..."
    
    Push-Location $ProjectPath
    
    try {
        # Xóa thư mục cũ
        if (Test-Path $PublishPath) {
            Remove-Item $PublishPath -Recurse -Force
            Write-Info "Cleaned old publish folder"
        }
        
        dotnet publish -c Release -o $PublishPath
        if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
        Write-Success "Publish completed to $PublishPath"
    }
    catch {
        Write-Error-Custom "Publish failed: $_"
        Pop-Location
        exit 1
    }
    
    Pop-Location
}

# ============================================
# BƯỚC 3: DATABASE MIGRATION
# ============================================
function Run-Migrations {
    Write-Info "Running database migrations..."
    
    Push-Location $ProjectPath
    
    try {
        # Kiểm tra EF tools
        dotnet tool list --global | Select-String "dotnet-ef" > $null
        if ($LASTEXITCODE -ne 0) {
            Write-Info "Installing dotnet-ef tools..."
            dotnet tool install --global dotnet-ef
        }
        
        dotnet ef database update
        if ($LASTEXITCODE -ne 0) { throw "Migration failed" }
        Write-Success "Database migrations completed"
    }
    catch {
        Write-Error-Custom "Migration failed: $_"
        Pop-Location
        exit 1
    }
    
    Pop-Location
}

# ============================================
# BƯỚC 4: INSTALL WINDOWS SERVICE
# ============================================
function Install-Service {
    Write-Info "Installing Windows Service..."
    
    try {
        # Kiểm tra NSSM
        $nssm = Get-Command nssm -ErrorAction SilentlyContinue
        if (!$nssm) {
            Write-Error-Custom "NSSM not found. Install from: https://nssm.cc/download"
            Write-Info "After installing NSSM, run:"
            Write-Info "  nssm install $AppName `"$PublishPath\$AppName.exe`""
            return
        }
        
        # Check if service exists
        $service = Get-Service -Name $AppName -ErrorAction SilentlyContinue
        if ($service) {
            Write-Info "Service already exists, removing..."
            nssm remove $AppName confirm
            Start-Sleep -Seconds 2
        }
        
        # Install service
        nssm install $AppName "$PublishPath\$AppName.exe"
        nssm set $AppName AppParameters "--urls `"http://0.0.0.0:$Port`""
        nssm set $AppName AppDirectory "$PublishPath"
        nssm set $AppName AppExit Default Restart
        nssm set $AppName AppStdout "$PublishPath\logs\stdout.log"
        nssm set $AppName AppStderr "$PublishPath\logs\stderr.log"
        
        Write-Success "Service installed as $AppName"
    }
    catch {
        Write-Error-Custom "Service installation failed: $_"
        exit 1
    }
}

# ============================================
# BƯỚC 5: START SERVICE
# ============================================
function Start-ServiceApp {
    Write-Info "Starting service..."
    
    try {
        Start-Service -Name $AppName -ErrorAction Stop
        Start-Sleep -Seconds 2
        
        $service = Get-Service -Name $AppName
        if ($service.Status -eq "Running") {
            Write-Success "Service started successfully"
            Write-Info "Access application at: http://localhost:$Port"
        } else {
            Write-Error-Custom "Service failed to start"
            exit 1
        }
    }
    catch {
        Write-Error-Custom "Failed to start service: $_"
        exit 1
    }
}

# ============================================
# BƯỚC 6: STOP SERVICE
# ============================================
function Stop-ServiceApp {
    Write-Info "Stopping service..."
    
    try {
        Stop-Service -Name $AppName -Force
        Write-Success "Service stopped"
    }
    catch {
        Write-Error-Custom "Failed to stop service: $_"
    }
}

# ============================================
# BƯỚC 7: RUN DIRECTLY (TESTING)
# ============================================
function Run-Directly {
    Write-Info "Running application directly..."
    
    Push-Location $PublishPath
    
    Write-Success "Starting $AppName on port $Port"
    Write-Info "Press Ctrl+C to stop"
    
    & ".\$AppName.exe"
    
    Pop-Location
}

# ============================================
# BƯỚC 8: SHOW STATUS
# ============================================
function Show-Status {
    Write-Info "=== $AppName Status ==="
    
    $service = Get-Service -Name $AppName -ErrorAction SilentlyContinue
    if ($service) {
        Write-Host "Service: $($service.Status)" -ForegroundColor $(if ($service.Status -eq "Running") { "Green" } else { "Red" })
        Write-Host "Start Type: $($service.StartType)"
    } else {
        Write-Host "Service: Not installed" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Info "=== Web Application ==="
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$Port" -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Success "Application is accessible at http://localhost:$Port"
        }
    } catch {
        Write-Host "Application: Offline" -ForegroundColor Red
    }
}

# ============================================
# MAIN EXECUTION
# ============================================
Write-Host ""
Write-Host "╔════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║    $AppName Deployment Script           ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

switch ($Mode.ToLower()) {
    "full" {
        Write-Info "Running FULL deployment..."
        Build-Project
        Publish-Project
        Run-Migrations
        Install-Service
        Start-ServiceApp
        Show-Status
    }
    "build" {
        Build-Project
        Publish-Project
    }
    "publish" {
        Publish-Project
    }
    "migrate" {
        Run-Migrations
    }
    "run" {
        Run-Directly
    }
    "install-service" {
        Install-Service
    }
    "start-service" {
        Start-ServiceApp
    }
    "stop-service" {
        Stop-ServiceApp
    }
    "status" {
        Show-Status
    }
    default {
        Write-Error-Custom "Unknown mode: $Mode"
        Write-Info "Available modes:"
        Write-Host "  - full: Build, publish, migrate, install, start"
        Write-Host "  - build: Build & publish"
        Write-Host "  - publish: Publish only"
        Write-Host "  - migrate: Run database migrations"
        Write-Host "  - run: Run directly (for testing)"
        Write-Host "  - install-service: Install Windows Service"
        Write-Host "  - start-service: Start service"
        Write-Host "  - stop-service: Stop service"
        Write-Host "  - status: Show status"
        exit 1
    }
}

Write-Host ""
Write-Success "Deployment script completed!"
