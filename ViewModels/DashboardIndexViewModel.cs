// ViewModels/DashboardViewModel.cs
using WebsiteQL_Testcase.Models.Enums;

namespace WebsiteQL_Testcase.ViewModels
{
    public class DashboardIndexViewModel
    {
        public int TotalCases { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public double PassRate { get; set; }
        public double FailRate { get; set; }
        public List<LatestTestCaseViewModel> LatestTestCases { get; set; } = new();
        public List<LatestProjectViewModel> LatestProjects { get; set; } = new();
    }

    public class LatestTestCaseViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string TestSuiteName { get; set; } = null!;
        public TestResult? LatestResult { get; set; }
    }

    public class LatestProjectViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}