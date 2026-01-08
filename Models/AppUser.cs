using Microsoft.AspNetCore.Identity;

namespace WebsiteQL_Testcase.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }

        public ICollection<TestExecution> Executions { get; set; } = new List<TestExecution>();
    }
}
