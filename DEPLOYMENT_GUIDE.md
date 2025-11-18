# Ghid de Deployment MesEnterprise_v3

## Rezolvarea Erorilor de Compilare

### Problema: Erori CS0260 și CS0234

Dacă întâmpinați erori de tipul:
```
error CS0260: Missing partial modifier on declaration of type 'Program'
error CS0234: The type or namespace name 'Models' does not exist
```

**Cauză:** Ați descărcat codul din branch-ul `main` în loc de branch-ul `copilot/expand-mesentprise-v3-implementation` care conține toate implementările noi.

**Soluție:**

1. **Descărcați codul corect:**
   - Mergeți la: https://github.com/Ristian171/MesEnterprise_v3
   - Schimbați branch-ul la `copilot/expand-mesentprise-v3-implementation`
   - Click pe "Code" → "Download ZIP"
   
   SAU folosiți git:
   ```bash
   git clone https://github.com/Ristian171/MesEnterprise_v3.git
   cd MesEnterprise_v3
   git checkout copilot/expand-mesentprise-v3-implementation
   ```

2. **Verificați structura folderelor:**
   După descărcare, trebuie să aveți:
   ```
   MesEnterprise_v3/
   ├── Models/              ← IMPORTANT: Acest folder trebuie să existe
   │   ├── Core/
   │   ├── Production/
   │   ├── Maintenance/
   │   ├── Quality/
   │   ├── Inventory/
   │   ├── Alerts/
   │   ├── Export/
   │   └── Config/
   ├── Data/
   ├── Endpoints/
   ├── Services/
   ├── Program.cs           ← Doar un singur fișier Program.cs
   ├── MesEnterprise.csproj
   └── ...
   ```

3. **Verificați că NU există folder MesEnterprise/:**
   ```bash
   # Dacă există un subfolder MesEnterprise/, ȘTERGEȚI-L
   # Structura corectă: toate fișierele trebuie să fie la rădăcină
   ```

---

## Pași de Deployment

### 1. Prerequisite

**Windows:**
- .NET 8.0 SDK instalat: https://dotnet.microsoft.com/download/dotnet/8.0
- SQL Server instalat (LocalDB sau Express)
- IIS (opțional, pentru production)

Verificați instalarea:
```cmd
dotnet --version
# Trebuie să afișeze: 8.0.x
```

### 2. Configurare Baza de Date

**Editați `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MesEnterprise;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Sau pentru SQL Server Express:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=MesEnterprise;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Creare Baza de Date

```cmd
cd MesEnterprise_v3
dotnet ef database update
```

Aceasta va crea baza de date și va rula toate migrațiile.

### 4. Build și Test Local

```cmd
# Build
dotnet build -c Release

# Test run local
dotnet run --urls="http://localhost:5000;https://localhost:5001"
```

Deschideți browser: `https://localhost:5001`

### 5. Publish pentru Production

#### Opțiunea A: Self-Contained (Include .NET Runtime)

```cmd
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish

# Windows x86
dotnet publish -c Release -r win-x86 --self-contained true -o ./publish
```

**Avantaje:**
- Nu necesită .NET instalat pe server
- Mai mare (include runtime-ul)

#### Opțiunea B: Framework-Dependent (Mai mic)

```cmd
dotnet publish -c Release -o ./publish
```

**Avantaje:**
- Fișiere mai mici
- **Dezavantaje:** Necesită .NET 8.0 Runtime pe server

### 6. Deployment pe IIS (Production)

1. **Instalați ASP.NET Core Hosting Bundle:**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Căutați "Hosting Bundle"

2. **Creați Application Pool în IIS:**
   - Nume: MesEnterprise
   - .NET CLR version: No Managed Code
   - Managed pipeline mode: Integrated

3. **Creați Website în IIS:**
   - Site name: MesEnterprise
   - Physical path: C:\inetpub\wwwroot\MesEnterprise (copiați fișierele din ./publish aici)
   - Binding: http, port 80 sau https, port 443
   - Application pool: MesEnterprise

4. **Setați Permisiuni:**
   ```
   icacls "C:\inetpub\wwwroot\MesEnterprise" /grant "IIS AppPool\MesEnterprise:(OI)(CI)F" /T
   ```

5. **Configurați `appsettings.Production.json`:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=SERVER_NAME;Database=MesEnterprise;User Id=mes_user;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Warning"
       }
     }
   }
   ```

### 7. Deployment ca Windows Service

```cmd
# Publish
dotnet publish -c Release -o C:\MesEnterprise

# Install service
sc create MesEnterprise binPath="C:\MesEnterprise\MesEnterprise.exe" start=auto

