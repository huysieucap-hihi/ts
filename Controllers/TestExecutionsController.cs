using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;
using WebsiteQL_Testcase.Models.Enums;

namespace WebsiteQL_Testcase.Controllers
{
    [Authorize] // Chỉ user đăng nhập mới được chạy test
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
                .Include(tc => tc.TestSuite)
                    .ThenInclude(s => s.Project)
                .Include(tc => tc.Steps.OrderBy(st => st.StepNumber))
                .FirstOrDefaultAsync(tc => tc.Id == testCaseId);

            if (testCase == null)
                return NotFound("Test Case không tồn tại.");

            var model = new TestExecution
            {
                TestCaseId = testCaseId,
                TestCase = testCase,
                ExecutedAt = DateTime.UtcNow,
                Result = TestResult.Pending // mặc định
            };

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
            // BỎ VALIDATION CHO NAVIGATION PROPERTY (TestCase, ExecutedBy)
            ModelState.Remove("TestCase");
            ModelState.Remove("ExecutedBy");

            // Debug: In lỗi ModelState ra Output window
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                System.Diagnostics.Debug.WriteLine("=== EXECUTION CREATE ERRORS ===");
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine($"{error.Key}: {string.Join(", ", error.Value)}");
                }
                System.Diagnostics.Debug.WriteLine("==================================");

                // Load lại TestCase để hiển thị steps và thông tin
                var testCase = await _context.TestCases
                    .Include(tc => tc.TestSuite).ThenInclude(s => s.Project)
                    .Include(tc => tc.Steps.OrderBy(st => st.StepNumber))
                    .FirstOrDefaultAsync(tc => tc.Id == execution.TestCaseId);

                if (testCase == null)
                {
                    return NotFound("Test Case không tồn tại.");
                }

                ViewData["TestCaseCode"] = testCase.Code;
                ViewData["TestCaseTitle"] = testCase.Title;
                ViewData["TestSuiteName"] = testCase.TestSuite.Name;
                ViewData["ProjectName"] = testCase.TestSuite.Project.Name;

                return View("Create", execution);
            }

            // Lưu thành công
            var currentUser = await _userManager.GetUserAsync(User);
            execution.ExecutedById = currentUser?.Id;
            execution.ExecutedAt = DateTime.UtcNow;
            execution.Id = Guid.NewGuid();

            _context.Add(execution);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Chạy test thành công! Kết quả: {execution.Result}";
            return RedirectToAction("Details", "TestCases", new { id = execution.TestCaseId });
        }
        // GET: TestExecutions/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var execution = await _context.TestExecutions
                .Include(e => e.TestCase)
                    .ThenInclude(tc => tc.TestSuite)
                        .ThenInclude(s => s.Project)
                .Include(e => e.TestCase.Steps.OrderBy(st => st.StepNumber))
                .Include(e => e.ExecutedBy)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution == null) return NotFound();

            return View(execution);
        }

        // GET: TestExecutions/History?testCaseId=xxx (lịch sử execution của 1 test case)
        // GET: TestExecutions/History?testCaseId=xxx
        public async Task<IActionResult> History(Guid testCaseId)
        {
            var testCase = await _context.TestCases
                .Include(tc => tc.TestSuite).ThenInclude(s => s.Project)
                .Include(tc => tc.Executions)
                    .ThenInclude(e => e.ExecutedBy)
                .FirstOrDefaultAsync(tc => tc.Id == testCaseId);

            if (testCase == null)
            {
                return NotFound("Test Case không tồn tại.");
            }

            var executions = testCase.Executions.OrderByDescending(e => e.ExecutedAt).ToList();

            // Gắn TestCase vào mỗi execution để View dùng
            foreach (var e in executions)
            {
                e.TestCase = testCase;
            }

            return View(executions);
        }
    }
}