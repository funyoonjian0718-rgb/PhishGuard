using System.Net.Http.Json;
using System.Text.Json;
using PhishGuard.Models;

namespace PhishGuard.Services
{
    public class ServerlessAlertService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServerlessAlertService> _logger;

        public ServerlessAlertService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ServerlessAlertService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServerlessAlertResult> SendPhishingAlertAsync(PhishingAlertRequest alert)
        {
            _logger.LogWarning("PHISHGUARD DEBUG: Serverless alert service started.");

            string? apiUrl = _configuration["Serverless:PhishingAlertApiUrl"]?.Trim();

            _logger.LogWarning(
                "PHISHGUARD DEBUG: Phishing alert API URL configured: {Configured}",
                !string.IsNullOrWhiteSpace(apiUrl)
            );

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                return new ServerlessAlertResult
                {
                    Success = false,
                    Message = "Phishing alert API URL is not configured.",
                    S3ObjectKey = string.Empty
                };
            }

            if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out var apiUri))
            {
                return new ServerlessAlertResult
                {
                    Success = false,
                    Message = "Invalid API Gateway URL.",
                    S3ObjectKey = string.Empty
                };
            }

            try
            {
                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Sending phishing alert to API Gateway. ScanId: {ScanId}, RiskLevel: {RiskLevel}, RiskScore: {RiskScore}",
                    alert.ScanId,
                    alert.RiskLevel,
                    alert.RiskScore
                );

                var response = await _httpClient.PostAsJsonAsync(apiUri, alert);

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: API Gateway response status code: {StatusCode}",
                    response.StatusCode
                );

                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ServerlessAlertResult
                    {
                        Success = false,
                        Message = $"Failed to send phishing alert. Status code: {response.StatusCode}",
                        S3ObjectKey = string.Empty
                    };
                }

                string message = "Phishing alert sent successfully.";
                string s3ObjectKey = string.Empty;

                try
                {
                    using JsonDocument document = JsonDocument.Parse(responseBody);

                    if (document.RootElement.TryGetProperty("message", out JsonElement messageElement))
                    {
                        message = messageElement.GetString() ?? message;
                    }

                    if (document.RootElement.TryGetProperty("s3ObjectKey", out JsonElement s3Element))
                    {
                        s3ObjectKey = s3Element.GetString() ?? string.Empty;
                    }
                }
                catch
                {
                    _logger.LogWarning("PHISHGUARD DEBUG: Could not parse serverless response JSON.");
                }

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Phishing alert sent successfully. S3ObjectKey: {S3ObjectKey}",
                    s3ObjectKey
                );

                return new ServerlessAlertResult
                {
                    Success = true,
                    Message = message,
                    S3ObjectKey = s3ObjectKey
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PHISHGUARD DEBUG: Error sending phishing alert to serverless API.");

                return new ServerlessAlertResult
                {
                    Success = false,
                    Message = "Error sending phishing alert to serverless API.",
                    S3ObjectKey = string.Empty
                };
            }
        }
    }
}