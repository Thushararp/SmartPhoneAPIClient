using Entities.DTOs;
using LoggerService.Contract;
using Moq;
using Moq.Protected;
using Service;
using Service.Contract;
using System.Net;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace ServiceTests
{
    public class SmartPhoneServiceTests
    {
        private readonly SmartPhoneService _smartPhoneService;
        private readonly Mock<ILoggerManager> _loggerManagerMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;        

        public SmartPhoneServiceTests()
        {
            _loggerManagerMock = new Mock<ILoggerManager>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();            

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://dummyjson.com/")
            };

            _smartPhoneService = new SmartPhoneService(_loggerManagerMock.Object)
            {
                HTTPClient = httpClient
            };
        }

        #region Unit Tests for  GetMostExpensiveSmartPhonesAsync()
        [Fact]
        public async Task GetMostExpensiveSmartPhonesAsync_ValidInputs_ReturnsSmartPhones()
        {
            // Arrange
            var authorizationToken = "your-authorization-token";
            var limit = 3;
            var smartPhones = new List<SmartPhoneDto>
            {
                new SmartPhoneDto { Id = 1, Brand = "Apple", Title = "iPhone 12", Price = 999.99 },
                new SmartPhoneDto { Id = 2, Brand = "Samsung", Title = "Galaxy S21", Price = 899.99 },
                new SmartPhoneDto { Id = 3, Brand = "Google", Title = "Pixel 6", Price = 799.99 },
                new SmartPhoneDto { Id = 4, Brand = "Apple", Title = "iPhone 13 pro", Price = 1250.75 },
                new SmartPhoneDto { Id = 5, Brand = "Samsung", Title = "S21", Price = 1150.77 }
            };
            var smartPhoneListWithPagingMetaData = new SmartPhoneListWithPagingMetaDataDto
            {
                SmartPhones = smartPhones,
                Total = 5,

            };
            var expectedIds = smartPhones.OrderByDescending(sp => sp.Price).Take(limit).Select(sp => sp.Id).ToList();

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var serializedSmartPhoneListWithPagingMetaData = JsonSerializer.Serialize(smartPhoneListWithPagingMetaData);
            httpResponse.Content = new StringContent(serializedSmartPhoneListWithPagingMetaData);
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _smartPhoneService.GetMostExpensiveSmartPhonesAsync(authorizationToken, limit);
         
            // Assert
            Assert.NotNull(result);
            Assert.Equal(limit, result.Count());
            Assert.Equal(expectedIds, result.Select(sp => sp.Id).ToList());
        }

        [Fact]
        public async Task GetMostExpensiveSmartPhonesAsync_InvalidLimit_ReturnsEmptyList()
        {
            // Arrange
            string authorizationToken = "your-authorization-token";
            int limit = -5;

            // Act
            var result = await _smartPhoneService.GetMostExpensiveSmartPhonesAsync(authorizationToken, limit);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        #endregion

        #region Unit Tests for  GetAllSmartPhonesAsync()
        [Fact]
        public async Task GetAllSmartPhonesAsync_ValidAuthorizationToken_ReturnsSmartPhoneListWithPagingMetaData()
        {
            // Arrange
            var authorizationToken = "your-authorization-token";
            var smartPhoneListWithPagingMetaData = new SmartPhoneListWithPagingMetaDataDto
            {
                Total = 10,
                SmartPhones = new List<SmartPhoneDto>
                {
                    new SmartPhoneDto { Id = 1, Brand = "Apple", Title = "iPhone 12", Price = 999.99 },
                    new SmartPhoneDto { Id = 2, Brand = "Samsung", Title = "Galaxy S21", Price = 899.99 },
                    new SmartPhoneDto { Id = 3, Brand = "Google", Title = "Pixel 6", Price = 799.99 }
                }
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var serializedSmartPhoneListWithPagingMetaData = JsonSerializer.Serialize(smartPhoneListWithPagingMetaData);
            httpResponse.Content = new StringContent(serializedSmartPhoneListWithPagingMetaData);
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _smartPhoneService.GetAllSmartPhonesAsync(authorizationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(smartPhoneListWithPagingMetaData.Total, result.Total);
            Assert.Equal(smartPhoneListWithPagingMetaData.SmartPhones.Count(), result.SmartPhones.Count());
        }

        [Fact]
        public async Task GetAllSmartPhonesAsync_ValidAuthorizationToken_SendsHttpRequestWithAuthorizationHeader()
        {
            // Arrange
            var authorizationToken = "your-authorization-token";

            var expectedUrl = "https://dummyjson.com/category/smartphones";
            var expectedHeaders = new[] { "Authorization" };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var serializedSmartPhoneListWithPagingMetaData = JsonSerializer.Serialize(new SmartPhoneListWithPagingMetaDataDto());
            httpResponse.Content = new StringContent(serializedSmartPhoneListWithPagingMetaData);            
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    // Assert
                    Assert.Equal(expectedUrl, request.RequestUri.ToString());
                    foreach (var header in expectedHeaders)
                    {
                        Assert.True(request.Headers.Contains(header));
                        Assert.Equal($"Bearer {authorizationToken}", request.Headers.GetValues(header).First());
                    }
                })
                .ReturnsAsync(httpResponse);

            // Act
            await _smartPhoneService.GetAllSmartPhonesAsync(authorizationToken);

            // Assert
            _httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }


        [Fact]
        public async Task GetAllSmartPhonesAsync_ErrorResponse_ThrowsException()
        {
            // Arrange
            var authorizationToken = "your-authorization-token";

            var expectedErrorMessage = "An error occurred during the HTTP request.";
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(expectedErrorMessage)
            };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _smartPhoneService.GetAllSmartPhonesAsync(authorizationToken));
        }
        #endregion

        #region Unit Tests for  UpdateSmartPhonePricesAsync()
        [Fact]
        public async Task UpdateSmartPhonePricesAsync_ValidInputs_SendsPutRequestForAllSmartphones()
        {
            // Arrange
            var authorizationToken = "your-authorization-token";
            var smartphonesToUpdate = new List<SmartPhoneDto>
            {
                new SmartPhoneDto { Id = 1, Brand = "Apple", Title = "iPhone 12", Price = 999.99 },
                new SmartPhoneDto { Id = 2, Brand = "Samsung", Title = "Galaxy S21", Price = 899.99 },
                new SmartPhoneDto { Id = 3, Brand = "Google", Title = "Pixel 6", Price = 799.99 }
            };
            var percentageToIncreasePriceBy = 10;

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            await _smartPhoneService.UpdateSmartPhonePricesAsync(authorizationToken, smartphonesToUpdate, percentageToIncreasePriceBy);

            // Assert
            for (int i = 0; i < smartphonesToUpdate.Count; i++)
            {
                _httpMessageHandlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Exactly(1),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri == new Uri($"{_smartPhoneService.HTTPClient.BaseAddress}{smartphonesToUpdate[i].Id}")),
                    ItExpr.IsAny<CancellationToken>());
            }
        }
        #endregion
    }
}

