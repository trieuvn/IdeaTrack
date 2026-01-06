namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// BOARD ROLE - Vai trò trong Hội đồng
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Defines roles a member can have within an evaluation board.
    /// Each role has different responsibilities and permissions.
    /// 
    /// ROLE RESPONSIBILITIES:
    /// - Chairman: Leads discussions, makes final decisions, can adjust scores
    /// - Secretary: Records minutes, manages logistics, compiles reports
    /// - Member: Scores initiatives, participates in discussions
    /// </summary>
    public enum BoardRole
    {
        /// <summary>
        /// Chairman (Chủ tịch) - Board leader with final decision authority.
        /// Can override scores, make final Approve/Reject decisions.
        /// Each board should have exactly 1 Chairman.
        /// </summary>
        Chairman,
        
        /// <summary>
        /// Secretary (Thư ký) - Administrative role.
        /// Records meeting minutes, coordinates schedules, compiles reports.
        /// Each board should have exactly 1 Secretary.
        /// </summary>
        Secretary,
        
        /// <summary>
        /// Member (Ủy viên/Phản biện) - Regular voting member.
        /// Scores initiatives based on assigned criteria.
        /// Board can have multiple Members.
        /// </summary>
        Member
    }
}
