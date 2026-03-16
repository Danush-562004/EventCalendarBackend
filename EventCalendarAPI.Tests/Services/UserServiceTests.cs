using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using EventCalendarAPI.Services;
using Moq;

namespace EventCalendarAPI.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IPasswordService> _passwordServiceMock = new();
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _sut = new UserService(_userRepoMock.Object, _passwordServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserNotFound_ThrowsEntityNotFoundException()
        {
            _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByIdAsync(99));
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ReturnsUserDto()
        {
            var user = new User { Id = 1, Username = "john", Email = "john@test.com", FirstName = "John", LastName = "Doe", Role = "User" };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _sut.GetByIdAsync(1);

            Assert.Equal("john", result.Username);
            Assert.Equal("John Doe", result.FullName);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPaged()
        {
            var users = new List<User> { new User { Id = 1, Username = "john", FirstName = "John", LastName = "Doe" } };
            _userRepoMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(new PagedResult<User> { Items = users, TotalCount = 1 });

            var result = await _sut.GetAllAsync(1, 10);

            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotOwner_ThrowsUnauthorizedException()
        {
            var user = new User { Id = 1 };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.UpdateAsync(1, new UpdateUserRequestDto(), requestingUserId: 2));
        }

        [Fact]
        public async Task UpdateAsync_WhenOwner_UpdatesAndReturns()
        {
            var user = new User { Id = 1, Username = "john", Email = "j@j.com", FirstName = "John", LastName = "Doe", Role = "User" };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

            var result = await _sut.UpdateAsync(1, new UpdateUserRequestDto { FirstName = "Jane" }, requestingUserId: 1);

            Assert.Equal("Jane", result.FirstName);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotOwner_ThrowsUnauthorizedException()
        {
            var user = new User { Id = 1 };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.DeleteAsync(1, requestingUserId: 2));
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenWrongCurrentPassword_ThrowsUnauthorizedException()
        {
            var user = new User { Id = 1, PasswordHash = "hash", PasswordSalt = new byte[64] };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>())).Returns(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.ChangePasswordAsync(1, new ChangePasswordRequestDto { CurrentPassword = "wrong", NewPassword = "new123" }));
        }
    }
}
