namespace PhishGuard.Models
{
    public class ScreenshotEmailAnalysis
    {
        public string? SenderEmail { get; set; }

        public string? Link { get; set; }

        public IFormFile? ScreenshotFile { get; set; }
    }
}