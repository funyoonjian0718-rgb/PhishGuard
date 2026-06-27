using System.Collections.Generic;

namespace PhishGuard.Models
{
    public class AnalysisResult
    {
        public int ScanId { get; set; }

        public int RiskScore { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<string> DetectedIssues { get; set; } = new List<string>();

        public List<string> Recommendations { get; set; } = new List<string>();

        public DateTime AnalyzedAt { get; set; } = DateTime.Now;
    }
}