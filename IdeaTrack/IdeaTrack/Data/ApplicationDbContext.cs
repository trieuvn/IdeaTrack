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
        // NHOM HE THONG & NGUOI DUNG
        // ========================================
        public DbSet<Department> Departments { get; set; }
        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }

        // ========================================
        // NHOM NAM HOC & DOT SANG KIEN
        // ========================================
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<InitiativePeriod> InitiativePeriods { get; set; }
        public DbSet<InitiativeCategory> InitiativeCategories { get; set; }
        public DbSet<ReferenceForm> ReferenceForms { get; set; }

        // ========================================
        // NHOM SANG KIEN & DONG TAC GIA
        // ========================================
        public DbSet<Initiative> Initiatives { get; set; }
        public DbSet<InitiativeAuthorship> InitiativeAuthorships { get; set; }
        public DbSet<InitiativeFile> InitiativeFiles { get; set; }
        public DbSet<RevisionRequest> RevisionRequests { get; set; }

        // ========================================
        // NHOM TIEU CHI & CHAM DIEM
        // ========================================
        public DbSet<EvaluationTemplate> EvaluationTemplates { get; set; }
        public DbSet<EvaluationCriteria> EvaluationCriteria { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }
        public DbSet<InitiativeAssignment> InitiativeAssignments { get; set; }
        public DbSet<EvaluationDetail> EvaluationDetails { get; set; }

        // ========================================
        // NHOM KET QUA
        // ========================================
        public DbSet<FinalResult> FinalResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ========================================
            // 1. CAU HINH DO CHINH XAC CHO DECIMAL
            // ========================================
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // ========================================
            // 2. CAU HINH QUAN HE NAM HOC -> DOT SANG KIEN
            // ========================================
            builder.Entity<InitiativePeriod>()
                .HasOne(p => p.AcademicYear)
                .WithMany(a => a.Periods)
                .HasForeignKey(p => p.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // 3. CAU HINH QUAN HE DOT -> DANH MUC
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
            // 4. CAU HINH QUAN HE SANG KIEN
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
            // 5. CAU HINH QUAN HE DONG TAC GIA (N-N)
            // ========================================
            builder.Entity<InitiativeAuthorship>()
                .HasOne(a => a.Initiative)
                .WithMany(i => i.Authorships)
                .HasForeignKey(a => a.InitiativeId)
                .OnDelete(DeleteBehavior.Cascade); // Delete sang kien -> xoa authorships

            builder.Entity<InitiativeAuthorship>()
                .HasOne(a => a.Author)
                .WithMany(u => u.Authorships)
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Khong xoa user neu con authorships

            // Unique constraint: Mot user chi co 1 authorship record cho moi initiative
            builder.Entity<InitiativeAuthorship>()
                .HasIndex(a => new { a.InitiativeId, a.AuthorId })
                .IsUnique();

            // ========================================
            // 6. CAU HINH QUAN HE BIEU MAU
            // ========================================
            builder.Entity<ReferenceForm>()
                .HasOne(r => r.Period)
                .WithMany(p => p.ReferenceForms)
                .HasForeignKey(r => r.PeriodId)
                .OnDelete(DeleteBehavior.Cascade); // Delete dot -> xoa bieu mau

            // ========================================
            // 7. CAU HINH QUAN HE PHAN CONG CHAM DIEM
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
            // 8. CAU HINH QUAN HE CHI TIET CHAM DIEM
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
            // 9. CAU HINH QUAN HE YEU CAU SUA DOI
            // ========================================
            builder.Entity<RevisionRequest>()
                .HasOne(r => r.Initiative)
                .WithMany(i => i.RevisionRequests)
                .HasForeignKey(r => r.InitiativeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // 10. CAU HINH QUAN HE KET QUA CUOI CUNG (1-1)
            // ========================================
            builder.Entity<FinalResult>()
                .HasOne(r => r.Initiative)
                .WithOne(i => i.FinalResult)
                .HasForeignKey<FinalResult>(r => r.InitiativeId);

            // ========================================
            // 11. CAU HINH QUAN HE THANH VIEN HOI DONG
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
            // 12. CAU HINH UNIQUE INDEX
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