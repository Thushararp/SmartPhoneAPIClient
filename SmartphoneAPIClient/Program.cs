using Entities.DTOs;
using LoggerService;
using LoggerService.Contract;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Service;
using Service.Contract;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace SmartPhoneAPIClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

            //DI Setup
            var services = new ServiceCollection();
            ConfigureServices(services);
            var provider = services.BuildServiceProvider();

            ILoggerManager logger = provider.GetRequiredService<ILoggerManager>();

            try
            {
                logger.LogInfo("Starting Smart Phone API Clinet App...");

                string userAuthenticationToken = await HandleLogin(provider, logger);
                if (string.IsNullOrWhiteSpace(userAuthenticationToken))
                {
                    logger.LogInfo("Login unsuccessful. Exiting the Application");
                    return;
                }
                logger.LogInfo("Login successful!");

                logger.LogInfo("Retrieving most expensive smartphones");
                ISmartPhoneService smartPhoneService = provider.GetRequiredService<ISmartPhoneService>();
                int limitResultsTo = 3;
                IEnumerable<SmartPhoneDto> smartPhones = await smartPhoneService.GetMostExpensiveSmartPhonesAsync(userAuthenticationToken, limitResultsTo);
                DisplaySmartPhones(smartPhones, "Most Expensive Smartphones");

                double percentageToIncreasePriceBy = ReadPercentagePriceIncreaseFromConsole(logger);

                logger.LogInfo("Updating smartphone prices");
                await smartPhoneService.UpdateSmartPhonePricesAsync(userAuthenticationToken, smartPhones, percentageToIncreasePriceBy);
                DisplaySmartPhones(smartPhones, "Most Expensive Smartphones");

                logger.LogInfo("Smart Phone API Clinet App Finished.");
            }
            catch (Exception ex)
            {
                logger.LogError($"An Error occurred: {ex.Message}");
                logger.LogDebug($"An Error occurred: {ex.Message}, stacktrace: {ex.StackTrace}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();            
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<ISmartPhoneService, SmartPhoneService>();
        }

        private static async Task<string> HandleLogin(ServiceProvider provider, ILoggerManager logger)
        {
            string token = string.Empty;

            Console.WriteLine("Please enter your Username:");
            string userName = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Please enter your Password:");
            string password = Console.ReadLine() ?? string.Empty;

            //password not logged
           logger.LogDebug($"Username: {userName}");

            LoginCredentialsDTO loginCredentialsDto = new LoginCredentialsDTO
            {
                Username = userName,
                Password = password
            };

            if (!isLoginCredentialsValid(loginCredentialsDto, logger))
            {
                return token;
            }

            try
            {
                var authenticationService = provider.GetRequiredService<IAuthenticationService>();
                token = await authenticationService.LoginAsync(loginCredentialsDto);
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Login failed - Message: {ex.Message}, Stack trace: {ex.StackTrace}");
                return token;
            }

            return token;

            #region local functions
            static bool isLoginCredentialsValid(LoginCredentialsDTO loginCredentialsDto, ILoggerManager logger)
            {
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(loginCredentialsDto, new ValidationContext(loginCredentialsDto), validationResults, true);
                if (!isValid)
                {
                    foreach (var validationResult in validationResults)
                    {
                        logger.LogInfo(validationResult.ErrorMessage);
                    }
                }

                return isValid;
            }
            #endregion
        }

        private static void DisplaySmartPhones(IEnumerable<SmartPhoneDto> smartPhones, string listHeading = "Smartphones List")
        {
            if (!smartPhones.Any())
            {
                Console.WriteLine("No smpartphones available to list");
            }
            else
            {
                Console.WriteLine(listHeading);
                foreach (SmartPhoneDto smartPhone in smartPhones)
                {
                    Console.WriteLine("Brand: {0}", smartPhone.Brand);
                    Console.WriteLine("Title: {0}", smartPhone.Title);
                    Console.WriteLine("Price: {0}", smartPhone.Price);
                    Console.WriteLine("------------------------------");
                }
            }
        }

        private static double ReadPercentagePriceIncreaseFromConsole(ILoggerManager logger)
        {
            double percentageToIncreasePriceBy = 0;
            string userInput = string.Empty;

            while (true)
            {
                Console.WriteLine("Please Enter a percentage value to increase the price by: ");
                userInput = Console.ReadLine() ?? String.Empty;
                logger.LogDebug($"Percentage value entered by the user: {userInput}");

                if (Double.TryParse(userInput, out percentageToIncreasePriceBy) && percentageToIncreasePriceBy is >= 1 and <= 100)
                {
                    return percentageToIncreasePriceBy;
                }
                else
                {
                    logger.LogInfo("Invalid percentage value, percentage value must be between 1 and 100");
                }

            }
        }
    }
}