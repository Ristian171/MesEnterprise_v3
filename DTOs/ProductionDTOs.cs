using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.DTOs
{
    public class ProductionLogRequest
    {
        [Required]
        public required string LogTime { get; set; }
        
        public int GoodParts { get; set; }
        public int ScrapParts { get; set; }
        public int NrftParts { get; set; }
        public List<DefectInput>? Defecte { get; set; }
        
        public int? DeclaredDowntimeReasonId { get; set; }
        public int DeclaredDowntimeMinutes { get; set; }
    }

    public class DefectInput
    {
        public int DefectCodeId { get; set; }
        public int Quantity { get; set; }
    }

    public class OperatorCommandRequest
    {
        [Required]
        public required string Command { get; set; }
        
        public int? Value { get; set; }
    }
}

    public record TimeSlotDto
    {
        [Required] public required string Value { get; init; } // Data-ora ISO: "2023-10-27T14:00:00"
        [Required] public required string Text { get; init; } // Text afișat: "14:00 - 15:00"
        public bool IsAvailable { get; init; }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers] 
        public TimeSlotDto(TimeSpan ts, DateTime now, int dayOffset = 0)
        {
            var startTime = ts;
            var endTime = ts.Add(TimeSpan.FromHours(1));
            var logDate = now.Date.AddDays(dayOffset);
            
            var logDateTime = logDate.Add(startTime);
            Value = logDateTime.ToString("o");
            Text = $"{startTime:hh\\:mm} - {endTime:hh\\:mm}";
            IsAvailable = logDate.Add(endTime) <= now;
        }
    }

    public record OperatorStateDto
    {
        public int LineId { get; init; }
        public required string LineName { get; init; }
        public required string LineStatus { get; init; }
        public int? ProductId { get; init; }
        public string? ProductName { get; init; }
        public int? ShiftId { get; init; }
        public string? ShiftName { get; init; }
        public string? ShiftStartTime { get; init; }
        public string? ShiftEndTime { get; init; }

        public int CurrentHourTarget { get; init; }
        public int RealTimeTarget { get; init; } 
        public int CurrentHourGoodParts { get; init; }
        public int CurrentHourScrap { get; init; }
        public double CurrentHourOEE { get; init; }

        // Live Scan properties
        public string? ScanIdentifier { get; init; }
        public bool LiveScanAvailable { get; init; }
        public bool LiveScanEnabled { get; init; }

        // OEE Target property
        public double? OeeTarget { get; init; }

        public required List<TimeSlotDto> AvailableTimeSlots { get; init; }
        public required List<ProductionLogDto> CurrentShiftLogs { get; init; }
    }

    public record ProductionLogDto
    {
        public int Id { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public int Target { get; init; }
        public int Good { get; init; }
        public int Scrap { get; init; }
        public double Oee { get; init; }
        public bool NeedsJustification { get; init; }
        public string? JustificationReasonName { get; init; }
        public int DeclaredDowntimeMinutes { get; init; } 
        public string? DeclaredDowntimeReasonName { get; set; } 
        public int SystemStopMinutes { get; set; } 
    }

    public record ObservatieOperatorRequest(string Text);

    public record JustificationRequest(int BreakdownReasonId, string? Comments);
    
    public record JustifyOeeRequest(int ProductionLogId, string Reason);

    public record StartInterventieTichetRequest(
        int LineId,
        int EquipmentId,
        int ProblemaRaportataId
    );

    public record UpdateInterventieTichetRequest(
        string Status,
        DateTime? DataStartInterventie,
        DateTime? DataStopInterventie,
        int? ProblemaRaportataId,
        int? DefectiuneIdentificataId,
        string? DefectiuneTextLiber,
        bool InfluenteazaProdusul
    );

    public record StartProductionRequest(int LineId, int ProductId);
    public record StartChangeoverRequest(int LineId, int ProductFromId, int ProductToId);
    public record EndChangeoverRequest(int LineId);
    public record ScanRequest(string Identifier, string ScanData);
    public record LiveScanToggleRequest(bool Enabled);
    public record ProductionLogEditRequest(int ActualParts, int ScrapParts, int NrftParts);

    public class AppSettingsDto
    {
        public string GoodPartsLoggingMode { get; set; } = "Overwrite";
        public string DowntimeScrapLoggingMode { get; set; } = "Overwrite";
        public int JustificationThresholdPercent { get; set; } = 85;
        public bool RequireJustification { get; set; } = true;
        public int AutoCloseTicketMinutes { get; set; } = 0;
    }
