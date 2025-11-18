# MesEnterprise v3

Manufacturing Execution System - Enterprise Edition

## Overview

MesEnterprise is a comprehensive Manufacturing Execution System (MES) built with ASP.NET Core 8.0 and PostgreSQL. It provides production tracking, maintenance management, quality control, and real-time monitoring capabilities for manufacturing environments.

## Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL 12 or later
- (Optional) Entity Framework Core tools: `dotnet tool install --global dotnet-ef`

## Environment Variables

The application requires the following environment variables:

- `MES_CONN_STRING`: PostgreSQL connection string (format: `Host=localhost;Database=mes;Username=postgres;Password=yourpassword`)
- `MES_JWT_KEY`: Secret key for JWT token generation (minimum 32 characters recommended)

Example on Windows:
```cmd
set MES_CONN_STRING=Host=localhost;Database=mes;Username=postgres;Password=yourpassword
set MES_JWT_KEY=your-secret-jwt-key-at-least-32-characters-long
```

Example on Linux/Mac:
```bash
export MES_CONN_STRING="Host=localhost;Database=mes;Username=postgres;Password=yourpassword"
export MES_JWT_KEY="your-secret-jwt-key-at-least-32-characters-long"
```

## Building the Project

```bash
dotnet build
```

## Database Setup

1. Ensure PostgreSQL is running and accessible
2. Set the `MES_CONN_STRING` environment variable
3. Apply database migrations:

```bash
dotnet ef database update
```

The database will be automatically initialized with:
- Default roles (Admin, Operator, InginerMentenanta, PlantManager, TeamLeader, Quality, Warehouse, Planner)
- Admin user (username: `admin`, password: `admin`)
- Default shifts (Shift 1, 2, 3)
- System settings with module toggles

## Running the Application

```bash
dotnet run
```

The application will be available at:
- Development: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger (Development mode only)

## Publishing

To create a production build:

```bash
dotnet publish -c Release -o ./publish
```

The published application can be found in the `./publish` directory.

## Default Credentials

- Username: `admin`
- Password: `admin`

⚠️ **Important**: Change the default admin password immediately after first login in production environments.

## Modules

The system includes the following modules (can be enabled/disabled via SystemSettings):

- **Production**: Production logging, line status tracking, OEE calculations
- **Maintenance**: Preventive and corrective maintenance management
- **Quality**: Defect tracking, quality tests, MRB tickets
- **Inventory**: Spare parts and raw materials management
- **Planning**: Work order management
- **Alerts**: Real-time alert system based on configurable rules
- **Export**: Data export functionality with templates
- **Analysis**: Production data analysis and reporting

## Project Structure

```
MesEnterprise_v3/
├── Data/               # Database context and configuration
├── Models/             # Domain entities
│   ├── Core/          # Core entities (User, Role, Line, Product, etc.)
│   ├── Production/    # Production-related entities
│   ├── Maintenance/   # Maintenance entities
│   ├── Quality/       # Quality entities
│   ├── Inventory/     # Inventory entities
│   ├── Planning/      # Planning entities
│   ├── Alerts/        # Alert system entities
│   ├── Export/        # Export configuration entities
│   └── Config/        # System configuration entities
├── Services/          # Business logic and background services
├── Endpoints/         # API endpoints
├── Infrastructure/    # Middleware and cross-cutting concerns
├── Migrations/        # EF Core migrations
└── wwwroot/          # Static files (HTML, CSS, JS)
```

## API Documentation

When running in Development mode, API documentation is available via Swagger UI at `/swagger`.

## Troubleshooting

### Build Errors

If you encounter build errors related to missing Models namespace:
- Ensure all files in the `Models/` directory are properly included in the project
- Try running `dotnet clean` followed by `dotnet build`

### Database Connection Issues

If you cannot connect to the database:
- Verify PostgreSQL is running: `pg_isready`
- Check your connection string format
- Ensure the database user has sufficient permissions

### Migration Errors

If migrations fail to apply:
- Ensure the database exists (create it manually if needed)
- Check that the user has CREATE/ALTER permissions
- Review the migration files in the `Migrations/` directory for any issues

## License

[Add your license information here]

## Support

[Add support contact information here]
