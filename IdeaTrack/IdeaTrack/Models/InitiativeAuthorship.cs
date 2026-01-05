using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Bang trung gian quan ly dong tac gia (N-N relationship)
    /// Cho phep nhieu giang vien cung tham gia mot sang kien
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
        /// Danh dau nguoi tao ban dau tien (khong the xoa)
        /// </summary>
        public bool IsCreator { get; set; } = false;
        
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
