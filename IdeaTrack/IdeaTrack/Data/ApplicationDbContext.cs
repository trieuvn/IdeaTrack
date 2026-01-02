using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IdeaTrack.Models;

namespace IdeaTrack.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- NHÓM HỆ THỐNG & NGƯỜI DÙNG ---
        public DbSet<Department> Departments { get; set; }
        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }

        // --- NHÓM SÁNG KIẾN ---
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<InitiativeCategory> InitiativeCategories { get; set; }
        public DbSet<Initiative> Initiatives { get; set; }
        public DbSet<InitiativeFile> InitiativeFiles { get; set; }
        public DbSet<RevisionRequest> RevisionRequests { get; set; }

        // --- NHÓM TIÊU CHÍ & CHẤM ĐIỂM ---
        public DbSet<EvaluationTemplate> EvaluationTemplates { get; set; }
        public DbSet<EvaluationCriteria> EvaluationCriteria { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }
        public DbSet<InitiativeAssignment> InitiativeAssignments { get; set; }
        public DbSet<EvaluationDetail> EvaluationDetails { get; set; }

        // --- NHÓM KẾT QUẢ ---
        public DbSet<FinalResult> FinalResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Khai báo độ chính xác cho kiểu Decimal (Bắt buộc trong EF Core)
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // 2. Xử lý lỗi Multiple Cascade Paths (Chặn xóa dây chuyền gây vòng lặp)

            // Initiative -> Department
            builder.Entity<Initiative>()
                .HasOne(i => i.Department)
                .WithMany(d => d.Initiatives)
                .HasForeignKey(i => i.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Initiative -> Proposer (User)
            builder.Entity<Initiative>()
                .HasOne(i => i.Proposer)
                .WithMany(u => u.MyInitiatives)
                .HasForeignKey(i => i.ProposerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Initiative -> AcademicYear
            builder.Entity<Initiative>()
                .HasOne(i => i.AcademicYear)
                .WithMany(a => a.Initiatives)
                .HasForeignKey(i => i.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            // InitiativeAssignment -> Member (User)
            builder.Entity<InitiativeAssignment>()
                .HasOne(a => a.Member)
                .WithMany(u => u.MyAssignments)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Cấu hình Quan hệ 1-1 cho Kết quả cuối cùng
            builder.Entity<FinalResult>()
                .HasOne(r => r.Initiative)
                .WithOne(i => i.FinalResult)
                .HasForeignKey<FinalResult>(r => r.InitiativeId);

            // 4. Cấu hình Index Unique
            builder.Entity<Initiative>()
                .HasIndex(i => i.InitiativeCode)
                .IsUnique();

            builder.Entity<AcademicYear>()
                .HasIndex(a => a.Name)
                .IsUnique();

            // 1. Tắt xóa dây chuyền từ Assignment sang EvaluationDetail
            builder.Entity<EvaluationDetail>()
                .HasOne(ed => ed.Assignment)
                .WithMany(a => a.EvaluationDetails)
                .HasForeignKey(ed => ed.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict); // Thay vì Cascade

            // 2. Tắt xóa dây chuyền từ Criteria sang EvaluationDetail
            builder.Entity<EvaluationDetail>()
                .HasOne(ed => ed.Criteria)
                .WithMany()
                .HasForeignKey(ed => ed.CriteriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Bạn cũng nên làm tương tự với RevisionRequest nếu nó gây lỗi tương tự
            builder.Entity<RevisionRequest>()
                .HasOne(r => r.Initiative)
                .WithMany(i => i.RevisionRequests)
                .HasForeignKey(r => r.InitiativeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}