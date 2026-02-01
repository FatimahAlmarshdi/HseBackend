using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HseBackend.Models
{
    public class InspectionQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // User provided specific IDs
        public int Id { get; set; }

        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "Choice"; // Choice, Text, Upload
        public string Options { get; set; } = string.Empty; // Comma separated options
    }

    public class InspectionResponse
    {
        [Key]
        public int Id { get; set; }

        public int ReportId { get; set; } // Link to the main Report/Inspection record
        public int QuestionId { get; set; }
        
        public string Answer { get; set; } = string.Empty;
    }

    public class InspectionSubmissionDto
    {
        public required Report Report { get; set; }
        public required List<InspectionResponseDto> Responses { get; set; }
    }

    public class InspectionResponseDto
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; } = string.Empty;
    }
}
