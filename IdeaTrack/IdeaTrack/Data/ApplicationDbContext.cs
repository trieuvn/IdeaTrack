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

        // ========================================
        // NHÓM HỆ THỐNG & NGƯỜI DÙNG
        // ========================================
        public DbSet<Department> Departments { get; set; }
        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }

        // ========================================
        // NHÓM NĂM HỌC & ĐỢT SÁNG KIẾN
        // ========================================
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<InitiativePeriod> InitiativePeriods { get; set; }
        public DbSet<InitiativeCategory> InitiativeCategories { get; set; }
        public DbSet<ReferenceForm> ReferenceForms { get; set; }

        // ========================================
        // NHÓM SÁNG KIẾN & ĐỒNG TÁC GIẢ
        // ========================================
        public DbSet<Initiative> Initiatives { get; set; }
        public DbSet<InitiativeAuthorship> InitiativeAuthorships { get; set; }
        public DbSet<InitiativeFile> InitiativeFiles { get; set; }
        public DbSet<RevisionRequest> RevisionRequests { get; set; }

        // ========================================
        // NHÓM TIÊU CHÍ & CHẤM ĐIỂM
        // ========================================
        public DbSet<EvaluationTemplate> EvaluationTemplates { get; set; }
        public DbSet<EvaluationCriteria> EvaluationCriteria { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }
        public DbSet<InitiativeAssignment> InitiativeAssignments { get; set; }
        public DbSet<EvaluationDetail> EvaluationDetails { get; set; }

        // ========================================
        // NHÓM KẾT QUẢ
        // ========================================
        public DbSet<FinalResult> FinalResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ========================================
            // 1. CẤU HÌNH ĐỘ CHÍNH XÁC CHO DECIMAL
            // ========================================
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // ========================================
            // 2. CẤU HÌNH QUAN HỆ NĂM HỌC -> ĐỢT SÁNG KIẾN
            // ========================================
            builder.Entity<InitiativePeriod>()
                .HasOne(p => p.AcademicYear)
                .WithMany(a => a.Periods)
                .HasForeignKey(p => p.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // 3. CẤU HÌNH QUAN HỆ ĐỢT -> DANH MỤC
            // ========================================
            builder.Entity<InitiativeCategory>()
                .HasOne(c => c.Period)
                .WithMany(p => p.Categories)
                .HasForeignKey(c => c.PeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category -> Board (1-1-1 mechanism, optional)
            builder.Entity<InitiativeCategory>()
                .HasOne(c => c.Board)
                .WithMany()
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.SetNull);

            // Category -> Template (1-1-1 mechanism, optional)
            builder.Entity<InitiativeCategory>()
                .HasOne(c => c.Template)
                .WithMany()
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            // ========================================
            // 4. CẤU HÌNH QUAN HỆ SÁNG KIẾN
            // ========================================
            
            // Initiative -> Creator (User)
            builder.Entity<Initiative>()
                .HasOne(i => i.Creator)
                .WithMany(u => u.MyInitiatives)
                .HasForeignKey(i => i.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Initiative -> Department
            builder.Entity<Initiative>()
                .HasOne(i => i.Department)
                .WithMany(d => d.Initiatives)
                .HasForeignKey(i => i.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Initiative -> Category
            builder.Entity<Initiative>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Initiatives)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Initiative -> Period (optional - null when Draft)
            builder.Entity<Initiative>()
                .HasOne(i => i.Period)
                .WithMany(p => p.Initiatives)
                .HasForeignKey(i => i.PeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // 5. CẤU HÌNH QUAN HỆ ĐỒNG TÁC GIẢ (N-N)
            // ========================================
            builder.Entity<InitiativeAuthorship>()
                .HasOne(a => a.Initiative)
                .WithMany(i => i.Authorships)
                .HasForeignKey(a => a.InitiativeId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa sáng kiến -> xóa authorships

            builder.Entity<InitiativeAuthorship>()
                .HasOne(a => a.Author)
                .WithMany(u => u.Authorships)
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa user nếu còn authorships

            // Unique constraint: Một user chỉ có 1 authorship record cho mỗi initiative
            builder.Entity<InitiativeAuthorship>()
                .HasIndex(a => new { a.InitiativeId, a.AuthorId })
                .IsUnique();

            // ========================================
            // 6. CẤU HÌNH QUAN HỆ BIỂU MẪU
            // ========================================
            builder.Entity<ReferenceForm>()
                .HasOne(r => r.Period)
                .WithMany(p => p.ReferenceForms)
                .HasForeignKey(r => r.PeriodId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa đợt -> xóa biểu mẫu

            // ========================================
            // 7. CẤU HÌNH QUAN HỆ PHÂN CÔNG CHẤM ĐIỂM
            // ========================================
            builder.Entity<InitiativeAssignment>()
                .HasOne(a => a.Member)
                .WithMany(u => u.MyAssignments)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InitiativeAssignment>()
                .HasOne(a => a.Initiative)
                .WithMany(i => i.Assignments)
                .HasForeignKey(a => a.InitiativeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InitiativeAssignment>()
                .HasOne(a => a.Template)
                .WithMany()
                .HasForeignKey(a => a.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InitiativeAssignment>()
                .HasOne(a => a.Board)
                .WithMany(b => b.Assignments)
                .HasForeignKey(a => a.BoardId)
                .OnDelete(DeleteBehavior.SetNull);

            // ========================================
            // 8. CẤU HÌNH QUAN HỆ CHI TIẾT CHẤM ĐIỂM
            // ========================================
            builder.Entity<EvaluationDetail>()
                .HasOne(ed => ed.Assignment)
                .WithMany(a => a.EvaluationDetails)
                .HasForeignKey(ed => ed.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EvaluationDetail>()
                .HasOne(ed => ed.Criteria)
                .WithMany()
                .HasForeignKey(ed => ed.CriteriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // 9. CẤU HÌNH QUAN HỆ YÊU CẦU SỬA ĐỔI
            // ========================================
            builder.Entity<RevisionRequest>()
                .HasOne(r => r.Initiative)
                .WithMany(i => i.RevisionRequests)
                .HasForeignKey(r => r.InitiativeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // 10. CẤU HÌNH QUAN HỆ KẾT QUẢ CUỐI CÙNG (1-1)
            // ========================================
            builder.Entity<FinalResult>()
                .HasOne(r => r.Initiative)
                .WithOne(i => i.FinalResult)
                .HasForeignKey<FinalResult>(r => r.InitiativeId);

            // ========================================
            // 11. CẤU HÌNH QUAN HỆ THÀNH VIÊN HỘI ĐỒNG
            // ========================================
            builder.Entity<BoardMember>()
                .HasOne(bm => bm.User)
                .WithMany(u => u.BoardMemberships)
                .HasForeignKey(bm => bm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BoardMember>()
                .HasOne(bm => bm.Board)
                .WithMany(b => b.Members)
                .HasForeignKey(bm => bm.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========================================
            // 12. CẤU HÌNH UNIQUE INDEX
            // ========================================
            builder.Entity<Initiative>()
                .HasIndex(i => i.InitiativeCode)
                .IsUnique();

            builder.Entity<AcademicYear>()
                .HasIndex(a => a.Name)
                .IsUnique();
        }
    }
}