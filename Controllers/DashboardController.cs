using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models.Enums;
using WebsiteQL_Testcase.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebsiteQL_Testcase.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardIndexViewModel();

            // 1. TÍNH TOÁN CÁC CON SỐ (Giữ nguyên logic của bạn)
            model.TotalCases = await _context.TestCases.CountAsync();

            var casesWithExecution = await _context.TestCases
                .Include(tc => tc.Executions)
                .Where(tc => tc.Executions.Any())
                .ToListAsync();

            model.PassCount = casesWithExecution.Count(tc => tc.LatestResult == TestResult.Pass);
            model.FailCount = casesWithExecution.Count(tc => tc.LatestResult == TestResult.Fail);

            var executedCount = model.PassCount + model.FailCount;
            model.PassRate = executedCount > 0 ? Math.Round((double)model.PassCount / executedCount * 100, 1) : 0;
            model.FailRate = executedCount > 0 ? Math.Round((double)model.FailCount / executedCount * 100, 1) : 0;

            // 2. LẤY LIST TEST CASE MỚI NHẤT (Đã sửa lại ánh xạ)
            model.LatestTestCases = await _context.TestCases
                .Include(tc => tc.TestSuite)
                .Include(tc => tc.Executions)
                .Where(tc => tc.Executions.Any()) // Chỉ lấy những cái đã chạy
                .OrderByDescending(tc => tc.Executions.Max(e => e.ExecutedAt))
                .Take(5) // Lấy 5 cái mới nhất
                .Select(tc => new LatestTestCaseViewModel
                {
                    Id = tc.Id,
                    Code = tc.Code,

                    // SỬA: Gán Title vào Name
                    Name = tc.Title,

                    // SỬA: Gán TestSuiteName
                    TestSuiteName = tc.TestSuite.Name,

                    // SỬA: Lấy kết quả mới nhất và chuyển thành chuỗi (String) cho Status
                    Status = tc.Executions
                                .OrderByDescending(e => e.ExecutedAt)
                                .FirstOrDefault().Result.ToString()
                })
                .ToListAsync();

            // 3. LẤY LIST PROJECT MỚI NHẤT (Đã sửa lại ánh xạ)
            model.LatestProjects = await _context.Projects
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new LatestProjectViewModel
                {
                    Id = p.Id,
                    Name = p.Name,

                    // SỬA: Gán CreatedAt vào UpdatedDate
                    UpdatedDate = p.CreatedAt
                })
                .ToListAsync();

            return View(model);
        }
    }
}