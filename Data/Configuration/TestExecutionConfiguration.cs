using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsiteQL_Testcase.Models;

namespace WebsiteQL_Testcase.Data.Configurations
{
    public class TestExecutionConfiguration : IEntityTypeConfiguration<TestExecution>
    {
        public void Configure(EntityTypeBuilder<TestExecution> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Result)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ActualResult)
                .HasMaxLength(2000);

            builder.Property(x => x.Note)
                .HasMaxLength(2000);

            builder.Property(x => x.ExecutedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(x => x.ExecutedBy)
                .WithMany(x => x.Executions)
                .HasForeignKey(x => x.ExecutedById)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.TestCaseId);
            builder.HasIndex(x => x.ExecutedById);
            builder.HasIndex(x => x.ExecutedAt);
            builder.HasIndex(x => x.Result);
        }
    }
}