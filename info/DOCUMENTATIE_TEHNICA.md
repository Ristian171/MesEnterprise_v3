# MES Enterprise - Technical Documentation

## System Overview

MES Enterprise is a comprehensive Manufacturing Execution System built on .NET 8.0, PostgreSQL, and modern web standards. The system provides real-time production monitoring, maintenance management, quality control, inventory tracking, and advanced analytics for manufacturing operations.

## Architecture

### Technology Stack
- **Backend**: .NET 8.0 Minimal APIs
- **Database**: PostgreSQL 15+ with EF Core 8.0
- **Authentication**: JWT (HS256) with BCrypt password hashing
- **Logging**: Serilog (Console + RollingFile)
- **ORM**: Entity Framework Core with snake_case naming convention
- **Export**: NPOI for Excel, RFC4180 CSV

### Project Structure
```
MesEnterprise/
├── Data/                         # EF Core DbContext
├── Models/                       # Domain entities (28 total)
│   ├── Core/                    # User, Role, Line, Equipment, Product
│   ├── Production/              # ProductionLog, LineStatus, ChangeoverLog
│   ├── Maintenance/             # InterventieTichet, PM plans
│   ├── Quality/                 # DefectCategory, QualityTest, MRB
│   ├── Inventory/               # SparePart, RawMaterial, ProductBOM
│   ├── Planning/                # ProductionWorkOrder
│   ├── Alerts/                  # AlertRule, AlertLog
│   ├── Export/                  # ExportJob, ExportTemplate
│   └── Config/                  # SystemSetting, StopOnDefectRule
├── Services/                     # Business logic services
│   ├── BackgroundServices/      # 8 hosted services for automation
│   ├── PasswordService.cs       # BCrypt hashing
│   ├── TokenService.cs          # JWT generation/validation
│   ├── ApiHelpers.cs            # Core business logic (OEE, targets)
│   └── DatabaseInitializer.cs   # Seed data
├── Infrastructure/               # Cross-cutting concerns
│   ├── GlobalExceptionHandlerMiddleware.cs
│   ├── SecurityHeadersMiddleware.cs
│   └── ModuleGateMiddleware.cs
├── DTOs/                         # Request/Response contracts
├── Endpoints/                    # API endpoint extensions
│   ├── AuthEndpoints.cs         # Authentication & user management
│   ├── ConfigEndpoints.cs       # Configuration management
│   ├── OperatorEndpoints.cs     # Operator dashboard
│   ├── ProductionEndpoints.cs   # Production logging
│   ├── InterventiiEndpoints.cs  # Maintenance tickets
│   ├── ChangeoverEndpoints.cs   # Changeover tracking
│   ├── PublicEndpoints.cs       # Health, Scan APIs
│   ├── ExportEndpoints.cs       # Export management
│   └── Enterprise/              # Enterprise feature endpoints
│       ├── PlanningEndpoints.cs # Work orders
│       └── AdminEndpoints.cs    # System administration
├── Migrations/                   # EF Core database migrations
└── Program.cs                    # Application bootstrap

wwwroot/                          # Static frontend assets
├── login.html                    # Authentication page
├── index.html                    # Operator dashboard
├── config*.html                  # Configuration pages
├── interventii.html              # Maintenance management
├── changeover.html               # Changeover interface
├── edit.html                     # Production log editing
├── js/                           # JavaScript modules
└── style.css                     # Global styles
```

## Core Components

### Authentication & Authorization

**JWT Configuration:**
- Algorithm: HS256
- Issuer/Audience: MES_Enterprise
- Expiration: Configurable (default 24h)
- Secret: Environment variable `MES_JWT_KEY` (256-bit minimum)

**Roles:**
- Admin - Full system access
- Operator - Production logging, operator dashboard
- InginerMentenanta - Maintenance management
- PlantManager - Strategic overview, analytics
- TeamLeader - Team coordination
- Quality - Quality management
- Warehouse - Inventory management
- Planner - Production planning

**Authorization Policies:**
- AdminOnly: Requires Admin role
- TechOrAdmin: Admin or InginerMentenanta
- OperatorOrHigher: Any authenticated role

### Database Design

