using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsiteQL_Testcase.Models;

namespace WebsiteQL_Testcase.Data.Configurations
{
    public class TestSuiteConfiguration : IEntityTypeConfiguration<TestSuite>
    {
        public void Configure(EntityTypeBuilder<TestSuite> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.HasMany(x => x.TestCases)
                .WithOne(x => x.TestSuite)
                .HasForeignKey(x => x.TestSuiteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ProjectId);
            builder.HasIndex(x => x.Name);
        }
    }
}