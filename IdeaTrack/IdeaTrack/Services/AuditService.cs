using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdeaTrack.Services
{
    /// <summary>
    /// Implementation of audit logging service
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task LogAsync(int userId, string action, string targetTable, int targetId, string? details = null, string? ipAddress = null)
        {
            try
            {
                var log = new SystemAuditLog
                {
                    UserId = userId,
                    Action = action,
                    TargetTable = targetTable,
                    TargetId = targetId,
                    Details = details,
                    Timestamp = DateTime.Now,
                    IpAddress = ipAddress ?? GetClientIpAddress()
                };

                _context.SystemAuditLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Audit log: User {UserId} performed {Action} on {Table}:{Id}",
                    userId, action, targetTable, targetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log for {Action} on {Table}:{Id}",
                    action, targetTable, targetId);
                // Don't throw - audit logging should not break the main flow
            }
        }

        public async Task LogAsync(string action, string targetTable, int targetId, string? details = null)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId.HasValue)
            {
                await LogAsync(userId.Value, action, targetTable, targetId, details);
            }
            else
            {
                _logger.LogWarning("Attempted to log audit without authenticated user: {Action} on {Table}:{Id}",
                    action, targetTable, targetId);
            }
        }

        public async Task<List<AuditLogEntry>> GetLogsAsync(string? targetTable = null, int? targetId = null, int take = 50)
        {
            var query = _context.SystemAuditLogs
                .AsNoTracking()
                .Include(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(targetTable))
            {
                query = query.Where(l => l.TargetTable == targetTable);
            }

            if (targetId.HasValue)
            {
                query = query.Where(l => l.TargetId == targetId.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .Take(take)
                .Select(l => new AuditLogEntry
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User != null ? l.User.FullName : "Unknown",
                    Action = l.Action,
                    TargetTable = l.TargetTable,
                    TargetId = l.TargetId,
                    Details = l.Details,
                    Timestamp = l.Timestamp,
                    IpAddress = l.IpAddress
                })
                .ToListAsync();
        }

        public async Task<List<AuditLogEntry>> GetUserLogsAsync(int userId, int take = 50)
        {
            return await _context.SystemAuditLogs
                .AsNoTracking()
                .Include(l => l.User)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .Take(take)
                .Select(l => new AuditLogEntry
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User != null ? l.User.FullName : "Unknown",
                    Action = l.Action,
                    TargetTable = l.TargetTable,
                    TargetId = l.TargetId,
                    Details = l.Details,
                    Timestamp = l.Timestamp,
                    IpAddress = l.IpAddress
                })
                .ToListAsync();
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(httpContext.User);
                return user?.Id;
            }
            return null;
        }

        private string GetClientIpAddress()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return "Unknown";

            // Check for forwarded IP (reverse proxy)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
