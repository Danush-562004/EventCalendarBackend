using EventCalendarAPI.Services;

namespace EventCalendarAPI.Tests.Services
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _sut = new();

        [Fact]
        public void HashPassword_ReturnsNonEmptyHash()
        {
            var hash = _sut.HashPassword("mypassword", out var salt);

            Assert.NotEmpty(hash);
            Assert.NotEmpty(salt);
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            var hash = _sut.HashPassword("mypassword", out var salt);

            var result = _sut.VerifyPassword("mypassword", hash, salt);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WithWrongPassword_ReturnsFalse()
        {
            var hash = _sut.HashPassword("mypassword", out var salt);

            var result = _sut.VerifyPassword("wrongpassword", hash, salt);

            Assert.False(result);
        }

        [Fact]
        public void HashPassword_TwiceSameInput_ProducesDifferentHashes()
        {
            var hash1 = _sut.HashPassword("mypassword", out var salt1);
            var hash2 = _sut.HashPassword("mypassword", out var salt2);

            // Different salts should produce different hashes
            Assert.NotEqual(hash1, hash2);
        }
    }
}
