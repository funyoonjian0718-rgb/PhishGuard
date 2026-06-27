using Microsoft.AspNetCore.Mvc;
using PhishGuard.Models;
using PhishGuard.Services;

namespace PhishGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailAnalysisController : ControllerBase
    {
        private readonly PhishingAnalysisService _phishingAnalysisService;
        private readonly EmailHistoryService _emailHistoryService;

        public EmailAnalysisController(
            PhishingAnalysisService phishingAnalysisService,
            EmailHistoryService emailHistoryService)
        {
            _phishingAnalysisService = phishingAnalysisService;
            _emailHistoryService = emailHistoryService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] EmailAnalysis email)
        {
            if (email == null)
            {
                return BadRequest(new
                {
                    message = "Email data is required."
                });
            }

            var result = _phishingAnalysisService.AnalyzeEmail(email);

            var savedRecord = await _emailHistoryService.SaveScanAsync(email, result);

            result.ScanId = savedRecord.Id;

            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var history = await _emailHistoryService.GetRecentScansAsync();
            return Ok(history);
        }

        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetHistoryDetails(int id)
        {
            var details = await _emailHistoryService.GetScanDetailsAsync(id);

            if (details == null)
            {
                return NotFound(new
                {
                    message = "Scan record not found."
                });
            }

            return Ok(details);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _emailHistoryService.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}