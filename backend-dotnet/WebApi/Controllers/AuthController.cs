using Application.Commands;
using Application.Commands.Auth;
using Application.DTOs;
using Application.Events;
using Application.Interfaces;
using Application.Queries;
using Application.Queries.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ICommandHandler<RegisterUserCommand, (string token, User user)> _registerHandler;
        private readonly IQueryHandler<LoginQuery, string> _loginHandler;
        private readonly IElasticService _elasticService;

        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ICommandHandler<RegisterUserCommand, (string token, User user)> registerHandler,
            IQueryHandler<LoginQuery, string> loginHandler,
            IElasticService elasticService,
            IEventPublisher eventPublisher,
            ILogger<AuthController> logger)
        {
            _registerHandler = registerHandler;
            _loginHandler = loginHandler;
            _elasticService = elasticService;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var command = new RegisterUserCommand(dto);
                var (token, user) = await _registerHandler.HandleAsync(command);

                // Index user in Elasticsearch
                await _elasticService.IndexUserAsync(new Application.Common.UserIndex
                {
                    Id = user.Id,
                    Username = user.Username,
                    ProfileImageUrl = user.ProfileImageUrl
                });

                var userEvent = new UserEvent
                {
                    EventType = "register",
                    TargetUserId = user.Id.ToString(),
                    Metadata = new Dictionary<string, object>
                {
                    { "username", user.Username },
                    { "registrationMethod", "email" }
                }
                };

                HttpContext.EnrichEvent(userEvent);
                await _eventPublisher.QueueUserEventAsync(userEvent);

                return Ok(new { token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var query = new LoginQuery(dto);
                var token = await _loginHandler.HandleAsync(query);

                var loginEvent = new UserEvent
                {
                    EventType = "login",
                    Metadata = new Dictionary<string, object>
                {
                    { "loginMethod", "email" }
                }
                };

                HttpContext.EnrichEvent(loginEvent);
                await _eventPublisher.QueueUserEventAsync(loginEvent);

                return Ok(new { token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }
    }
}
