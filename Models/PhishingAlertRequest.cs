namespace PhishGuard.Models
{
    public class PhishingAlertRequest
    {
        public int ScanId { get; set; }

        public string SenderEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;

        public string AttachmentName { get; set; } = string.Empty;

        public int RiskScore { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<string> DetectedIssues { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}