# Start service
sc start MesEnterprise
```

**Sau folosiți tool NSSM (Non-Sucking Service Manager):**
```cmd
nssm install MesEnterprise "C:\MesEnterprise\MesEnterprise.exe"
nssm set MesEnterprise AppDirectory C:\MesEnterprise
nssm set MesEnterprise AppEnvironmentExtra ASPNETCORE_ENVIRONMENT=Production
nssm start MesEnterprise
```

### 8. Deployment cu Docker (Opțional)

Creați `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MesEnterprise.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MesEnterprise.dll"]
```

Build și Run:
```cmd
docker build -t mesenterprise .
docker run -d -p 5000:80 -p 5001:443 --name mes mesenterprise
```

---

## Verificare După Deployment

1. **Test Conectivitate:**
   - Deschideți `http://YOUR_SERVER:PORT`
   - Trebuie să vedeți pagina de login

2. **Test Bază de Date:**
   - Login cu user default (trebuie creat în DatabaseInitializer)
   - Verificați că paginile se încarcă

3. **Verificați Roluri:**
   ```sql
   SELECT * FROM Roles;
   -- Trebuie să vedeți 10 roluri
   ```

4. **Test Funcționalități:**
   - [ ] Login/Logout
   - [ ] Adăugare linie în Config
   - [ ] Linia apare în toate paginile (Staționări, Spare Parts, TPM)
   - [ ] Creare staționare tehnică
   - [ ] Export Excel

---

## Troubleshooting

### Eroare: "Database connection failed"
**Soluție:**
- Verificați connection string în `appsettings.json`
- Testați conexiunea SQL Server Management Studio
- Verificați că SQL Server service rulează

### Eroare: "HTTP Error 500.31 - Failed to load ASP.NET Core runtime"
**Soluție:**
- Instalați ASP.NET Core Hosting Bundle
- Restart IIS: `iisreset`

### Aplicația nu pornește
**Verificați logs:**
```
# Windows Service
C:\MesEnterprise\logs\

# IIS
C:\inetpub\logs\LogFiles\
Event Viewer → Windows Logs → Application
```

### Eroare: "Models namespace not found"
**Soluție:**
- Descărcați branch-ul corect: `copilot/expand-mesentprise-v3-implementation`
- Verificați că folderul `Models/` există
- Recompilați: `dotnet clean && dotnet build`

---

## Performance Tuning

### 1. Configurare SQL Server
```sql
-- Enable Query Store
ALTER DATABASE MesEnterprise SET QUERY_STORE = ON;

-- Indexes pentru performance
CREATE INDEX IX_ProductionLog_LineId ON ProductionLogs(LineId);
CREATE INDEX IX_InterventieTichet_LineId ON InterventieTichete(LineId);
CREATE INDEX IX_SparePartUsage_InterventieId ON SparePartUsages(InterventieTichetId);
```

### 2. Configurare IIS Application Pool
- Memory limit: 4GB+
- Idle timeout: 0 (pentru production cu trafic constant)
- Regular time interval (minutes): 1740 (29 ore - recycling daily)

### 3. Configurare appsettings.Production.json
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    }
  },
  "EnableSwagger": false
}
```

---

## Securitate

1. **Schimbați JWT Secret în production:**
   ```json
   {
     "Jwt": {
       "Key": "GENERATED_STRONG_KEY_HERE_MIN_32_CHARS"
     }
   }
   ```

2. **Enable HTTPS:**
   ```cmd
   # Generați certificat pentru development
   dotnet dev-certs https --trust
   ```

3. **Firewall Rules:**
   - Permiteți doar porturile necesare (80, 443)
   - Blocați acces direct la SQL Server din extern

---

## Backup

### Backup Bază de Date
```sql
-- Full backup
BACKUP DATABASE MesEnterprise 
TO DISK = 'C:\Backups\MesEnterprise_Full.bak'
WITH INIT, COMPRESSION;

-- Differential backup (daily)
BACKUP DATABASE MesEnterprise 
TO DISK = 'C:\Backups\MesEnterprise_Diff.bak'
WITH DIFFERENTIAL, COMPRESSION;
```

### Backup Fișiere
```cmd
# Backup logs
xcopy C:\MesEnterprise\logs C:\Backups\Logs /E /I /Y

# Backup appsettings
copy C:\MesEnterprise\appsettings.Production.json C:\Backups\
```

---

## Contact Support

Pentru probleme sau întrebări:
- GitHub Issues: https://github.com/Ristian171/MesEnterprise_v3/issues
- Email: [contact aici]

**Versiune:** 3.0 Production Ready
**Ultima actualizare:** 18 Noiembrie 2024
