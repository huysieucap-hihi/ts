using WebsiteQL_Testcase.Models.Enums;

namespace WebsiteQL_Testcase.Models
{
    public class TestExecution
    {
        public Guid Id { get; set; }
        public TestResult Result { get; set; }
        public string? ActualResult { get; set; }
        public string? Note { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Guid TestCaseId { get; set; }
        public string? ExecutedById { get; set; }

        // Navigation properties
        public TestCase TestCase { get; set; } = null!;
        public AppUser? ExecutedBy { get; set; }
    }

}

