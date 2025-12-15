using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
<<<<<<< HEAD
        public string FullName { get; set; }

        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<Initiative> Initiatives { get; set; }
    }

=======
        public string FullName { get; set; } = null!;
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        // Vai trò: 0: Người nộp | 1: Khoa | 2: Hội đồng | 3: Admin
        public int Role { get; set; }

        public ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
>>>>>>> d1c795b103ceec0ecfab0e936d47e204b79df8d4
}
