# MES Enterprise - Deployment Guide

## Overview
This guide provides step-by-step instructions for deploying MES Enterprise v3 in production environments (automotive manufacturing).

The application can be hosted in two ways:
1. **Self-hosted Kestrel** - Direct execution (recommended for development/testing)
2. **IIS (Windows Server)** - Enterprise hosting with IIS (recommended for production)

---

## Prerequisites

### Server Requirements
- **Operating System**: Windows Server 2016+ or Windows 10/11
- **RAM**: Minimum 4GB, recommended 8GB+
- **Storage**: 20GB+ free space
- **Network**: Access to PostgreSQL database server and NAS (if applicable)

### Software Requirements
1. **.NET 8.0 Runtime or SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - **For IIS hosting**: Install the ASP.NET Core Hosting Bundle (includes runtime + IIS module)
   - Verify installation: `dotnet --version` (should show 8.0.x)

2. **PostgreSQL Database** (12+)
   - Can be hosted on the same server or a dedicated database server/NAS
   - Network connectivity required between app server and database server

3. **IIS (for IIS hosting only)**
   - Available on Windows Server or Windows Pro editions
   - Enable via: Control Panel → Programs → Turn Windows features on/off → IIS

---

## Configuration

### 1. Database Connection String
The application needs to connect to a PostgreSQL database. Configure this in one of two ways:

#### Option A: Environment Variable (Recommended for Production)
```cmd
setx MES_CONN_STRING "Host=192.168.1.100;Port=5432;Database=mesdb;Username=mesuser;Password=SecurePassword123" /M
```

#### Option B: Configuration File (appsettings.json)
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=192.168.1.100;Port=5432;Database=mesdb;Username=mesuser;Password=SecurePassword123"
  }
}
```

**Connection String Parameters:**
- `Host`: IP address or hostname of PostgreSQL server
- `Port`: PostgreSQL port (default: 5432)
- `Database`: Database name
- `Username`: PostgreSQL user
- `Password`: User password

### 2. JWT Secret Key
Used for securing authentication tokens. **MUST be changed in production!**

#### Option A: Environment Variable (Recommended)
```cmd
setx MES_JWT_KEY "your-very-secure-256-bit-key-change-this-in-production" /M
```

#### Option B: Configuration File
Edit `appsettings.json`:
```json
{
  "JwtSettings": {
    "Key": "your-very-secure-256-bit-key-change-this-in-production",
    "Issuer": "MES_Enterprise",
    "Audience": "MES_Enterprise"
  }
}
```

### 3. Server URL Configuration
Control which URLs the server listens on:

#### Environment Variable:
```cmd
setx ASPNETCORE_URLS "http://0.0.0.0:5000" /M
```

**URL Options:**
- `http://localhost:5000` - Only accessible from local machine
- `http://0.0.0.0:5000` - Accessible from any network interface
- `http://*:5000` - Same as 0.0.0.0
- `http://192.168.1.50:5000` - Specific IP address only

---

## Deployment Method 1: Self-Hosted (Kestrel)

### When to Use
- Development/testing environments
- Quick deployment without IIS complexity
- Linux servers (if needed in future)
- Troubleshooting

### Step 1: Publish the Application
On your development machine or build server:

```bash
cd /path/to/MesEnterprise_v3
dotnet publish -c Release -o ./publish
```

This creates a `publish` folder with all necessary files.

### Step 2: Copy Files to Server
Copy the entire `publish` folder to your server, for example:
```
C:\inetpub\MesEnterprise\
```

