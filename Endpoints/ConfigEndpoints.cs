/*
 * ===================================================================================
 * FIȘIER: MesSimplu/ConfigApi.cs
 * ROL: API pentru funcțiile de configurare.
 * STARE: MODIFICAT (CORECTAT AVERTISMENT COMPILARE)
 *
 * MODIFICARE (Senior Dev):
 * - CORECTAT AVERTISMENT CS8600: Variabila 'productName' din importul
 * Excel a fost schimbată în 'string?' (nullable) pentru a gestiona
 * corect rândurile goale.
 * ===================================================================================
 */

using MesEnterprise.Data;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;
using MesEnterprise.Models.Config;
using MesEnterprise.Models.Quality;
using MesEnterprise.Models.Maintenance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;

namespace MesEnterprise.Endpoints
{
    public static class ConfigEndpoints
    {
        public static IEndpointRouteBuilder MapConfigApi(this IEndpointRouteBuilder app)
        {
            var configApi = app.MapGroup("/api/config").RequireAuthorization("AdminOnly");

            // Linii
            configApi.MapGet("/lines", async (MesDbContext db) =>
                await db.Lines.Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.ScanIdentifier,
                    l.HasLiveScanning
                }).OrderBy(l => l.Name).ToListAsync());

            configApi.MapPost("/lines", async (Line line, MesDbContext db) =>
            {
                if (!string.IsNullOrEmpty(line.ScanIdentifier))
                {
                    var existing = await db.Lines.FirstOrDefaultAsync(l => l.ScanIdentifier == line.ScanIdentifier && l.Id != line.Id);
                    if (existing != null)
                    {
                        return Results.BadRequest(new { Message = "Acest Identificator Scanare este deja folosit de altă linie." });
                    }
                }

                db.Lines.Add(line);
                await db.SaveChangesAsync();
                
                // --- Creare LineStatus asociat ---
                var lineStatus = new LineStatus { LineId = line.Id, Status = "Stopped", LastStatusChange = DateTime.UtcNow };
                db.LineStatuses.Add(lineStatus);
                await db.SaveChangesAsync();
                // --- SFÂRȘIT ---

                return Results.Created($"/api/config/lines/{line.Id}", line);
            });

            configApi.MapPut("/lines/{id}", async (int id, [FromBody] Line updatedLine, MesDbContext db) =>
            {
                var line = await db.Lines.FindAsync(id);
                if (line == null) return Results.NotFound();

                if (!string.IsNullOrEmpty(updatedLine.ScanIdentifier))
                {
                    var existing = await db.Lines.FirstOrDefaultAsync(l => l.ScanIdentifier == updatedLine.ScanIdentifier && l.Id != id);
                    if (existing != null)
                    {
                        return Results.BadRequest(new { Message = "Acest Identificator Scanare este deja folosit de altă linie." });
                    }
                }

                line.Name = updatedLine.Name;
                line.HasLiveScanning = updatedLine.HasLiveScanning;
                line.ScanIdentifier = updatedLine.ScanIdentifier;

                await db.SaveChangesAsync();
                return Results.Ok(line);
            });

            configApi.MapDelete("/lines/{id}", async (int id, MesDbContext db) =>
            {
                var line = await db.Lines.FindAsync(id);
                if (line == null) return Results.NotFound();

                var hasEquipment = await db.Equipments.AnyAsync(e => e.LineId == id);
                if (hasEquipment)
                {
                    return Results.BadRequest(new { Message = "Linia nu poate fi ștearsă. Ștergeți mai întâi echipamentele asociate." });
                }
                
                // --- Ștergere LineStatus asociat ---
                var lineStatus = await db.LineStatuses.FirstOrDefaultAsync(ls => ls.LineId == id);
                if (lineStatus != null)
                {
                    db.LineStatuses.Remove(lineStatus);
                }
                // --- SFÂRȘIT ---

                db.Lines.Remove(line);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });


            // Echipamente
            configApi.MapGet("/equipments", async (MesDbContext db) =>
                await db.Equipments.Include(e => e.Line)
                    .Select(e => new { e.Id, e.Name, e.LineId, LineName = e.Line.Name })
                    .OrderBy(e => e.LineName).ThenBy(e => e.Name)
                    .ToListAsync());

