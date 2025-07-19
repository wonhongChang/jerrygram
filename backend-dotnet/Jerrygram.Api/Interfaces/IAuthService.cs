using Jerrygram.Api.Dtos;
using Jerrygram.Api.Models;
using Nest;

namespace Jerrygram.Api.Interfaces
{
    public interface IAuthService
    {
        Task<(string token, User user)> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
    }
}
