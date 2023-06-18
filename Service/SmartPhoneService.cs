using Entities.DTOs;
using LoggerService.Contract;
using Service.Contract;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text.Json;

namespace Service
{
    public class SmartPhoneService : ISmartPhoneService
    {
        public HttpClient HTTPClient = new HttpClient();
        private readonly JsonSerializerOptions _options;
        private readonly ILoggerManager _logger;
        public SmartPhoneService(ILoggerManager loggerManager)
        {
            _logger = loggerManager;

            HTTPClient.BaseAddress = new Uri("https://dummyjson.com/auth/products/");
            HTTPClient.Timeout = new TimeSpan(0, 0, 30);
            HTTPClient.DefaultRequestHeaders.Clear();

            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IEnumerable<SmartPhoneDto>> GetMostExpensiveSmartPhonesAsync(string authrorizationToken, int limit)
        {
            try
            {
                _logger.LogDebug($"Starting execution of {nameof(GetMostExpensiveSmartPhonesAsync)} service method ...");

                IEnumerable<SmartPhoneDto> smartPhones = new List<SmartPhoneDto>();
                if (limit is <= 0 or > 100)
                {
                    _logger.LogError($"{nameof(GetMostExpensiveSmartPhonesAsync)}: Limit must be between 1 and 100");
                    return smartPhones;
                }

                SmartPhoneListWithPagingMetaDataDto smartPhoneListWithPagingMetaData = await GetAllSmartPhonesAsync(authrorizationToken);
                limit = (smartPhoneListWithPagingMetaData.Total < limit) ? smartPhoneListWithPagingMetaData.Total : limit;

                smartPhones = smartPhoneListWithPagingMetaData.SmartPhones.OrderByDescending(sp => sp.Price).Take(limit);

                _logger.LogDebug($"{nameof(GetMostExpensiveSmartPhonesAsync)} service method finished executing.");
                return smartPhones;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error occurred in the {nameof(GetMostExpensiveSmartPhonesAsync)} service method {ex}");
                throw;
            }
        }

        public async Task<SmartPhoneListWithPagingMetaDataDto> GetAllSmartPhonesAsync(string authrorizationToken)
        {
            try
            {
                _logger.LogDebug($"Starting execution of {nameof(GetAllSmartPhonesAsync)} service method ...");

                HTTPClient.DefaultRequestHeaders.Clear();
                HTTPClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authrorizationToken}");
                _logger.LogDebug($"Sending HTTP GET Request to: {HTTPClient.BaseAddress}category/smartphones, request headers: {HTTPClient.DefaultRequestHeaders}");

                var response = await HTTPClient.GetAsync("category/smartphones");
                _logger.LogDebug($"HTTP Response: {@response}");
                response.EnsureSuccessStatusCode();

                SmartPhoneListWithPagingMetaDataDto smartPhoneListWithPagingMetaData = new SmartPhoneListWithPagingMetaDataDto();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"HTTP Response content: {content}");
                smartPhoneListWithPagingMetaData = JsonSerializer.Deserialize<SmartPhoneListWithPagingMetaDataDto>(content, _options) ?? new SmartPhoneListWithPagingMetaDataDto();

                _logger.LogDebug($"{nameof(GetAllSmartPhonesAsync)} service method finished executing.");
                return smartPhoneListWithPagingMetaData;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error occurred in the {nameof(GetAllSmartPhonesAsync)} service method {ex}");
                throw;
            }
        }

        public async Task UpdateSmartPhonePricesAsync(string authorizationToken, IEnumerable<SmartPhoneDto> smartphonesToUpdate, double percentageToIncresePriceBy)
        {

            try
            {
                _logger.LogDebug($"Starting execution of {nameof(UpdateSmartPhonePricesAsync)} service method ...");                
                HTTPClient.DefaultRequestHeaders.Clear();
                HTTPClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorizationToken}");          

                foreach (var smartphone in smartphonesToUpdate)
                {
                    var updatedPrice = smartphone.Price + (smartphone.Price * percentageToIncresePriceBy / 100);
                    smartphone.Price = updatedPrice;

                    var smartPhonePriceJSONString = JsonSerializer.Serialize(new SmartPhoneForPriceUpdateDto() { Price = smartphone.Price});
                    var content = new StringContent(smartPhonePriceJSONString, System.Text.Encoding.UTF8, "application/json");

                    _logger.LogDebug($"Sending HTTP PUT Request to: {HTTPClient.BaseAddress}{smartphone.Id}, request headers: {HTTPClient.DefaultRequestHeaders}, request payload:{smartPhonePriceJSONString}");

                    var response = await HTTPClient.PutAsync($"{smartphone.Id}", content);
                    _logger.LogDebug($"HTTP Response: {@response}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                       _logger.LogError($"Failed to update price for smartphone with ID {smartphone.Id} with status code {response.StatusCode}");
                    }
                }

                _logger.LogDebug($"{nameof(UpdateSmartPhonePricesAsync)} service method finished executing.");                
            }
            catch (Exception ex) {
                _logger.LogDebug($"Error occurred in the {nameof(UpdateSmartPhonePricesAsync)} service method {ex}");
                throw;
            }
        }
    }
}

