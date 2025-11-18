# Manual Bază de Date - MES Enterprise

## Prezentare Generală

MES Enterprise utilizează PostgreSQL ca sistem de management al bazei de date. Schema este construită folosind Entity Framework Core cu migrații pentru gestionarea modificărilor structurale.

## Convenții de Nomenclatură

- **Tabele**: snake_case (ex: `production_logs`, `interventie_tichete`)
- **Coloane**: snake_case (ex: `line_id`, `created_at`)
- **Indecși**: `ix_{table}_{column(s)}` (ex: `ix_production_logs_line_id_timestamp`)
- **Chei străine**: `fk_{table}_{referenced_table}_{column}`

## Schema Entităților

### Core Domain (Utilizatori, Roluri, Linii, Echipamente)

#### users
```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role_id INTEGER NOT NULL REFERENCES roles(id),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
```

#### roles
```sql
CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500)
);
```

#### lines
```sql
CREATE TABLE lines (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    scan_identifier VARCHAR(50) UNIQUE,
    department_id INTEGER REFERENCES departments(id),
    cost_operare_pe_ora DECIMAL(10,2) DEFAULT 0,
    data_acquisition_mode VARCHAR(50) DEFAULT 'Manual', -- Manual|LiveScan|PLC_Input
    is_active BOOLEAN DEFAULT true
);
```

#### equipments
```sql
CREATE TABLE equipments (
    id SERIAL PRIMARY KEY,
    line_id INTEGER NOT NULL REFERENCES lines(id),
    name VARCHAR(200) NOT NULL,
    manufacturer VARCHAR(200),
    model VARCHAR(200),
    serial_number VARCHAR(100),
    ore_functionare DECIMAL(10,2) DEFAULT 0,
    is_active BOOLEAN DEFAULT true
);
```

### Production Domain

#### production_logs
```sql
CREATE TABLE production_logs (
    id SERIAL PRIMARY KEY,
    line_id INTEGER NOT NULL REFERENCES lines(id),
    product_id INTEGER NOT NULL REFERENCES products(id),
    shift_id INTEGER NOT NULL REFERENCES shifts(id),
    hour_interval VARCHAR(10) NOT NULL, -- "14:00"
    timestamp TIMESTAMP NOT NULL,
    target_parts INTEGER DEFAULT 0,
    actual_parts INTEGER DEFAULT 0,
    scrap_parts INTEGER DEFAULT 0,
    nrft_parts INTEGER DEFAULT 0,
    declared_downtime_minutes INTEGER,
    declared_downtime_reason_id INTEGER REFERENCES breakdown_reasons(id),
    system_stop_minutes INTEGER,
    justification_required BOOLEAN DEFAULT false,
    justification_reason VARCHAR(1000),
    production_work_order_id INTEGER REFERENCES production_work_orders(id),
    CONSTRAINT uq_production_log_line_hour_date UNIQUE(line_id, hour_interval, timestamp::date)
);

CREATE INDEX ix_production_logs_line_id_timestamp 
ON production_logs(line_id, timestamp);
```

#### line_statuses
```sql
CREATE TABLE line_statuses (
    id SERIAL PRIMARY KEY,
    line_id INTEGER NOT NULL UNIQUE REFERENCES lines(id),
    status VARCHAR(50) DEFAULT 'Stopped', -- Running|Stopped|Breakdown
    current_product_id INTEGER REFERENCES products(id),
    current_shift_id INTEGER REFERENCES shifts(id),
    operator_username VARCHAR(100),
    last_updated TIMESTAMP DEFAULT NOW()
);
```

#### changeover_logs
```sql
CREATE TABLE changeover_logs (
    id SERIAL PRIMARY KEY,
    line_id INTEGER NOT NULL REFERENCES lines(id),
    product_from_id INTEGER REFERENCES products(id),
    product_to_id INTEGER NOT NULL REFERENCES products(id),
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP,
    duration_minutes INTEGER
);

CREATE INDEX ix_changeover_logs_line_id_start_time
ON changeover_logs(line_id, start_time);
```

### Maintenance Domain

