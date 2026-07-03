namespace PhishGuard.Models
{
    public class UploadedEmailAnalysis
    {
        public string SenderEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;

        public IFormFile? AttachmentFile { get; set; }
    }
}
