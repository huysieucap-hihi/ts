using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;
using WebsiteQL_Testcase.Models.Enums;
using WebsiteQL_Testcase.ViewModels;

namespace WebsiteQL_Testcase.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trong DashboardController.Index
        public async Task<IActionResult> Index()
        {
            var model = new DashboardIndexViewModel();

            // Tổng số test case
            model.TotalCases = await _context.TestCases.CountAsync();

            // Các case có execution
            var casesWithExecution = await _context.TestCases
                .Include(tc => tc.Executions)
                .Where(tc => tc.Executions.Any())
                .ToListAsync();

            model.PassCount = casesWithExecution.Count(tc => tc.LatestResult == TestResult.Pass);
            model.FailCount = casesWithExecution.Count(tc => tc.LatestResult == TestResult.Fail);

            var executedCount = model.PassCount + model.FailCount;
            model.PassRate = executedCount > 0 ? Math.Round((double)model.PassCount / executedCount * 100, 1) : 0;
            model.FailRate = executedCount > 0 ? Math.Round((double)model.FailCount / executedCount * 100, 1) : 0;

            // Latest Test Cases
            model.LatestTestCases = await _context.TestCases
                .Include(tc => tc.TestSuite)
                .Include(tc => tc.Executions)
                .Where(tc => tc.Executions.Any())
                .OrderByDescending(tc => tc.Executions.Max(e => e.ExecutedAt))
                .Take(10)
                .Select(tc => new LatestTestCaseViewModel
                {
                    Id = tc.Id,
                    Code = tc.Code,
                    Title = tc.Title,
                    TestSuiteName = tc.TestSuite.Name,
                    LatestResult = tc.Executions.OrderByDescending(e => e.ExecutedAt).FirstOrDefault().Result
                })
                .ToListAsync();

            // Latest Projects
            model.LatestProjects = await _context.Projects
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new LatestProjectViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return View(model);
        }
    }
}