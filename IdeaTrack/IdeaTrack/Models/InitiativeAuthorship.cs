using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Bảng trung gian quản lý đồng tác giả (N-N relationship)
    /// Cho phép nhiều giảng viên cùng tham gia một sáng kiến
    /// </summary>
    public class InitiativeAuthorship
    {
        public int Id { get; set; }
        
        public int InitiativeId { get; set; }
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; } = null!;
        
        public int AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public virtual ApplicationUser Author { get; set; } = null!;
        
        /// <summary>
        /// Đánh dấu người tạo bản đầu tiên (không thể xóa)
        /// </summary>
        public bool IsCreator { get; set; } = false;
        
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