### Step 3: Configure Environment Variables (if not done yet)
See [Configuration](#configuration) section above.

### Step 4: Test Database Connection
Before starting the full application, verify database connectivity:
```cmd
cd C:\inetpub\MesEnterprise
dotnet MesEnterprise.dll --help
```

If database connection fails, check:
- PostgreSQL is running and accessible
- Firewall rules allow connection to PostgreSQL port
- Connection string is correct

### Step 5: Run the Application

#### Manual Start (for testing):
```cmd
cd C:\inetpub\MesEnterprise
dotnet MesEnterprise.dll
```

You should see output like:
```
[08:55:42 INF] ==========================================================
[08:55:42 INF] MES Enterprise Server Starting
[08:55:42 INF] ==========================================================
[08:55:42 INF] Server listening on: http://localhost:5000
[08:55:42 INF] Environment: Production
[08:55:42 INF] ==========================================================
```

Access the application at: http://localhost:5000 (or configured URL)

#### Run as Windows Service (Production):
For production, use NSSM (Non-Sucking Service Manager) or sc.exe:

**Using NSSM:**
1. Download NSSM from: https://nssm.cc/download
2. Install service:
```cmd
nssm install MesEnterprise "C:\Program Files\dotnet\dotnet.exe" "C:\inetpub\MesEnterprise\MesEnterprise.dll"
nssm set MesEnterprise AppDirectory C:\inetpub\MesEnterprise
nssm set MesEnterprise DisplayName "MES Enterprise Server"
nssm set MesEnterprise Description "Manufacturing Execution System - Enterprise Edition"
nssm set MesEnterprise Start SERVICE_AUTO_START
nssm start MesEnterprise
```

**Check service status:**
```cmd
nssm status MesEnterprise
```

---

## Deployment Method 2: IIS Hosting (Recommended for Production)

### When to Use
- Production environments
- Need for centralized Windows authentication (future)
- Integration with existing IIS infrastructure
- Better process management and monitoring

### Prerequisites
1. **.NET 8.0 ASP.NET Core Hosting Bundle installed**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Select "Hosting Bundle" under "Run apps"
   - **Restart IIS after installation:** `iisreset`

2. **Verify IIS Module:**
```cmd
cd %windir%\System32\inetsrv
appcmd list modules | findstr AspNetCoreModuleV2
```
Should show: `AspNetCoreModuleV2`

### Step 1: Publish the Application
```bash
dotnet publish -c Release -o ./publish
```

### Step 2: Copy Files to IIS Root
Copy `publish` folder contents to:
```
C:\inetpub\MesEnterprise\
```

### Step 3: Configure web.config
The `web.config` file should already be in the publish folder. Verify/update it:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\MesEnterprise.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="MES_CONN_STRING" value="Host=192.168.1.100;Database=mesdb;Username=mesuser;Password=pass" />
          <environmentVariable name="MES_JWT_KEY" value="your-secure-jwt-key-here" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

### Step 4: Create Application Pool

1. Open **IIS Manager** (inetmgr)
2. Navigate to **Application Pools**
3. Click **Add Application Pool** (right panel)
4. **Configuration:**
   - Name: `MesEnterprisePool`
   - .NET CLR version: **No Managed Code**
   - Managed pipeline mode: **Integrated**
5. Click **OK**
6. Select the new pool → **Advanced Settings:**
   - **Process Model > Identity**: `ApplicationPoolIdentity` (or custom account with database permissions)
   - **Start Mode**: `AlwaysRunning` (optional, for faster first request)

### Step 5: Create IIS Site

1. In IIS Manager, navigate to **Sites**
2. Click **Add Website**
3. **Configuration:**
   - Site name: `MesEnterprise`
   - Application pool: `MesEnterprisePool`
   - Physical path: `C:\inetpub\MesEnterprise`
   - Binding:
     - Type: `http`
     - IP address: `All Unassigned` (or specific IP)
     - Port: `5000` (or your chosen port)
     - Host name: (leave empty or specify domain)
4. Click **OK**

### Step 6: Set Folder Permissions

The IIS application pool identity needs permissions:

```cmd
icacls "C:\inetpub\MesEnterprise" /grant "IIS AppPool\MesEnterprisePool:(OI)(CI)(RX)"
icacls "C:\inetpub\MesEnterprise\logs" /grant "IIS AppPool\MesEnterprisePool:(OI)(CI)(M)"
```

Create logs folder if it doesn't exist:
```cmd
mkdir C:\inetpub\MesEnterprise\logs
```

### Step 7: Start the Site

1. In IIS Manager, select your site
2. Click **Start** (right panel)
3. Click **Browse *.80 (http)** to test

### Step 8: Verify Deployment

**Check logs:**
```
C:\inetpub\MesEnterprise\logs\stdout_*.log
C:\inetpub\MesEnterprise\logs\mes_log_YYYYMMDD.txt
```

**Test access:**
- From server: http://localhost:5000
- From other machines: http://[server-ip]:5000

---

## Troubleshooting

### Issue: "Server not responding" or "Connection refused"

**Check 1: Is the application running?**
```cmd
netstat -ano | findstr :5000
```
Should show LISTENING on port 5000.

**Check 2: Firewall**
```cmd
netsh advfirewall firewall add rule name="MES Enterprise" dir=in action=allow protocol=TCP localport=5000
```

**Check 3: Application logs**
```
C:\inetpub\MesEnterprise\logs\mes_log_YYYYMMDD.txt
```

### Issue: "Database connection failed"

**Check 1: PostgreSQL is accessible**
```cmd
psql -h 192.168.1.100 -U mesuser -d mesdb
```

**Check 2: Firewall on database server**
Ensure port 5432 is open from app server IP.

**Check 3: Connection string is correct**
Verify in environment variables or appsettings.json.

### Issue: "502.5 Process Failure" (IIS only)

**Common causes:**
1. .NET 8.0 Runtime not installed → Install ASP.NET Core Hosting Bundle
2. web.config misconfigured → Check processPath and arguments
3. Application crash on startup → Check stdout logs

**Enable detailed errors:**
Edit web.config:
```xml
<aspNetCore stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" ...>
```

### Issue: "Access denied" or permission errors

**Grant permissions to IIS app pool:**
```cmd
icacls "C:\inetpub\MesEnterprise" /grant "IIS AppPool\MesEnterprisePool:(OI)(CI)(RX)"
```

---

## Post-Deployment

### Initial Setup

1. **Access the application:**
   - Navigate to: http://[server-ip]:5000/login.html

2. **First-time database initialization:**
   - On first startup, the application automatically:
     - Creates database if it doesn't exist
     - Runs migrations to create tables
     - Seeds initial data (admin user, default shifts, etc.)

3. **Default admin credentials:**
   - Check the logs or database for initial admin credentials
   - **Change immediately after first login!**

### Monitoring

**Application Logs:**
```
C:\inetpub\MesEnterprise\logs\mes_log_YYYYMMDD.txt
```

**IIS Logs:**
```
C:\inetpub\logs\LogFiles\W3SVC[site-id]\
```

### Backup Strategy

1. **Database backups** (automated via AutoBackupService)
   - Configured in SystemSettings table
   - Backups stored as configured

2. **Application files backup:**
   - Backup `C:\inetpub\MesEnterprise` before updates

### Updates

**To update the application:**

1. Stop the application/site
2. Backup current version
3. Copy new files over existing
4. Restart application/site
5. Verify functionality

---

## Security Checklist

- [ ] Change default JWT key
- [ ] Use strong PostgreSQL passwords
- [ ] Configure firewall rules (only necessary ports open)
- [ ] Enable HTTPS (recommended for production) - requires SSL certificate
- [ ] Regular database backups configured
- [ ] Application logs reviewed regularly
- [ ] Operating system and .NET runtime kept up to date
- [ ] Least-privilege principle for service accounts

---

## Network Access for Operators

**Scenario:** Operators access from PCs, tablets, phones

1. **Ensure server is reachable:**
   - Server IP: e.g., 192.168.1.50
   - Application listens on: http://0.0.0.0:5000 (all interfaces)

2. **Operators access via:**
   - PC browser: http://192.168.1.50:5000
   - Tablet browser: http://192.168.1.50:5000
   - Phone browser: http://192.168.1.50:5000

3. **For easier access, configure DNS:**
   - Set up local DNS entry: mes.local → 192.168.1.50
   - Operators use: http://mes.local:5000

4. **For HTTPS (recommended):**
   - Obtain SSL certificate
   - Configure in IIS or Kestrel
   - Access via: https://mes.local

---

## Support

For issues or questions:
1. Check logs: `logs/mes_log_YYYYMMDD.txt`
2. Review this guide's Troubleshooting section
3. Contact system administrator

---

**Document Version:** 1.0  
**Last Updated:** 2025-01-18  
**Application:** MES Enterprise v3
