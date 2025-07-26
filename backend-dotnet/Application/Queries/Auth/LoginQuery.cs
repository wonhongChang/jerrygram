using Application.DTOs;

namespace Application.Queries.Auth
{
    public class LoginQuery : IQuery<string>
    {
        public LoginDto Dto { get; set; }
        
        public LoginQuery(LoginDto dto)
        {
            Dto = dto;
        }
    }
}