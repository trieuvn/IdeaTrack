using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Sáng kiến - Thực thể trung tâm của hệ thống
    /// </summary>
    public class Initiative
    {
        public int Id { get; set; }
        
        [Required]
        public string InitiativeCode { get; set; } = ""; // Mã sáng kiến duy nhất
        
        [Required]
        public string Title { get; set; } = "";
        
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }
        
        public InitiativeStatus Status { get; set; } = InitiativeStatus.Draft;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SubmittedDate { get; set; }
        
        // ===== HỖ TRỢ CHẤM ĐIỂM ĐA VÒNG =====
        /// <summary>
        /// Vòng chấm hiện tại (mặc định là 1)
        /// Tăng lên khi PKHCN yêu cầu chấm lại
        /// </summary>
        public int CurrentRound { get; set; } = 1;
        
        // ===== QUAN HỆ VỚI NGƯỜI TẠO (Creator) =====
        /// <summary>
        /// Người tạo bản đầu tiên (primary author)
        /// Được giữ lại để tương thích ngược
        /// </summary>
        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public virtual ApplicationUser Creator { get; set; } = null!;
        
        // ===== QUAN HỆ VỚI PHÒNG BAN/KHOA =====
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;
        
        // ===== QUAN HỆ VỚI DANH MỤC =====
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual InitiativeCategory Category { get; set; } = null!;
        
        // ===== QUAN HỆ VỚI ĐỢT SÁNG KIẾN =====
        /// <summary>
        /// Đợt sáng kiến mà bài này được nộp vào
        /// Null khi còn ở trạng thái Draft
        /// </summary>
        public int? PeriodId { get; set; }
        [ForeignKey("PeriodId")]
        public virtual InitiativePeriod? Period { get; set; }
        
        // ===== KẾT QUẢ CUỐI CÙNG =====
        public virtual FinalResult? FinalResult { get; set; }
        
        // ===== NAVIGATION COLLECTIONS =====
        
        /// <summary>
        /// Danh sách đồng tác giả (N-N relationship)
        /// </summary>
        public virtual ICollection<InitiativeAuthorship> Authorships { get; set; } = new List<InitiativeAuthorship>();
        
        public virtual ICollection<InitiativeFile> Files { get; set; } = new List<InitiativeFile>();
        public virtual ICollection<InitiativeAssignment> Assignments { get; set; } = new List<InitiativeAssignment>();
        public virtual ICollection<RevisionRequest> RevisionRequests { get; set; } = new List<RevisionRequest>();
    }
}
