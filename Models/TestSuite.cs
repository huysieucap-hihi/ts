namespace WebsiteQL_Testcase.Models
{
    public class TestSuite
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        // Foreign key
        public Guid ProjectId { get; set; }

        // Navigation properties
        public Project Project { get; set; } = null!;
        public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    }
}
