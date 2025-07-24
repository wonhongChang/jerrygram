using Application.DTOs;
using Application.Interfaces;
using BCrypt.Net;
using Domain.Entities;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;

        public AuthService(IUserRepository userRepository, JwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<(string token, User user)> RegisterAsync(RegisterDto dto)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
                throw new ArgumentException("This email is already in use.");

            if (await _userRepository.ExistsByUsernameAsync(dto.Username))
                throw new ArgumentException("This username is already taken.");

            var user = new User
            {
                Email = dto.Email,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _userRepository.Add(user);
            await _userRepository.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            return (token, user);
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new ArgumentException("Invalid email or password.");

            return _jwtService.GenerateToken(user);
        }
    }
}