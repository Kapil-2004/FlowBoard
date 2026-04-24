using System.Text.Json;
using FlowBoard.Auth.DTOs;
using FlowBoard.Auth.Interfaces;
using FlowBoard.Auth.Models;
using FlowBoard.Auth.Repositories;

namespace FlowBoard.Auth.Services
{
    /// <summary>
    /// Handles Google and GitHub OAuth2 access-token validation.
    ///
    /// Flow:
    ///   1. Client authenticates with the provider (Google/GitHub) and gets an access token.
    ///   2. Client sends that token to POST /api/auth/oauth/google (or /github).
    ///   3. This service calls the provider's userinfo endpoint to validate the token
    ///      and extract the user's profile.
    ///   4. If the user already exists (matched by provider+providerId), return their JWT.
    ///      If not, create a new account then return a JWT.
    /// </summary>
    public class OAuthService : IOAuthService
    {
        private readonly UserRepository  _userRepo;
        private readonly IJwtService     _jwtService;
        private readonly HttpClient      _httpClient;
        private readonly ILogger<OAuthService> _logger;

        // Provider userinfo endpoints
        private const string GoogleUserInfoUrl = "https://www.googleapis.com/oauth2/v3/userinfo";
        private const string GitHubUserInfoUrl = "https://api.github.com/user";
        private const string GitHubEmailsUrl   = "https://api.github.com/user/emails";

        public OAuthService(
            UserRepository       userRepo,
            IJwtService          jwtService,
            IHttpClientFactory   httpClientFactory,
            ILogger<OAuthService> logger)
        {
            _userRepo   = userRepo;
            _jwtService = jwtService;
            _httpClient = httpClientFactory.CreateClient("oauth");
            _logger     = logger;
        }

        /// <inheritdoc />
        public async Task<AuthResponseDto> HandleOAuthAsync(OAuthRequestDto dto)
        {
            _logger.LogInformation("OAuth login attempt via {Provider}", dto.Provider);

            return dto.Provider.ToUpper() switch
            {
                "GOOGLE" => await HandleGoogleAsync(dto.AccessToken),
                "GITHUB" => await HandleGitHubAsync(dto.AccessToken),
                _        => throw new ArgumentException($"Unsupported OAuth provider: {dto.Provider}")
            };
        }

        // ── GOOGLE ───────────────────────────────────────────────────────────

        private async Task<AuthResponseDto> HandleGoogleAsync(string accessToken)
        {
            // Call Google's userinfo endpoint to validate the token
            using var request = new HttpRequestMessage(HttpMethod.Get, GoogleUserInfoUrl);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Invalid Google access token.");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Extract profile fields from Google's response
            var providerId = root.GetProperty("sub").GetString()
                             ?? throw new InvalidOperationException("Google userinfo missing 'sub'.");
            var email      = root.GetProperty("email").GetString()
                             ?? throw new InvalidOperationException("Google userinfo missing 'email'.");
            var name       = root.TryGetProperty("name", out var n) ? n.GetString() : email;
            var avatar     = root.TryGetProperty("picture", out var p) ? p.GetString() : null;

            return await FindOrCreateOAuthUserAsync("GOOGLE", providerId, email, name, avatar);
        }

        // ── GITHUB ───────────────────────────────────────────────────────────

        private async Task<AuthResponseDto> HandleGitHubAsync(string accessToken)
        {
            // GitHub requires User-Agent header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent",    "FlowBoard-Auth-Service/1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept",        "application/vnd.github+json");

            // Get basic profile
            var profileResponse = await _httpClient.GetAsync(GitHubUserInfoUrl);
            if (!profileResponse.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Invalid GitHub access token.");

            var profileJson = await profileResponse.Content.ReadAsStringAsync();
            using var profileDoc = JsonDocument.Parse(profileJson);
            var profile = profileDoc.RootElement;

            var providerId = profile.GetProperty("id").GetInt64().ToString();
            var name       = profile.TryGetProperty("name", out var n) && n.ValueKind != JsonValueKind.Null
                             ? n.GetString() : null;
            var avatar     = profile.TryGetProperty("avatar_url", out var a) ? a.GetString() : null;

            // GitHub may not expose email in profile – fetch from /user/emails if needed
            string? email = null;
            if (profile.TryGetProperty("email", out var e) && e.ValueKind != JsonValueKind.Null)
                email = e.GetString();

            if (string.IsNullOrEmpty(email))
                email = await GetGitHubPrimaryEmailAsync(accessToken);

            if (string.IsNullOrEmpty(email))
                throw new InvalidOperationException("Could not retrieve email from GitHub account.");

            return await FindOrCreateOAuthUserAsync("GITHUB", providerId, email, name, avatar);
        }

        private async Task<string?> GetGitHubPrimaryEmailAsync(string accessToken)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, GitHubEmailsUrl);
            req.Headers.Add("Authorization", $"Bearer {accessToken}");
            req.Headers.Add("User-Agent",    "FlowBoard-Auth-Service/1.0");

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            // Prefer the primary, verified email
            foreach (var entry in doc.RootElement.EnumerateArray())
            {
                bool primary  = entry.TryGetProperty("primary",  out var p) && p.GetBoolean();
                bool verified = entry.TryGetProperty("verified", out var v) && v.GetBoolean();
                if (primary && verified)
                    return entry.GetProperty("email").GetString();
            }
            return null;
        }

        // ── FIND OR CREATE ────────────────────────────────────────────────────

        /// <summary>
        /// Finds an existing OAuth user by (provider, providerId).
        /// Falls back to email lookup (in case the user previously registered locally).
        /// Creates a new account if no match is found.
        /// </summary>
        private async Task<AuthResponseDto> FindOrCreateOAuthUserAsync(
            string provider, string providerId,
            string email, string? name, string? avatar)
        {
            // 1. Try to find by provider identity (most accurate)
            var user = await _userRepo.GetByProviderAsync(provider, providerId);

            if (user == null)
            {
                // 2. Fallback: match by email to detect existing local account
                user = await _userRepo.GetByEmailAsync(email);

                if (user != null)
                {
                    // Link the OAuth provider to the existing account
                    user.Provider   = provider;
                    user.ProviderId = providerId;
                    if (avatar is not null) user.AvatarUrl = avatar;
                    await _userRepo.UpdateAsync(user);
                    _logger.LogInformation("Linked {Provider} to existing user {UserId}", provider, user.UserId);
                }
                else
                {
                    // 3. First OAuth login – create a brand-new account
                    user = new User
                    {
                        UserId     = Guid.NewGuid(),
                        FullName   = name?.Trim(),
                        Email      = email.Trim().ToLower(),
                        Provider   = provider,
                        ProviderId = providerId,
                        AvatarUrl  = avatar,
                        CreatedAt  = DateTime.UtcNow,
                        UpdatedAt  = DateTime.UtcNow
                    };
                    await _userRepo.CreateAsync(user);
                    _logger.LogInformation("New OAuth user created: {UserId} via {Provider}", user.UserId, provider);
                }
            }

            return new AuthResponseDto
            {
                Token     = _jwtService.GenerateToken(user),
                TokenType = "Bearer",
                ExpiresIn = 86400,
                User      = new DTOs.UserDto
                {
                    UserId    = user.UserId,
                    FullName  = user.FullName ?? string.Empty,
                    Email     = user.Email,
                    Provider  = user.Provider,
                    AvatarUrl = user.AvatarUrl,
                    CreatedAt = user.CreatedAt
                }
            };
        }
    }
}
