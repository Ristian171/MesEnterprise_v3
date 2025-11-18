# Changelog - MES Enterprise

All notable changes to this project will be documented in this file.

## [1.0.0] - 2025-11-17

### Added - Complete Enterprise Refactoring

#### Architecture & Foundation
- Modular Minimal API architecture replacing 3,407-line monolith
- 28 domain entities across 8 domains (Core, Production, Maintenance, Quality, Inventory, Planning, Alerts, Export, Config)
- EF Core with PostgreSQL and snake_case naming convention
- Comprehensive migration system (InitialCreate - 20251117112058)
- Environment variable support for secrets (MES_CONN_STRING, MES_JWT_KEY)

#### Security & Authentication
- JWT HS256 authentication with configurable secret
- BCrypt password hashing
- 8 enterprise roles (Admin, Operator, InginerMentenanta, PlantManager, TeamLeader, Quality, Warehouse, Planner)
- 3 authorization policies (AdminOnly, TechOrAdmin, OperatorOrHigher)
- Security headers middleware (HSTS, X-Frame-Options, CSP, Referrer-Policy)
- ModuleGateMiddleware for feature gating
- GlobalExceptionHandlerMiddleware for structured error handling

#### Core API Endpoints (30+)
- **AuthEndpoints**: Login, user management (CRUD)
- **ConfigEndpoints**: Lines, Products, Shifts, Equipment, Defects, Settings management
- **OperatorEndpoints**: Dashboard state, commands (Start/Stop/Breakdown), session management
- **ProductionEndpoints**: Production log CRUD, editing interface
- **InterventiiEndpoints**: Maintenance ticket lifecycle
- **ChangeoverEndpoints**: Product changeover tracking
- **PublicEndpoints**: Health check, scanner integration

#### Enterprise Extensions
- **PlanningEndpoints**: Work order management (CRUD), active WO tracking
- **AdminEndpoints**: Database backup, optimize (VACUUM), reindex, statistics, log cleanup
- **ExportEndpoints**: Export job management (stub for Phase 5)

#### Background Services (8 Automated Tasks)
1. **JustificationCheckService** (60 min) - OEE justification monitoring
2. **AutoBackupService** (24 hours) - Automated pg_dump backups
3. **PmSchedulerService** (4 hours) - Preventive maintenance scheduling
4. **AlertScannerService** (5 minutes) - Real-time alert rule evaluation
5. **EquipmentHourTrackingService** (1 hour) - Equipment operating hours
6. **InventoryAlertService** (6 hours) - Low stock monitoring
7. **TokenCleanupService** (12 hours) - Expired token cleanup
8. **ExportWorkerService** (on-demand) - Async export processing via Channel queue

#### Data Models
**Core Domain:**
- User, Role, Permission, RolePermission (RBAC foundation)
- Department (organizational hierarchy)
- Line (+ CostOperarePeOra, DataAcquisitionMode: Manual|LiveScan|PLC_Input)
- Equipment (+ OreFunctionare tracking)
- Product, BreakdownReason, Shift, ShiftBreak, PlannedDowntime

**Production Domain:**
- ProductionLog (+ ProductionWorkOrderId linkage)
- LineStatus (current state tracking)
- ChangeoverLog (ProductFromId/ProductToId, StartTime/EndTime)
- ProductionLogDefect (defect allocation)

**Maintenance Domain:**
- InterventieTichet (Guid UnicIdTicket, operator/technician workflow, CAPA fields)
- ProblemaRaportata, DefectiuneIdentificata (simplified catalog)
- ProblemaDefectiuneCorelatie (correlation tracking)
- PreventiveMaintenancePlan (PM scheduling)

**Quality Domain:**
- DefectCategory, DefectCode
- QualityTest (test definitions)
- ProductionLogQualityCheck (automatic NRFT checks)
- MrbTicket (Material Review Board)

**Inventory Domain:**
- SparePart (QuantityInStock, MinimumStock)
- RawMaterial (QuantityInStock, MinimumStock, Unit)
- ProductBOM (Bill of Materials)

**Planning Domain:**
- ProductionWorkOrder (PlannedQuantity, ProducedQuantity, status tracking)

**Alerts Domain:**
- AlertRule (rule definitions with condition_json)
- AlertLog (triggered alerts with severity)

**Export Domain:**
- ExportJob (async job tracking)
- ExportTemplate (reusable export definitions)

**Config Domain:**
- SystemSetting (module toggles, operational settings)
- StopOnDefectRule (quality control rules)

#### Database Performance
- **Indexes**: ProductionLog(LineId, Timestamp), AlertLog(TriggeredAt, RuleId), ChangeoverLog(LineId, StartTime), InterventieTichet(Status, DataRaportareOperator)
- **Unique Constraints**: LineStatus.LineId, Line.ScanIdentifier
- **Relationships**: Properly configured One-to-Many and Many-to-Many
- **Migration**: Full schema with 28 tables