#### interventie_tichete
```sql
CREATE TABLE interventie_tichete (
    id SERIAL PRIMARY KEY,
    unic_id_ticket UUID DEFAULT gen_random_uuid(),
    line_id INTEGER NOT NULL REFERENCES lines(id),
    operator_nume VARCHAR(200),
    data_raportare_operator TIMESTAMP NOT NULL,
    problema_raportata_id INTEGER REFERENCES problema_raportata(id),
    descriere_problema TEXT,
    status VARCHAR(50) DEFAULT 'Open', -- Open|InProgress|Closed|Cancelled
    tehnician_nume VARCHAR(200),
    data_start_interventie TIMESTAMP,
    data_stop_interventie TIMESTAMP,
    defectiune_identificata_id INTEGER REFERENCES defectiune_identificata(id),
    defectiune_text_liber TEXT,
    influent eaza_produsul BOOLEAN DEFAULT false,
    root_cause TEXT,
    corrective_action TEXT,
    preventive_action TEXT
);

CREATE INDEX ix_interventie_tichete_status_data_raportare
ON interventie_tichete(status, data_raportare_operator);
```

#### preventive_maintenance_plans
```sql
CREATE TABLE preventive_maintenance_plans (
    id SERIAL PRIMARY KEY,
    equipment_id INTEGER NOT NULL REFERENCES equipments(id),
    interval_hours DECIMAL(10,2) NOT NULL,
    last_maintenance_date TIMESTAMP,
    next_due_date TIMESTAMP NOT NULL,
    is_active BOOLEAN DEFAULT true
);
```

### Quality Domain

#### defect_categories
```sql
CREATE TABLE defect_categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000)
);
```

#### defect_codes
```sql
CREATE TABLE defect_codes (
    id SERIAL PRIMARY KEY,
    category_id INTEGER NOT NULL REFERENCES defect_categories(id),
    code VARCHAR(50) NOT NULL,
    description VARCHAR(500) NOT NULL,
    is_active BOOLEAN DEFAULT true
);
```

#### quality_tests
```sql
CREATE TABLE quality_tests (
    id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES products(id),
    test_name VARCHAR(200) NOT NULL,
    test_type VARCHAR(100), -- Dimensional|Visual|Functional
    specification TEXT,
    frequency VARCHAR(100), -- PerHour|PerShift|PerBatch
    is_active BOOLEAN DEFAULT true
);
```

#### mrb_tickets
```sql
CREATE TABLE mrb_tickets (
    id SERIAL PRIMARY KEY,
    production_log_id INTEGER REFERENCES production_logs(id),
    quantity INTEGER NOT NULL,
    defect_description TEXT NOT NULL,
    disposition VARCHAR(100), -- Scrap|Rework|UseAsIs|ReturnToVendor
    created_at TIMESTAMP DEFAULT NOW(),
    resolved_at TIMESTAMP,
    status VARCHAR(50) DEFAULT 'Open'
);
```

### Inventory Domain

#### spare_parts
```sql
CREATE TABLE spare_parts (
    id SERIAL PRIMARY KEY,
    part_number VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    quantity_in_stock INTEGER DEFAULT 0,
    minimum_stock INTEGER DEFAULT 0,
    unit_cost DECIMAL(10,2) DEFAULT 0,
    location VARCHAR(100),
    is_active BOOLEAN DEFAULT true
);
```

#### raw_materials
```sql
CREATE TABLE raw_materials (
    id SERIAL PRIMARY KEY,
    material_code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    quantity_in_stock DECIMAL(15,3) DEFAULT 0,
    minimum_stock DECIMAL(15,3) DEFAULT 0,
    unit VARCHAR(50) NOT NULL, -- kg, m, pieces
    unit_cost DECIMAL(10,2) DEFAULT 0,
    is_active BOOLEAN DEFAULT true
);
```

#### product_boms
```sql
CREATE TABLE product_boms (
    id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES products(id),
    raw_material_id INTEGER NOT NULL REFERENCES raw_materials(id),
    quantity_per_unit DECIMAL(15,3) NOT NULL,
    unit VARCHAR(50) NOT NULL
);
```

### Planning Domain

#### production_work_orders
```sql
CREATE TABLE production_work_orders (
    id SERIAL PRIMARY KEY,
    work_order_number VARCHAR(100) NOT NULL UNIQUE,
    line_id INTEGER NOT NULL REFERENCES lines(id),
    product_id INTEGER NOT NULL REFERENCES products(id),
    planned_quantity INTEGER NOT NULL,
    produced_quantity INTEGER DEFAULT 0,
    planned_start_date TIMESTAMP NOT NULL,
    planned_end_date TIMESTAMP,
    actual_start_date TIMESTAMP,
    actual_end_date TIMESTAMP,
    status VARCHAR(50) DEFAULT 'Planned', -- Planned|InProgress|Completed|Cancelled
    notes VARCHAR(1000),
    created_at TIMESTAMP DEFAULT NOW()
);
```

