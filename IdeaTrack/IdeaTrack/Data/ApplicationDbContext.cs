using IdeaTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet
        public DbSet<Initiative> Initiatives => Set<Initiative>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<ApprovalRecord> ApprovalRecords => Set<ApprovalRecord>();
        public DbSet<Criterion> Criteria => Set<Criterion>();
        public DbSet<ScoreItem> ScoreItems => Set<ScoreItem>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<InitiativeCategory> InitiativeCategories => Set<InitiativeCategory>();
        public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        public DbSet<CommitteeAssignment> CommitteeAssignments => Set<CommitteeAssignment>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ======================================================
            // 1️⃣ APPROVAL RECORD
            // ======================================================
            builder.Entity<ApprovalRecord>(entity =>
            {
                entity.HasOne(ar => ar.Initiative)
                      .WithMany(i => i.ApprovalRecords)
                      .HasForeignKey(ar => ar.InitiativeId)
                      .OnDelete(DeleteBehavior.Cascade); // duy nhất cascade

                entity.HasOne(ar => ar.Approver)
                      .WithMany()
                      .HasForeignKey(ar => ar.ApproverId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => new { a.InitiativeId, a.Level }).IsUnique();
            });

            // ======================================================
            // 2️⃣ ATTACHMENT
            // ======================================================
            builder.Entity<Attachment>(entity =>
            {
                entity.HasOne(a => a.Initiative)
                      .WithMany(i => i.Attachments)
                      .HasForeignKey(a => a.InitiativeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.UploadedBy)
                      .WithMany()
                      .HasForeignKey(a => a.UploadedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================================================
            // 3️⃣ SCORE ITEM (CHI TIẾT CHẤM ĐIỂM)
            // ======================================================
            builder.Entity<ScoreItem>(entity =>
            {
                entity.HasOne(s => s.Initiative)
                      .WithMany(i => i.ScoreItems)
                      .HasForeignKey(s => s.InitiativeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Criterion)
                      .WithMany()
                      .HasForeignKey(s => s.CriterionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Reviewer)
                      .WithMany()
                      .HasForeignKey(s => s.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(s => new { s.InitiativeId, s.CriterionId, s.ReviewerId })
                      .IsUnique();
            });

            // ======================================================
            // 4️⃣ COMMITTEE ASSIGNMENT (PHÂN CÔNG HỘI ĐỒNG)
            // ======================================================
            builder.Entity<CommitteeAssignment>(entity =>
            {
                entity.HasOne(c => c.Initiative)
                      .WithMany(i => i.Assignments)
                      .HasForeignKey(c => c.InitiativeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Reviewer)
                      .WithMany()
                      .HasForeignKey(c => c.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.InitiativeId, c.ReviewerId })
                      .IsUnique();
            });

            // ======================================================
            // 5️⃣ ACTIVITY LOG
            // ======================================================
            builder.Entity<ActivityLog>(entity =>
            {
                entity.HasOne(l => l.User)
                      .WithMany()
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Initiative)
                      .WithMany(i => i.ActivityLogs)
                      .HasForeignKey(l => l.InitiativeId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);
            });

            // ======================================================
            // 6️⃣ INITIATIVE
            // ======================================================
            builder.Entity<Initiative>(entity =>
            {
                entity.HasOne(i => i.Department)
                      .WithMany(d => d.Initiatives)
                      .HasForeignKey(i => i.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Category)
                      .WithMany(c => c.Initiatives)
                      .HasForeignKey(i => i.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Creator)
                      .WithMany(u => u.Initiatives)
                      .HasForeignKey(i => i.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.AcademicYear)
                      .WithMany(a => a.Initiatives)
                      .HasForeignKey(i => i.AcademicYearId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(i => i.Status);
                entity.HasIndex(i => new { i.AcademicYearId, i.CreatorId });
            });

            // ======================================================
            // 7️⃣ ACADEMIC YEAR – FIX FILTERED INDEX
            // ======================================================
            builder.Entity<AcademicYear>(entity =>
            {
                entity.HasIndex(y => y.IsCurrent)
                      .IsUnique()
                      .HasFilter("[IsCurrent] = 1"); // sửa lỗi "incorrect where clause"
            });
        }
    }
}
