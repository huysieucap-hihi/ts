using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;
using WebsiteQL_Testcase.Models.Enums; // Đảm bảo đã có namespace này

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
        public async Task<IActionResult> Index(Guid testSuiteId, string searchString, string status, string priority)
        {
            // ====================================================
            // BƯỚC 1: TRUY VẤN CƠ BẢN & LỌC NHỮNG CỘT CÓ THẬT TRONG DB
            // ====================================================
            var testCasesQuery = _context.TestCases
                .Include(tc => tc.TestSuite)
                // [Quan trọng] Phải Include Executions để tí nữa C# tính toán được LatestResult
                .Include(tc => tc.Executions)
                .Where(tc => tc.TestSuiteId == testSuiteId)
                .AsQueryable();

            // Lấy thông tin Header (giữ nguyên)
            var testSuite = await _context.TestSuites
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == testSuiteId);

            if (testSuite == null) return NotFound("Test Suite không tồn tại.");

            ViewData["TestSuiteId"] = testSuiteId;
            ViewData["TestSuiteName"] = testSuite.Name;
            ViewData["ProjectId"] = testSuite.ProjectId;
            ViewData["ProjectName"] = testSuite.Project?.Name;

            // Lọc tìm kiếm (Database làm được)
            if (!string.IsNullOrEmpty(searchString))
            {
                testCasesQuery = testCasesQuery.Where(s => s.Title.Contains(searchString) || s.Code.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            // Lọc Priority (Database làm được - vì Priority là cột thật)
            if (!string.IsNullOrEmpty(priority))
            {
                if (Enum.TryParse<TestPriority>(priority, out var priorityEnum))
                {
                    testCasesQuery = testCasesQuery.Where(t => t.Priority == priorityEnum);
                }
                ViewData["CurrentPriority"] = priority;
            }

            // ====================================================
            // BƯỚC 2: LẤY DỮ LIỆU VỀ RAM (Để tránh lỗi SQL không hiểu LatestResult)
            // ====================================================
            var testCases = await testCasesQuery
                .OrderByDescending(tc => tc.CreatedAt)
                .ToListAsync(); // <--- Lệnh này sẽ ngắt kết nối DB và tải dữ liệu về

            // ====================================================
            // BƯỚC 3: LỌC STATUS (LATEST RESULT) BẰNG C#
            // ====================================================
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Draft")
                {
                    // Lọc trên RAM: Những cái chưa chạy lần nào
                    testCases = testCases.Where(t => t.LatestResult == null).ToList();
                }
                else if (Enum.TryParse<TestResult>(status, out var resultEnum))
                {
                    // Lọc trên RAM: Những cái có kết quả khớp
                    testCases = testCases.Where(t => t.LatestResult == resultEnum).ToList();
                }
                ViewData["CurrentStatus"] = status;
            }

            // Trả về danh sách đã lọc
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

            // Gợi ý Code tự động
            var count = await _context.TestCases.CountAsync(tc => tc.TestSuiteId == testSuiteId);
            var safeName = testSuite.Name.ToUpper().Replace(" ", "_");
            // Cắt ngắn tên nếu quá dài để mã không bị xấu
            if (safeName.Length > 10) safeName = safeName.Substring(0, 10);

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
                var suite = await _context.TestSuites
                    .Include(s => s.Project)
                    .FirstOrDefaultAsync(s => s.Id == testCase.TestSuiteId);

                if (suite != null)
                {
                    ViewData["TestSuiteId"] = suite.Id;
                    ViewData["TestSuiteName"] = suite.Name;
                    ViewData["ProjectName"] = suite.Project.Name;
                }
                return View(testCase);
            }

            testCase.Id = Guid.NewGuid();
            testCase.CreatedAt = DateTime.UtcNow;

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

            var testCaseInDb = await _context.TestCases
                .Include(tc => tc.Steps)
                .FirstOrDefaultAsync(tc => tc.Id == id);

            if (testCaseInDb == null) return NotFound();

            ModelState.Remove("TestSuite");

            if (ModelState.IsValid)
            {
                try
                {
                    testCaseInDb.Code = postedTestCase.Code;
                    testCaseInDb.Title = postedTestCase.Title;
                    testCaseInDb.Description = postedTestCase.Description;
                    testCaseInDb.Priority = postedTestCase.Priority;
                    testCaseInDb.Status = postedTestCase.Status;
                    testCaseInDb.UpdatedAt = DateTime.UtcNow;

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
                var suiteId = testCase.TestSuiteId;
                _context.TestCases.Remove(testCase);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa Test Case thành công!";
                return RedirectToAction(nameof(Index), new { testSuiteId = suiteId });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}