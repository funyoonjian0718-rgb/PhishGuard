using Microsoft.AspNetCore.Mvc;
using PhishGuard.Models;
using PhishGuard.Services;
//backend API endpoint after reveive data from frontend, calls analyssis service and return results
namespace PhishGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailAnalysisController : ControllerBase
    {
        private readonly PhishingAnalysisService _phishingAnalysisService;

        public EmailAnalysisController(PhishingAnalysisService phishingAnalysisService)
        {
            _phishingAnalysisService = phishingAnalysisService;
        }

        [HttpPost("analyze")]
        public IActionResult Analyze([FromBody] EmailAnalysis email)
        {
            if (email == null)
            {
                return BadRequest(new
                {
                    message = "Email data is required."
                });
            }

            var result = _phishingAnalysisService.AnalyzeEmail(email);

            return Ok(result);
        }
    }
}