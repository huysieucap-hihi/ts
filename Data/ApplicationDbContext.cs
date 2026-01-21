using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Models;

namespace WebsiteQL_Testcase.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets - CHỈ còn 5 tables chính
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TestSuite> TestSuites => Set<TestSuite>();
        public DbSet<TestCase> TestCases => Set<TestCase>();
        public DbSet<TestStep> TestSteps => Set<TestStep>();
        public DbSet<TestExecution> TestExecutions => Set<TestExecution>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply tất cả configurations từ assembly
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);

            // --- CẤU HÌNH XÓA CASCADE (SỬA LỖI) ---
            // Khi xóa TestCase -> Tự động xóa các TestExecution liên quan
            modelBuilder.Entity<TestExecution>()
                .HasOne(te => te.TestCase)
                .WithMany(tc => tc.Executions) // Đã sửa tên thành Executions cho khớp với Model
                .HasForeignKey(te => te.TestCaseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}