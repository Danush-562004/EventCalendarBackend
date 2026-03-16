using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventCalendarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("User ID not found in token.");
            return int.Parse(claim);
        }

        protected string GetCurrentUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? "User";
    }
}
