using MesEnterprise.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace MesEnterprise.Endpoints
{
    public static class ExcelExportEndpoints
    {
        public static IEndpointRouteBuilder MapExcelExportApi(this IEndpointRouteBuilder app)
        {
            var exportApi = app.MapGroup("/api/export");

            // Export Technical Downtimes
            exportApi.MapGet("/stationari-tehnice", async (
                [FromQuery] DateTime? startDate,
                [FromQuery] DateTime? endDate,
                [FromQuery] int? lineId,
                [FromQuery] string? status,
                MesDbContext db) =>
            {
                var query = db.InterventieTichete
                    .Include(i => i.Line)
                    .Include(i => i.Equipment)
                    .Include(i => i.Product)
                    .Include(i => i.ProblemaRaportata)
                    .Include(i => i.DefectiuneIdentificata)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(i => i.DataRaportareOperator >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(i => i.DataRaportareOperator <= endDate.Value);
                
                if (lineId.HasValue)
                    query = query.Where(i => i.LineId == lineId.Value);
                
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(i => i.Status == status);

                var data = await query.OrderByDescending(i => i.DataRaportareOperator).ToListAsync();

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Staționări Tehnice");

                // Header
                var headerRow = sheet.CreateRow(0);
                var headerStyle = workbook.CreateCellStyle();
                var font = workbook.CreateFont();
                font.IsBold = true;
                headerStyle.SetFont(font);

                string[] headers = { "ID", "Data", "Linie", "Echipament", "Produs", "Operator", 
                    "Problemă", "Defecțiune", "Durată (min)", "Status", "Influențează Produsul" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

                // Data rows
                int rowIndex = 1;
                foreach (var item in data)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(item.Id);
                    row.CreateCell(1).SetCellValue(item.DataRaportareOperator.ToString("yyyy-MM-dd HH:mm"));
                    row.CreateCell(2).SetCellValue(item.Line?.Name ?? "");
                    row.CreateCell(3).SetCellValue(item.Equipment?.Name ?? "");
                    row.CreateCell(4).SetCellValue(item.Product?.Name ?? "");
                    row.CreateCell(5).SetCellValue(item.OperatorNume ?? "");
                    row.CreateCell(6).SetCellValue(item.ProblemaRaportata?.Nume ?? "");
                    row.CreateCell(7).SetCellValue(item.DefectiuneIdentificata?.Nume ?? "");
                    row.CreateCell(8).SetCellValue(item.DurataMinute ?? 0);
                    row.CreateCell(9).SetCellValue(item.Status);
                    row.CreateCell(10).SetCellValue(item.InfluenteazaProdusul ? "Da" : "Nu");
                }

                // Auto-size columns
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                var stream = new MemoryStream();
                workbook.Write(stream, true);
                stream.Position = 0;

                return Results.File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Stationari_Tehnice_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            });

            // Export Spare Parts Usage
            exportApi.MapGet("/spare-parts-usage", async (
                [FromQuery] DateTime? startDate,
                [FromQuery] DateTime? endDate,
                [FromQuery] int? sparePartId,
                MesDbContext db) =>
            {
                var query = db.SparePartUsages
                    .Include(u => u.SparePart)
                    .Include(u => u.InterventieTichet)
                        .ThenInclude(i => i.Line)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(u => u.UsedAt >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(u => u.UsedAt <= endDate.Value);
                
                if (sparePartId.HasValue)
                    query = query.Where(u => u.SparePartId == sparePartId.Value);

                var data = await query.OrderByDescending(u => u.UsedAt).ToListAsync();

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Utilizare Spare Parts");

                var headerRow = sheet.CreateRow(0);
                string[] headers = { "Data", "Part Number", "Nume Piesă", "Cantitate", "Intervenție ID", 
                    "Linie", "Cost Total", "Note" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(headers[i]);
                }

                int rowIndex = 1;
                foreach (var item in data)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(item.UsedAt.ToString("yyyy-MM-dd HH:mm"));
                    row.CreateCell(1).SetCellValue(item.SparePart?.PartNumber ?? "");
                    row.CreateCell(2).SetCellValue(item.SparePart?.Name ?? "");
                    row.CreateCell(3).SetCellValue(item.QuantityUsed);
                    row.CreateCell(4).SetCellValue(item.InterventieTichetId);
                    row.CreateCell(5).SetCellValue(item.InterventieTichet?.Line?.Name ?? "");
                    row.CreateCell(6).SetCellValue((double)(item.QuantityUsed * (item.SparePart?.UnitCost ?? 0)));
                    row.CreateCell(7).SetCellValue(item.Notes ?? "");
                }

                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                var stream = new MemoryStream();
                workbook.Write(stream, true);
                stream.Position = 0;

                return Results.File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Spare_Parts_Usage_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            });

            // Export TPM Plans
            exportApi.MapGet("/tpm-plans", async (MesDbContext db) =>
            {
                var data = await db.PreventiveMaintenancePlans
                    .Include(p => p.Line)
                    .Include(p => p.Equipment)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Planuri TPM");

                var headerRow = sheet.CreateRow(0);
                string[] headers = { "Nume", "Descriere", "Linie", "Echipament", "Frecvență", 
                    "Ultima Execuție", "Următoarea Scadență", "Status" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(headers[i]);
                }

                int rowIndex = 1;
                foreach (var item in data)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(item.Name);
                    row.CreateCell(1).SetCellValue(item.Description ?? "");
                    row.CreateCell(2).SetCellValue(item.Line?.Name ?? "Toate");
                    row.CreateCell(3).SetCellValue(item.Equipment?.Name ?? "Toate");
                    row.CreateCell(4).SetCellValue($"{item.FrequencyValue} {item.FrequencyType}");
                    row.CreateCell(5).SetCellValue(item.LastExecutedDate?.ToString("yyyy-MM-dd") ?? "-");
                    row.CreateCell(6).SetCellValue(item.NextDueDate?.ToString("yyyy-MM-dd") ?? "-");
                    row.CreateCell(7).SetCellValue(item.IsActive ? "Activ" : "Inactiv");
                }

                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                var stream = new MemoryStream();
                workbook.Write(stream, true);
                stream.Position = 0;

                return Results.File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"TPM_Plans_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            });

            // Export Changeover History
            exportApi.MapGet("/changeover", async (
                [FromQuery] DateTime? startDate,
                [FromQuery] DateTime? endDate,
                [FromQuery] int? lineId,
                MesDbContext db) =>
            {
                var query = db.ChangeoverLogs
                    .Include(c => c.Line)
                    .Include(c => c.ProductFrom)
                    .Include(c => c.ProductTo)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(c => c.StartTime >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(c => c.StartTime <= endDate.Value);
                
                if (lineId.HasValue)
                    query = query.Where(c => c.LineId == lineId.Value);

                var data = await query.OrderByDescending(c => c.StartTime).ToListAsync();
                var lines = await db.Lines.ToDictionaryAsync(l => l.Id, l => l.ChangeoverTargetMinutes);

                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Changeover History");

                var headerRow = sheet.CreateRow(0);
                string[] headers = { "Data Start", "Data End", "Linie", "Produs De La", "Produs La", 
                    "Durată (min)", "Target (min)", "Depășit Target" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(headers[i]);
                }

                int rowIndex = 1;
                foreach (var item in data)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(item.StartTime.ToString("yyyy-MM-dd HH:mm"));
                    row.CreateCell(1).SetCellValue(item.EndTime?.ToString("yyyy-MM-dd HH:mm") ?? "În curs");
                    row.CreateCell(2).SetCellValue(item.Line?.Name ?? "");
                    row.CreateCell(3).SetCellValue(item.ProductFrom?.Name ?? "");
                    row.CreateCell(4).SetCellValue(item.ProductTo?.Name ?? "");
                    
                    var duration = item.EndTime.HasValue ? (int)(item.EndTime.Value - item.StartTime).TotalMinutes : 0;
                    row.CreateCell(5).SetCellValue(duration);
                    
                    var target = lines.GetValueOrDefault(item.LineId);
                    row.CreateCell(6).SetCellValue(target ?? 0);
                    row.CreateCell(7).SetCellValue(target.HasValue && duration > target.Value ? "DA" : "NU");
                }

                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                var stream = new MemoryStream();
                workbook.Write(stream, true);
                stream.Position = 0;

                return Results.File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Changeover_History_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            });

            return app;
        }
    }
}
