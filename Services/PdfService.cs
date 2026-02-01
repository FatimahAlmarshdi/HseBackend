using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HseBackend.Models;
using System.IO;

namespace HseBackend.Services
{
    public class PdfService
    {
        private readonly IWebHostEnvironment _env;

        public PdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GenerateReportPdf(Report report)
        {
            var fileName = $"report_{report.Id}_{DateTime.Now.Ticks}.pdf";
            var outputFolder = Path.Combine(_env.WebRootPath, "pdfs");
            Directory.CreateDirectory(outputFolder);
            var filePath = Path.Combine(outputFolder, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Cairo"));

                    page.Header()
                        .Text($"HSE Report - #{report.Id}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(15);
                            x.Item().Text($"Date: {report.CreatedAt:yyyy-MM-dd HH:mm}");
                            x.Item().Text($"Status: {report.Status}").FontColor(report.Status == "Closed" ? Colors.Green.Medium : Colors.Red.Medium).Bold();
                            x.Item().Text($"Report Type: {report.ReportType}");
                            x.Item().Text($"Employee: {report.EmployeeName} ({report.Email})");
                            
                            x.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            x.Item().Text("Incident Details:").Bold().FontSize(14);
                            x.Item().Text($"Type: {report.IncidentType}");
                            x.Item().Text($"Description: {report.Description ?? "N/A"}");
                            if (!string.IsNullOrEmpty(report.Location)) x.Item().Text($"Location: {report.Location}");
                            if (!string.IsNullOrEmpty(report.Department)) x.Item().Text($"Department: {report.Department}");

                            if (report.InspectionResponses != null && report.InspectionResponses.Any())
                            {
                                x.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                x.Item().Text("Inspection Checklist Results:").Bold().FontSize(14);
                                foreach(var resp in report.InspectionResponses)
                                {
                                    x.Item().Row(row => {
                                        row.RelativeItem().Text($"Q: {resp.QuestionId}"); // Ideally fetch Question Text if possible
                                        row.RelativeItem().Text(resp.Answer).FontColor(resp.Answer == "Yes" ? Colors.Green.Medium : Colors.Red.Medium);
                                    });
                                }
                            }
                            
                            // Inspector Section
                            if(!string.IsNullOrEmpty(report.InspectorComment))
                            {
                                x.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                x.Item().Text("Inspector Review:").Bold().FontSize(14);
                                x.Item().Text(report.InspectorComment);
                            }

                            // Assignment / Supervisor Section
                            if(!string.IsNullOrEmpty(report.AssignedTo))
                            {
                                x.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                x.Item().Text("Assignment:").Bold().FontSize(14);
                                x.Item().Text($"Assigned To: {report.AssignedTo}");
                            }

                            // Solution Section (Recipient)
                            if(!string.IsNullOrEmpty(report.SolutionReply) || !string.IsNullOrEmpty(report.CorrectiveAction))
                            {
                                x.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                x.Item().Text("Solution & Actions:").Bold().FontSize(14);
                                
                                if(!string.IsNullOrEmpty(report.SolutionReply))
                                    x.Item().Text($"General Reply: {report.SolutionReply}");
                                
                                if(!string.IsNullOrEmpty(report.CorrectiveAction))
                                    x.Item().Text($"Corrective Action: {report.CorrectiveAction}");

                                if(!string.IsNullOrEmpty(report.PreventiveAction))
                                    x.Item().Text($"Preventive Action: {report.PreventiveAction}");
                            }

                            // Final Supervisor Note
                            if(!string.IsNullOrEmpty(report.SupervisorNote))
                            {
                                x.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                x.Item().Text("Final Supervisor Decision/Note:").Bold().FontSize(14);
                                x.Item().Text(report.SupervisorNote);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
            .GeneratePdf(filePath);

            // Return relative path for URL
            return $"/pdfs/{fileName}";
        }
    }
}
