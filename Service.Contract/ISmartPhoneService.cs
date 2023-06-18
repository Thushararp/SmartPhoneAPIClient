using Entities.DTOs;

namespace Service.Contract
{
    public interface ISmartPhoneService
    {
        Task<IEnumerable<SmartPhoneDto>> GetMostExpensiveSmartPhonesAsync(string authorizationToken, int limit);

        Task<SmartPhoneListWithPagingMetaDataDto> GetAllSmartPhonesAsync(string authorizationToken);

        Task UpdateSmartPhonePricesAsync(string authorizationToken, IEnumerable<SmartPhoneDto> smartphonesToUpdate, double percentageToIncresePriceBy);
    }
}
