namespace IdeaTrack.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        // Danh sách nhân sự thuộc khoa
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        // Danh sách các sáng kiến thuộc khoa (để thống kê thi đua)
        public virtual ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
