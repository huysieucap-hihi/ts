using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;
using WebsiteQL_Testcase.Models.Enums;

namespace WebsiteQL_Testcase.Controllers
{
    [Authorize]
    public class TestExecutionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TestExecutionsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TestExecutions/Create?testCaseId=xxx
        public async Task<IActionResult> Create(Guid testCaseId)
        {
            var testCase = await _context.TestCases
                .Include(tc => tc.TestSuite).ThenInclude(s => s.Project)
                .Include(tc => tc.Steps.OrderBy(st => st.StepNumber))
                .FirstOrDefaultAsync(tc => tc.Id == testCaseId);

            if (testCase == null) return NotFound("Test Case không tồn tại.");

            var model = new TestExecution
            {
                TestCaseId = testCaseId,
                TestCase = testCase,
                ExecutedAt = DateTime.UtcNow,
                Result = TestResult.Pending
            };

            // Truyền dữ liệu cho Header
            ViewData["TestCaseCode"] = testCase.Code;
            ViewData["TestCaseTitle"] = testCase.Title;
            ViewData["TestSuiteName"] = testCase.TestSuite.Name;
            ViewData["ProjectName"] = testCase.TestSuite.Project.Name;

            return View(model);
        }

        // POST: TestExecutions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestExecution execution)
        {
            ModelState.Remove("TestCase");
            ModelState.Remove("ExecutedBy");

            if (!ModelState.IsValid)
            {
                var testCase = await _context.TestCases
                    .Include(tc => tc.TestSuite).ThenInclude(s => s.Project)
                    .Include(tc => tc.Steps.OrderBy(st => st.StepNumber))
                    .FirstOrDefaultAsync(tc => tc.Id == execution.TestCaseId);

                if (testCase != null)
                {
                    ViewData["TestCaseCode"] = testCase.Code;
                    ViewData["TestCaseTitle"] = testCase.Title;
                    ViewData["TestSuiteName"] = testCase.TestSuite.Name;
                    ViewData["ProjectName"] = testCase.TestSuite.Project.Name;
                }
                return View("Create", execution);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            execution.ExecutedById = currentUser?.Id;
            execution.ExecutedAt = DateTime.UtcNow;
            execution.Id = Guid.NewGuid();

            _context.Add(execution);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Chạy test thành công! Kết quả: {execution.Result}";
            return RedirectToAction("Details", "TestCases", new { id = execution.TestCaseId });
        }

        // GET: TestExecutions/History?testCaseId=xxx
        public async Task<IActionResult> History(Guid testCaseId)
        {
            // 1. Lấy thông tin Test Case ĐỘC LẬP (để luôn có Header & ID cho nút Quay lại)
            var testCase = await _context.TestCases
                .Include(tc => tc.TestSuite).ThenInclude(s => s.Project)
                .FirstOrDefaultAsync(tc => tc.Id == testCaseId);

            if (testCase == null) return NotFound("Test Case không tồn tại.");

            // [QUAN TRỌNG NHẤT] Truyền ID này sang View để nút "Quay lại" hoạt động
            ViewData["TestCaseId"] = testCase.Id;
            ViewData["TestCaseCode"] = testCase.Code;
            ViewData["TestCaseTitle"] = testCase.Title;
            ViewData["SuiteName"] = testCase.TestSuite.Name;
            ViewData["ProjectName"] = testCase.TestSuite.Project.Name;

            // 2. Lấy danh sách lịch sử
            var executions = await _context.TestExecutions
                .Include(e => e.ExecutedBy)
                .Where(e => e.TestCaseId == testCaseId)
                .OrderByDescending(e => e.ExecutedAt)
                .ToListAsync();

            return View(executions);
        }
    }
}