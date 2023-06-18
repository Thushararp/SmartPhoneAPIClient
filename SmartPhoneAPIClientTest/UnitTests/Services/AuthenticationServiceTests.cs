using Entities.DTOs;
using LoggerService.Contract;
using Moq;
using Moq.Protected;
using Service;
using System.Net;

namespace ServiceTests
{
    public class AuthenticationServiceTests
    {
        private readonly AuthenticationService _authenticationService;
        private readonly Mock<ILoggerManager> _loggerManagerMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public AuthenticationServiceTests()
        {
            _loggerManagerMock = new Mock<ILoggerManager>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://dummyjson.com")
            };

            _authenticationService = new AuthenticationService(_loggerManagerMock.Object)
            {
                HTTPClient = httpClient
            };
            
        }

        [Fact]
        public async Task LoginAsync_SuccessfulAuthentication_ReturnsToken()
        {
            // Arrange            
            var loginCredentials = new LoginCredentialsDTO
            {
                Username = "testuser",
                Password = "password"
            };

            var responseContent = @"{
                ""id"": 1,
                ""username"": ""testuser"",
                ""email"": ""testuser@example.com"",
                ""firstName"": ""Terry"",
                ""lastName"": ""Medhurst"",
                ""gender"": ""male"",
                ""image"": ""test image url"",
                ""token"": ""abc123""
            }";

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _authenticationService.LoginAsync(loginCredentials);

            // Assert
            Assert.Equal("abc123", result);

            _loggerManagerMock.Verify(
                x => x.LogDebug(It.Is<string>(s => s.Contains("Sending HTTP POST Request to"))), Times.Once);
            _loggerManagerMock.Verify(
                x => x.LogDebug(It.Is<string>(s => s.Contains("HTTP Response:"))), Times.Once);
            _loggerManagerMock.Verify(
                x => x.LogDebug(It.Is<string>(s => s.Contains("HTTP Response content:"))), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_FailedAuthentication_ThrowsException()
        {
            // Arrange
            var loginCredentials = new LoginCredentialsDTO
            {
                Username = "testuser",
                Password = "password"
            };

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _authenticationService.LoginAsync(loginCredentials));
        }
    }
}
