using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendarAPI.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Get all users with pagination (Admin only).</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<UserResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            var users = await _userService.GetAllAsync(page, pageSize, search);
            return Ok(ApiResponseDto<PagedResponseDto<UserResponseDto>>.Ok(users));
        }

        /// <summary>Get a user by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<UserResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(ApiResponseDto<UserResponseDto>.Ok(user));
        }

        /// <summary>Get the currently authenticated user's profile.</summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponseDto<UserResponseDto>), 200)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userService.GetByIdAsync(GetCurrentUserId());
            return Ok(ApiResponseDto<UserResponseDto>.Ok(user));
        }

        /// <summary>Update a user profile.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<UserResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserRequestDto request)
        {
            var user = await _userService.UpdateAsync(id, request, GetCurrentUserId());
            return Ok(ApiResponseDto<UserResponseDto>.Ok(user, "Profile updated successfully."));
        }

        /// <summary>Change the authenticated user's password.</summary>
        [HttpPut("me/change-password")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            await _userService.ChangePasswordAsync(GetCurrentUserId(), request);
            return Ok(ApiResponseDto<object>.Ok(null!, "Password changed successfully."));
        }

        /// <summary>Deactivate a user account.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), 403)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _userService.DeleteAsync(id, GetCurrentUserId());
            return Ok(ApiResponseDto<object>.Ok(null!, "Account deactivated successfully."));
        }
    }
}