#### Services & Business Logic
- **ApiHelpers**: Core business logic (calculate target, OEE, shift handling, justification checks)
- **TokenService**: JWT generation/validation with HS256
- **PasswordService**: BCrypt hashing
- **DatabaseInitializer**: Seed data (8 roles, admin user, shifts, settings)

#### Middleware & Infrastructure
- **SecurityHeadersMiddleware**: HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, CSP
- **ModuleGateMiddleware**: 403 if module disabled via SystemSettings
- **GlobalExceptionHandlerMiddleware**: Structured error responses

#### Logging & Observability
- Serilog configuration (Console + RollingFile)
- Log location: `logs/mes_log_YYYYMMDD.txt`
- 30-day retention
- Comprehensive logging in all services and endpoints

#### Frontend Assets
- Complete operator dashboard (index.html)
- Configuration pages (config-*.html)
- Maintenance interface (interventii.html)
- Changeover interface (changeover.html)
- Production log editing (edit.html)
- Login page (login.html)
- JavaScript modules (auth.js, config.js, operator.js, etc.)
- Global styling (style.css)

#### Configuration
- appsettings.json (base configuration)
- appsettings.Development.json (dev overrides)
- appsettings.Production.json (production settings)
- Environment variable priority for secrets

#### Documentation
- DOCUMENTATIE_TEHNICA.md (architecture, deployment, scaling)
- MANUAL_BAZA_DE_DATE.md (schemas, relationships, SQL queries, optimization)
- IMPLEMENTATION_STATUS.md (phase tracking)
- README_ENTERPRISE.md (developer guide)
- This CHANGELOG.md

### Changed
- **Namespace**: MesSimplu.* → MesEnterprise.*
- **Database name**: mesenterprise → mesdb (compatibility with original)
- **Architecture**: Monolithic Program.cs → Modular endpoint extensions
- **Models**: Single Models.cs (522 lines) → 28 individual entity files
- **User model**: string Role → Role navigation property (proper RBAC)

### Fixed
- Database connection configuration (now uses mesdb)
- Model property alignments (InterventieTichet, ProductionLog, etc.)
- DTO property completeness (OperatorStateDto, ProductionLogDto)
- SystemSetting constant references
- Background service property accesses

### Security
- No hardcoded secrets (all via environment variables)
- JWT tokens with HS256 and configurable secret
- BCrypt password hashing (cost factor 11)
- HTTPS enforcement via HSTS in production
- Security headers applied to all responses
- SQL injection prevention via EF Core parameterized queries
- XSS prevention via Content-Security-Policy

### Performance
- Indexed queries for production logs, alerts, maintenance
- AsNoTracking for read-only queries
- Query splitting for complex includes
- Async/await throughout
- Background service intervals optimized for load

### Breaking Changes
- API endpoint structure changed (old endpoint paths may not work)
- Database schema completely redesigned (migration required)
- Authentication now requires JWT tokens
- Configuration moved to SystemSettings table

### Migration Notes
From MesSimplu to MesEnterprise:
1. Export existing data from MesSimplu
2. Run `dotnet ef database update` to create MesEnterprise schema
3. Import data using custom migration scripts
4. Update frontend to call new endpoint paths
5. Configure environment variables (MES_CONN_STRING, MES_JWT_KEY)
6. Test all workflows thoroughly

### Known Issues
- Export functionality is stub implementation (full implementation in future)
- PM work order auto-creation pending in PmSchedulerService
- Alert rule evaluation needs custom logic per rule type
- SMS/Email notifications are placeholder endpoints

### Future Plans
- SignalR integration for real-time updates
- Additional enterprise endpoints (Quality, Inventory, Analysis, Live View)
- Frontend rebranding completion
- Mobile app integration
- Advanced analytics dashboards
- Machine learning for predictive maintenance
- Multi-tenant support
- Internationalization (i18n)

## Version History

### [0.9.0] - 2025-11-17 - Phase 4 Complete
- All 8 background services implemented
- Planning and Admin enterprise endpoints
- Model alignments completed
- Build succeeds (0 errors)

### [0.8.0] - 2025-11-17 - Phase 3 Complete
- All 8 core endpoint groups enabled
- Model alignment with original MesSimplu
- 30+ API endpoints operational

### [0.7.0] - 2025-11-17 - Phase 2 Complete
- Authentication endpoints (login, user management)
- Database migration generated
- Seed data initialization
- Health check endpoint

### [0.6.0] - 2025-11-17 - Phase 1 Complete
- Complete domain modeling (28 entities)
- Security infrastructure
- Services layer
- Configuration system

### [0.5.0] - 2025-11-17 - Foundation
- Project structure created
- Base models defined
- Initial middleware implemented

---

## Semantic Versioning

This project follows [Semantic Versioning](https://semver.org/):
- **MAJOR**: Incompatible API changes
- **MINOR**: New functionality (backwards-compatible)
- **PATCH**: Bug fixes (backwards-compatible)

---

*For detailed technical documentation, see DOCUMENTATIE_TEHNICA.md*
*For database schema details, see MANUAL_BAZA_DE_DATE.md*

© 2025 M.E.S - Made by Joja Cristian
