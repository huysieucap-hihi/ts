namespace WebsiteQL_Testcase.Models
{
    public class TestStep
    {
        public Guid Id { get; set; }
        public int StepNumber { get; set; }
        public string Action { get; set; } = null!;
        public string ExpectedResult { get; set; } = null!;

        // Foreign key
        public Guid TestCaseId { get; set; }

        // Navigation properties
        public TestCase TestCase { get; set; } = null!;
    }
}
