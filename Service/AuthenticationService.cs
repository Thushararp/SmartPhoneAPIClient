using Entities.DTOs;
using LoggerService.Contract;
using Service.Contract;
using System.Text;
using System.Text.Json;

namespace Service
{
    public class AuthenticationService : IAuthenticationService
    {
        public HttpClient HTTPClient = new HttpClient();
        private readonly JsonSerializerOptions _options;
        private readonly ILoggerManager _logger;

        public AuthenticationService(ILoggerManager loggerManager)
        {
            HTTPClient.BaseAddress = new Uri("https://dummyjson.com/auth/");
            HTTPClient.Timeout = new TimeSpan(0, 0, 30);
            HTTPClient.DefaultRequestHeaders.Clear();

            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _logger = loggerManager;
        }

        public async Task<string> LoginAsync(LoginCredentialsDTO loginCredentialsDTO)
        {
            try
            {
                _logger.LogDebug($"Starting execution of {nameof(LoginAsync)} service method...");

                var loginCredentialsJSONString = JsonSerializer.Serialize(loginCredentialsDTO, _options);
                var requestContent = new StringContent(loginCredentialsJSONString, Encoding.UTF8, "application/json");

                _logger.LogDebug($"Sending HTTP POST Request to: {HTTPClient.BaseAddress}login");
                var response = await HTTPClient.PostAsync("login", requestContent);
                _logger.LogDebug($"HTTP Response: {@response}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"HTTP Response content: {content}");

                var authenticatedUser = JsonSerializer.Deserialize<UserDTO>(content, _options);

                _logger.LogDebug($"{nameof(LoginAsync)} service method finished executing.");
                return authenticatedUser.Token;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error occurred in ${nameof(LoginAsync)} service method ${ex}");
                throw;
            }

        }
    }
}
