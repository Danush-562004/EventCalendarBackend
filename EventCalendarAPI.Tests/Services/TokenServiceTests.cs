using EventCalendarAPI.Services;
using Microsoft.Extensions.Configuration;

namespace EventCalendarAPI.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _sut;

        public TokenServiceTests()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "TestSuperSecretKeyThatIsAtLeast32CharactersLong!",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience",
                    ["Jwt:ExpiryHours"] = "24"
                })
                .Build();

            _sut = new TokenService(config);
        }

        [Fact]
        public void GenerateToken_ReturnsNonEmptyToken()
        {
            var token = _sut.GenerateToken(1, "john", "User");

            Assert.NotEmpty(token);
        }

        [Fact]
        public void GenerateToken_ReturnsDifferentTokensForDifferentUsers()
        {
            var token1 = _sut.GenerateToken(1, "john", "User");
            var token2 = _sut.GenerateToken(2, "jane", "Admin");

            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GetTokenExpiry_ReturnsDateInFuture()
        {
            var expiry = _sut.GetTokenExpiry();

            Assert.True(expiry > DateTime.UtcNow);
        }
    }
}
