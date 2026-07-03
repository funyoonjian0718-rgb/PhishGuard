using Microsoft.EntityFrameworkCore;
using PhishGuard.Data;
using PhishGuard.Models;
using System.Text.Json;

namespace PhishGuard.Services
{
    public class EmailHistoryService
    {
        private readonly PhishGuardDbContext _context;

        public EmailHistoryService(PhishGuardDbContext context)
        {
            _context = context;
        }

        public async Task<EmailScanRecord> SaveScanAsync(
            EmailAnalysis email,
            AnalysisResult result,
            string scanType = "Manual")
        {
            var record = new EmailScanRecord
            {
                SenderEmail = email.SenderEmail,
                Subject = email.Subject,
                EmailBody = email.EmailBody,
                Link = email.Link,
                AttachmentName = email.AttachmentName,

                RiskScore = result.RiskScore,
                RiskLevel = result.RiskLevel,
                Summary = result.Summary,
                DetectedIssuesJson = JsonSerializer.Serialize(result.DetectedIssues),
                RecommendationsJson = JsonSerializer.Serialize(result.Recommendations),

                ScanType = scanType,
                UploadedFileS3Key = email.AttachmentName,

                AlertSent = result.AlertSent,
                AlertMessage = result.AlertMessage,
                AlertS3ObjectKey = result.AlertS3ObjectKey,

                CreatedAt = DateTime.Now
            };

            _context.EmailScanRecords.Add(record);
            await _context.SaveChangesAsync();

            return record;
        }

        public async Task UpdateAlertInfoAsync(int scanId, AnalysisResult result)
        {
            var record = await _context.EmailScanRecords.FindAsync(scanId);

            if (record == null)
            {
                return;
            }

            record.AlertSent = result.AlertSent;
            record.AlertMessage = result.AlertMessage;
            record.AlertS3ObjectKey = result.AlertS3ObjectKey;

            await _context.SaveChangesAsync();
        }

        public async Task<List<EmailScanRecord>> GetRecentScansAsync()
        {
            return await _context.EmailScanRecords
                .OrderByDescending(record => record.CreatedAt)
                .Take(30)
                .ToListAsync();
        }

        public async Task<EmailScanRecord?> GetScanDetailsAsync(int id)
        {
            return await _context.EmailScanRecords
                .FirstOrDefaultAsync(record => record.Id == id);
        }

        public async Task<object> GetDashboardStatsAsync()
        {
            int totalScans = await _context.EmailScanRecords.CountAsync();

            int safeScans = await _context.EmailScanRecords
                .CountAsync(record => record.RiskLevel == "Safe");

            int suspiciousScans = await _context.EmailScanRecords
                .CountAsync(record => record.RiskLevel == "Suspicious");

            int phishingScans = await _context.EmailScanRecords
                .CountAsync(record => record.RiskLevel == "Phishing");

            int manualScans = await _context.EmailScanRecords
                .CountAsync(record => record.ScanType == "Manual");

            int manualUploadScans = await _context.EmailScanRecords
                .CountAsync(record => record.ScanType == "Manual Upload");

            int screenshotScans = await _context.EmailScanRecords
                .CountAsync(record => record.ScanType == "Screenshot");

            int bulkScans = await _context.EmailScanRecords
                .CountAsync(record => record.ScanType == "Bulk");

            int alertSentCount = await _context.EmailScanRecords
                .CountAsync(record => record.AlertSent == true);

            int uploadedFileCount = await _context.EmailScanRecords
                .CountAsync(record => record.UploadedFileS3Key != "");

            return new
            {
                totalScans,
                safeScans,
                suspiciousScans,
                phishingScans,
                manualScans,
                manualUploadScans,
                screenshotScans,
                bulkScans,
                alertSentCount,
                uploadedFileCount
            };
        }
    }
}