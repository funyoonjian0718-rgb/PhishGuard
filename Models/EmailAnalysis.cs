namespace PhishGuard.Models  //checks the email data submitted by the user
{
    public class EmailAnalysis
    {
        public string SenderEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string AttachmentName { get; set; } = string.Empty;
    }
}