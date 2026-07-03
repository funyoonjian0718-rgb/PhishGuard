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
        private readonly ServerlessAlertService _serverlessAlertService;
        private readonly LocalFileStorageService _localFileStorageService;
        private readonly ILogger<EmailAnalysisController> _logger;

        public EmailAnalysisController(
            PhishingAnalysisService phishingAnalysisService,
            EmailHistoryService emailHistoryService,
            ServerlessAlertService serverlessAlertService,
            LocalFileStorageService localFileStorageService,
            ILogger<EmailAnalysisController> logger)
        {
            _phishingAnalysisService = phishingAnalysisService;
            _emailHistoryService = emailHistoryService;
            _serverlessAlertService = serverlessAlertService;
            _localFileStorageService = localFileStorageService;
            _logger = logger;
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

            _logger.LogWarning(
                "PHISHGUARD DEBUG: Email analyzed. ScanId: {ScanId}, RiskLevel: {RiskLevel}, RiskScore: {RiskScore}",
                savedRecord.Id,
                result.RiskLevel,
                result.RiskScore
            );

            await SendAlertIfPhishing(email, result, savedRecord.Id);

            return Ok(result);
        }

        [HttpPost("analyze-upload")]
        public async Task<IActionResult> AnalyzeUpload([FromForm] UploadedEmailAnalysis uploadedEmail)
        {
            if (uploadedEmail == null)
            {
                return BadRequest(new
                {
                    message = "Email data is required."
                });
            }

            string uploadedFileName = string.Empty;

            if (uploadedEmail.AttachmentFile != null)
            {
                uploadedFileName = await _localFileStorageService.SaveFileAsync(uploadedEmail.AttachmentFile);
            }

            var email = new EmailAnalysis
            {
                SenderEmail = uploadedEmail.SenderEmail,
                Subject = uploadedEmail.Subject,
                EmailBody = uploadedEmail.EmailBody,
                Link = uploadedEmail.Link,
                AttachmentName = uploadedFileName
            };

            var result = _phishingAnalysisService.AnalyzeEmail(email);

            var savedRecord = await _emailHistoryService.SaveScanAsync(email, result);

            result.ScanId = savedRecord.Id;

            _logger.LogWarning(
                "PHISHGUARD DEBUG: Uploaded email analyzed. ScanId: {ScanId}, FileName: {FileName}, RiskLevel: {RiskLevel}, RiskScore: {RiskScore}",
                savedRecord.Id,
                uploadedFileName,
                result.RiskLevel,
                result.RiskScore
            );

            await SendAlertIfPhishing(email, result, savedRecord.Id);

            return Ok(result);
        }

        private async Task SendAlertIfPhishing(EmailAnalysis email, AnalysisResult result, int scanId)
        {
            if (string.Equals(result.RiskLevel, "Phishing", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("PHISHGUARD DEBUG: Phishing detected. Calling API Gateway now.");

                var alert = new PhishingAlertRequest
                {
                    ScanId = scanId,
                    SenderEmail = email.SenderEmail,
                    Subject = email.Subject,
                    Link = email.Link,
                    AttachmentName = email.AttachmentName,
                    RiskScore = result.RiskScore,
                    RiskLevel = result.RiskLevel,
                    Summary = result.Summary,
                    DetectedIssues = result.DetectedIssues,
                    CreatedAt = DateTime.Now
                };

                bool alertSent = await _serverlessAlertService.SendPhishingAlertAsync(alert);

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Serverless alert sent result: {AlertSent}",
                    alertSent
                );
            }
            else
            {
                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Not phishing, serverless alert not triggered. RiskLevel was: {RiskLevel}",
                    result.RiskLevel
                );
            }
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