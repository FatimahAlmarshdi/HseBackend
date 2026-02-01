using HseBackend.Data;
using HseBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HseBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InspectionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("questions")]
        public async Task<ActionResult<IEnumerable<InspectionQuestion>>> GetQuestions()
        {
            return await _context.InspectionQuestions.OrderBy(q => q.Id).ToListAsync();
        }

        [HttpPost("submit")]
        public async Task<ActionResult<Report>> SubmitInspection([FromBody] InspectionSubmissionDto submission)
        {
            if (submission.Report == null || submission.Responses == null)
            {
                return BadRequest("Report and Responses are required.");
            }

            // Create the main Report entry
            // Force ReportType to Inspection if not set, or trust client
            submission.Report.ReportType = "Inspection";
            submission.Report.CreatedAt = DateTime.Now;
            submission.Report.Status = "Submitted"; // Ready for Supervisor

            _context.Reports.Add(submission.Report);
            await _context.SaveChangesAsync(); // Generates Report.Id

            // Add responses
            foreach (var responseDto in submission.Responses)
            {
                var response = new InspectionResponse
                {
                    ReportId = submission.Report.Id,
                    QuestionId = responseDto.QuestionId,
                    Answer = responseDto.Answer
                };
                _context.InspectionResponses.Add(response);
            }

            await _context.SaveChangesAsync();

            return Ok(submission.Report);
        }
    }
}
