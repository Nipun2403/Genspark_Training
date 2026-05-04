using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OmniBus.Server.Services
{
    public interface ISmsService
    {
        Task SendOtpSmsAsync(string phone, string code);
    }

    public class SmsService : ISmsService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<SmsService> _logger;

        public SmsService(HttpClient http, IConfiguration config, ILogger<SmsService> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        public async Task SendOtpSmsAsync(string phone, string code)
        {
            var baseUrl = _config["SmsGateway:BaseUrl"];
            var token = _config["SmsGateway:Token"];

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("SMS Gateway configuration is missing. Cannot send SMS.");
                return;
            }

            var endpoint = $"{baseUrl.TrimEnd('/')}/api/3rdparty/v1/message";

            var payload = new
            {
                phoneNumbers = new[] { phone },
                textMessage = new { text = $"Your OmniBus login OTP is {code}. It is valid for 10 minutes." }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token); // or token directly depending on the gateway requirements, standardizing on Basic for now based on some docs or just Bearer
            request.Headers.Add("Authorization", token); // Sometimes android-sms-gateway expects just the token value if it's an API Key. We will supply both or let the user configure.
            request.Content = content;

            // Proper way based on most generic implementations:
            // The Android SMS gateway usually accepts standard HTTP auth or a custom header. 
            // We will set the Authorization header to just the token as is common for simple gateways unless basic auth is specified.
            
            try
            {
                // We'll replace the request setup to strictly use the Authorization header with the token.
                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("Authorization", token);

                var response = await _http.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent OTP SMS to {Phone}", phone);
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send OTP SMS to {Phone}. Status Code: {StatusCode}. Response: {Response}", phone, response.StatusCode, responseBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while attempting to send OTP SMS to {Phone}", phone);
            }
        }
    }
}
