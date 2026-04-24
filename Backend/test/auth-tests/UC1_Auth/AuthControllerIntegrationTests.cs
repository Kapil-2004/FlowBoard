using System.Net;
using System.Net.Http.Json;
using FlowBoard.Auth.Controllers;
using FlowBoard.Auth.Data;
using FlowBoard.Auth.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace auth_service.Tests.UC1_Auth
{
    /// <summary>
    /// Integration tests for AuthController.
    /// Uses WebApplicationFactory to spin up a real in-process ASP.NET Core server,
    /// replacing PostgreSQL with the EF Core InMemory provider.
    /// Tests the full request/response pipeline: routing → middleware → controller → service → DB.
    /// </summary>
    public class AuthControllerIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly string _dbName = "IntegrationTestDb_" + Guid.NewGuid();

        public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real PostgreSQL DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Register a fresh, isolated InMemory database for this test instance
                    services.AddDbContext<AuthDbContext>(opts =>
                        opts.UseInMemoryDatabase(_dbName));
                });
            })
            .CreateClient();
        }

        // ── HEALTH ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GET_Health_Returns200()
        {
            var response = await _client.GetAsync("/api/auth/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ── REGISTER ──────────────────────────────────────────────────────────

        [Fact]
        public async Task POST_Register_ValidBody_Returns201WithToken()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FullName = "Alice Test",
                Email    = $"alice_{Guid.NewGuid()}@test.com",  // unique per test run
                Password = "StrongPass123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            body!.Success.Should().BeTrue();
            body.Data!.Token.Should().NotBeNullOrWhiteSpace();
            body.Data.User.Email.Should().Be(dto.Email.ToLower());
        }

        [Fact]
        public async Task POST_Register_DuplicateEmail_Returns409()
        {
            // Arrange
            var dto = new RegisterDto
            {
                FullName = "Bob Dup",
                Email    = "bob_dup@test.com",
                Password = "AnyPass123!"
            };
            await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Act – second registration with the same email
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task POST_Register_MissingEmail_Returns400()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FullName = "No Email",
                Password = "AnyPass123!"
                // Email is intentionally omitted
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task POST_Register_ShortPassword_Returns400()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FullName = "Short Pass",
                Email    = "shortpass@test.com",
                Password = "1234567"  // 7 chars – below 8 minimum
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task POST_Login_ValidCredentials_Returns200WithToken()
        {
            // Arrange – register then login
            var email = $"login_user_{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
            {
                FullName = "Login User", Email = email, Password = "ValidPass123!"
            });

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
            {
                Email = email, Password = "ValidPass123!"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            body!.Data!.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task POST_Login_WrongPassword_Returns401()
        {
            // Arrange
            var email = $"wrong_pass_{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
            {
                FullName = "Wrong Pass User", Email = email, Password = "CorrectPass!"
            });

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
            {
                Email = email, Password = "WrongPass!"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task POST_Login_UnknownEmail_Returns401()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
            {
                Email = "nobody@nowhere.com", Password = "AnyPass!"
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── PROFILE (requires auth) ───────────────────────────────────────────

        [Fact]
        public async Task GET_Profile_WithValidJwt_Returns200()
        {
            // Arrange – register and get token
            var email = $"profile_user_{Guid.NewGuid()}@test.com";
            var reg = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
            {
                FullName = "Profile User", Email = email, Password = "ProfilePass1!"
            });
            var regBody = await reg.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            var token   = regBody!.Data!.Token;

            // Act
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
            body!.Data!.Email.Should().Be(email.ToLower());
        }

        [Fact]
        public async Task GET_Profile_WithoutJwt_Returns401()
        {
            var response = await _client.GetAsync("/api/auth/profile");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── SEARCH (requires auth) ────────────────────────────────────────────

        [Fact]
        public async Task GET_Search_WithValidJwt_ReturnsResults()
        {
            // Arrange – register a searchable user and get a token
            var email = $"searchme_{Guid.NewGuid()}@test.com";
            var reg = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
            {
                FullName = "Search Target User", Email = email, Password = "SearchPass1!"
            });
            var token = (await reg.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>())!.Data!.Token;

            // Act
            using var request = new HttpRequestMessage(HttpMethod.Get,
                "/api/auth/users/search?q=Search+Target");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GET_Search_WithoutJwt_Returns401()
        {
            var response = await _client.GetAsync("/api/auth/users/search?q=alice");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GET_Search_QueryTooShort_Returns400()
        {
            // Arrange – get a valid token first
            var reg = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
            {
                FullName = "Search Short", Email = $"short_{Guid.NewGuid()}@test.com", Password = "ShortPass1!"
            });
            var token = (await reg.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>())!.Data!.Token;

            // Act
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/users/search?q=a");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
