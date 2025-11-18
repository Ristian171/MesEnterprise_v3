# MES Enterprise - Manufacturing Execution System

## 🎯 Project Status: Foundation Complete (Phase 1 of 7)

This repository contains the enterprise-grade refactoring of the MesSimplu prototype into a modular, scalable Manufacturing Execution System.

### ✅ What's Implemented (Phase 1)

**Complete and Working:**
- ✅ **Builds successfully** (`dotnet build` - 0 errors, 0 warnings)
- ✅ **28 domain entities** across 8 business domains
- ✅ **Modular architecture** with clean separation of concerns
- ✅ **Security infrastructure** (JWT, BCrypt, RBAC foundation, middleware)
- ✅ **Database context** with proper relationships and indexes
- ✅ **Core services** (Password, Token, ApiHelpers)
- ✅ **Serilog logging** configured
- ✅ **Environment variable** configuration support

### Architecture

```
MesEnterprise/
├── Data/
│   └── MesDbContext.cs                 # EF Core context with all entities
├── Models/
│   ├── Core/                           # User, Role, Permission, Department, Line, Equipment, Product, etc.
│   ├── Production/                     # ProductionLog, LineStatus, ChangeoverLog
│   ├── Maintenance/                    # InterventieTichet, PM plans, CAPA
│   ├── Quality/                        # Defect tracking, Tests, MRB
│   ├── Inventory/                      # SpareParts, RawMaterial, ProductBOM
│   ├── Planning/                       # ProductionWorkOrder
│   ├── Alerts/                         # AlertRule, AlertLog
│   ├── Export/                         # ExportJob, ExportTemplate
│   └── Config/                         # SystemSetting, StopOnDefectRule
├── Services/
│   ├── PasswordService.cs              # BCrypt hashing
│   ├── TokenService.cs                 # JWT generation
│   └── ApiHelpers.cs                   # Business logic helpers
├── Infrastructure/
│   ├── GlobalExceptionHandlerMiddleware.cs
│   ├── SecurityHeadersMiddleware.cs
│   └── ModuleGateMiddleware.cs
├── DTOs/
│   ├── AuthDTOs.cs
│   └── ProductionDTOs.cs
├── Endpoints/                          # To be implemented
├── wwwroot/                            # Frontend assets (copied from MesSimplu)
├── Program.cs                          # Minimal APIs bootstrap
├── appsettings.json
└── MesEnterprise.csproj
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 12+
- (Optional) Docker for PostgreSQL

### Build
```bash
cd MesEnterprise
dotnet restore
dotnet build
```

### Configure
Set environment variables:
```bash
export MES_CONN_STRING="Host=localhost;Database=mesenterprise;Username=postgres;Password=yourpassword"
export MES_JWT_KEY="your-256-bit-secret-key-change-in-production"
```

Or update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mesenterprise;Username=postgres;Password=admin"
  },
  "JwtSettings": {
    "Key": "your-secret-key-here"
  }
}
```

### Run Migrations (once endpoints are implemented)
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Run
```bash
dotnet run
```

The application will start on `http://localhost:5000` (Development) or configured port.

## 📊 Domain Models

### Core Domain (11 entities)
- **User**: Authentication with RBAC
- **Role, Permission, RolePermission**: Role-based access control
- **Department**: Organizational units
- **Line**: Production lines with cost tracking, data acquisition mode
- **Equipment**: Equipment with operating hours tracking
- **Product**: Products with cycle times
- **BreakdownReason**: Downtime justification reasons
- **Shift, ShiftBreak**: Shift configuration
- **PlannedDowntime**: Dummy time configuration
- **ObservatieOperator**: Operator observations

### Production Domain (4 entities)
- **LineStatus**: Current line state
- **ProductionLog**: Hourly production logs with OEE data
- **ProductionLogDefect**: Defect allocations
- **ChangeoverLog**: Changeover tracking

### Planning Domain (1 entity)
- **ProductionWorkOrder**: Work order management

### Maintenance Domain (5 entities)
- **InterventieTichet**: Maintenance tickets (Corrective, Preventive, Predictive)
- **ProblemaRaportata, DefectiuneIdentificata**: Problem-defect correlation
- **ProblemaDefectiuneCorelatie**: Many-to-many relationship
- **PreventiveMaintenancePlan**: PM scheduling

### Quality Domain (5 entities)
- **DefectCategory, DefectCode**: Defect classification
- **QualityTest**: Quality test definitions
- **ProductionLogQualityCheck**: Quality checks on production
- **MrbTicket**: Material Review Board tickets

### Inventory Domain (3 entities)
- **SparePart**: Spare parts inventory
- **RawMaterial**: Raw material tracking
- **ProductBOM**: Bill of materials

### Alerts Domain (2 entities)
- **AlertRule**: Configurable alert rules
- **AlertLog**: Triggered alerts

### Export Domain (2 entities)
- **ExportJob**: Async export jobs
- **ExportTemplate**: Export templates

