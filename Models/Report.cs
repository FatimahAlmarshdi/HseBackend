using System;
using System.ComponentModel.DataAnnotations;

namespace HseBackend.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        public string EmployeeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 

        // New: Report Type Classification
        public string ReportType { get; set; } = "General"; // HIRA, NearMiss, Accident, CAPA, Training, Other

        // Common Fields
        public string IncidentType { get; set; } = string.Empty; // Used as Title/Subject
        public string Description { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? PdfUrl { get; set; } // Path to generated PDF
        
        // --- Specific Fields for Accident / CAPA ---
        public string? Location { get; set; }
        public string? EquipmentNumber { get; set; }
        public string? Department { get; set; }
        public string? RiskLevel { get; set; } // High, Medium, Low
        public string? UnsafeType { get; set; } // Unsafe Act, Unsafe Condition

        // --- Workflow States ---
        public string Status { get; set; } = "Submitted"; 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // --- Inspector Action ---
        public string? InspectorComment { get; set; }
        
        // --- Supervisor Action ---
        public string? SupervisorNote { get; set; }
        public string? AssignedTo { get; set; } // Email of the responsible employee (for CAPA)

        // --- CAPA / Recipient Action (Employee Response) ---
        public string? CorrectiveAction { get; set; } 
        public string? PreventiveAction { get; set; }
        public string? SolutionReply { get; set; } // General reply if not CAPA

        // --- Training & Awareness Specifics ---
        public string? TrainingType { get; set; } // Drill, Internal, External...
        public int? AttendeesCount { get; set; }
        public string? TrainingDuration { get; set; }
        public string? Recommendations { get; set; }
        public string? AttendanceSheetUrl { get; set; }

        // --- Accident Specifics ---
        public string? AccidentTime { get; set; }
        public string? InjuredPerson { get; set; }
        public string? InjuryType { get; set; }
        public string? ImmediateAction { get; set; }

        // --- Inspection Responses ---
        public ICollection<InspectionResponse>? InspectionResponses { get; set; }
    }
}
