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

            // [QUAN TRỌNG] Truyền đủ ProjectId để nút Quay lại hoạt động
            ViewData["TestSuiteId"] = testSuiteId;
            ViewData["TestSuiteName"] = testSuite.Name;
            ViewData["ProjectId"] = testSuite.ProjectId;
            ViewData["ProjectName"] = testSuite.Project?.Name;

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

            // Gợi ý Code tự động (ví dụ: TC_001...)
            var count = await _context.TestCases.CountAsync(tc => tc.TestSuiteId == testSuiteId);
            var safeName = testSuite.Name.ToUpper().Replace(" ", "_"); // Xử lý tên suite để tạo mã đẹp hơn
            var suggestedCode = $"TC_{safeName}_{(count + 1):000}";

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestCase testCase, List<string> Actions, List<string> ExpectedResults)
        {
            ModelState.Remove("TestSuite");

            if (!ModelState.IsValid)
            {
                // [FIX LỖI 1] Load lại thông tin hiển thị nếu nhập lỗi
                var suite = await _context.TestSuites
                    .Include(s => s.Project)
                    .FirstOrDefaultAsync(s => s.Id == testCase.TestSuiteId);

                if (suite != null)
                {
                    ViewData["TestSuiteId"] = suite.Id; // Cần cái này cho nút Quay lại
                    ViewData["TestSuiteName"] = suite.Name;
                    ViewData["ProjectName"] = suite.Project.Name;
                }
                return View(testCase);
            }

            // Tạo thành công
            testCase.Id = Guid.NewGuid();
            testCase.CreatedAt = DateTime.UtcNow;

            // Thêm Steps
            if (Actions != null)
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(Actions[i]))
                    {
                        testCase.Steps.Add(new TestStep
                        {
                            Id = Guid.NewGuid(),
                            StepNumber = i + 1,
                            Action = Actions[i].Trim(),
                            ExpectedResult = ExpectedResults != null && i < ExpectedResults.Count ? (ExpectedResults[i]?.Trim() ?? "") : ""
                        });
                    }
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
        public async Task<IActionResult> Edit(Guid id, TestCase postedTestCase, List<string> Actions, List<string> ExpectedResults)
        {
            if (id != postedTestCase.Id) return NotFound();

            // Lấy TestCase gốc từ DB (bao gồm cả Steps để xử lý xóa/thêm)
            var testCaseInDb = await _context.TestCases
                .Include(tc => tc.Steps)
                .FirstOrDefaultAsync(tc => tc.Id == id);

            if (testCaseInDb == null) return NotFound();

            // Validate thủ công một chút để bỏ qua các lỗi không cần thiết
            ModelState.Remove("TestSuite");

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin chính
                    testCaseInDb.Code = postedTestCase.Code;
                    testCaseInDb.Title = postedTestCase.Title;
                    testCaseInDb.Description = postedTestCase.Description;
                    testCaseInDb.Priority = postedTestCase.Priority;
                    testCaseInDb.Status = postedTestCase.Status;
                    testCaseInDb.UpdatedAt = DateTime.UtcNow;

                    // Xử lý Steps: Xóa hết cũ, thêm mới (Cách đơn giản nhất)
                    _context.TestSteps.RemoveRange(testCaseInDb.Steps);

                    if (Actions != null)
                    {
                        for (int i = 0; i < Actions.Count; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(Actions[i]))
                            {
                                testCaseInDb.Steps.Add(new TestStep
                                {
                                    Id = Guid.NewGuid(),
                                    StepNumber = i + 1,
                                    Action = Actions[i].Trim(),
                                    ExpectedResult = ExpectedResults != null && i < ExpectedResults.Count ? (ExpectedResults[i]?.Trim() ?? "") : ""
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật Test Case thành công!";
                }
                catch (DbUpdateException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index), new { testSuiteId = testCaseInDb.TestSuiteId });
            }

            // [FIX LỖI 2] Nếu lỗi Validation, phải load lại tên Project/Suite để hiển thị Header
            var suite = await _context.TestSuites
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == testCaseInDb.TestSuiteId);

            if (suite != null)
            {
                ViewData["TestSuiteName"] = suite.Name;
                ViewData["ProjectName"] = suite.Project.Name;
            }

            return View(postedTestCase);
        }

        // GET: TestCases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var testCase = await _context.TestCases
                .Include(tc => tc.TestSuite).ThenInclude(s => s.Project)
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
                var suiteId = testCase.TestSuiteId; // Lưu lại ID để redirect
                _context.TestCases.Remove(testCase);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa Test Case thành công!";
                return RedirectToAction(nameof(Index), new { testSuiteId = suiteId });
            }
            return RedirectToAction("Index", "Home"); // Fallback
        }
    }
}