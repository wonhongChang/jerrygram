using Application.DTOs;
using Domain.Entities;

namespace Application.Commands.Auth
{
    public class RegisterUserCommand : ICommand<(string token, User user)>
    {
        public RegisterDto Dto { get; set; }
        
        public RegisterUserCommand(RegisterDto dto)
        {
            Dto = dto;
        }
    }
}