**Performance Indexes:**
- `production_logs(line_id, timestamp)` - Core query optimization
- `alert_logs(triggered_at, rule_id)` - Alert retrieval
- `changeover_logs(line_id, start_time)` - Changeover history
- `interventie_tichete(status, data_raportare_operator)` - Maintenance tracking

**Relationships:**
- One-to-Many: Line → Equipment, Line → ProductionLogs
- Many-to-Many: User → Roles (through RolePermission)
- Optional Foreign Keys: ProductionLog → ProductionWorkOrder (enterprise feature)

### Background Services

**1. JustificationCheckService** (Interval: 60 min)
- Monitors OEE justification requirements
- Identifies logs below threshold needing justification

**2. AutoBackupService** (Interval: 24 hours)
- Scheduled database backups via pg_dump
- Configurable retention policy

**3. PmSchedulerService** (Interval: 4 hours)
- Preventive Maintenance schedule monitoring
- Automatic work order generation (future)

**4. AlertScannerService** (Interval: 5 minutes)
- Real-time alert rule evaluation
- Triggers notifications based on conditions

**5. EquipmentHourTrackingService** (Interval: 1 hour)
- Automatic equipment operating hour increments
- Supports maintenance scheduling

**6. InventoryAlertService** (Interval: 6 hours)
- Low stock monitoring
- Reorder point notifications

**7. TokenCleanupService** (Interval: 12 hours)
- Expired token cleanup
- Authentication hygiene

**8. ExportWorkerService** (On-demand via queue)
- Async export job processing
- Channel-based queue for scalability

### Middleware Pipeline

**Order:**
1. CORS
2. HSTS (Production only)
3. SecurityHeadersMiddleware
4. GlobalExceptionHandlerMiddleware
5. Swagger (Development only)
6. Static Files
7. Authentication
8. Authorization
9. ModuleGateMiddleware
10. Endpoint Routing

**Security Headers Applied:**
- Strict-Transport-Security (HSTS)
- X-Frame-Options: DENY
- X-Content-Type-Options: nosniff
- Referrer-Policy: strict-origin-when-cross-origin
- Content-Security-Policy: (minimal, SSE-compatible)

## API Endpoints

### Core APIs

**Authentication (`/api/auth`, `/api/admin/users`)**
- POST /api/auth/login - User authentication
- GET /api/admin/users - List users
- POST /api/admin/users - Create user
- PUT /api/admin/users/{id} - Update user
- DELETE /api/admin/users/{id} - Delete user

**Configuration (`/api/config`)**
- Lines, Products, Shifts, Equipment CRUD
- DefectCategories, DefectCodes management
- SystemSettings updates
- Import/Export functionality

**Operator (`/api/operator`)**
- GET /api/operator/state - Complete dashboard state
- POST /api/operator/command - Start/Stop/Breakdown
- POST /api/operator/session - Session management
- GET /api/operator/history-preview - Shift logs

**Production (`/api/productionlogs`, `/api/edit`)**
- POST /api/productionlogs - Create/update log
- GET /api/productionlogs - List logs
- GET /api/edit/logs-by-date - Editing interface data

**Maintenance (`/api/interventii`)**
- GET /api/interventii - List tickets (filterable)
- POST /api/interventii - Create ticket
- PUT /api/interventii/{id} - Update ticket

**Changeover (`/api/changeover`)**
- POST /api/changeover/start - Initiate changeover
- POST /api/changeover/end - Complete changeover
- GET /api/changeover/history - Changeover logs

**Public (`/api/scan`, `/api/public`)**
- GET /api/scan/status-by-identifier/{identifier}
- POST /api/scan/log - Log scanned parts
- GET /api/public/health - Health check

### Enterprise APIs

**Planning (`/api/planning`)**
- GET /api/planning/workorders - List work orders
- POST /api/planning/workorders - Create work order
- GET /api/planning/workorders/active/{lineId} - Active WO

**Admin (`/api/admin`)**
- POST /api/admin/backup - Database backup
- POST /api/admin/optimize - VACUUM ANALYZE
- GET /api/admin/stats - System statistics
- DELETE /api/admin/logs/cleanup - Log cleanup

