using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FlowBoard.Auth.DTOs;
using FlowBoard.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Auth.Controllers
{
    /// <summary>
    /// UC9 – Platform Admin Controller
    ///
    /// Implements every method from the AdminController class diagram:
    ///   AdminDashboard, ManageAllUsers, SuspendUser, DeleteUser,
    ///   ManageAllWorkspaces, DeleteWorkspace, ManageAllBoards, DeleteBoard,
    ///   ViewPlatformAnalytics, ViewAllCards, ViewOverdueCards,
    ///   SendPlatformNotification, ViewAuditLogs, GenerateActivityReport
    ///
    /// Credentials: admin@flowboard.app / Admin@FlowBoard123
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "PlatformAdmin")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        // ── Injected services (matching class diagram deps) ────────────────────
        private readonly IAuthService  _authService;
        private readonly IHttpClientFactory _http;
        private readonly ILogger<AdminController> _logger;

        // Internal service URLs (align with docker-compose service names)
        private const string WS_URL   = "http://workspace-service:5002/api";
        private const string BOARD_URL = "http://board-service:5003/api";
        private const string CARD_URL  = "http://card-service:5005/api";
        private const string NOTIF_URL = "http://notification-service:5008/api";

        public AdminController(
            IAuthService authService,
            IHttpClientFactory http,
            ILogger<AdminController> logger)
        {
            _authService = authService;
            _http        = http;
            _logger      = logger;
        }

        // ── 1. ADMIN DASHBOARD ────────────────────────────────────────────────

        /// <summary>AdminDashboard(): IActionResult — stats from all services</summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<AdminDashboardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AdminDashboard()
        {
            var users = (await _authService.GetAllUsersAsync()).ToList();

            // Fetch board/workspace counts via proxy
            int totalBoards = 0, totalWorkspaces = 0, totalCards = 0, overdueCards = 0;
            try
            {
                var client = CreateAuthClient();
                var wsResp = await client.GetAsync($"{WS_URL}/workspaces");
                if (wsResp.IsSuccessStatusCode)
                {
                    var wsList = await ParseArray(wsResp);
                    totalWorkspaces = wsList.Count;
                }
                var boardResp = await client.GetAsync($"{BOARD_URL}/boards");
                if (boardResp.IsSuccessStatusCode)
                {
                    var boardList = await ParseArray(boardResp);
                    totalBoards = boardList.Count;
                }
                var cardResp = await client.GetAsync($"{CARD_URL}/cards");
                if (cardResp.IsSuccessStatusCode)
                {
                    var cardList = await ParseArray(cardResp);
                    totalCards = cardList.Count;
                }
                var overdueResp = await client.GetAsync($"{CARD_URL}/cards/overdue");
                if (overdueResp.IsSuccessStatusCode)
                {
                    var overdueList = await ParseArray(overdueResp);
                    overdueCards = overdueList.Count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not fetch cross-service stats: {Msg}", ex.Message);
            }

            var dashboard = new AdminDashboardDto
            {
                TotalUsers         = users.Count,
                ActiveUsers        = users.Count(u => u.IsActive),
                SuspendedUsers     = users.Count(u => !u.IsActive),
                MemberCount        = users.Count(u => u.Role == "Member"),
                BoardAdminCount    = users.Count(u => u.Role == "BoardAdmin"),
                PlatformAdminCount = users.Count(u => u.Role == "PlatformAdmin"),
                TotalWorkspaces    = totalWorkspaces,
                TotalBoards        = totalBoards,
                TotalCards         = totalCards,
                OverdueCards       = overdueCards
            };
            return Ok(ApiResponse<AdminDashboardDto>.Ok("Dashboard data retrieved.", dashboard));
        }

        // ── 2. MANAGE ALL USERS ───────────────────────────────────────────────

        /// <summary>ManageAllUsers(): IActionResult</summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ManageAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<UserDto>>.Ok("All users retrieved.", users));
        }

        /// <summary>Get a single user by ID.</summary>
        [HttpGet("users/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            try   { return Ok(ApiResponse<UserDto>.Ok("User retrieved.", await _authService.GetUserByIdAsync(id))); }
            catch (KeyNotFoundException ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
        }

        /// <summary>Change a user's platform role. Valid: Member | BoardAdmin | PlatformAdmin</summary>
        [HttpPut("users/{id:guid}/role")]
        public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<string>.Fail(ModelState.GetErrors()));
            try
            {
                var updated = await _authService.ChangeRoleAsync(id, dto.Role);
                _logger.LogInformation("Admin {Admin} changed role of {UserId} → {Role}", GetAdminEmail(), id, dto.Role);
                LogAudit("ROLE_CHANGE", $"User {id} role changed to {dto.Role}");
                return Ok(ApiResponse<UserDto>.Ok($"Role changed to '{dto.Role}'.", updated));
            }
            catch (KeyNotFoundException   ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
            catch (ArgumentException      ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
        }

        /// <summary>SuspendUser(int): string — suspends user by GUID (diagram shows int, GUID used for safety)</summary>
        [HttpPut("users/{id:guid}/suspend")]
        public async Task<IActionResult> SuspendUser(Guid id)
        {
            try
            {
                var user = await _authService.SuspendUserAsync(id);
                _logger.LogWarning("Admin {Admin} suspended {UserId}", GetAdminEmail(), id);
                LogAudit("USER_SUSPEND", $"User {id} ({user.Email}) suspended");
                return Ok(ApiResponse<UserDto>.Ok("User suspended.", user));
            }
            catch (KeyNotFoundException   ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
        }

        /// <summary>Reactivate a suspended account.</summary>
        [HttpPut("users/{id:guid}/reactivate")]
        public async Task<IActionResult> ReactivateUser(Guid id)
        {
            try
            {
                var user = await _authService.ReactivateUserAsync(id);
                LogAudit("USER_REACTIVATE", $"User {id} reactivated");
                return Ok(ApiResponse<UserDto>.Ok("User reactivated.", user));
            }
            catch (KeyNotFoundException ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
        }

        /// <summary>DeleteUser(int): string — permanently removes user</summary>
        [HttpDelete("users/{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _authService.DeleteUserAsync(id);
                _logger.LogWarning("Admin {Admin} deleted {UserId}", GetAdminEmail(), id);
                LogAudit("USER_DELETE", $"User {id} permanently deleted");
                return NoContent();
            }
            catch (KeyNotFoundException   ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
        }

        // ── 3. MANAGE ALL WORKSPACES ──────────────────────────────────────────

        /// <summary>ManageAllWorkspaces(): IActionResult — proxy to workspace service</summary>
        [HttpGet("workspaces")]
        public async Task<IActionResult> ManageAllWorkspaces()
        {
            try
            {
                var client = CreateAuthClient();
                var resp = await client.GetAsync($"{WS_URL}/workspaces/all");
                if (!resp.IsSuccessStatusCode)
                    resp = await client.GetAsync($"{WS_URL}/workspaces");

                var json = await resp.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ManageAllWorkspaces failed");
                return Ok(ApiResponse<object[]>.Ok("Workspace service unavailable — no data.", Array.Empty<object>()));
            }
        }

        /// <summary>DeleteWorkspace(int): string — proxy delete to workspace service</summary>
        [HttpDelete("workspaces/{id:int}")]
        public async Task<IActionResult> DeleteWorkspace(int id)
        {
            try
            {
                var client = CreateAuthClient();
                var resp   = await client.DeleteAsync($"{WS_URL}/workspaces/{id}");
                LogAudit("WORKSPACE_DELETE", $"Workspace {id} deleted by admin");
                return resp.IsSuccessStatusCode
                    ? Ok(ApiResponse<string>.Ok("Workspace deleted.", $"Workspace {id} deleted."))
                    : StatusCode((int)resp.StatusCode, ApiResponse<string>.Fail($"Workspace service returned {resp.StatusCode}"));
            }
            catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
        }

        // ── 4. MANAGE ALL BOARDS ──────────────────────────────────────────────

        /// <summary>ManageAllBoards(): IActionResult — proxy to board service</summary>
        [HttpGet("boards")]
        public async Task<IActionResult> ManageAllBoards()
        {
            try
            {
                var client = CreateAuthClient();
                var resp   = await client.GetAsync($"{BOARD_URL}/boards/admin/all");
                if (!resp.IsSuccessStatusCode)
                    resp = await client.GetAsync($"{BOARD_URL}/boards");

                var json = await resp.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ManageAllBoards failed");
                return Ok(ApiResponse<object[]>.Ok("Board service unavailable — no data.", Array.Empty<object>()));
            }
        }

        /// <summary>DeleteBoard(int): string — proxy delete to board service</summary>
        [HttpDelete("boards/{id:int}")]
        public async Task<IActionResult> DeleteBoard(int id)
        {
            try
            {
                var client = CreateAuthClient();
                var resp   = await client.DeleteAsync($"{BOARD_URL}/boards/{id}");
                LogAudit("BOARD_DELETE", $"Board {id} deleted by admin");
                return resp.IsSuccessStatusCode
                    ? Ok(ApiResponse<string>.Ok("Board deleted.", $"Board {id} deleted."))
                    : StatusCode((int)resp.StatusCode, ApiResponse<string>.Fail($"Board service returned {resp.StatusCode}"));
            }
            catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
        }

        // ── 5. VIEW PLATFORM ANALYTICS ────────────────────────────────────────

        /// <summary>ViewPlatformAnalytics(): IActionResult — aggregated metrics</summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> ViewPlatformAnalytics()
        {
            var users = (await _authService.GetAllUsersAsync()).ToList();

            var analytics = new PlatformAnalyticsDto
            {
                TotalUsers         = users.Count,
                ActiveUsers        = users.Count(u => u.IsActive),
                NewUsersThisMonth  = users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                NewUsersThisWeek   = users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
                RoleDistribution   = new Dictionary<string, int>
                {
                    ["Member"]        = users.Count(u => u.Role == "Member"),
                    ["BoardAdmin"]    = users.Count(u => u.Role == "BoardAdmin"),
                    ["PlatformAdmin"] = users.Count(u => u.Role == "PlatformAdmin")
                },
                RecentLogins = users
                    .Where(u => u.LastLoginAt.HasValue)
                    .OrderByDescending(u => u.LastLoginAt)
                    .Take(10)
                    .Select(u => new RecentLoginDto { Email = u.Email, Role = u.Role, LastLoginAt = u.LastLoginAt!.Value })
                    .ToList()
            };

            return Ok(ApiResponse<PlatformAnalyticsDto>.Ok("Analytics retrieved.", analytics));
        }

        // ── 6. VIEW ALL CARDS / OVERDUE CARDS ────────────────────────────────

        /// <summary>ViewAllCards(): IActionResult — proxy to card service</summary>
        [HttpGet("cards")]
        public async Task<IActionResult> ViewAllCards()
        {
            try
            {
                var client = CreateAuthClient();
                var resp   = await client.GetAsync($"{CARD_URL}/cards");
                var json   = await resp.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
        }

        /// <summary>ViewOverdueCards(): IActionResult — proxy to card service overdue endpoint</summary>
        [HttpGet("cards/overdue")]
        public async Task<IActionResult> ViewOverdueCards()
        {
            try
            {
                var client = CreateAuthClient();
                var resp   = await client.GetAsync($"{CARD_URL}/cards/overdue");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
                return Ok(ApiResponse<object[]>.Ok("No overdue card endpoint on card service.", Array.Empty<object>()));
            }
            catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
        }

        // ── 7. SEND PLATFORM NOTIFICATION ────────────────────────────────────

        /// <summary>SendPlatformNotification(string,string): string — broadcast to all users</summary>
        [HttpPost("notify")]
        public async Task<IActionResult> SendPlatformNotification([FromBody] PlatformNotificationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(ApiResponse<string>.Fail("Title and Message are required."));

            try
            {
                var users   = (await _authService.GetAllUsersAsync()).ToList();
                var ids     = users.Where(u => u.IsActive).Select(u => u.UserId.ToString()).ToList();

                var client  = CreateAuthClient();
                var body    = JsonSerializer.Serialize(new { recipientIds = ids, title = dto.Title, message = dto.Message });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var resp    = await client.PostAsync($"{NOTIF_URL}/notifications/bulk", content);

                LogAudit("PLATFORM_NOTIFICATION", $"Broadcast: '{dto.Title}' → {ids.Count} users");
                _logger.LogInformation("Platform notification sent by {Admin}: {Title}", GetAdminEmail(), dto.Title);

                return resp.IsSuccessStatusCode
                    ? Ok(ApiResponse<string>.Ok($"Notification sent to {ids.Count} active users.", $"Sent to {ids.Count} users."))
                    : Ok(ApiResponse<string>.Ok($"Notification queued (notification service returned {resp.StatusCode}).", dto.Title));
            }
            catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
        }

        // ── 8. VIEW AUDIT LOGS ────────────────────────────────────────────────

        /// <summary>ViewAuditLogs(): IActionResult — returns in-memory audit log</summary>
        [HttpGet("audit-logs")]
        public IActionResult ViewAuditLogs()
        {
            return Ok(ApiResponse<List<AuditLogDto>>.Ok("Audit logs retrieved.", _auditLog));
        }

        // ── 9. GENERATE ACTIVITY REPORT ───────────────────────────────────────

        /// <summary>GenerateActivityReport(): IActionResult — aggregated CSV-style report</summary>
        [HttpGet("report")]
        public async Task<IActionResult> GenerateActivityReport()
        {
            var users = (await _authService.GetAllUsersAsync()).ToList();

            var report = new ActivityReportDto
            {
                GeneratedAt        = DateTime.UtcNow,
                GeneratedBy        = GetAdminEmail(),
                TotalUsers         = users.Count,
                ActiveUsers        = users.Count(u => u.IsActive),
                SuspendedUsers     = users.Count(u => !u.IsActive),
                RecentSignups      = users.OrderByDescending(u => u.CreatedAt).Take(20)
                                         .Select(u => new { u.Email, u.Role, u.CreatedAt, u.IsActive })
                                         .ToList<object>(),
                AuditSummary       = $"{_auditLog.Count} audit events recorded this session."
            };

            LogAudit("REPORT_GENERATED", $"Activity report generated by {GetAdminEmail()}");
            return Ok(ApiResponse<ActivityReportDto>.Ok("Activity report generated.", report));
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private string GetAdminEmail() => User.FindFirst(ClaimTypes.Email)?.Value ?? "admin";

        /// <summary>Creates an HttpClient with the current admin's Bearer token forwarded.</summary>
        private HttpClient CreateAuthClient()
        {
            var client = _http.CreateClient();
            var token  = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static async Task<List<JsonElement>> ParseArray(HttpResponseMessage resp)
        {
            var json = await resp.Content.ReadAsStringAsync();
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    return doc.RootElement.EnumerateArray().ToList();
                // Try data property
                if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                    return data.EnumerateArray().ToList();
            }
            catch { }
            return new List<JsonElement>();
        }

        // Simple in-memory audit log (survives restarts only if service doesn't restart)
        private static readonly List<AuditLogDto> _auditLog = new();

        private void LogAudit(string action, string detail)
        {
            _auditLog.Insert(0, new AuditLogDto
            {
                Timestamp = DateTime.UtcNow,
                Action    = action,
                Detail    = detail,
                Actor     = GetAdminEmail()
            });
            // Keep last 500 entries
            if (_auditLog.Count > 500) _auditLog.RemoveRange(500, _auditLog.Count - 500);
        }
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    public class ChangeRoleDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Role { get; set; } = string.Empty;
    }

    public class AdminDashboardDto
    {
        public int TotalUsers         { get; set; }
        public int ActiveUsers        { get; set; }
        public int SuspendedUsers     { get; set; }
        public int MemberCount        { get; set; }
        public int BoardAdminCount    { get; set; }
        public int PlatformAdminCount { get; set; }
        public int TotalWorkspaces    { get; set; }
        public int TotalBoards        { get; set; }
        public int TotalCards         { get; set; }
        public int OverdueCards       { get; set; }
    }

    public class PlatformNotificationDto
    {
        public string Title   { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class AuditLogDto
    {
        public DateTime Timestamp { get; set; }
        public string Action      { get; set; } = string.Empty;
        public string Detail      { get; set; } = string.Empty;
        public string Actor       { get; set; } = string.Empty;
    }

    public class PlatformAnalyticsDto
    {
        public int TotalUsers         { get; set; }
        public int ActiveUsers        { get; set; }
        public int NewUsersThisMonth  { get; set; }
        public int NewUsersThisWeek   { get; set; }
        public Dictionary<string, int> RoleDistribution { get; set; } = new();
        public List<RecentLoginDto> RecentLogins { get; set; } = new();
    }

    public class RecentLoginDto
    {
        public string   Email       { get; set; } = string.Empty;
        public string   Role        { get; set; } = string.Empty;
        public DateTime LastLoginAt { get; set; }
    }

    public class ActivityReportDto
    {
        public DateTime      GeneratedAt    { get; set; }
        public string        GeneratedBy    { get; set; } = string.Empty;
        public int           TotalUsers     { get; set; }
        public int           ActiveUsers    { get; set; }
        public int           SuspendedUsers { get; set; }
        public List<object>  RecentSignups  { get; set; } = new();
        public string        AuditSummary   { get; set; } = string.Empty;
    }
}
