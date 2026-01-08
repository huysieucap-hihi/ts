using WebsiteQL_Testcase.Models.Enums;

namespace WebsiteQL_Testcase.Models
{
    public class TestCase
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!; // VD: TC_AUTH_001
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TestPriority Priority { get; set; } = TestPriority.Medium;
        public TestStatus Status { get; set; } = TestStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key
        public Guid TestSuiteId { get; set; }

        // Navigation properties
        public TestSuite TestSuite { get; set; } = null!;
        public ICollection<TestStep> Steps { get; set; } = new List<TestStep>();
        public ICollection<TestExecution> Executions { get; set; } = new List<TestExecution>();

        // Computed property - Latest execution result
        public TestResult? LatestResult
        {
            get
            {
                var latest = Executions
                    .OrderByDescending(e => e.ExecutedAt)
                    .FirstOrDefault();
                return latest?.Result;
            }
        }
    }
}