### Config Domain (2 entities)
- **SystemSetting**: System-wide settings and module toggles
- **StopOnDefectRule**: Stop-on-defect configurations

## 🔒 Security Features

### Authentication & Authorization
- JWT HS256 tokens (8-hour expiration)
- BCrypt password hashing
- RBAC with 8 roles:
  - Admin
  - Operator
  - InginerMentenanta (Maintenance Engineer)
  - PlantManager
  - TeamLeader
  - Quality
  - Warehouse
  - Planner

### Security Headers
- HSTS (production only)
- X-Frame-Options: DENY
- X-Content-Type-Options: nosniff
- Referrer-Policy: strict-origin-when-cross-origin
- Content-Security-Policy (minimal, SSE-compatible)

### Module Gating
ModuleGateMiddleware enforces module access based on SystemSettings:
- Production_Module_Enabled
- PM_Module_Enabled
- TPM_Module_Enabled
- Quality_Module_Enabled
- Inventory_Module_Enabled
- Analysis_Module_Enabled
- Alerts_Module_Enabled
- Export_Module_Enabled
- LiveView_Module_Enabled

## 📝 Configuration

### Environment Variables (Production Recommended)
- `MES_CONN_STRING`: PostgreSQL connection string
- `MES_JWT_KEY`: JWT signing key (256-bit recommended)
- `ASPNETCORE_ENVIRONMENT`: Development/Production

### System Settings (Database)
Module toggles and operational settings stored in `SystemSettings` table:
- Module enable/disable flags
- GoodPartsLoggingMode (Overwrite/Aggregate)
- DowntimeScrapLoggingMode
- JustificationThresholdPercent
- RequireJustification
- Auto backup settings

## 🔍 Observability

### Logging (Serilog)
- Console sink (Development)
- Rolling file sink (`logs/mes_log_YYYYMMDD.txt`, 30-day retention)
- Database sink (planned)
- Structured logging with correlation IDs

### Health Check
```bash
curl http://localhost:5000/api/public/health
```

## 🎨 Frontend

Frontend assets copied from MesSimplu prototype:
- login.html, index.html (operator dashboard)
- Configuration pages (config/*.html)
- Maintenance tracking (interventii.html)
- Changeover management (changeover.html)
- Scanner interface (scan.html)

**Rebranding Pending**: Update to "M.E.S - Made by Joja Cristian" branding.

## 📋 Remaining Work

See [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) for detailed tracking.

### Priority 1: Core Functionality
1. **API Endpoints** (6-8 hours)
   - Migrate AuthApi, ConfigApi, OperatorApi, InterventiiApi, ExportApi from MesSimplu
   - Extract production/changeover/scan endpoints from original Program.cs
   
2. **Database Migrations** (1 hour)
   - Create initial migration
   - Seed data (admin user, default shifts, etc.)

3. **Basic Testing** (2 hours)
   - Smoke tests
   - Migration validation

**After Priority 1: System functionally equivalent to MesSimplu with enterprise architecture**

### Priority 2: Enterprise Extensions
4. **New Endpoints** (4-6 hours)
   - Planning (Work Orders)
   - Quality (Tests, MRB)
   - Inventory (BOM, Consumption)
   - Admin (Backup, Optimization)
   - Analysis (KPI, Pareto)
   - Live (SSE streaming)

5. **Background Services** (4-6 hours)
   - JustificationCheckService
   - PmSchedulerService
   - AlertScannerService
   - EquipmentHourTrackingService
   - ExportWorkerService
   - Others

6. **Frontend** (2-4 hours)
   - Rebrand existing pages
   - Create enterprise UI pages

### Priority 3: Polish
7. **Documentation** (2-3 hours)
8. **Comprehensive Testing** (3-4 hours)

**Total remaining: 24-36 development hours**

## 🛠 Development Guide

### Adding New Entity
1. Create model in appropriate `Models/{Domain}/` folder
2. Add DbSet to `MesDbContext.cs`
3. Configure relationships in `OnModelCreating`
4. Create DTOs in `DTOs/` if needed
5. Generate migration: `dotnet ef migrations add Add{EntityName}`

### Adding New Endpoint
1. Create extension class in `Endpoints/`
2. Implement `public static IEndpointRouteBuilder Map{Domain}Api(this IEndpointRouteBuilder app)`
3. Register in `Program.cs`: `app.Map{Domain}Api();`

### Adding Background Service
1. Implement `BackgroundService` or `IHostedService`
2. Register in `Program.cs`: `builder.Services.AddHostedService<YourService>();`

## 📚 API Documentation

Once endpoints are implemented, Swagger UI available at:
- Development: `http://localhost:5000/swagger`

## 🤝 Contributing

This is a single-developer enterprise refactoring project. External contributions not currently accepted.

## 📄 License

Proprietary - © 2025 Joja Cristian

## 🆘 Support

For issues or questions, contact the development team.

---

**Note**: This is a work in progress. Phase 1 (Foundation) is complete and builds successfully. Phases 2-7 are in progress.

