using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Sang kien - Thuc the trung tam cua he thong
    /// </summary>
    public class Initiative
    {
        public int Id { get; set; }
        
        [Required]
        public string InitiativeCode { get; set; } = ""; // Ma sang kien duy nhat
        
        [Required]
        public string Title { get; set; } = "";
        
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000000, ErrorMessage = "Budget must be a valid positive number")]
        public decimal Budget { get; set; }
        
        public InitiativeStatus Status { get; set; } = InitiativeStatus.Draft;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SubmittedDate { get; set; }
        
        // ===== HO TRO CHAM DIEM DA VONG =====
        /// <summary>
        /// Vong cham hien tai (mac dinh la 1)
        /// Tang len khi PKHCN yeu cau cham lai
        /// </summary>
        public int CurrentRound { get; set; } = 1;
        
        // ===== QUAN HE VOI NGUOI TAO (Creator) =====
        /// <summary>
        /// Nguoi tao ban dau tien (primary author)
        /// Duoc giu lai de tuong thich nguoc
        /// </summary>
        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public virtual ApplicationUser Creator { get; set; } = null!;
        
        // ===== QUAN HE VOI PHONG BAN/KHOA =====
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;
        
        // ===== QUAN HE VOI DANH MUC =====
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual InitiativeCategory Category { get; set; } = null!;
        
        // ===== QUAN HE VOI DOT SANG KIEN =====
        /// <summary>
        /// Dot sang kien ma bai nay duoc nop vao
        /// Null khi con o trang thai Draft
        /// </summary>
        public int? PeriodId { get; set; }
        [ForeignKey("PeriodId")]
        public virtual InitiativePeriod? Period { get; set; }
        
        // ===== KET QUA CUOI CUNG =====
        public virtual FinalResult? FinalResult { get; set; }
        
        // ===== NAVIGATION COLLECTIONS =====
        
        /// <summary>
        /// Danh sach dong tac gia (N-N relationship)
        /// </summary>
        public virtual ICollection<InitiativeAuthorship> Authorships { get; set; } = new List<InitiativeAuthorship>();
        
        public virtual ICollection<InitiativeFile> Files { get; set; } = new List<InitiativeFile>();
        public virtual ICollection<InitiativeAssignment> Assignments { get; set; } = new List<InitiativeAssignment>();
        public virtual ICollection<RevisionRequest> RevisionRequests { get; set; } = new List<RevisionRequest>();
    }
}
