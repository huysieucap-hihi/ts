using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;
using WebsiteQL_Testcase.Models.Enums;

namespace WebsiteQL_Testcase.Controllers
{
    public class TestCasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestCasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TestCases?testSuiteId=xxx
        public async Task<IActionResult> Index(Guid testSuiteId)
        {
            var testSuite = await _context.TestSuites
                .Include(s => s.Project)
                .Include(s => s.TestCases)
                    .ThenInclude(tc => tc.Executions)
                .FirstOrDefaultAsync(s => s.Id == testSuiteId);

            if (testSuite == null)
                return NotFound("Test Suite không tồn tại.");

            ViewData["TestSuiteId"] = testSuiteId;
            ViewData["TestSuiteName"] = testSuite.Name;
            ViewData["ProjectId"] = testSuite.ProjectId;
            ViewData["ProjectName"] = testSuite.Project.Name;

            var testCases = testSuite.TestCases
                .OrderBy(tc => tc.Code)
                .ToList();

            return View(testCases);
        }

        // GET: TestCases/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var testCase = await _context.TestCases
                .Include(tc => tc.TestSuite)
                    .ThenInclude(s => s.Project)
                .Include(tc => tc.Steps.OrderBy(st => st.StepNumber))
                .Include(tc => tc.Executions.OrderByDescending(e => e.ExecutedAt))
                    .ThenInclude(e => e.ExecutedBy)
                .FirstOrDefaultAsync(tc => tc.Id == id);

            if (testCase == null) return NotFound();

            return View(testCase);
        }

        // GET: TestCases/Create?testSuiteId=xxx
        public async Task<IActionResult> Create(Guid testSuiteId)
        {
            var testSuite = await _context.TestSuites
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == testSuiteId);

            if (testSuite == null) return NotFound();

            ViewData["TestSuiteId"] = testSuiteId;
            ViewData["TestSuiteName"] = testSuite.Name;
            ViewData["ProjectName"] = testSuite.Project.Name;

            // Gợi ý Code tự động (ví dụ: TC_001, TC_002...)
            var count = await _context.TestCases.CountAsync(tc => tc.TestSuiteId == testSuiteId);
            var suggestedCode = $"TC_{testSuite.Name.ToUpper().Replace(" ", "_")}_{(count + 1):000}";

            var model = new TestCase
            {
                TestSuiteId = testSuiteId,
                Code = suggestedCode,
                Priority = TestPriority.Medium,
                Status = TestStatus.Draft
            };

            return View(model);
        }

        // POST: TestCases/Create
        // POST: TestCases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestCase testCase, List<string> Actions, List<string> ExpectedResults)
        {
            // BỎ VALIDATION CHO TestSuite (navigation property) ĐỂ TRÁNH LỖI "The TestSuite field is required"
            ModelState.Remove("TestSuite");
            ModelState.Remove("TestSuiteId"); // Bỏ validation cho TestSuiteId nếu cần

            // Debug: In lỗi ModelState ra Output window
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                System.Diagnostics.Debug.WriteLine("=== TESTCASE CREATE ERRORS ===");
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine($"{error.Key}: {string.Join(", ", error.Value)}");
                }
                System.Diagnostics.Debug.WriteLine("==================================");

                // Load lại thông tin TestSuite để hiển thị
                var suite = await _context.TestSuites
                    .Include(s => s.Project)
                    .FirstOrDefaultAsync(s => s.Id == testCase.TestSuiteId);

                if (suite != null)
                {
                    ViewData["TestSuiteName"] = suite.Name;
                    ViewData["ProjectName"] = suite.Project.Name;
                }
                else
                {
                    return NotFound("Test Suite không tồn tại.");
                }

                return View(testCase);
            }

            // Tạo thành công
            testCase.Id = Guid.NewGuid();
            testCase.CreatedAt = DateTime.UtcNow;

            // Thêm Steps
            for (int i = 0; i < Actions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(Actions[i]))
                {
                    testCase.Steps.Add(new TestStep
                    {
                        Id = Guid.NewGuid(),
                        StepNumber = i + 1,
                        Action = Actions[i].Trim(),
                        ExpectedResult = ExpectedResults[i]?.Trim() ?? ""
                    });
                }
            }

            _context.Add(testCase);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tạo Test Case thành công!";
            return RedirectToAction(nameof(Index), new { testSuiteId = testCase.TestSuiteId });
        }
        // GET: TestCases/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var testCase = await _context.TestCases
                .Include(tc => tc.TestSuite).ThenInclude(s => s.Project)
                .Include(tc => tc.Steps.OrderBy(s => s.StepNumber))
                .FirstOrDefaultAsync(tc => tc.Id == id);

            if (testCase == null) return NotFound();

            ViewData["TestSuiteName"] = testCase.TestSuite.Name;
            ViewData["ProjectName"] = testCase.TestSuite.Project.Name;

            return View(testCase);
        }

        // POST: TestCases/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TestCase postedTestCase, List<Guid> StepIds, List<int> StepNumbers, List<string> Actions, List<string> ExpectedResults)
        {
            if (id != postedTestCase.Id) return NotFound();

            var testCase = await _context.TestCases
                .Include(tc => tc.Steps)
                .FirstOrDefaultAsync(tc => tc.Id == id);

            if (testCase == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin chính
                    testCase.Code = postedTestCase.Code;
                    testCase.Title = postedTestCase.Title;
                    testCase.Description = postedTestCase.Description;
                    testCase.Priority = postedTestCase.Priority;
                    testCase.Status = postedTestCase.Status;
                    testCase.UpdatedAt = DateTime.UtcNow;

                    // Xử lý Steps: xóa cũ, thêm mới/cập nhật
                    _context.TestSteps.RemoveRange(testCase.Steps);

                    for (int i = 0; i < Actions.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(Actions[i]))
                        {
                            testCase.Steps.Add(new TestStep
                            {
                                Id = Guid.NewGuid(),
                                StepNumber = i + 1,
                                Action = Actions[i].Trim(),
                                ExpectedResult = ExpectedResults[i]?.Trim() ?? ""
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật Test Case thành công!";
                }
                catch (DbUpdateException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index), new { testSuiteId = testCase.TestSuiteId });
            }

            return View(postedTestCase);
        }

        // GET: TestCases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var testCase = await _context.TestCases
                .Include(tc => tc.TestSuite).ThenInclude(s => s.Project)
                .Include(tc => tc.Executions)
                .FirstOrDefaultAsync(tc => tc.Id == id);

            if (testCase == null) return NotFound();

            return View(testCase);
        }

        // POST: TestCases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var testCase = await _context.TestCases.FindAsync(id);
            if (testCase != null)
            {
                _context.TestCases.Remove(testCase);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa Test Case thành công!";
            }
            return RedirectToAction(nameof(Index), new { testSuiteId = testCase.TestSuiteId });
        }
    }
}