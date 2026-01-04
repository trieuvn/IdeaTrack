namespace IdeaTrack.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        // Danh sach nhan su thuoc khoa
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        // Danh sach cac sang kien thuoc khoa (de thong ke thi dua)
        public virtual ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
