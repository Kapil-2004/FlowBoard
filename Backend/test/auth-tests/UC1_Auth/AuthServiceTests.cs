using FlowBoard.Auth.Data;
using FlowBoard.Auth.DTOs;
using FlowBoard.Auth.Interfaces;
using FlowBoard.Auth.Repositories;
using FlowBoard.Auth.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using FluentAssertions;

namespace auth_service.Tests.UC1_Auth
{
    /// <summary>
    /// Unit tests for AuthService.
    /// Uses EF Core InMemory provider so no PostgreSQL instance is needed.
    /// IJwtService is mocked so tests stay focused on AuthService logic only.
    /// </summary>
    public class AuthServiceTests : IDisposable
    {
        // ── Shared setup ──────────────────────────────────────────────────────
        private readonly AuthDbContext    _db;
        private readonly Mock<IJwtService> _jwtMock;
        private readonly AuthService      _sut;   // System Under Test

        public AuthServiceTests()
        {
            // Each test gets a fresh, isolated in-memory database
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db      = new AuthDbContext(options);
            _jwtMock = new Mock<IJwtService>();

            // JWT mock always returns a predictable token string
            _jwtMock.Setup(j => j.GenerateToken(It.IsAny<FlowBoard.Auth.Models.User>()))
                    .Returns("mock-jwt-token");

            _sut = new AuthService(
                new UserRepository(_db),
                _jwtMock.Object,
                NullLogger<AuthService>.Instance
            );
        }

        public void Dispose() => _db.Dispose();

        // ── REGISTER ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_ValidInput_ReturnsTokenAndUserProfile()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FullName = "Alice Smith",
                Email    = "alice@example.com",
                Password = "SecurePass123!"
            };

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("mock-jwt-token");
            result.TokenType.Should().Be("Bearer");
            result.ExpiresIn.Should().Be(86400);
            result.User.Email.Should().Be("alice@example.com");
            result.User.FullName.Should().Be("Alice Smith");
            result.User.Provider.Should().Be("LOCAL");
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange – register first user
            var dto = new RegisterDto
            {
                FullName = "Bob",
                Email    = "bob@example.com",
                Password = "Password123!"
            };
            await _sut.RegisterAsync(dto);

            // Act & Assert – second registration with same email must throw
            var act = async () => await _sut.RegisterAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*already registered*");
        }

        [Fact]
        public async Task Register_EmailStoredAsLowerCase()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FullName = "Charlie",
                Email    = "CHARLIE@EXAMPLE.COM",
                Password = "Password123!"
            };

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert – email must be normalised to lowercase
            result.User.Email.Should().Be("charlie@example.com");
        }

        [Fact]
        public async Task Register_PasswordIsHashed_NotStoredInPlainText()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FullName = "Dave",
                Email    = "dave@example.com",
                Password = "MyPlainPassword!"
            };

            // Act
            await _sut.RegisterAsync(dto);

            // Assert – the stored hash must NOT equal the original password
            var storedUser = await _db.Users.FirstAsync(u => u.Email == "dave@example.com");
            storedUser.PasswordHash.Should().NotBe("MyPlainPassword!");
            storedUser.PasswordHash.Should().MatchRegex(@"^\$2[ab]\$.*"); // BCrypt prefix
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange – register first, then login
            var registerDto = new RegisterDto
            {
                FullName = "Eve",
                Email    = "eve@example.com",
                Password = "EveSecret99!"
            };
            await _sut.RegisterAsync(registerDto);

            var loginDto = new LoginDto
            {
                Email    = "eve@example.com",
                Password = "EveSecret99!"
            };

            // Act
            var result = await _sut.LoginAsync(loginDto);

            // Assert
            result.Token.Should().Be("mock-jwt-token");
            result.User.Email.Should().Be("eve@example.com");
        }

        [Fact]
        public async Task Login_WrongPassword_ThrowsUnauthorized()
        {
            // Arrange
            await _sut.RegisterAsync(new RegisterDto
            {
                FullName = "Frank", Email = "frank@example.com", Password = "RightPass!"
            });

            // Act & Assert
            var act = async () => await _sut.LoginAsync(new LoginDto
            {
                Email = "frank@example.com", Password = "WrongPass!"
            });
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task Login_UnknownEmail_ThrowsUnauthorized()
        {
            var act = async () => await _sut.LoginAsync(new LoginDto
            {
                Email = "nobody@example.com", Password = "AnyPass!"
            });
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task Login_EmailCaseInsensitive_Succeeds()
        {
            // Arrange
            await _sut.RegisterAsync(new RegisterDto
            {
                FullName = "Grace", Email = "grace@example.com", Password = "GracePass!"
            });

            // Act – login with upper-case email
            var result = await _sut.LoginAsync(new LoginDto
            {
                Email = "GRACE@EXAMPLE.COM", Password = "GracePass!"
            });

            // Assert
            result.Should().NotBeNull();
        }

        // ── PROFILE ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetProfile_ExistingUser_ReturnsDto()
        {
            // Arrange
            var auth = await _sut.RegisterAsync(new RegisterDto
            {
                FullName = "Heidi", Email = "heidi@example.com", Password = "Pass123!"
            });

            // Act
            var profile = await _sut.GetProfileAsync(auth.User.UserId);

            // Assert
            profile.Email.Should().Be("heidi@example.com");
            profile.FullName.Should().Be("Heidi");
        }

        [Fact]
        public async Task GetProfile_UnknownId_ThrowsKeyNotFound()
        {
            var act = async () => await _sut.GetProfileAsync(Guid.NewGuid());
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateProfile_ChangesFullName()
        {
            // Arrange
            var auth = await _sut.RegisterAsync(new RegisterDto
            {
                FullName = "Ivan", Email = "ivan@example.com", Password = "Pass123!"
            });

            // Act
            var updated = await _sut.UpdateProfileAsync(auth.User.UserId,
                new UpdateProfileDto { FullName = "Ivan The Great" });

            // Assert
            updated.FullName.Should().Be("Ivan The Great");
        }

        // ── SEARCH ────────────────────────────────────────────────────────────

        [Fact]
        public async Task SearchUsers_PartialNameMatch_ReturnsResults()
        {
            // Arrange – seed two users
            await _sut.RegisterAsync(new RegisterDto
            {
                FullName = "Judy Chen", Email = "judy@example.com", Password = "Pass123!"
            });
            await _sut.RegisterAsync(new RegisterDto
            {
                FullName = "Karl Mueller", Email = "karl@example.com", Password = "Pass123!"
            });

            // Act
            var results = (await _sut.SearchUsersAsync("judy")).ToList();

            // Assert
            results.Should().HaveCount(1);
            results[0].FullName.Should().Be("Judy Chen");
        }

        [Fact]
        public async Task SearchUsers_QueryTooShort_ReturnsEmpty()
        {
            var results = await _sut.SearchUsersAsync("a"); // length < 2
            results.Should().BeEmpty();
        }
    }
}
