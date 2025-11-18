@echo off
REM ============================================================================
REM MesEnterprise_v3 - Script de Publicare
REM ============================================================================

echo.
echo ============================================================================
echo MesEnterprise_v3 - Publish Script
echo ============================================================================
echo.

REM Verificare .NET SDK
echo [1/6] Verificare .NET SDK...
dotnet --version > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo EROARE: .NET SDK nu este instalat!
    echo Descarcati de la: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)
echo OK - .NET SDK gasit

REM Verificare structura proiect
echo.
echo [2/6] Verificare structura proiect...
if not exist "Models" (
    echo.
    echo ============================================================================
    echo EROARE CRITICA: Folderul Models/ lipseste!
    echo ============================================================================
    echo.
    echo Ati descarcat branch-ul gresit de pe GitHub.
    echo.
    echo SOLUTIE:
    echo 1. Mergeti la: https://github.com/Ristian171/MesEnterprise_v3
    echo 2. Schimbati branch-ul la: copilot/expand-mesentprise-v3-implementation
    echo 3. Click "Code" -^> "Download ZIP"
    echo 4. Extrageti si rulati din nou acest script
    echo.
    echo SAU folositi git:
    echo    git clone https://github.com/Ristian171/MesEnterprise_v3.git
    echo    cd MesEnterprise_v3
    echo    git checkout copilot/expand-mesentprise-v3-implementation
    echo.
    pause
    exit /b 1
)
echo OK - Structura proiect corecta

REM Verificare fișier duplicat Program.cs
echo.
echo [3/6] Verificare fisiere duplicate...
if exist "MesEnterprise\Program.cs" (
    echo AVERTISMENT: Gasit Program.cs duplicat in folderul MesEnterprise\
    echo Stergere fisier duplicat...
    rd /s /q "MesEnterprise"
)
echo OK - Nu exista fisiere duplicate

REM Clean
echo.
echo [4/6] Curatare build anterior...
dotnet clean -c Release > nul 2>&1
if exist "publish" (
    rd /s /q "publish"
)
echo OK - Build anterior sters

REM Build
echo.
echo [5/6] Compilare proiect...
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ============================================================================
    echo EROARE: Compilarea a esuat!
    echo ============================================================================
    echo.
    echo Verificati erorile de mai sus.
    echo.
    echo Probleme comune:
    echo - Lipsesc pachete NuGet: rulati "dotnet restore"
    echo - Connection string gresit in appsettings.json
    echo - Lipsesc fisiere Models/
    echo.
    pause
    exit /b 1
)
echo OK - Compilare reusita

REM Publish
echo.
echo [6/6] Publicare aplicatie...
echo.
echo Alegeti tipul de publicare:
echo 1. Framework-Dependent (mai mic, necesita .NET Runtime pe server)
echo 2. Self-Contained Windows x64 (include .NET Runtime, mai mare)
echo 3. Self-Contained Windows x86 (include .NET Runtime, 32-bit)
echo.
set /p choice="Alegeti optiunea (1-3): "

if "%choice%"=="1" (
    echo.
    echo Publishing Framework-Dependent...
    dotnet publish -c Release -o ./publish
) else if "%choice%"=="2" (
    echo.
    echo Publishing Self-Contained (Windows x64)...
    dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
) else if "%choice%"=="3" (
    echo.
    echo Publishing Self-Contained (Windows x86)...
    dotnet publish -c Release -r win-x86 --self-contained true -o ./publish
) else (
    echo Optiune invalida!
    pause
    exit /b 1
)

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo EROARE: Publicarea a esuat!
    pause
    exit /b 1
)

echo.
echo ============================================================================
echo SUCCESS! Aplicatia a fost publicata cu succes!
echo ============================================================================
echo.
echo Fisierele se gasesc in: .\publish\
echo.
echo URMATORI PASI:
echo.
echo 1. CONFIGURARE BAZA DE DATE:
echo    - Editati publish\appsettings.json
echo    - Setati Connection String corect
echo    - Rulati: dotnet ef database update
echo.
echo 2. TESTARE LOCALA:
echo    - cd publish
echo    - MesEnterprise.exe
echo    - Deschideti browser: https://localhost:5001
echo.
echo 3. DEPLOYMENT IIS:
echo    - Copiati continutul din .\publish\ in C:\inetpub\wwwroot\MesEnterprise
echo    - Configurati Application Pool (No Managed Code)
echo    - Setati permisiuni: IIS AppPool\MesEnterprise
echo    - Instalati ASP.NET Core Hosting Bundle daca nu este deja
echo.
echo 4. DEPLOYMENT WINDOWS SERVICE:
echo    - sc create MesEnterprise binPath="C:\Path\To\publish\MesEnterprise.exe"
echo    - sc start MesEnterprise
echo.
echo Consultati DEPLOYMENT_GUIDE.md pentru detalii complete!
echo.
pause
