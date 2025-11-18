using Microsoft.EntityFrameworkCore;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;
using MesEnterprise.Models.Maintenance;
using MesEnterprise.Models.Quality;
using MesEnterprise.Models.Inventory;
using MesEnterprise.Models.Planning;
using MesEnterprise.Models.Alerts;
using MesEnterprise.Models.Export;
using MesEnterprise.Models.Config;

namespace MesEnterprise.Data
{
    public class MesDbContext : DbContext
    {
        public MesDbContext(DbContextOptions<MesDbContext> options) : base(options) { }

        // Core
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Line> Lines { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<BreakdownReason> BreakdownReasons { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftBreak> ShiftBreaks { get; set; }
        public DbSet<PlannedDowntime> PlannedDowntimes { get; set; }
        public DbSet<ObservatieOperator> ObservatiiOperator { get; set; }

        // Production
        public DbSet<LineStatus> LineStatuses { get; set; }
        public DbSet<ProductionLog> ProductionLogs { get; set; }
        public DbSet<ProductionLogDefect> ProductionLogDefects { get; set; }
        public DbSet<ChangeoverLog> ChangeoverLogs { get; set; }

        // Planning
        public DbSet<ProductionWorkOrder> ProductionWorkOrders { get; set; }

        // Maintenance
        public DbSet<InterventieTichet> InterventieTichete { get; set; }
        public DbSet<ProblemaRaportata> ProblemeRaportate { get; set; }
        public DbSet<DefectiuneIdentificata> DefectiuniIdentificate { get; set; }
        public DbSet<ProblemaDefectiuneCorelatie> ProblemaDefectiuneCorelatii { get; set; }
        public DbSet<PreventiveMaintenancePlan> PreventiveMaintenancePlans { get; set; }

        // Quality
        public DbSet<DefectCategory> DefectCategories { get; set; }
        public DbSet<DefectCode> DefectCodes { get; set; }
        public DbSet<QualityTest> QualityTests { get; set; }
        public DbSet<ProductionLogQualityCheck> ProductionLogQualityChecks { get; set; }
        public DbSet<MrbTicket> MrbTickets { get; set; }

        // Inventory
        public DbSet<SparePart> SpareParts { get; set; }
        public DbSet<RawMaterial> RawMaterials { get; set; }
        public DbSet<ProductBOM> ProductBOMs { get; set; }

        // Alerts
        public DbSet<AlertRule> AlertRules { get; set; }
        public DbSet<AlertLog> AlertLogs { get; set; }

        // Export
        public DbSet<ExportJob> ExportJobs { get; set; }
        public DbSet<ExportTemplate> ExportTemplates { get; set; }

        // Config
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<StopOnDefectRule> StopOnDefectRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique indexes
            modelBuilder.Entity<LineStatus>()
                .HasIndex(ls => ls.LineId)
                .IsUnique();

            modelBuilder.Entity<PlannedDowntime>()
                .HasIndex(pd => new { pd.LineId, pd.ProductId })
                .IsUnique();

            modelBuilder.Entity<SystemSetting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            modelBuilder.Entity<Line>()
                .HasIndex(l => l.ScanIdentifier)
                .IsUnique();

            modelBuilder.Entity<InterventieTichet>()
                .HasIndex(t => t.UnicIdTicket)
                .IsUnique();

            modelBuilder.Entity<ProductionWorkOrder>()
                .HasIndex(w => w.WorkOrderNumber)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // Performance indexes
            modelBuilder.Entity<ProductionLog>()
                .HasIndex(pl => new { pl.LineId, pl.Timestamp });

            modelBuilder.Entity<AlertLog>()
                .HasIndex(al => new { al.TriggeredAt, al.AlertRuleId });

            modelBuilder.Entity<ChangeoverLog>()
                .HasIndex(cl => new { cl.LineId, cl.StartTime });

            modelBuilder.Entity<InterventieTichet>()
                .HasIndex(it => new { it.Status, it.DataRaportareOperator });

            // Relationships
            modelBuilder.Entity<ProductionLog>()
                .HasOne(pl => pl.DeclaredDowntimeReason)
                .WithMany()
                .HasForeignKey(pl => pl.DeclaredDowntimeReasonId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DefectCode>()
                .HasOne(d => d.DefectCategory)
                .WithMany(c => c.DefectCodes)
                .HasForeignKey(d => d.DefectCategoryId);

            modelBuilder.Entity<ProductionLogDefect>()
                .HasOne(pld => pld.ProductionLog)
                .WithMany(pl => pl.Defects)
                .HasForeignKey(pld => pld.ProductionLogId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductionLogDefect>()
                .HasOne(pld => pld.DefectCode)
                .WithMany()
                .HasForeignKey(pld => pld.DefectCodeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StopOnDefectRule>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .IsRequired(false);

            modelBuilder.Entity<ObservatieOperator>()
                .HasOne(o => o.Line)
                .WithMany()
                .HasForeignKey(o => o.LineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ObservatieOperator>()
                .HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ObservatieOperator>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // RBAC Relationships
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // Maintenance Correlations
            modelBuilder.Entity<ProblemaDefectiuneCorelatie>()
                .HasKey(pdc => new { pdc.ProblemaRaportataId, pdc.DefectiuneIdentificataId });

            modelBuilder.Entity<ProblemaDefectiuneCorelatie>()
                .HasOne(pdc => pdc.ProblemaRaportata)
                .WithMany(p => p.Corelatii)
                .HasForeignKey(pdc => pdc.ProblemaRaportataId);

            modelBuilder.Entity<ProblemaDefectiuneCorelatie>()
                .HasOne(pdc => pdc.DefectiuneIdentificata)
                .WithMany(d => d.Corelatii)
                .HasForeignKey(pdc => pdc.DefectiuneIdentificataId);
        }
    }
}
