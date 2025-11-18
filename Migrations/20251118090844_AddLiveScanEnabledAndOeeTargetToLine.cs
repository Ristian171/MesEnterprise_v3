using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MesEnterprise.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveScanEnabledAndOeeTargetToLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "breakdown_reason",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_breakdown_reason", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "defect_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_defect_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "defectiune_identificata",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nume = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_defectiune_identificata", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "department",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "export_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    configuration = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    template_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_export_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "problema_raportata",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nume = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problema_raportata", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cycle_time_seconds = table.Column<double>(type: "double precision", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "raw_material",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    material_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    minimum_stock = table.Column<decimal>(type: "numeric", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    quantity_in_stock = table.Column<decimal>(type: "numeric", nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    unit_cost = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_raw_material", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shift",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shift", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spare_part",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    minimum_stock = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    part_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantity_in_stock = table.Column<int>(type: "integer", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spare_part", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_setting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_setting", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "defect_code",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    defect_category_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_defect_code", x => x.id);
                    table.ForeignKey(
                        name: "fk_defect_code_defect_category_defect_category_id",
                        column: x => x.defect_category_id,
                        principalTable: "defect_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "line",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    scan_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    has_live_scanning = table.Column<bool>(type: "boolean", nullable: false),
                    live_scan_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    cost_operare_pe_ora = table.Column<decimal>(type: "numeric", nullable: false),
                    data_acquisition_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    oee_target = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_line", x => x.id);
                    table.ForeignKey(
                        name: "fk_line_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "problema_defectiune_corelatie",
                columns: table => new
                {
                    problema_raportata_id = table.Column<int>(type: "integer", nullable: false),
                    defectiune_identificata_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problema_defectiune_corelatie", x => new { x.problema_raportata_id, x.defectiune_identificata_id });
                    table.ForeignKey(
                        name: "fk_problema_defectiune_corelatie_defectiune_identificata_defec",
                        column: x => x.defectiune_identificata_id,
                        principalTable: "defectiune_identificata",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_problema_defectiune_corelatie_problema_raportata_problema_r",
                        column: x => x.problema_raportata_id,
                        principalTable: "problema_raportata",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quality_test",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    acceptance_criteria = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    test_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quality_test", x => x.id);
                    table.ForeignKey(
                        name: "fk_quality_test_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "product_bom",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity_per_unit = table.Column<decimal>(type: "numeric", nullable: false),
                    raw_material_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_bom", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_bom_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_bom_raw_material_raw_material_id",
                        column: x => x.raw_material_id,
                        principalTable: "raw_material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    permission_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permission", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permission_permission_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permission_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    role_id = table.Column<int>(type: "integer", nullable: true),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "shift_break",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    break_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shift_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shift_break", x => x.id);
                    table.ForeignKey(
                        name: "fk_shift_break_shift_shift_id",
                        column: x => x.shift_id,
                        principalTable: "shift",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alert_rule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    evaluation_window_minutes = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    line_id = table.Column<int>(type: "integer", nullable: true),
                    rule_configuration = table.Column<string>(type: "text", nullable: true),
                    rule_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    rule_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    threshold_value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alert_rule", x => x.id);
                    table.ForeignKey(
                        name: "fk_alert_rule_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "changeover_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    product_from_id = table.Column<int>(type: "integer", nullable: false),
                    product_to_id = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_changeover_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_changeover_log_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_changeover_log_product_product_from_id",
                        column: x => x.product_from_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_changeover_log_product_product_to_id",
                        column: x => x.product_to_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_maintenance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ore_functionare = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipment_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "line_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    current_shift_id = table.Column<int>(type: "integer", nullable: true),
                    last_status_change = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_line_status", x => x.id);
                    table.ForeignKey(
                        name: "fk_line_status_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_line_status_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_line_status_shift_current_shift_id",
                        column: x => x.current_shift_id,
                        principalTable: "shift",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "planned_downtime",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    minutes_per_hour = table.Column<double>(type: "double precision", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_planned_downtime", x => x.id);
                    table.ForeignKey(
                        name: "fk_planned_downtime_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_planned_downtime_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "production_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    actual_parts = table.Column<int>(type: "integer", nullable: false),
                    declared_downtime_minutes = table.Column<int>(type: "integer", nullable: true),
                    declared_downtime_reason_id = table.Column<int>(type: "integer", nullable: true),
                    hour_interval = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    justification_reason = table.Column<string>(type: "text", nullable: true),
                    justification_required = table.Column<bool>(type: "boolean", nullable: false),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    nrft_parts = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    production_work_order_id = table.Column<int>(type: "integer", nullable: true),
                    scrap_parts = table.Column<int>(type: "integer", nullable: false),
                    shift_id = table.Column<int>(type: "integer", nullable: false),
                    system_stop_minutes = table.Column<int>(type: "integer", nullable: true),
                    target_parts = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_production_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_production_log_breakdown_reason_declared_downtime_reason_id",
                        column: x => x.declared_downtime_reason_id,
                        principalTable: "breakdown_reason",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_production_log_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_production_log_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_production_log_shift_shift_id",
                        column: x => x.shift_id,
                        principalTable: "shift",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "production_work_order",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    actual_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    actual_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    planned_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    planned_quantity = table.Column<int>(type: "integer", nullable: false),
                    planned_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    produced_quantity = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    work_order_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_production_work_order", x => x.id);
                    table.ForeignKey(
                        name: "fk_production_work_order_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_production_work_order_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stop_on_defect_rule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    max_consecutive_nrft = table.Column<int>(type: "integer", nullable: false),
                    max_consecutive_scrap = table.Column<int>(type: "integer", nullable: false),
                    max_nrft_per_hour = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stop_on_defect_rule", x => x.id);
                    table.ForeignKey(
                        name: "fk_stop_on_defect_rule_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stop_on_defect_rule_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "export_job",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    query_configuration = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_export_job", x => x.id);
                    table.ForeignKey(
                        name: "fk_export_job_user_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "observatie_operator",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_ora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_observatie_operator", x => x.id);
                    table.ForeignKey(
                        name: "fk_observatie_operator_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_observatie_operator_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_observatie_operator_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "alert_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alert_rule_id = table.Column<int>(type: "integer", nullable: true),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    triggered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alert_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_alert_log_alert_rule_alert_rule_id",
                        column: x => x.alert_rule_id,
                        principalTable: "alert_rule",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "interventie_tichet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    capa_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    corrective_action = table.Column<string>(type: "text", nullable: true),
                    cost_manopera = table.Column<decimal>(type: "numeric", nullable: true),
                    cost_piese = table.Column<decimal>(type: "numeric", nullable: true),
                    data_raportare_operator = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_start_interventie = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_stop_interventie = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    defectiune_identificata_id = table.Column<int>(type: "integer", nullable: true),
                    defectiune_text_liber = table.Column<string>(type: "text", nullable: true),
                    durata_minute = table.Column<int>(type: "integer", nullable: true),
                    equipment_id = table.Column<int>(type: "integer", nullable: false),
                    influenteaza_produsul = table.Column<bool>(type: "boolean", nullable: false),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    operator_nume = table.Column<string>(type: "text", nullable: true),
                    preventive_action = table.Column<string>(type: "text", nullable: true),
                    problema_raportata_id = table.Column<int>(type: "integer", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    root_cause = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tehnician_nume = table.Column<string>(type: "text", nullable: true),
                    unic_id_ticket = table.Column<Guid>(type: "uuid", nullable: false),
                    tehnician_asignat_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_interventie_tichet", x => x.id);
                    table.ForeignKey(
                        name: "fk_interventie_tichet_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_interventie_tichet_line_line_id",
                        column: x => x.line_id,
                        principalTable: "line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_interventie_tichet_problema_raportata_problema_raportata_id",
                        column: x => x.problema_raportata_id,
                        principalTable: "problema_raportata",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_interventie_tichet_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_interventie_tichet_user_tehnician_asignat_id",
                        column: x => x.tehnician_asignat_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "preventive_maintenance_plan",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    equipment_id = table.Column<int>(type: "integer", nullable: true),
                    frequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    frequency_value = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_executed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    line_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    next_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    task_description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_preventive_maintenance_plan", x => x.id);
                    table.ForeignKey(
                        name: "fk_preventive_maintenance_plan_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mrb_ticket",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    defect_description = table.Column<string>(type: "text", nullable: true),
                    defective_quantity = table.Column<int>(type: "integer", nullable: false),
                    disposition = table.Column<string>(type: "text", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    production_log_id = table.Column<int>(type: "integer", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ticket_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mrb_ticket", x => x.id);
                    table.ForeignKey(
                        name: "fk_mrb_ticket_production_log_production_log_id",
                        column: x => x.production_log_id,
                        principalTable: "production_log",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "production_log_defect",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    defect_code_id = table.Column<int>(type: "integer", nullable: false),
                    production_log_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_production_log_defect", x => x.id);
                    table.ForeignKey(
                        name: "fk_production_log_defect_defect_code_defect_code_id",
                        column: x => x.defect_code_id,
                        principalTable: "defect_code",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_production_log_defect_production_log_production_log_id",
                        column: x => x.production_log_id,
                        principalTable: "production_log",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "production_log_quality_check",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notes = table.Column<string>(type: "text", nullable: true),
                    production_log_id = table.Column<int>(type: "integer", nullable: false),
                    quality_test_id = table.Column<int>(type: "integer", nullable: false),
                    result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_production_log_quality_check", x => x.id);
                    table.ForeignKey(
                        name: "fk_production_log_quality_check_production_log_production_log_",
                        column: x => x.production_log_id,
                        principalTable: "production_log",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_production_log_quality_check_quality_test_quality_test_id",
                        column: x => x.quality_test_id,
                        principalTable: "quality_test",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alert_log_alert_rule_id",
                table: "alert_log",
                column: "alert_rule_id");

            migrationBuilder.CreateIndex(
                name: "ix_alert_log_triggered_at_alert_rule_id",
                table: "alert_log",
                columns: new[] { "triggered_at", "alert_rule_id" });

            migrationBuilder.CreateIndex(
                name: "ix_alert_rule_line_id",
                table: "alert_rule",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_changeover_log_line_id_start_time",
                table: "changeover_log",
                columns: new[] { "line_id", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_changeover_log_product_from_id",
                table: "changeover_log",
                column: "product_from_id");

            migrationBuilder.CreateIndex(
                name: "ix_changeover_log_product_to_id",
                table: "changeover_log",
                column: "product_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_defect_code_defect_category_id",
                table: "defect_code",
                column: "defect_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_line_id",
                table: "equipment",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_export_job_created_by_user_id",
                table: "export_job",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_interventie_tichet_equipment_id",
                table: "interventie_tichet",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_interventie_tichet_line_id",
                table: "interventie_tichet",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_interventie_tichet_problema_raportata_id",
                table: "interventie_tichet",
                column: "problema_raportata_id");

            migrationBuilder.CreateIndex(
                name: "ix_interventie_tichet_product_id",
                table: "interventie_tichet",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_interventie_tichet_status_data_raportare_operator",
                table: "interventie_tichet",
                columns: new[] { "status", "data_raportare_operator" });

            migrationBuilder.CreateIndex(
                name: "ix_interventie_tichet_tehnician_asignat_id",
                table: "interventie_tichet",
                column: "tehnician_asignat_id");

            migrationBuilder.CreateIndex(
                name: "ix_interventie_tichet_unic_id_ticket",
                table: "interventie_tichet",
                column: "unic_id_ticket",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_line_department_id",
                table: "line",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_line_scan_identifier",
                table: "line",
                column: "scan_identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_line_status_current_shift_id",
                table: "line_status",
                column: "current_shift_id");

            migrationBuilder.CreateIndex(
                name: "ix_line_status_line_id",
                table: "line_status",
                column: "line_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_line_status_product_id",
                table: "line_status",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_mrb_ticket_production_log_id",
                table: "mrb_ticket",
                column: "production_log_id");

            migrationBuilder.CreateIndex(
                name: "ix_observatie_operator_line_id",
                table: "observatie_operator",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_observatie_operator_product_id",
                table: "observatie_operator",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_observatie_operator_user_id",
                table: "observatie_operator",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_planned_downtime_line_id_product_id",
                table: "planned_downtime",
                columns: new[] { "line_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_planned_downtime_product_id",
                table: "planned_downtime",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_preventive_maintenance_plan_equipment_id",
                table: "preventive_maintenance_plan",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_problema_defectiune_corelatie_defectiune_identificata_id",
                table: "problema_defectiune_corelatie",
                column: "defectiune_identificata_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_bom_product_id",
                table: "product_bom",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_bom_raw_material_id",
                table: "product_bom",
                column: "raw_material_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_log_declared_downtime_reason_id",
                table: "production_log",
                column: "declared_downtime_reason_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_log_line_id_timestamp",
                table: "production_log",
                columns: new[] { "line_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_production_log_product_id",
                table: "production_log",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_log_shift_id",
                table: "production_log",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_log_defect_defect_code_id",
                table: "production_log_defect",
                column: "defect_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_log_defect_production_log_id",
                table: "production_log_defect",
                column: "production_log_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_log_quality_check_production_log_id",
                table: "production_log_quality_check",
                column: "production_log_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_log_quality_check_quality_test_id",
                table: "production_log_quality_check",
                column: "quality_test_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_work_order_line_id",
                table: "production_work_order",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_work_order_product_id",
                table: "production_work_order",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_production_work_order_work_order_number",
                table: "production_work_order",
                column: "work_order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quality_test_product_id",
                table: "quality_test",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_name",
                table: "role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permission_permission_id",
                table: "role_permission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_shift_break_shift_id",
                table: "shift_break",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "ix_stop_on_defect_rule_line_id",
                table: "stop_on_defect_rule",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_stop_on_defect_rule_product_id",
                table: "stop_on_defect_rule",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_system_setting_key",
                table: "system_setting",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_role_id",
                table: "user",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert_log");

            migrationBuilder.DropTable(
                name: "changeover_log");

            migrationBuilder.DropTable(
                name: "export_job");

            migrationBuilder.DropTable(
                name: "export_template");

            migrationBuilder.DropTable(
                name: "interventie_tichet");

            migrationBuilder.DropTable(
                name: "line_status");

            migrationBuilder.DropTable(
                name: "mrb_ticket");

            migrationBuilder.DropTable(
                name: "observatie_operator");

            migrationBuilder.DropTable(
                name: "planned_downtime");

            migrationBuilder.DropTable(
                name: "preventive_maintenance_plan");

            migrationBuilder.DropTable(
                name: "problema_defectiune_corelatie");

            migrationBuilder.DropTable(
                name: "product_bom");

            migrationBuilder.DropTable(
                name: "production_log_defect");

            migrationBuilder.DropTable(
                name: "production_log_quality_check");

            migrationBuilder.DropTable(
                name: "production_work_order");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "shift_break");

            migrationBuilder.DropTable(
                name: "spare_part");

            migrationBuilder.DropTable(
                name: "stop_on_defect_rule");

            migrationBuilder.DropTable(
                name: "system_setting");

            migrationBuilder.DropTable(
                name: "alert_rule");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "defectiune_identificata");

            migrationBuilder.DropTable(
                name: "problema_raportata");

            migrationBuilder.DropTable(
                name: "raw_material");

            migrationBuilder.DropTable(
                name: "defect_code");

            migrationBuilder.DropTable(
                name: "production_log");

            migrationBuilder.DropTable(
                name: "quality_test");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "defect_category");

            migrationBuilder.DropTable(
                name: "breakdown_reason");

            migrationBuilder.DropTable(
                name: "line");

            migrationBuilder.DropTable(
                name: "shift");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "department");
        }
    }
}
