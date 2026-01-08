using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsiteQL_Testcase.Models;

namespace WebsiteQL_Testcase.Data.Configurations
{
    public class TestCaseConfiguration : IEntityTypeConfiguration<TestCase>
    {
        public void Configure(EntityTypeBuilder<TestCase> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.Priority)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasMany(x => x.Steps)
                .WithOne(x => x.TestCase)
                .HasForeignKey(x => x.TestCaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Executions)
                .WithOne(x => x.TestCase)
                .HasForeignKey(x => x.TestCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => x.TestSuiteId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.Priority);
            builder.HasIndex(x => x.CreatedAt);

            builder.Ignore(x => x.LatestResult);
        }
    }
}