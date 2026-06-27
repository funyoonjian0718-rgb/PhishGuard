using System.ComponentModel.DataAnnotations;

namespace PhishGuard.Models
{
    public class EmailScanRecord
    {
        [Key]
        public int Id { get; set; }

        public string SenderEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string EmailBody { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;

        public string AttachmentName { get; set; } = string.Empty;

        public int RiskScore { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string DetectedIssuesJson { get; set; } = string.Empty;

        public string RecommendationsJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}