            configApi.MapPost("/equipments", async (Equipment equipment, MesDbContext db) =>
            {
                db.Equipments.Add(equipment);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/equipments/{equipment.Id}", equipment);
            });

            configApi.MapDelete("/equipments/{id}", async (int id, MesDbContext db) =>
            {
                var eq = await db.Equipments.FindAsync(id);
                if (eq == null) return Results.NotFound();

                var hasTickets = await db.InterventieTichete.AnyAsync(t => t.EquipmentId == id);
                if (hasTickets)
                {
                    return Results.BadRequest(new { Message = "Echipamentul nu poate fi șters. Există tichete de intervenție asociate." });
                }

                db.Equipments.Remove(eq);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Produse
            configApi.MapGet("/products", async (MesDbContext db) =>
                await db.Products.OrderBy(p => p.Name).ToListAsync());

            configApi.MapPost("/products", async (Product product, MesDbContext db) =>
            {
                if (product.CycleTimeSeconds <= 0)
                {
                    return Results.BadRequest(new { Message = "Timpul de ciclu trebuie să fie mai mare ca 0." });
                }
                db.Products.Add(product);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/products/{product.Id}", product);
            });

            configApi.MapDelete("/products/{id}", async (int id, MesDbContext db) =>
            {
                var prod = await db.Products.FindAsync(id);
                if (prod == null) return Results.NotFound();

                var hasLogs = await db.ProductionLogs.AnyAsync(l => l.ProductId == id);
                if (hasLogs)
                {
                    return Results.BadRequest(new { Message = "Produsul nu poate fi șters. Există log-uri de producție asociate." });
                }

                db.Products.Remove(prod);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Motive Avarii
            configApi.MapGet("/breakdownreasons", async (MesDbContext db) =>
                await db.BreakdownReasons.OrderBy(r => r.Name).ToListAsync());

            configApi.MapPost("/breakdownreasons", async (BreakdownReason reason, MesDbContext db) =>
            {
                db.BreakdownReasons.Add(reason);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/breakdownreasons/{reason.Id}", reason);
            });

            configApi.MapDelete("/breakdownreasons/{id}", async (int id, MesDbContext db) =>
            {
                var reason = await db.BreakdownReasons.FindAsync(id);
                if (reason == null) return Results.NotFound();

                if (reason.Name.ToLower() == "stop la defect" || reason.Name.ToLower() == "lipsă material")
                {
                    return Results.BadRequest(new { Message = "Motivele de bază ale sistemului nu pot fi șterse." });
                }

                db.BreakdownReasons.Remove(reason);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Schimburi
            configApi.MapGet("/shifts", async (MesDbContext db) =>
                await db.Shifts.OrderBy(s => s.StartTime).ToListAsync());

            configApi.MapPost("/shifts", async (Shift shift, MesDbContext db) =>
            {
                db.Shifts.Add(shift);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/shifts/{shift.Id}", shift);
            });

            configApi.MapPut("/shifts/{id}", async (int id, [FromBody] Shift updatedShift, MesDbContext db) =>
            {
                var shift = await db.Shifts.FindAsync(id);
                if (shift == null) return Results.NotFound();

                shift.Name = updatedShift.Name;
                shift.StartTime = updatedShift.StartTime;
                shift.EndTime = updatedShift.EndTime;

                await db.SaveChangesAsync();
                return Results.Ok(shift);
            });

            configApi.MapDelete("/shifts/{id}", async (int id, MesDbContext db) =>
            {
                var shift = await db.Shifts.Include(s => s.Breaks).FirstOrDefaultAsync(s => s.Id == id);
                if (shift == null) return Results.NotFound();

                db.ShiftBreaks.RemoveRange(shift.Breaks);
                db.Shifts.Remove(shift);

                await db.SaveChangesAsync();
                return Results.NoContent();
            });


            // Pauze pe Schimb
            configApi.MapGet("/shifts/{shiftId}/breaks", async (int shiftId, MesDbContext db) =>
                await db.ShiftBreaks.Where(b => b.ShiftId == shiftId).OrderBy(b => b.BreakTime).ToListAsync());

            configApi.MapPost("/shifts/{shiftId}/breaks", async (int shiftId, ShiftBreak shiftBreak, MesDbContext db) =>
            {
                shiftBreak.ShiftId = shiftId;
                db.ShiftBreaks.Add(shiftBreak);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/shifts/{shiftId}/breaks/{shiftBreak.Id}", shiftBreak);
            });

            configApi.MapDelete("/breaks/{id}", async (int id, MesDbContext db) =>
            {
                var br = await db.ShiftBreaks.FindAsync(id);
                if (br == null) return Results.NotFound();
                db.ShiftBreaks.Remove(br);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Timp Dummy (Opriri Planificate)
            configApi.MapGet("/planneddowntime", async (MesDbContext db) =>
                await db.PlannedDowntimes
                    .Include(pd => pd.Line)
                    .Include(pd => pd.Product)
                    .Select(pd => new
                    {
                        pd.Id,
                        pd.LineId,
                        LineName = pd.Line.Name,
                        pd.ProductId,
                        ProductName = pd.Product.Name,
                        pd.MinutesPerHour
                    })
                    .OrderBy(pd => pd.LineName).ThenBy(pd => pd.ProductName)
                    .ToListAsync());

            configApi.MapPost("/planneddowntime", async (PlannedDowntime downtime, MesDbContext db) =>
            {
                var existing = await db.PlannedDowntimes
                    .FirstOrDefaultAsync(pd => pd.LineId == downtime.LineId && pd.ProductId == downtime.ProductId);

                if (existing != null)
                {
                    existing.MinutesPerHour = downtime.MinutesPerHour;
                }
                else
                {
                    db.PlannedDowntimes.Add(downtime);
                }
                await db.SaveChangesAsync();
                return Results.Ok(downtime);
            });

            configApi.MapDelete("/planneddowntime/{id}", async (int id, MesDbContext db) =>
            {
                var downtime = await db.PlannedDowntimes.FindAsync(id);
                if (downtime == null) return Results.NotFound();
                db.PlannedDowntimes.Remove(downtime);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // API CRUD Categorii Defecte
            configApi.MapGet("/defectcategories", async (MesDbContext db) =>
                await db.DefectCategories.OrderBy(dc => dc.Name).ToListAsync());

            configApi.MapPost("/defectcategories", async (DefectCategory category, MesDbContext db) =>
            {
                db.DefectCategories.Add(category);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/defectcategories/{category.Id}", category);
            });

            configApi.MapPut("/defectcategories/{id}", async (int id, DefectCategory updatedCategory, MesDbContext db) =>
            {
                var cat = await db.DefectCategories.FindAsync(id);
                if (cat == null) return Results.NotFound();

                cat.Name = updatedCategory.Name;
                cat.Color = updatedCategory.Color;
                await db.SaveChangesAsync();
                return Results.Ok(cat);
            });

            configApi.MapDelete("/defectcategories/{id}", async (int id, MesDbContext db) =>
            {
                var cat = await db.DefectCategories.FindAsync(id);
                if (cat == null) return Results.NotFound();

                var hasDefects = await db.DefectCodes.AnyAsync(d => d.DefectCategoryId == id);
                if (hasDefects)
                {
                    return Results.BadRequest(new { Message = "Categoria nu poate fi ștearsă. Ștergeți mai întâi defectele asociate." });
                }

                db.DefectCategories.Remove(cat);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // API CRUD Defecte
            configApi.MapGet("/defectcategories/{categoryId}/defects", async (int categoryId, MesDbContext db) =>
                await db.DefectCodes.Where(d => d.DefectCategoryId == categoryId)
                    .Include(d => d.Category)
                    .Select(d => new {
                        d.Id,
                        d.Name,
                        d.DefectCategoryId,
                        CategoryName = d.Category.Name
                    })
                    .OrderBy(d => d.Name)
                    .ToListAsync());

            configApi.MapPost("/defects", async (DefectCode defectCode, MesDbContext db) =>
            {
                db.DefectCodes.Add(defectCode);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/defects/{defectCode.Id}", defectCode);
            });

            configApi.MapDelete("/defects/{id}", async (int id, MesDbContext db) =>
            {
                var defect = await db.DefectCodes.FindAsync(id);
                if (defect == null) return Results.NotFound();

                var hasLogs = await db.ProductionLogDefects.AnyAsync(pld => pld.DefectCodeId == id);
                if (hasLogs)
                {
                    return Results.BadRequest(new { Message = "Defectul nu poate fi șters. Este folosit în log-uri de producție." });
                }

                db.DefectCodes.Remove(defect);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });


            // API CRUD Configurare Mentenanță
            var mentenantaApi = configApi.MapGroup("/mentenanta");

            // CRUD Probleme Raportate
            mentenantaApi.MapGet("/probleme", async (MesDbContext db) =>
                await db.ProblemeRaportate.OrderBy(p => p.Nume).ToListAsync());

            mentenantaApi.MapPost("/probleme", async (ProblemaRaportata p, MesDbContext db) =>
            {
                db.ProblemeRaportate.Add(p);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/mentenanta/probleme/{p.Id}", p);
            });

            // NOU: PUT pentru ProblemaRaportata
            mentenantaApi.MapPut("/probleme/{id}", async (int id, [FromBody] ProblemaRaportata updatedProblema, MesDbContext db) =>
            {
                var problema = await db.ProblemeRaportate.FindAsync(id);
                if (problema == null) return Results.NotFound();

                problema.Nume = updatedProblema.Nume;
                await db.SaveChangesAsync();
                return Results.Ok(problema);
            });

            mentenantaApi.MapDelete("/probleme/{id}", async (int id, MesDbContext db) =>
            {
                var item = await db.ProblemeRaportate.FindAsync(id);
                if (item == null) return Results.NotFound();

                var hasCorelatii = await db.ProblemaDefectiuneCorelatii.AnyAsync(pdc => pdc.ProblemaRaportataId == id);
                if (hasCorelatii)
                {
                    return Results.BadRequest(new { Message = "Problema nu poate fi ștearsă. Există corelații asociate." });
                }
                var hasTickets = await db.InterventieTichete.AnyAsync(t => t.ProblemaRaportataId == id);
                if (hasTickets)
                {
                    return Results.BadRequest(new { Message = "Problema nu poate fi ștearsă. Există tichete de intervenție asociate." });
                }

                db.ProblemeRaportate.Remove(item);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // NOU: GET pentru corelațiile unei probleme
            mentenantaApi.MapGet("/corelatii/{problemaId}", async (int problemaId, MesDbContext db) =>
            {
                var corelatii = await db.ProblemaDefectiuneCorelatii
                    .Where(pdc => pdc.ProblemaRaportataId == problemaId)
                    .Select(pdc => new {
                        pdc.ProblemaRaportataId,
                        pdc.DefectiuneIdentificataId
                    })
                    .ToListAsync();
                return Results.Ok(corelatii);
            });


            // CRUD Defecțiuni Identificate
            mentenantaApi.MapGet("/defectiuni", async (MesDbContext db) =>
                await db.DefectiuniIdentificate.OrderBy(d => d.Nume).ToListAsync());
            mentenantaApi.MapPost("/defectiuni", async (DefectiuneIdentificata d, MesDbContext db) =>
            {
                db.DefectiuniIdentificate.Add(d);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/mentenanta/defectiuni/{d.Id}", d);
            });

            // NOU: PUT pentru DefectiuneIdentificata
            mentenantaApi.MapPut("/defectiuni/{id}", async (int id, [FromBody] DefectiuneIdentificata updatedDefectiune, MesDbContext db) =>
            {
                var defectiune = await db.DefectiuniIdentificate.FindAsync(id);
                if (defectiune == null) return Results.NotFound();

                defectiune.Nume = updatedDefectiune.Nume;
                // Proprietatea 'EsteOptiuneAltul' nu există în DefectiuneIdentificata, linia a fost eliminată.
                await db.SaveChangesAsync();
                return Results.Ok(defectiune);
            });

            mentenantaApi.MapDelete("/defectiuni/{id}", async (int id, MesDbContext db) =>
            {
                var item = await db.DefectiuniIdentificate.FindAsync(id);
                if (item == null) return Results.NotFound();

                var hasCorelatii = await db.ProblemaDefectiuneCorelatii.AnyAsync(pdc => pdc.DefectiuneIdentificataId == id);
                if (hasCorelatii)
                {
                    return Results.BadRequest(new { Message = "Defecțiunea nu poate fi ștearsă. Există corelații asociate." });
                }

                db.DefectiuniIdentificate.Remove(item);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // API CRUD REGULI "STOP LA DEFECT"
            configApi.MapGet("/stopondefectrules", async (MesDbContext db) =>
                await db.StopOnDefectRules
                    .Include(r => r.Line)
                    .Include(r => r.Product)
                    .Select(r => new {
                        r.Id,
                        r.LineId,
                        LineName = r.Line.Name,
                        r.ProductId,
                        ProductName = r.Product != null ? r.Product.Name : "Toate Produsele",
                        r.MaxConsecutiveNRFT,
                        r.MaxConsecutiveScrap,
                        r.MaxNrftPerHour
                    })
                    .ToListAsync());

            configApi.MapPost("/stopondefectrules", async (StopOnDefectRule rule, MesDbContext db) =>
            {
                db.StopOnDefectRules.Add(rule);
                await db.SaveChangesAsync();
                return Results.Created($"/api/config/stopondefectrules/{rule.Id}", rule);
            });

            configApi.MapPut("/stopondefectrules/{id}", async (int id, StopOnDefectRule updatedRule, MesDbContext db) =>
            {
                var rule = await db.StopOnDefectRules.FindAsync(id);
                if (rule == null) return Results.NotFound();

                rule.LineId = updatedRule.LineId;
                rule.ProductId = updatedRule.ProductId;
                rule.MaxConsecutiveNRFT = updatedRule.MaxConsecutiveNRFT;
                rule.MaxConsecutiveScrap = updatedRule.MaxConsecutiveScrap;
                rule.MaxNrftPerHour = updatedRule.MaxNrftPerHour;

                await db.SaveChangesAsync();
                return Results.Ok(rule);
            });

            configApi.MapDelete("/stopondefectrules/{id}", async (int id, MesDbContext db) =>
            {
                var rule = await db.StopOnDefectRules.FindAsync(id);
                if (rule == null) return Results.NotFound();

                db.StopOnDefectRules.Remove(rule);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Setări Sistem
            configApi.MapGet("/settings", async (MesDbContext db) =>
            {
                var settingsList = await db.SystemSettings.ToListAsync();
                var settingsMap = settingsList.ToDictionary(s => s.Key, s => s.Value);

                var settingsDto = new AppSettingsDto
                {
                    GoodPartsLoggingMode = settingsMap.GetValueOrDefault("GoodPartsLoggingMode", "Overwrite"),
                    DowntimeScrapLoggingMode = settingsMap.GetValueOrDefault("DowntimeScrapLoggingMode", "Overwrite"),
                    JustificationThresholdPercent = int.TryParse(settingsMap.GetValueOrDefault("JustificationThresholdPercent", "85"), out var p) ? p : 85,
                    RequireJustification = bool.TryParse(settingsMap.GetValueOrDefault("RequireJustification", "true"), out var r) ? r : true,
                    AutoCloseTicketMinutes = int.TryParse(settingsMap.GetValueOrDefault("AutoCloseTicketMinutes", "0"), out var a) ? a : 0
                };
                return Results.Ok(settingsDto);
            });

            configApi.MapPut("/settings", async ([FromBody] AppSettingsDto settingsDto, MesDbContext db) =>
            {
                await UpdateSetting(db, "GoodPartsLoggingMode", settingsDto.GoodPartsLoggingMode);
                await UpdateSetting(db, "DowntimeScrapLoggingMode", settingsDto.DowntimeScrapLoggingMode);
                await UpdateSetting(db, "JustificationThresholdPercent", settingsDto.JustificationThresholdPercent.ToString());
                await UpdateSetting(db, "RequireJustification", settingsDto.RequireJustification.ToString());
                await UpdateSetting(db, "AutoCloseTicketMinutes", settingsDto.AutoCloseTicketMinutes.ToString());

                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Setările au fost salvate." });
            });

            // Import Excel
            configApi.MapPost("/import/products", async (IFormFile file, MesDbContext db) =>
            {
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { Message = "Niciun fișier încărcat." });
                }

                int added = 0;
                int updated = 0;
                List<string> errors = new List<string>();

                using (var stream = file.OpenReadStream())
                {
                    IWorkbook workbook = new XSSFWorkbook(stream);
                    ISheet sheet = workbook.GetSheetAt(0);

                    IRow headerRow = sheet.GetRow(0);
                    if (headerRow == null ||
                        headerRow.GetCell(0)?.StringCellValue?.Trim().ToLower() != "numeprodus" ||
                        headerRow.GetCell(1)?.StringCellValue?.Trim().ToLower() != "timpciclusecunde")
                    {
                        errors.Add("Eroare Antet: Coloanele așteptate sunt 'NumeProdus' (A) și 'TimpCicluSecunde' (B).");
                        return Results.BadRequest(new { Errors = errors, Added = 0, Updated = 0 });
                    }

                    for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
                    {
                        IRow row = sheet.GetRow(rowIdx);
                        if (row == null) continue;

                        try
                        {
                            // --- MODIFICARE PENTRU A CORECTA AVERTISMENTUL CS8600 ---
                            string? productName = row.GetCell(0)?.StringCellValue?.Trim();
                            // --- SFÂRȘIT MODIFICARE ---

                            ICell cycleTimeCell = row.GetCell(1);
                            double cycleTime = 0;

                            if (cycleTimeCell == null || cycleTimeCell.CellType == CellType.Blank)
                            {
                                errors.Add($"Eroare la rândul {rowIdx + 1}: TimpCicluSecunde lipsește.");
                                continue;
                            }

                            if (cycleTimeCell.CellType != CellType.Numeric)
                            {
                                if (cycleTimeCell.CellType == CellType.String)
                                {
                                    if (!double.TryParse(cycleTimeCell.StringCellValue, NumberStyles.Any, CultureInfo.InvariantCulture, out cycleTime))
                                    {
                                        errors.Add($"Eroare la rândul {rowIdx + 1}: TimpCicluSecunde '{cycleTimeCell.StringCellValue}' nu este un număr valid.");
                                        continue;
                                    }
                                }
                                else
                                {
                                    errors.Add($"Eroare la rândul {rowIdx + 1}: TimpCicluSecunde (coloana B) nu este un număr.");
                                    continue;
                                }
                            }
                            else
                            {
                                cycleTime = cycleTimeCell.NumericCellValue;
                            }


                            if (string.IsNullOrEmpty(productName))
                            {
                                errors.Add($"Eroare la rândul {rowIdx + 1}: NumeProdus (coloana A) lipsește.");
                                continue;
                            }

                            if (cycleTime <= 0)
                            {
                                errors.Add($"Eroare la rândul {rowIdx + 1}: TimpCicluSecunde trebuie să fie > 0.");
                                continue;
                            }

                            var existingProduct = await db.Products.FirstOrDefaultAsync(p => p.Name == productName);
                            if (existingProduct != null)
                            {
                                existingProduct.CycleTimeSeconds = cycleTime;
                                updated++;
                            }
                            else
                            {
                                db.Products.Add(new Product { Name = productName, CycleTimeSeconds = cycleTime });
                                added++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Eroare la procesarea rândului {rowIdx + 1}: {ex.Message}");
                        }
                    }
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new { Added = added, Updated = updated, Errors = errors });
            });

            return app;
        }

        // Funcție helper pentru salvarea setărilor
        private static async Task UpdateSetting(MesDbContext db, string key, string value)
        {
            var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                db.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
            }
        }
    }
}