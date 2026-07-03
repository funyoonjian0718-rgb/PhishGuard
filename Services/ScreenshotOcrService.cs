using Amazon.Textract;
using Amazon.Textract.Model;

namespace PhishGuard.Services
{
    public class ScreenshotOcrService
    {
        private readonly IAmazonTextract _textractClient;
        private readonly ILogger<ScreenshotOcrService> _logger;

        public ScreenshotOcrService(
            IAmazonTextract textractClient,
            ILogger<ScreenshotOcrService> logger)
        {
            _textractClient = textractClient;
            _logger = logger;
        }

        public async Task<string> ExtractTextFromScreenshotAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            string extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".png" &&
                extension != ".jpg" &&
                extension != ".jpeg" &&
                extension != ".pdf" &&
                extension != ".tif" &&
                extension != ".tiff")
            {
                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Unsupported screenshot OCR file type: {Extension}",
                    extension
                );

                return string.Empty;
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var request = new DetectDocumentTextRequest
                {
                    Document = new Document
                    {
                        Bytes = memoryStream
                    }
                };

                var response = await _textractClient.DetectDocumentTextAsync(request);

                var lines = response.Blocks
                    .Where(block => block.BlockType == BlockType.LINE && !string.IsNullOrWhiteSpace(block.Text))
                    .Select(block => block.Text)
                    .ToList();

                string extractedText = string.Join("\n", lines);

                if (extractedText.Length > 5000)
                {
                    extractedText = extractedText.Substring(0, 5000);
                }

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Screenshot OCR completed. Extracted characters: {Length}",
                    extractedText.Length
                );

                return extractedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "PHISHGUARD DEBUG: Screenshot OCR failed. The system will continue without extracted text."
                );

                return string.Empty;
            }
        }
    }
}