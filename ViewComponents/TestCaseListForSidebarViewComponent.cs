using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;

namespace WebsiteQL_Testcase.ViewComponents
{
    public class TestCaseListForSidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public TestCaseListForSidebarViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var testCases = await _context.TestCases
                .Include(tc => tc.TestSuite)
                    .ThenInclude(s => s.Project)
                .Include(tc => tc.Executions)
                .Where(tc => tc.Executions.Any()) // Chỉ hiện Test Case đã chạy ít nhất 1 lần
                .OrderByDescending(tc => tc.Executions.Max(e => e.ExecutedAt))
                .Take(20) // Giới hạn 20 để không dài quá
                .Select(tc => new
                {
                    tc.Id,
                    tc.Code,
                    tc.Title,
                    ProjectName = tc.TestSuite.Project.Name,
                    SuiteName = tc.TestSuite.Name,
                    LatestResult = tc.LatestResult
                })
                .ToListAsync();

            return View(testCases);
        }
    }
}