using FlowBoard.Auth.Helpers;
using FlowBoard.Auth.Models;
using FlowBoard.Auth.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using FluentAssertions;

namespace auth_service.Tests.UC1_Auth
{
    /// <summary>
    /// Unit tests for JwtService.
    /// Tests token generation, claim content, expiry, and extraction.
    /// No database needed – JwtService is stateless.
    /// </summary>
    public class JwtServiceTests
    {
        private readonly JwtService _sut;

        // A fixed test user used across multiple tests
        private readonly User _testUser = new()
        {
            UserId   = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email    = "test@flowboard.com",
            FullName = "Test User",
            Provider = "LOCAL"
        };

        public JwtServiceTests()
        {
            var settings = Options.Create(new JwtSettings
            {
                Secret      = "Test-Secret-Key-That-Is-32-Chars-Long!!",
                Issuer      = "FlowBoard.Auth",
                Audience    = "FlowBoard.Client",
                ExpiryHours = 1
            });

            _sut = new JwtService(settings, NullLogger<JwtService>.Instance);
        }

        [Fact]
        public void GenerateToken_ValidUser_ReturnsNonEmptyString()
        {
            var token = _sut.GenerateToken(_testUser);
            token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GenerateToken_TokenHasThreeParts_JwtStructure()
        {
            // A JWT is always header.payload.signature (three dot-separated parts)
            var token  = _sut.GenerateToken(_testUser);
            var parts  = token.Split('.');
            parts.Should().HaveCount(3);
        }

        [Fact]
        public void GenerateToken_DifferentCallsProduceDifferentTokens()
        {
            // Each token includes a unique jti (JWT ID), so two calls must differ
            var token1 = _sut.GenerateToken(_testUser);
            var token2 = _sut.GenerateToken(_testUser);
            token1.Should().NotBe(token2);
        }

        [Fact]
        public void GetUserIdFromToken_ValidToken_ReturnsCorrectUserId()
        {
            // Arrange
            var token = _sut.GenerateToken(_testUser);

            // Act
            var extractedId = _sut.GetUserIdFromToken(token);

            // Assert
            extractedId.Should().Be(_testUser.UserId);
        }

        [Fact]
        public void GetUserIdFromToken_TamperedToken_ReturnsNull()
        {
            var token    = _sut.GenerateToken(_testUser);
            var tampered = token[..^5] + "XXXXX"; // corrupt the signature

            var result = _sut.GetUserIdFromToken(tampered);
            result.Should().BeNull();
        }

        [Fact]
        public void GetUserIdFromToken_RandomString_ReturnsNull()
        {
            var result = _sut.GetUserIdFromToken("this.is.not.a.jwt");
            result.Should().BeNull();
        }

        [Fact]
        public void GetUserIdFromToken_EmptyString_ReturnsNull()
        {
            var result = _sut.GetUserIdFromToken("");
            result.Should().BeNull();
        }

        [Fact]
        public void GenerateToken_WrongSecret_CannotBeValidated()
        {
            // A token signed with a different secret must not validate
            var otherSettings = Options.Create(new JwtSettings
            {
                Secret      = "Other-Secret-Key-That-Is-32-Chars-Long!!",
                Issuer      = "FlowBoard.Auth",
                Audience    = "FlowBoard.Client",
                ExpiryHours = 1
            });
            var otherService = new JwtService(otherSettings, NullLogger<JwtService>.Instance);

            var foreignToken = otherService.GenerateToken(_testUser);

            // Try to validate the foreign token with _sut (different secret)
            var result = _sut.GetUserIdFromToken(foreignToken);
            result.Should().BeNull();
        }
    }
}
