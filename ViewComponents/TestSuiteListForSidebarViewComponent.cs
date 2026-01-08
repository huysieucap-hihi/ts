using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;

namespace WebsiteQL_Testcase.ViewComponents
{
    public class TestSuiteListForSidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public TestSuiteListForSidebarViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var suites = await _context.TestSuites
                .Include(s => s.Project)
                .Include(s => s.TestCases)
                .OrderBy(s => s.Project.Name)
                .ThenBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    ProjectName = s.Project.Name,
                    TestCaseCount = s.TestCases.Count
                })
                .ToListAsync();

            return View(suites);
        }
    }
}