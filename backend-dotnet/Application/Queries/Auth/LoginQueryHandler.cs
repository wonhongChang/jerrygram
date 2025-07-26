using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Auth
{
    public class LoginQueryHandler : IQueryHandler<LoginQuery, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LoginQueryHandler> _logger;

        public LoginQueryHandler(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<LoginQueryHandler> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<string> HandleAsync(LoginQuery query)
        {
            var dto = query.Dto;
            
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found", dto.Email);
                throw new ArgumentException("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
                throw new ArgumentException("Invalid email or password.");
            }

            var token = _jwtService.GenerateToken(user);
            
            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return token;
        }
    }
}