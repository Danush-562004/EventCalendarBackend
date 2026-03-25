using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;

namespace EventCalendarAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly IAuditLogService _auditLog;

        public AuthService(IUserRepository userRepository, IPasswordService passwordService,
            ITokenService tokenService, IAuditLogService auditLog)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _auditLog = auditLog;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
                throw new DuplicateEntityException($"Username '{request.Username}' is already taken.");

            if (await _userRepository.EmailExistsAsync(request.Email))
                throw new DuplicateEntityException($"Email '{request.Email}' is already registered.");

            var passwordHash = _passwordService.HashPassword(request.Password, out var salt);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = salt,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = "User"
            };

            var created = await _userRepository.AddAsync(user);
            await _auditLog.LogAsync("Register", "User", created.Id.ToString(),
                created.Id, created.Username, newValues: $"{{\"username\":\"{created.Username}\",\"email\":\"{created.Email}\"}}");
            return BuildAuthResponse(created);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail)
                ?? throw new UnauthorizedException("No account found with that username or email.");

            if (!user.IsActive)
                throw new UnauthorizedException("This account has been deactivated.");

            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedException("Wrong password!");

            await _auditLog.LogAsync("Login", "User", user.Id.ToString(), user.Id, user.Username);
            return BuildAuthResponse(user);
        }

        private AuthResponseDto BuildAuthResponse(User user) => new()
        {
            Token = _tokenService.GenerateToken(user.Id, user.Username, user.Role),
            Expiry = _tokenService.GetTokenExpiry(),
            User = MapToUserResponse(user)
        };

        private static UserResponseDto MapToUserResponse(User user) => new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ProfilePicture = user.ProfilePicture,
            Role = user.Role,
            EmailNotifications = user.EmailNotifications,
            PushNotifications = user.PushNotifications,
            CreatedAt = user.CreatedAt
        };
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IAuditLogService _auditLog;

        public UserService(IUserRepository userRepository, IPasswordService passwordService, IAuditLogService auditLog)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _auditLog = auditLog;
        }

        public async Task<UserResponseDto> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("User", id);
            return MapToResponse(user);
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllAsync(int page, int pageSize, string? search = null)
        {
            var result = await _userRepository.GetPagedAsync(page, pageSize, search);
            return new PagedResponseDto<UserResponseDto>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<UserResponseDto> UpdateAsync(int id, UpdateUserRequestDto request, int requestingUserId)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("User", id);

            if (id != requestingUserId)
                throw new UnauthorizedException("You can only update your own profile.");

            var old = $"{{\"firstName\":\"{user.FirstName}\",\"lastName\":\"{user.LastName}\"}}";
            if (request.FirstName != null) user.FirstName = request.FirstName;
            if (request.LastName != null) user.LastName = request.LastName;
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
            if (request.ProfilePicture != null) user.ProfilePicture = request.ProfilePicture;
            if (request.EmailNotifications.HasValue) user.EmailNotifications = request.EmailNotifications.Value;
            if (request.PushNotifications.HasValue) user.PushNotifications = request.PushNotifications.Value;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _auditLog.LogAsync("Update", "User", id.ToString(), requestingUserId, user.Username, old,
                $"{{\"firstName\":\"{user.FirstName}\",\"lastName\":\"{user.LastName}\"}}");
            return MapToResponse(user);
        }

        public async Task DeleteAsync(int id, int requestingUserId)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new EntityNotFoundException("User", id);

            if (id != requestingUserId)
                throw new UnauthorizedException("You can only delete your own account.");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            await _auditLog.LogAsync("Delete", "User", id.ToString(), requestingUserId, user.Username);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new EntityNotFoundException("User", userId);

            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedException("Current password is incorrect.");

            user.PasswordHash = _passwordService.HashPassword(request.NewPassword, out var salt);
            user.PasswordSalt = salt;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _auditLog.LogAsync("ChangePassword", "User", userId.ToString(), userId, user.Username);
        }

        private static UserResponseDto MapToResponse(User u) => new()
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            PhoneNumber = u.PhoneNumber,
            ProfilePicture = u.ProfilePicture,
            Role = u.Role,
            EmailNotifications = u.EmailNotifications,
            PushNotifications = u.PushNotifications,
            CreatedAt = u.CreatedAt
        };
    }
}
