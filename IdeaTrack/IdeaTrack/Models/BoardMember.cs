using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class BoardMember
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public int UserId { get; set; }
        public BoardRole Role { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [ForeignKey("BoardId")]
        public virtual Board Board { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
