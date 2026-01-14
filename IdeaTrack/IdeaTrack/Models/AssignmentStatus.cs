namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// ASSIGNMENT STATUS - Trạng thái Phân công
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Tracks the progress of an individual evaluator's assignment.
    /// 
    /// WORKFLOW:
    /// Assigned → InProgress → Completed
    ///         → Overdue (if past DueDate without completion)
    /// </summary>
    public enum AssignmentStatus
    {
        /// <summary>
        /// Assigned - Assignment created, evaluator notified.
        /// Evaluator has not yet started scoring.
        /// </summary>
        Assigned,
        
        /// <summary>
        /// InProgress - Evaluator has started but not completed.
        /// Some criteria may be scored, but not all.
        /// </summary>
        InProgress,
        
        /// <summary>
        /// Completed - All criteria scored, evaluation submitted.
        /// No further changes allowed unless reopened by Chairman.
        /// </summary>
        Completed,
        
        /// <summary>
        /// Overdue - DueDate has passed without completion.
        /// Triggers alert to OST for follow-up.
        /// </summary>
        Overdue
    }
}
