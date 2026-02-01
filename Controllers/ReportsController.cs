
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HseBackend.Data;
using HseBackend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace HseBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HseBackend.Services.PdfService _pdfService;
        private readonly HseBackend.Services.EmailService _emailService;

        public ReportsController(AppDbContext context, HseBackend.Services.PdfService pdfService, HseBackend.Services.EmailService emailService)
        {
            _context = context;
            _pdfService = pdfService;
            _emailService = emailService;
        }

        // 1. Employee: Create Report
        [HttpPost]
        public async Task<ActionResult<Report>> CreateReport(Report report)
        {
            report.Status = "Submitted";
            report.CreatedAt = DateTime.Now;
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMyReports), new { id = report.Id }, report);
        }

        // 1. Employee: Get My Reports
        [HttpGet("my-reports")]
        public async Task<ActionResult<IEnumerable<Report>>> GetMyReports()
        {
            // In real app, filter by User ID. Here we return all for demo.
            return await _context.Reports
                                 .Include(r => r.InspectionResponses)
                                 .OrderByDescending(r => r.CreatedAt)
                                 .ToListAsync();
        }

        // 1. Employee: Update Returned Report
        [HttpPut("{id}/resubmit")]
        public async Task<IActionResult> ResubmitReport(int id, [FromBody] Report updatedInfo)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            if (report.Status != "Returned") return BadRequest("Report is not in Returned state.");

            report.Description = updatedInfo.Description;
            report.IncidentType = updatedInfo.IncidentType;
            report.Status = "Submitted"; // Back to Inspector
            report.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Report Resubmitted Successfully" });
        }


        // 2. Inspector: Get Incoming (Only Submitted)
        [HttpGet("inspector/incoming")]
        public async Task<ActionResult<IEnumerable<Report>>> GetInspectorIncoming()
        {
            return await _context.Reports
                                 .Include(r => r.InspectionResponses)
                                 .Where(r => r.Status == "Submitted")
                                 .OrderByDescending(r => r.CreatedAt)
                                 .ToListAsync();
        }

        // 2. Inspector: Review (Approve or Return)
        [HttpPut("{id}/inspector-action")]
        public async Task<IActionResult> InspectorAction(int id, [FromBody] System.Text.Json.JsonElement action)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            string decision = action.GetProperty("decision").ToString(); // "Approve" or "Return"
            string comment = action.GetProperty("comment").ToString();

            if (decision == "Approve")
            {
                report.Status = "Reviewed"; // Goes to Supervisor
            }
            else if (decision == "Return")
            {
                report.Status = "Returned"; // Goes back to Employee
                report.InspectorComment = comment;
            }
            report.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Report {decision}ed Successfully" });
        }


        // 3. Supervisor: Get Incoming (Only Reviewed)
        [HttpGet("supervisor/incoming")]
        public async Task<ActionResult<IEnumerable<Report>>> GetSupervisorIncoming()
        {
            return await _context.Reports
                                 .Include(r => r.InspectionResponses)
                                 .Where(r => r.Status == "Reviewed")
                                 .OrderByDescending(r => r.CreatedAt)
                                 .ToListAsync();
        }

        // 3. Supervisor: Final Decision (FinalApprove, Reject, Direct)
        [HttpPut("{id}/supervisor-action")]
        public async Task<IActionResult> SupervisorAction(int id, [FromBody] System.Text.Json.JsonElement action)
        {
            var report = await _context.Reports
                                       .Include(r => r.InspectionResponses)
                                       .FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            string decision = action.GetProperty("decision").ToString(); // FinalApprove, Reject, Direct
            string note = action.GetProperty("note").ToString();

            if (decision == "FinalApprove")
            {
                report.Status = "FinalApproved";
            }
            else if (decision == "Reject")
            {
                report.Status = "Rejected";
            }
            else if (decision == "Direct")
            {
                report.Status = "Directed";
                if(action.TryGetProperty("assignedTo", out var assignedToProp)) {
                    report.AssignedTo = assignedToProp.ToString();
                }
                
                // Generate PDF
                try 
                {
                    // Ensure InspectionResponses are loaded if needed for PDF
                    // Here we rely on them being lazily loaded or we should have included them. 
                    // For simplicity, we just generate with what we have.
                    // If InspectionResponses are crucial, we should fetch report with .Include() at line 112.
                    var pdfUrl = _pdfService.GenerateReportPdf(report);
                    report.PdfUrl = pdfUrl;
                    
                    // Simulate Email Sending logic here
                    Console.WriteLine($"[Email Simulation] Sending PDF to {report.AssignedTo}: {pdfUrl}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error generating PDF: {ex.Message}");
                }
            }
            
            report.SupervisorNote = note;
            report.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Report {decision} Successfully" });
        }

        // 4. Employee: Get Assigned Tasks (Inbox) - Security: AssignedTo == Email
        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<Report>>> GetMyAssignedReports([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required");

            return await _context.Reports
                                 .Where(r => (r.Status == "Directed" || r.Status == "Closed") && r.AssignedTo == email)
                                 .OrderByDescending(r => r.CreatedAt)
                                 .ToListAsync();
        }

        // 4. Recipient: Reply (Solve)
        [HttpPut("{id}/recipient-reply")]
        public async Task<IActionResult> RecipientReply(int id, [FromBody] System.Text.Json.JsonElement replyData)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            if (replyData.TryGetProperty("reply", out var replyProp))
                 report.SolutionReply = replyProp.GetString();

            if (replyData.TryGetProperty("correctiveAction", out var caProp))
                 report.CorrectiveAction = caProp.GetString();

            if (replyData.TryGetProperty("preventiveAction", out var paProp))
                 report.PreventiveAction = paProp.GetString();

            report.Status = "Solved"; // Goes back to Supervisor
            report.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Reply Sent Successfully" });
        }

        // 5. Supervisor: Get Solved (Replies from Recipient)
        [HttpGet("supervisor/solved")]
        public async Task<ActionResult<IEnumerable<Report>>> GetSupervisorSolved()
        {
            return await _context.Reports
                                 .Where(r => r.Status == "Solved")
                                 .OrderByDescending(r => r.CreatedAt)
                                 .ToListAsync();
        }

        // 5. Supervisor: Evaluate Solution (Approve & Close OR Reject & Return)
        [HttpPut("{id}/evaluate-solution")]
        public async Task<IActionResult> EvaluateSolution(int id, [FromBody] System.Text.Json.JsonElement action)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            string decision = action.GetProperty("decision").ToString(); // "Approve", "Return"
            string note = action.GetProperty("note").ToString();
            
            report.SupervisorNote = note; // Stores the confirmation message or rejection reason

            if (decision == "Approve")
            {
                // Action 1: Forward to Employee (Close)
                report.Status = "Closed";

                // Generate Final PDF
                try
                {
                    // Ensure InspectionResponses are loaded for full detail
                    if (report.InspectionResponses == null)
                    {
                         await _context.Entry(report).Collection(r => r.InspectionResponses).LoadAsync();
                    }

                    var pdfUrl = _pdfService.GenerateReportPdf(report);
                    report.PdfUrl = pdfUrl;
                    
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var fullPdfUrl = baseUrl + pdfUrl;
                    var pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + pdfUrl.Replace("/", "\\"));

                    // Send Email to Recipient (Assigned User)
                    if (!string.IsNullOrEmpty(report.AssignedTo))
                    {
                       await _emailService.SendEmailWithAttachmentAsync(
                           report.AssignedTo,
                           $"HSE Report Closed & Approved: #{report.Id}",
                           $"<p>Dear User,</p><p>The report you worked on has been approved and closed by the supervisor.</p><p>Please find the final report attached.</p>",
                           pdfPath
                       );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during final approval logic: {ex.Message}");
                }
            }
            else if (decision == "Return")
            {
                // Action 2: Return to Recipient (Reject Solution)
                report.Status = "Directed"; 
                // The report goes back to the Recipient's Inbox (assigned tasks) 
                // carrying the SupervisorNote as feedback.
            }

            report.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Solution {decision}ed Successfully" });
        }

        // 6. General: Get All Finalized (Archive)
        [HttpGet("archive")]
        public async Task<ActionResult<IEnumerable<Report>>> GetArchive()
        {
            return await _context.Reports
                                 .Where(r => r.Status == "FinalApproved" || r.Status == "Closed" || r.Status == "Rejected")
                                 .ToListAsync();
        }
    }
}
