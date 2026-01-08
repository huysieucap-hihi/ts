using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsiteQL_Testcase.Models;

namespace WebsiteQL_Testcase.Data.Configurations
{
    public class TestStepConfiguration : IEntityTypeConfiguration<TestStep>
    {
        public void Configure(EntityTypeBuilder<TestStep> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StepNumber)
                .IsRequired();

            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.ExpectedResult)
                .IsRequired()
                .HasMaxLength(2000);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_TestStep_StepNumber",
                "[StepNumber] > 0"
            ));

            builder.HasIndex(x => x.TestCaseId);
            builder.HasIndex(x => new { x.TestCaseId, x.StepNumber });
        }
    }
}