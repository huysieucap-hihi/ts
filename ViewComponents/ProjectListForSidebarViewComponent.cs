using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;

namespace WebsiteQL_Testcase.ViewComponents
{
    public class ProjectListForSidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ProjectListForSidebarViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var projects = await _context.Projects
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    TestSuiteCount = p.TestSuites.Count
                })
                .ToListAsync();

            return View(projects);
        }
    }
}