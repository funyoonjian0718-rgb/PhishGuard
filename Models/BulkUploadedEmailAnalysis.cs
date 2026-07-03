namespace PhishGuard.Models
{
    public class BulkUploadedEmailAnalysis
    {
        public string? SenderEmail { get; set; }

        public string? Link { get; set; }

        public List<IFormFile> Files { get; set; } = new();
    }
}