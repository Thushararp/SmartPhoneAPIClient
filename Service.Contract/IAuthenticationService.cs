using Entities.DTOs;

namespace Service.Contract
{
    public interface IAuthenticationService
    {
        Task<string> LoginAsync(LoginCredentialsDTO loginCredentialsDTO);
    }
}