### Alerts Domain

#### alert_rules
```sql
CREATE TABLE alert_rules (
    id SERIAL PRIMARY KEY,
    rule_name VARCHAR(200) NOT NULL,
    rule_type VARCHAR(100) NOT NULL, -- ScrapConsecutiv|DowntimePesteLimita
    condition_json TEXT NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);
```

#### alert_logs
```sql
CREATE TABLE alert_logs (
    id SERIAL PRIMARY KEY,
    rule_id INTEGER NOT NULL REFERENCES alert_rules(id),
    triggered_at TIMESTAMP DEFAULT NOW(),
    line_id INTEGER REFERENCES lines(id),
    message TEXT NOT NULL,
    severity VARCHAR(50), -- Info|Warning|Critical
    acknowledged_at TIMESTAMP,
    acknowledged_by VARCHAR(200)
);

CREATE INDEX ix_alert_logs_triggered_at_rule_id
ON alert_logs(triggered_at, rule_id);
```

### Export Domain

#### export_jobs
```sql
CREATE TABLE export_jobs (
    id SERIAL PRIMARY KEY,
    job_name VARCHAR(200) NOT NULL,
    format VARCHAR(50) NOT NULL, -- CSV|XLSX|JSON
    parameters TEXT, -- JSON
    status VARCHAR(50) DEFAULT 'Pending', -- Pending|Processing|Completed|Failed
    file_path VARCHAR(500),
    created_at TIMESTAMP DEFAULT NOW(),
    completed_at TIMESTAMP
);
```

#### export_templates
```sql
CREATE TABLE export_templates (
    id SERIAL PRIMARY KEY,
    template_name VARCHAR(200) NOT NULL,
    query_template TEXT NOT NULL,
    format VARCHAR(50) NOT NULL,
    is_active BOOLEAN DEFAULT true
);
```

### Config Domain

#### system_settings
```sql
CREATE TABLE system_settings (
    id SERIAL PRIMARY KEY,
    key VARCHAR(100) NOT NULL UNIQUE,
    value VARCHAR(1000),
    description VARCHAR(500),
    updated_at TIMESTAMP DEFAULT NOW()
);
```

## Relații Principale

### One-to-Many
- `lines` → `production_logs`
- `products` → `production_logs`
- `shifts` → `production_logs`
- `lines` → `equipments`
- `lines` → `interventie_tichete`
- `defect_categories` → `defect_codes`

### Many-to-Many
- `users` ↔ `roles` (through `role_permissions`)
- `products` ↔ `raw_materials` (through `product_boms`)

### Optional Foreign Keys
- `production_logs.production_work_order_id` (enterprise feature)
- `production_logs.declared_downtime_reason_id` (when downtime declared)

## Interogări Comune

### OEE Calculation
```sql
SELECT 
    l.name AS line_name,
    p.name AS product_name,
    pl.timestamp::date AS production_date,
    SUM(pl.target_parts) AS total_target,
    SUM(pl.actual_parts) AS total_actual,
    SUM(pl.scrap_parts) AS total_scrap,
    ROUND(
        (SUM(pl.actual_parts)::decimal / NULLIF(SUM(pl.target_parts), 0)) * 100, 
        2
    ) AS oee_percent
FROM production_logs pl
JOIN lines l ON pl.line_id = l.id
JOIN products p ON pl.product_id = p.id
WHERE pl.timestamp >= CURRENT_DATE - INTERVAL '7 days'
GROUP BY l.name, p.name, pl.timestamp::date
ORDER BY pl.timestamp::date DESC, l.name;
```

### Active Maintenance Tickets
```sql
SELECT 
    it.id,
    it.unic_id_ticket,
    l.name AS line_name,
    it.status,
    it.data_raportare_operator,
    it.operator_nume,
    it.tehnician_nume,
    pr.nume AS problema,
    di.nume AS defectiune
FROM interventie_tichete it
JOIN lines l ON it.line_id = l.id
LEFT JOIN problema_raportata pr ON it.problema_raportata_id = pr.id
LEFT JOIN defectiune_identificata di ON it.defectiune_identificata_id = di.id
WHERE it.status IN ('Open', 'InProgress')
ORDER BY it.data_raportare_operator DESC;
```

