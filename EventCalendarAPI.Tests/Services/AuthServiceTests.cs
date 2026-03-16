using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using EventCalendarAPI.Services;
using Moq;

namespace EventCalendarAPI.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IPasswordService> _passwordServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _sut = new AuthService(_userRepoMock.Object, _passwordServiceMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_WhenUsernameExists_ThrowsDuplicateEntityException()
        {
            _userRepoMock.Setup(r => r.UsernameExistsAsync("john")).ReturnsAsync(true);

            var request = new RegisterRequestDto { Username = "john", Email = "john@test.com", Password = "pass123", FirstName = "John", LastName = "Doe" };

            await Assert.ThrowsAsync<DuplicateEntityException>(() => _sut.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailExists_ThrowsDuplicateEntityException()
        {
            _userRepoMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.EmailExistsAsync("john@test.com")).ReturnsAsync(true);

            var request = new RegisterRequestDto { Username = "john", Email = "john@test.com", Password = "pass123", FirstName = "John", LastName = "Doe" };

            await Assert.ThrowsAsync<DuplicateEntityException>(() => _sut.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
        {
            var salt = new byte[64];
            _userRepoMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _passwordServiceMock.Setup(p => p.HashPassword(It.IsAny<string>(), out salt)).Returns("hashedpw");
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
            _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns("token123");
            _tokenServiceMock.Setup(t => t.GetTokenExpiry()).Returns(DateTime.UtcNow.AddHours(24));

            var request = new RegisterRequestDto { Username = "john", Email = "john@test.com", Password = "pass123", FirstName = "John", LastName = "Doe" };

            var result = await _sut.RegisterAsync(request);

            Assert.NotNull(result);
            Assert.Equal("token123", result.Token);
            Assert.Equal("User", result.User.Role);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorizedException()
        {
            _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.LoginAsync(new LoginRequestDto { UsernameOrEmail = "nobody", Password = "wrong" }));
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedException()
        {
            var user = new User { IsActive = false, PasswordHash = "hash", PasswordSalt = new byte[64] };
            _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.LoginAsync(new LoginRequestDto { UsernameOrEmail = "john", Password = "pass" }));
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedException()
        {
            var user = new User { IsActive = true, PasswordHash = "hash", PasswordSalt = new byte[64] };
            _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>())).Returns(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.LoginAsync(new LoginRequestDto { UsernameOrEmail = "john", Password = "wrong" }));
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
        {
            var user = new User { Id = 1, Username = "john", Email = "john@test.com", Role = "User", IsActive = true, PasswordHash = "hash", PasswordSalt = new byte[64], FirstName = "John", LastName = "Doe" };
            _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>())).Returns(true);
            _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns("token123");
            _tokenServiceMock.Setup(t => t.GetTokenExpiry()).Returns(DateTime.UtcNow.AddHours(24));

            var result = await _sut.LoginAsync(new LoginRequestDto { UsernameOrEmail = "john", Password = "pass" });

            Assert.NotNull(result);
            Assert.Equal("token123", result.Token);
        }
    }
}
