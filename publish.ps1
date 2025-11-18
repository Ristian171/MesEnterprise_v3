# ============================================================================
# MesEnterprise_v3 - PowerShell Publish Script
# ============================================================================

Write-Host ""
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "MesEnterprise_v3 - Publish Script" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

# Function to check command exists
function Test-Command($command) {
    try {
        if (Get-Command $command -ErrorAction Stop) {
            return $true
        }
    } catch {
        return $false
    }
}

# [1/6] Verificare .NET SDK
Write-Host "[1/6] Verificare .NET SDK..." -ForegroundColor Yellow
if (-not (Test-Command "dotnet")) {
    Write-Host ""
    Write-Host "EROARE: .NET SDK nu este instalat!" -ForegroundColor Red
    Write-Host "Descarcati de la: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Read-Host "Apasati Enter pentru a iesi"
    exit 1
}

$dotnetVersion = dotnet --version
Write-Host "OK - .NET SDK $dotnetVersion gasit" -ForegroundColor Green

# [2/6] Verificare structura proiect
Write-Host ""
Write-Host "[2/6] Verificare structura proiect..." -ForegroundColor Yellow
if (-not (Test-Path "Models")) {
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host "EROARE CRITICA: Folderul Models/ lipseste!" -ForegroundColor Red
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Ati descarcat branch-ul gresit de pe GitHub." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "SOLUTIE:" -ForegroundColor Cyan
    Write-Host "1. Mergeti la: https://github.com/Ristian171/MesEnterprise_v3"
    Write-Host "2. Schimbati branch-ul la: copilot/expand-mesentprise-v3-implementation"
    Write-Host "3. Click 'Code' -> 'Download ZIP'"
    Write-Host "4. Extrageti si rulati din nou acest script"
    Write-Host ""
    Write-Host "SAU folositi git:" -ForegroundColor Cyan
    Write-Host "   git clone https://github.com/Ristian171/MesEnterprise_v3.git"
    Write-Host "   cd MesEnterprise_v3"
    Write-Host "   git checkout copilot/expand-mesentprise-v3-implementation"
    Write-Host ""
    Read-Host "Apasati Enter pentru a iesi"
    exit 1
}
Write-Host "OK - Structura proiect corecta" -ForegroundColor Green

# [3/6] Verificare fișiere duplicate
Write-Host ""
Write-Host "[3/6] Verificare fisiere duplicate..." -ForegroundColor Yellow
if (Test-Path "MesEnterprise\Program.cs") {
    Write-Host "AVERTISMENT: Gasit Program.cs duplicat in folderul MesEnterprise\" -ForegroundColor Yellow
    Write-Host "Stergere fisier duplicat..." -ForegroundColor Yellow
    Remove-Item -Path "MesEnterprise" -Recurse -Force
}
Write-Host "OK - Nu exista fisiere duplicate" -ForegroundColor Green

# [4/6] Clean
Write-Host ""
Write-Host "[4/6] Curatare build anterior..." -ForegroundColor Yellow
dotnet clean -c Release *> $null
if (Test-Path "publish") {
    Remove-Item -Path "publish" -Recurse -Force
}
Write-Host "OK - Build anterior sters" -ForegroundColor Green

# [5/6] Build
Write-Host ""
Write-Host "[5/6] Compilare proiect..." -ForegroundColor Yellow
$buildResult = dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host "EROARE: Compilarea a esuat!" -ForegroundColor Red
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Verificati erorile de mai sus." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Probleme comune:" -ForegroundColor Cyan
    Write-Host "- Lipsesc pachete NuGet: rulati 'dotnet restore'"
    Write-Host "- Connection string gresit in appsettings.json"
    Write-Host "- Lipsesc fisiere Models/"
    Write-Host ""
    Read-Host "Apasati Enter pentru a iesi"
    exit 1
}
Write-Host "OK - Compilare reusita" -ForegroundColor Green

# [6/6] Publish
Write-Host ""
Write-Host "[6/6] Publicare aplicatie..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Alegeti tipul de publicare:" -ForegroundColor Cyan
Write-Host "1. Framework-Dependent (mai mic, necesita .NET Runtime pe server)"
Write-Host "2. Self-Contained Windows x64 (include .NET Runtime, mai mare)"
Write-Host "3. Self-Contained Windows x86 (include .NET Runtime, 32-bit)"
Write-Host ""
$choice = Read-Host "Alegeti optiunea (1-3)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "Publishing Framework-Dependent..." -ForegroundColor Cyan
        dotnet publish -c Release -o ./publish
    }
    "2" {
        Write-Host ""
        Write-Host "Publishing Self-Contained (Windows x64)..." -ForegroundColor Cyan
        dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
    }
    "3" {
        Write-Host ""
        Write-Host "Publishing Self-Contained (Windows x86)..." -ForegroundColor Cyan
        dotnet publish -c Release -r win-x86 --self-contained true -o ./publish
    }
    default {
        Write-Host "Optiune invalida!" -ForegroundColor Red
        Read-Host "Apasati Enter pentru a iesi"
        exit 1
    }
}

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "EROARE: Publicarea a esuat!" -ForegroundColor Red
    Read-Host "Apasati Enter pentru a iesi"
    exit 1
}

Write-Host ""
Write-Host "============================================================================" -ForegroundColor Green
Write-Host "SUCCESS! Aplicatia a fost publicata cu succes!" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Fisierele se gasesc in: .\publish\" -ForegroundColor Cyan
Write-Host ""
Write-Host "URMATORI PASI:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. CONFIGURARE BAZA DE DATE:" -ForegroundColor Cyan
Write-Host "   - Editati publish\appsettings.json"
Write-Host "   - Setati Connection String corect"
Write-Host "   - Rulati: dotnet ef database update"
Write-Host ""
Write-Host "2. TESTARE LOCALA:" -ForegroundColor Cyan
Write-Host "   - cd publish"
Write-Host "   - .\MesEnterprise.exe"
Write-Host "   - Deschideti browser: https://localhost:5001"
Write-Host ""
Write-Host "3. DEPLOYMENT IIS:" -ForegroundColor Cyan
Write-Host "   - Copiati continutul din .\publish\ in C:\inetpub\wwwroot\MesEnterprise"
Write-Host "   - Configurati Application Pool (No Managed Code)"
Write-Host "   - Setati permisiuni: IIS AppPool\MesEnterprise"
Write-Host "   - Instalati ASP.NET Core Hosting Bundle daca nu este deja"
Write-Host ""
Write-Host "4. DEPLOYMENT WINDOWS SERVICE:" -ForegroundColor Cyan
Write-Host "   - sc create MesEnterprise binPath=`"C:\Path\To\publish\MesEnterprise.exe`""
Write-Host "   - sc start MesEnterprise"
Write-Host ""
Write-Host "Consultati DEPLOYMENT_GUIDE.md pentru detalii complete!" -ForegroundColor Yellow
Write-Host ""
Read-Host "Apasati Enter pentru a iesi"
