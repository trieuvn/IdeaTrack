using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// BOARD MEMBER - Thành viên Hội đồng
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Junction table linking Users to Boards with role information.
    /// Represents membership of a faculty member in an evaluation board.
    /// 
    /// BUSINESS RULES:
    /// 1. A user can be a member of multiple boards
    /// 2. Each membership has a role (Chairman, Secretary, or Member)
    /// 3. When an initiative is assigned to a board, ALL members of that
    ///    board become evaluators for the initiative
    /// 4. Removing a member from a board prevents them from being assigned
    ///    to new initiatives, but existing assignments remain
    /// 
    /// RELATIONSHIPS:
    /// - Many BoardMembers belong to 1 Board (N:1)
    /// - Many BoardMembers reference 1 User (N:1)
    /// 
    /// ROLES (via BoardRole enum):
    /// - Chairman: Leads the evaluation, has final say in disputes
    /// - Secretary: Records minutes, coordinates logistics
    /// - Member: Regular evaluator with voting rights
    /// </summary>
    public class BoardMember
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Foreign key to the parent Board
        /// </summary>
        public int BoardId { get; set; }
        
        /// <summary>
        /// Foreign key to the User (faculty member)
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Role within this board (Chairman, Secretary, Member)
        /// </summary>
        public BoardRole Role { get; set; }
        
        /// <summary>
        /// Date when this member joined the board
        /// </summary>
        public DateTime JoinDate { get; set; } = DateTime.Now;

        // =====================
        // Navigation Properties
        // =====================
        
        [ForeignKey("BoardId")]
        public virtual Board Board { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
