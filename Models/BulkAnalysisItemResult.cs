namespace PhishGuard.Models
{
    public class BulkAnalysisItemResult
    {
        public int ScanId { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public int RiskScore { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<string> DetectedIssues { get; set; } = new();

        public List<string> Recommendations { get; set; } = new();

        public bool AlertSent { get; set; }

        public string AlertMessage { get; set; } = string.Empty;

        public string AlertS3ObjectKey { get; set; } = string.Empty;
    }
}