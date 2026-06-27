using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Data;
using PhishGuard.Models;

namespace PhishGuard.Services
{
    public class EmailHistoryService
    {
        private readonly PhishGuardDbContext _dbContext;

        public EmailHistoryService(PhishGuardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<EmailScanRecord> SaveScanAsync(EmailAnalysis email, AnalysisResult result)
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
                CreatedAt = DateTime.Now
            };

            _dbContext.EmailScanRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            return record;
        }

        public async Task<List<object>> GetRecentScansAsync()
        {
            var records = await _dbContext.EmailScanRecords
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .ToListAsync();

            return records.Select(x => new
            {
                x.Id,
                x.SenderEmail,
                x.Subject,
                x.Link,
                x.AttachmentName,
                x.RiskScore,
                x.RiskLevel,
                x.Summary,
                x.CreatedAt
            }).Cast<object>().ToList();
        }

        public async Task<object?> GetScanDetailsAsync(int id)
        {
            var record = await _dbContext.EmailScanRecords.FindAsync(id);

            if (record == null)
            {
                return null;
            }

            var detectedIssues = JsonSerializer.Deserialize<List<string>>(record.DetectedIssuesJson)
                ?? new List<string>();

            var recommendations = JsonSerializer.Deserialize<List<string>>(record.RecommendationsJson)
                ?? new List<string>();

            return new
            {
                record.Id,
                record.SenderEmail,
                record.Subject,
                record.EmailBody,
                record.Link,
                record.AttachmentName,
                record.RiskScore,
                record.RiskLevel,
                record.Summary,
                DetectedIssues = detectedIssues,
                Recommendations = recommendations,
                record.CreatedAt
            };
        }

        public async Task<object> GetDashboardStatsAsync()
        {
            int total = await _dbContext.EmailScanRecords.CountAsync();
            int safe = await _dbContext.EmailScanRecords.CountAsync(x => x.RiskLevel == "Safe");
            int suspicious = await _dbContext.EmailScanRecords.CountAsync(x => x.RiskLevel == "Suspicious");
            int phishing = await _dbContext.EmailScanRecords.CountAsync(x => x.RiskLevel == "Phishing");

            return new
            {
                Total = total,
                Safe = safe,
                Suspicious = suspicious,
                Phishing = phishing
            };
        }
    }
}