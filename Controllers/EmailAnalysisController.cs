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
        private readonly ScreenshotOcrService _screenshotOcrService;
        private readonly S3FileStorageService _s3FileStorageService;
        private readonly ILogger<EmailAnalysisController> _logger;

        public EmailAnalysisController(
            PhishingAnalysisService phishingAnalysisService,
            EmailHistoryService emailHistoryService,
            ServerlessAlertService serverlessAlertService,
            LocalFileStorageService localFileStorageService,
            ScreenshotOcrService screenshotOcrService,
            S3FileStorageService s3FileStorageService,
            ILogger<EmailAnalysisController> logger)
        {
            _phishingAnalysisService = phishingAnalysisService;
            _emailHistoryService = emailHistoryService;
            _serverlessAlertService = serverlessAlertService;
            _localFileStorageService = localFileStorageService;
            _screenshotOcrService = screenshotOcrService;
            _s3FileStorageService = s3FileStorageService;
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

            var savedRecord = await _emailHistoryService.SaveScanAsync(email, result, "Manual");

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
                uploadedFileName = await _s3FileStorageService.UploadFileAsync(
                    uploadedEmail.AttachmentFile,
                    "manual-attachments"
                );
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

            var savedRecord = await _emailHistoryService.SaveScanAsync(email, result, "Manual Upload");

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

                ServerlessAlertResult alertResult = await _serverlessAlertService.SendPhishingAlertAsync(alert);

                result.AlertSent = alertResult.Success;
                result.AlertMessage = alertResult.Message;
                result.AlertS3ObjectKey = alertResult.S3ObjectKey;

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Serverless alert sent result: {AlertSent}, S3ObjectKey: {S3ObjectKey}",
                    alertResult.Success,
                    alertResult.S3ObjectKey
                );
                await _emailHistoryService.UpdateAlertInfoAsync(scanId, result);
            }
            else
            {
                result.AlertSent = false;
                result.AlertMessage = "No serverless alert was triggered because the email was not classified as phishing.";
                result.AlertS3ObjectKey = string.Empty;

                await _emailHistoryService.UpdateAlertInfoAsync(scanId, result);

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Not phishing, serverless alert not triggered. RiskLevel was: {RiskLevel}",
                    result.RiskLevel
                );
            }
        }

        private async Task<string> ExtractTextFromUploadedFile(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".txt" && extension != ".eml")
            {
                return string.Empty;
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                string text = await reader.ReadToEndAsync();

                if (text.Length > 5000)
                {
                    text = text.Substring(0, 5000);
                }

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "PHISHGUARD DEBUG: Failed to extract text from uploaded file: {FileName}",
                    file.FileName
                );

                return string.Empty;
            }
        }

        [HttpPost("bulk-analyze")]
        public async Task<IActionResult> BulkAnalyze([FromForm] BulkUploadedEmailAnalysis bulkUpload)
        {
            if (bulkUpload == null || bulkUpload.Files == null || bulkUpload.Files.Count == 0)
            {
                return BadRequest(new
                {
                    message = "At least one file is required for bulk analysis."
                });
            }

            var bulkResults = new List<BulkAnalysisItemResult>();

            foreach (var file in bulkUpload.Files)
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                string originalFileName = Path.GetFileName(file.FileName);
                string storedFileName = await _s3FileStorageService.UploadFileAsync(
                    file,
                    "bulk-uploads"
                );
                string extractedText = await ExtractTextFromUploadedFile(file);

                var email = new EmailAnalysis
                {
                    SenderEmail = string.IsNullOrWhiteSpace(bulkUpload.SenderEmail)
                       ? "bulk-upload@phishguard.local"
                       : bulkUpload.SenderEmail,

                    Subject = $"Bulk scan file: {originalFileName}",

                    EmailBody = string.IsNullOrWhiteSpace(extractedText)
                       ? $"Uploaded file for phishing analysis: {originalFileName}"
                       : extractedText,

                    Link = bulkUpload.Link ?? string.Empty,

                    AttachmentName = storedFileName
                };

                var result = _phishingAnalysisService.AnalyzeEmail(email);

                var savedRecord = await _emailHistoryService.SaveScanAsync(email, result, "Bulk");

                result.ScanId = savedRecord.Id;

                await SendAlertIfPhishing(email, result, savedRecord.Id);

                bulkResults.Add(new BulkAnalysisItemResult
                {
                    ScanId = savedRecord.Id,
                    OriginalFileName = originalFileName,
                    StoredFileName = storedFileName,
                    RiskScore = result.RiskScore,
                    RiskLevel = result.RiskLevel,
                    Summary = result.Summary,
                    DetectedIssues = result.DetectedIssues,
                    Recommendations = result.Recommendations,
                    AlertSent = result.AlertSent,
                    AlertMessage = result.AlertMessage,
                    AlertS3ObjectKey = result.AlertS3ObjectKey
                });

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Bulk file analyzed. FileName: {FileName}, ScanId: {ScanId}, RiskLevel: {RiskLevel}, RiskScore: {RiskScore}, AlertSent: {AlertSent}",
                    originalFileName,
                    savedRecord.Id,
                    result.RiskLevel,
                    result.RiskScore,
                    result.AlertSent
                );
            }

            return Ok(bulkResults);
        }

        [HttpPost("analyze-screenshot")]
        public async Task<IActionResult> AnalyzeScreenshot([FromForm] ScreenshotEmailAnalysis screenshotUpload)
        {
            if (screenshotUpload == null || screenshotUpload.ScreenshotFile == null)
            {
                return BadRequest(new
                {
                    message = "Screenshot file is required."
                });
            }

            string originalFileName = Path.GetFileName(screenshotUpload.ScreenshotFile.FileName);

            string storedFileName = await _s3FileStorageService.UploadFileAsync(
                screenshotUpload.ScreenshotFile,
                "screenshots"
            );

            string extractedText = await _screenshotOcrService.ExtractTextFromScreenshotAsync(
                screenshotUpload.ScreenshotFile
            );

            string emailBodyForAnalysis = string.IsNullOrWhiteSpace(extractedText)
                ? $"Screenshot uploaded for phishing analysis: {originalFileName}. No readable OCR text was extracted."
                : extractedText;

            var email = new EmailAnalysis
            {
                SenderEmail = string.IsNullOrWhiteSpace(screenshotUpload.SenderEmail)
                    ? "screenshot-upload@phishguard.local"
                    : screenshotUpload.SenderEmail,

                Subject = $"Screenshot scan: {originalFileName}",

                EmailBody = emailBodyForAnalysis,

                Link = screenshotUpload.Link ?? string.Empty,

                AttachmentName = storedFileName
            };

            var result = _phishingAnalysisService.AnalyzeEmail(email);

            if (!string.IsNullOrWhiteSpace(extractedText))
            {
                result.DetectedIssues.Add("Screenshot text was extracted and included in the phishing analysis.");
            }
            else
            {
                result.DetectedIssues.Add("Screenshot was uploaded, but no readable text was extracted.");
            }

            var savedRecord = await _emailHistoryService.SaveScanAsync(email, result, "Screenshot");

            result.ScanId = savedRecord.Id;

            _logger.LogWarning(
                "PHISHGUARD DEBUG: Screenshot analyzed. OriginalFileName: {OriginalFileName}, StoredFileName: {StoredFileName}, ScanId: {ScanId}, RiskLevel: {RiskLevel}, RiskScore: {RiskScore}",
                originalFileName,
                storedFileName,
                savedRecord.Id,
                result.RiskLevel,
                result.RiskScore
            );

            await SendAlertIfPhishing(email, result, savedRecord.Id);

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