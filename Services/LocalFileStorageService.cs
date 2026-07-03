namespace PhishGuard.Services
{
    public class LocalFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(
            IWebHostEnvironment environment,
            ILogger<LocalFileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string originalFileName = Path.GetFileName(file.FileName);
            string safeFileName = $"{Guid.NewGuid()}_{originalFileName}";
            string filePath = Path.Combine(uploadFolder, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogWarning(
                "PHISHGUARD DEBUG: File uploaded locally. FileName: {FileName}",
                safeFileName
            );

            return safeFileName;
        }
    }
}