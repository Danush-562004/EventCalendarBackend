using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendarAPI.Controllers
{
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Register a new user account.
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), ApiResponseDto<AuthResponseDto>.Ok(result, "Registration successful."));
        }

        // Login with username/email and password.
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponseDto<AuthResponseDto>.Ok(result, "Login successful."));
        }
    }
}
