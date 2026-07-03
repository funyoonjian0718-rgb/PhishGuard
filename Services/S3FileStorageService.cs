using Amazon.S3;
using Amazon.S3.Model;

namespace PhishGuard.Services
{
    public class S3FileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<S3FileStorageService> _logger;

        public S3FileStorageService(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<S3FileStorageService> logger)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            string? bucketName = _configuration["Storage:FileUploadBucketName"];

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                _logger.LogWarning("PHISHGUARD DEBUG: S3 upload bucket name is not configured.");
                return string.Empty;
            }

            string originalFileName = Path.GetFileName(file.FileName);
            string safeFileName = $"{Guid.NewGuid()}_{originalFileName}";

            string objectKey = $"uploaded-files/{folderName}/{safeFileName}";

            try
            {
                using var stream = file.OpenReadStream();

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectKey,
                    InputStream = stream,
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType
                };

                await _s3Client.PutObjectAsync(request);

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: File uploaded to S3. Bucket: {Bucket}, Key: {Key}",
                    bucketName,
                    objectKey
                );

                return objectKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "PHISHGUARD DEBUG: Failed to upload file to S3. FileName: {FileName}",
                    originalFileName
                );

                return string.Empty;
            }
        }
    }
}