namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// BOARD (Evaluation Board / Council) - Hội đồng Đánh giá
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Represents a permanent evaluation committee/council that reviews
    /// and scores initiatives. Boards are reusable across multiple periods.
    /// 
    /// BUSINESS RULES:
    /// 1. Boards are "permanent" entities - they persist across periods
    /// 2. Each board has multiple members (BoardMember) with different roles
    /// 3. Boards are assigned to Categories via the 1-1-1 mechanism
    /// 4. When an initiative is approved by OST, all members of the assigned
    ///    Board automatically become evaluators for that initiative
    /// 
    /// RELATIONSHIPS:
    /// - 1 Board has Many Members (1:N via BoardMember)
    /// - 1 Board is linked to Many Categories (1:N via Category.BoardId)
    /// - 1 Board has Many Assignments (1:N via InitiativeAssignment)
    /// 
    /// EXAMPLE BOARDS:
    /// - "Hội đồng Kỹ thuật" - Technical Innovation Council
    /// - "Hội đồng Kinh tế" - Economics Council
    /// - "Hội đồng Ngôn ngữ" - Language/Linguistics Council
    /// </summary>
    public class Board
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Display name of the board (e.g., "Hội đồng Kỹ thuật 2024")
        /// </summary>
        public string BoardName { get; set; } = "";
        
        /// <summary>
        /// Fiscal/Academic year this board operates in.
        /// Used for filtering and organizing boards by year.
        /// </summary>
        public int FiscalYear { get; set; }
        
        /// <summary>
        /// Optional description of the board's scope and expertise
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Whether this board is currently active and available for assignment.
        /// Inactive boards are hidden from dropdowns but preserved for history.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Record creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // =====================
        // Navigation Properties
        // =====================
        
        /// <summary>
        /// Members of this board (1-to-Many via BoardMember).
        /// Each member has a role (Chairman, Secretary, Member).
        /// </summary>
        public virtual ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
        
        /// <summary>
        /// Initiative assignments for this board (1-to-Many).
        /// Created automatically when OST approves initiatives in linked categories.
        /// </summary>
        public virtual ICollection<InitiativeAssignment> Assignments { get; set; } = new List<InitiativeAssignment>();
    }
}
