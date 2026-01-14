using IdeaTrack.Data;
using IdeaTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    /// <summary>
    /// Controller for viewing audit logs - Admin/SciTech only
    /// </summary>
    [Area("SciTech")]
    [Authorize(Roles = "Admin,SciTech,OST_Admin")]
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditController> _logger;

        public AuditController(
            IAuditService auditService,
            ApplicationDbContext context,
            ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Main audit log viewer - loads all recent logs for client-side DataTables
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Load recent audit logs (limit 500 for performance)
            var logs = await _context.SystemAuditLogs
                .AsNoTracking()
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Take(500)
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

            return View(logs);
        }

        /// <summary>
        /// View details of a specific audit log entry
        /// </summary>
        public async Task<IActionResult> Details(long id)
        {
            var log = await _context.SystemAuditLogs
                .AsNoTracking()
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (log == null)
                return NotFound();

            return View(log);
        }

        /// <summary>
        /// Statistics overview
        /// </summary>
        public async Task<IActionResult> Statistics()
        {
            var stats = new AuditStatistics();

            // Total logs
            stats.TotalLogs = await _context.SystemAuditLogs.CountAsync();

            // Logs today
            var today = DateTime.Today;
            stats.LogsToday = await _context.SystemAuditLogs
                .CountAsync(l => l.Timestamp >= today);

            // Logs this week
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            stats.LogsThisWeek = await _context.SystemAuditLogs
                .CountAsync(l => l.Timestamp >= weekStart);

            // Top actions
            stats.TopActions = await _context.SystemAuditLogs
                .GroupBy(l => l.Action)
                .Select(g => new ActionCount { Action = g.Key, Count = g.Count() })
                .OrderByDescending(a => a.Count)
                .Take(10)
                .ToListAsync();

            // Top tables
            stats.TopTables = await _context.SystemAuditLogs
                .GroupBy(l => l.TargetTable)
                .Select(g => new TableCount { Table = g.Key, Count = g.Count() })
                .OrderByDescending(t => t.Count)
                .Take(10)
                .ToListAsync();

            // Most active users
            stats.TopUsers = await _context.SystemAuditLogs
                .Include(l => l.User)
                .GroupBy(l => new { l.UserId, l.User.FullName })
                .Select(g => new UserActivityCount 
                { 
                    UserId = g.Key.UserId, 
                    UserName = g.Key.FullName, 
                    Count = g.Count() 
                })
                .OrderByDescending(u => u.Count)
                .Take(10)
                .ToListAsync();

            return View(stats);
        }
    }

    /// <summary>
    /// Statistics ViewModel
    /// </summary>
    public class AuditStatistics
    {
        public int TotalLogs { get; set; }
        public int LogsToday { get; set; }
        public int LogsThisWeek { get; set; }
        public List<ActionCount> TopActions { get; set; } = new();
        public List<TableCount> TopTables { get; set; } = new();
        public List<UserActivityCount> TopUsers { get; set; } = new();
    }

    public class ActionCount
    {
        public string Action { get; set; } = "";
        public int Count { get; set; }
    }

    public class TableCount
    {
        public string Table { get; set; } = "";
        public int Count { get; set; }
    }

    public class UserActivityCount
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int Count { get; set; }
    }
}