## Deployment

### Environment Variables
```bash
# Required
MES_CONN_STRING="Host=localhost;Database=mesdb;Username=postgres;Password=admin"
MES_JWT_KEY="<256-bit-secret-key>"

# Optional
ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="http://0.0.0.0:5000"
```

### Database Setup
```bash
# Install PostgreSQL 15+
# Create database
createdb mesdb

# Apply migrations
dotnet ef database update

# Database will be seeded on first run with:
# - 8 default roles
# - Admin user (admin/admin)
# - 3 default shifts
# - System settings
```

### Running the Application
```bash
# Development
dotnet run

# Production
dotnet publish -c Release
dotnet MesEnterprise.dll
```

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
ENV MES_CONN_STRING="Host=postgres;Database=mesdb;Username=postgres;Password=admin"
ENV MES_JWT_KEY="your-secret-key"
EXPOSE 5000
ENTRYPOINT ["dotnet", "MesEnterprise.dll"]
```

## Performance Tuning

### Database Optimization
- Regular VACUUM ANALYZE (via Admin API)
- Index maintenance with REINDEX
- Connection pooling (default in Npgsql)
- Query splitting for complex includes

### Application Optimization
- AsNoTracking() for read-only queries
- Async/await throughout
- Minimal API for reduced overhead
- Background service intervals tunable

### Scaling Considerations
- Horizontal scaling: Stateless API design
- Database: Read replicas for analytics
- Background services: Single instance or leader election
- Static files: CDN distribution

## Security

### Best Practices Implemented
- ✅ Environment variables for secrets
- ✅ BCrypt password hashing
- ✅ JWT token-based authentication
- ✅ Role-based authorization
- ✅ HTTPS enforcement (HSTS in production)
- ✅ Security headers
- ✅ SQL injection prevention (parameterized queries via EF Core)
- ✅ XSS prevention (Content-Security-Policy)

### Security Checklist
- [ ] Change default admin password immediately
- [ ] Rotate JWT key regularly
- [ ] Configure CORS for specific origins only
- [ ] Enable HTTPS with valid certificate
- [ ] Regular security updates
- [ ] Database backup encryption
- [ ] Audit log review

## Monitoring & Observability

### Logging
- **Serilog Sinks**: Console, RollingFile
- **Log Levels**: Debug, Information, Warning, Error, Fatal
- **Log Location**: `logs/mes_log_YYYYMMDD.txt`
- **Retention**: 30 days (configurable)

### Health Checks
- Database connectivity test
- API availability
- Endpoint: GET /api/public/health

### Metrics (Future Enhancement)
- Request rate, latency
- Production throughput
- OEE trends
- Alert frequency

## Troubleshooting

### Common Issues

**1. Database Connection Failed**
```
Solution: Verify MES_CONN_STRING, ensure PostgreSQL is running
```

**2. JWT Key Not Configured**
```
Solution: Set MES_JWT_KEY environment variable
```

**3. Migration Errors**
```
Solution: Drop database and recreate, or use dotnet ef migrations
```

**4. 403 Forbidden on Module**
```
Solution: Check SystemSettings, enable required module
```

### Debug Mode
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
# Access Swagger UI at /swagger
```

## Future Enhancements

### Planned Features
- [ ] SignalR for real-time updates (alternative to SSE)
- [ ] Mobile app integration
- [ ] Machine learning for predictive maintenance
- [ ] Advanced analytics dashboards
- [ ] Multi-tenant support
- [ ] Internationalization (i18n)
- [ ] PLC direct integration protocols
- [ ] SMS/Email notification integration
- [ ] Document management system
- [ ] Audit trail enhancements

### Scalability Roadmap
- [ ] Redis caching layer
- [ ] Message queue (RabbitMQ/Azure Service Bus)
- [ ] Microservices decomposition
- [ ] Kubernetes deployment
- [ ] Database sharding

## Support & Maintenance

### Version History
See CHANGELOG.md for detailed version history.

### License
Proprietary - © 2025 M.E.S - Made by Joja Cristian

### Contact
For technical support or feature requests, contact the development team.

---

*Last Updated: 2025-11-17*
*Version: 1.0.0*