### Low Stock Alert
```sql
-- Spare Parts
SELECT part_number, name, quantity_in_stock, minimum_stock
FROM spare_parts
WHERE quantity_in_stock <= minimum_stock AND is_active = true;

-- Raw Materials
SELECT material_code, name, quantity_in_stock, minimum_stock, unit
FROM raw_materials
WHERE quantity_in_stock <= minimum_stock AND is_active = true;
```

### Work Order Progress
```sql
SELECT 
    wo.work_order_number,
    l.name AS line_name,
    p.name AS product_name,
    wo.planned_quantity,
    wo.produced_quantity,
    ROUND((wo.produced_quantity::decimal / wo.planned_quantity * 100), 2) AS completion_percent,
    wo.status,
    wo.planned_start_date,
    wo.planned_end_date
FROM production_work_orders wo
JOIN lines l ON wo.line_id = l.id
JOIN products p ON wo.product_id = p.id
WHERE wo.status = 'InProgress'
ORDER BY wo.planned_start_date;
```

## Optimizare și Întreținere

### Indecși Recomandați
Toți indecșii enumerați mai sus sunt creați automat prin migrații EF Core.

### Vacuum și Analyze
```sql
-- Manual trigger (or use Admin API endpoint)
VACUUM ANALYZE;

-- Per table
VACUUM ANALYZE production_logs;
VACUUM ANALYZE interventie_tichete;
```

### Reindexare
```sql
-- All indexes
REINDEX DATABASE mesdb;

-- Specific table
REINDEX TABLE production_logs;
```

### Monitoring Size
```sql
-- Database size
SELECT pg_size_pretty(pg_database_size('mesdb'));

-- Table sizes
SELECT 
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

## Plan de Partiționing (Viitor)

Pentru scalabilitate pe termen lung, se recomandă partiționing for tabele mari:

### production_logs (Range Partitioning by Month)
```sql
-- Parent table
CREATE TABLE production_logs_partitioned (
    -- same structure
) PARTITION BY RANGE (timestamp);

-- Monthly partitions
CREATE TABLE production_logs_2025_01 PARTITION OF production_logs_partitioned
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');
    
CREATE TABLE production_logs_2025_02 PARTITION OF production_logs_partitioned
    FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');
-- etc.
```

### alert_logs (Range Partitioning by Quarter)
Similar approach for alert_logs to manage high-volume alert data.

## Backup și Restore

### Manual Backup
```bash
pg_dump -h localhost -U postgres -d mesdb -F c -f mesdb_backup_$(date +%Y%m%d).dump
```

### Automated Backup (via AutoBackupService)
Configurare în `appsettings.json`:
```json
{
  "BackupSettings": {
    "Enabled": true,
    "IntervalHours": 24,
    "RetentionDays": 30,
    "BackupPath": "/backups"
  }
}
```

### Restore
```bash
pg_restore -h localhost -U postgres -d mesdb -c mesdb_backup_20251117.dump
```

## Securitate

### Privilegii Utilizator
```sql
-- Create application user with limited privileges
CREATE USER mes_app WITH PASSWORD 'secure_password';
GRANT CONNECT ON DATABASE mesdb TO mes_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO mes_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO mes_app;
```

### Audit Trail (Viitor)
```sql
CREATE TABLE audit_logs (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(100),
    operation VARCHAR(20), -- INSERT|UPDATE|DELETE
    old_values JSONB,
    new_values JSONB,
    user_name VARCHAR(100),
    timestamp TIMESTAMP DEFAULT NOW()
);
```

## Troubleshooting

### Lock Monitoring
```sql
SELECT 
    pg_stat_activity.pid,
    pg_stat_activity.usename,
    pg_stat_activity.query,
    pg_stat_activity.state
FROM pg_stat_activity
WHERE pg_stat_activity.datname = 'mesdb'
  AND pg_stat_activity.state = 'active';
```

### Kill Blocking Query
```sql
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE pid = <blocking_pid>;
```

---

*Ultima actualizare: 2025-11-17*
*Versiune: 1.0.0*
