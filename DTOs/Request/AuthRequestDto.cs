using System.ComponentModel.DataAnnotations;

namespace EventCalendarAPI.DTOs.Request
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^\S+$", ErrorMessage = "Username must not contain spaces.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MinLength(10, ErrorMessage = "Phone number must be at least 10 digits.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class LoginRequestDto
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
