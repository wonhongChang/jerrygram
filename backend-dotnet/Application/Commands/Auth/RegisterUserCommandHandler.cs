using Application.Interfaces;
using BCrypt.Net;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Auth
{
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, (string token, User user)>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IJwtService jwtService,
            ICacheService cacheService,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<(string token, User user)> HandleAsync(RegisterUserCommand command)
        {
            var dto = command.Dto;
            
            _logger.LogInformation("Registering user with email: {Email}", dto.Email);

            if (await _userRepository.ExistsByEmailAsync(dto.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", dto.Email);
                throw new ArgumentException("This email is already in use.");
            }

            if (await _userRepository.ExistsByUsernameAsync(dto.Username))
            {
                _logger.LogWarning("Registration failed: Username {Username} already exists", dto.Username);
                throw new ArgumentException("This username is already taken.");
            }

            var user = new User
            {
                Email = dto.Email,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _userRepository.Add(user);
            await _userRepository.SaveChangesAsync();

            await InvalidateUserAutocompleteCaches(user.Username);

            var token = _jwtService.GenerateToken(user);
            
            _logger.LogInformation("User {UserId} registered successfully with username: {Username}", 
                user.Id, user.Username);

            return (token, user);
        }

        private Task InvalidateUserAutocompleteCaches(string username)
        {
            try
            {
                var normalizedUsername = username.ToLower();

                if (normalizedUsername.Length >= 2)
                {
                    var firstTwoChars = normalizedUsername.Substring(0, 2);
                    _cacheService.RemoveByPattern($"autocomplete:{firstTwoChars}*");
                }

                if (normalizedUsername.Length >= 1)
                {
                    var firstChar = normalizedUsername.Substring(0, 1);
                    _cacheService.RemoveByPattern($"autocomplete:{firstChar}*");
                }

                _logger.LogDebug("Invalidated autocomplete caches for new user: {Username}", username);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate autocomplete caches for user: {Username}", username);
            }

            return Task.CompletedTask;
        }
    }
}