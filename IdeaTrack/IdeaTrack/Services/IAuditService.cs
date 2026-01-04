namespace IdeaTrack.Services
{
    /// <summary>
    /// Interface for audit logging service
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Log an action to the audit trail
        /// </summary>
        /// <param name="userId">User performing the action</param>
        /// <param name="action">Action type (e.g., "Create", "Update", "Delete", "Approve")</param>
        /// <param name="targetTable">Table/Entity being affected</param>
        /// <param name="targetId">ID of the affected record</param>
        /// <param name="details">Additional details (optional, can be JSON)</param>
        /// <param name="ipAddress">Client IP address (optional)</param>
        Task LogAsync(int userId, string action, string targetTable, int targetId, string? details = null, string? ipAddress = null);

        /// <summary>
        /// Log an action with automatic user detection from HttpContext
        /// </summary>
        Task LogAsync(string action, string targetTable, int targetId, string? details = null);

        /// <summary>
        /// Get recent audit logs for a specific table/entity
        /// </summary>
        Task<List<AuditLogEntry>> GetLogsAsync(string? targetTable = null, int? targetId = null, int take = 50);
        
        /// <summary>
        /// Get recent audit logs for a user
        /// </summary>
        Task<List<AuditLogEntry>> GetUserLogsAsync(int userId, int take = 50);
    }

    /// <summary>
    /// DTO for audit log entries
    /// </summary>
    public class AuditLogEntry
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Action { get; set; } = "";
        public string TargetTable { get; set; } = "";
        public int TargetId { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = "";
    }
}
