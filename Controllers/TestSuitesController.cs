using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;

namespace WebsiteQL_Testcase.Controllers
{
    public class TestSuitesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestSuitesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TestSuites?projectId=xxx
        public async Task<IActionResult> Index(Guid projectId)
        {
            var project = await _context.Projects
                .Include(p => p.TestSuites)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound("Project không tồn tại.");
            }

            ViewData["ProjectId"] = projectId;
            ViewData["ProjectName"] = project.Name;

            var testSuites = project.TestSuites
                .OrderBy(s => s.Name)
                .ToList();
            return View(testSuites);
        }
        // GET: TestSuites/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var testSuite = await _context.TestSuites
                .Include(s => s.Project)
                .Include(s => s.TestCases)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (testSuite == null) return NotFound();

            return View(testSuite);
        }
        // GET: TestSuites/Create?projectId=xxx
        public async Task<IActionResult> Create(Guid projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project không tồn tại.");
            }
            ViewData["ProjectName"] = project.Name;

            // Tạo model mới với ProjectId đã có
            var model = new TestSuite
            {
                ProjectId = projectId
            };

            return View(model);
        }
        // POST: TestSuites/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,ProjectId")] TestSuite testSuite)
        {
            // DÒNG QUAN TRỌNG NHẤT: BỎ VALIDATION CHO PROJECT KHI INVALID
            ModelState.Remove("Project"); // Bỏ validation cho navigation property Project
            ModelState.Remove("ProjectId"); // Bỏ validation cho ProjectId nếu cần

            if (ModelState.IsValid)
            {
                testSuite.Id = Guid.NewGuid();
                _context.Add(testSuite);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo Test Suite thành công!";
                return RedirectToAction(nameof(Index), new { projectId = testSuite.ProjectId });
            }

            // Load lại ProjectName khi invalid
            var project = await _context.Projects.FindAsync(testSuite.ProjectId);
            ViewData["ProjectName"] = project?.Name ?? "Không xác định";

            return View(testSuite);
        }

        // GET: TestSuites/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var testSuite = await _context.TestSuites
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (testSuite == null) return NotFound();

            ViewData["ProjectName"] = testSuite.Project.Name;
            return View(testSuite);
        }

        // POST: TestSuites/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TestSuite model)
        {
            Console.WriteLine("EDIT POST HIT");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // LẤY ENTITY ĐANG TRACKING TỪ DB
            var suite = await _context.TestSuites
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (suite == null)
            {
                return NotFound();
            }

            // CẬP NHẬT FIELD CẦN THAY ĐỔI
            suite.Name = model.Name;
            suite.Description = model.Description;

            // LƯU DB
            await _context.SaveChangesAsync(); // 👈 DÒNG QUYẾT ĐỊNH

            TempData["Success"] = "Cập nhật Test Suite thành công!";

            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }


        // GET: TestSuites/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var testSuite = await _context.TestSuites
                .Include(s => s.Project)
                .Include(s => s.TestCases)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (testSuite == null) return NotFound();

            return View(testSuite);
        }

        // POST: TestSuites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var testSuite = await _context.TestSuites.FindAsync(id);
            if (testSuite != null)
            {
                _context.TestSuites.Remove(testSuite);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa Test Suite thành công!";
            }
            return RedirectToAction(nameof(Index), new { projectId = testSuite.ProjectId });
        }

        private bool TestSuiteExists(Guid id)
        {
            return _context.TestSuites.Any(e => e.Id == id);
        }
    }
}