using Application.Commands;
using Application.Commands.Auth;
using Application.DTOs;
using Application.Interfaces;
using Application.Queries;
using Application.Queries.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ICommandHandler<RegisterUserCommand, (string token, User user)> _registerHandler;
        private readonly IQueryHandler<LoginQuery, string> _loginHandler;
        private readonly IElasticService _elasticService;

        public AuthController(
            ICommandHandler<RegisterUserCommand, (string token, User user)> registerHandler,
            IQueryHandler<LoginQuery, string> loginHandler,
            IElasticService elasticService)
        {
            _registerHandler = registerHandler;
            _loginHandler = loginHandler;
            _elasticService = elasticService;
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

                return Ok(new { token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
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
                return Ok(new { token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }
    }
}