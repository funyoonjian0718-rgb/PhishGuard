using System.Net.Http.Json;
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

        public async Task<bool> SendPhishingAlertAsync(PhishingAlertRequest alert)
        {
            _logger.LogWarning("PHISHGUARD DEBUG: Serverless alert service started.");

            string? apiUrl = _configuration["Serverless:PhishingAlertApiUrl"];

            _logger.LogWarning(
                "PHISHGUARD DEBUG: Phishing alert API URL configured: {Configured}",
                !string.IsNullOrWhiteSpace(apiUrl)
            );

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                _logger.LogWarning("PHISHGUARD DEBUG: Phishing alert API URL is not configured.");
                return false;
            }

            try
            {
                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Sending phishing alert to API Gateway. ScanId: {ScanId}, RiskLevel: {RiskLevel}, RiskScore: {RiskScore}",
                    alert.ScanId,
                    alert.RiskLevel,
                    alert.RiskScore
                );

                var response = await _httpClient.PostAsJsonAsync(apiUrl, alert);

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: API Gateway response status code: {StatusCode}",
                    response.StatusCode
                );

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();

                    _logger.LogWarning(
                        "PHISHGUARD DEBUG: Failed to send phishing alert. Status Code: {StatusCode}. Response: {ResponseBody}",
                        response.StatusCode,
                        errorBody
                    );

                    return false;
                }

                string responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "PHISHGUARD DEBUG: Phishing alert sent successfully to serverless API. Response: {ResponseBody}",
                    responseBody
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PHISHGUARD DEBUG: Error sending phishing alert to serverless API.");
                return false;
            }
        }
    }
}