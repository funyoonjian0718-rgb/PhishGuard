namespace PhishGuard.Models
{
    public class ServerlessAlertResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string S3ObjectKey { get; set; } = string.Empty;
    }
}