using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;

namespace WebsiteQL_Testcase.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.TestSuites)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.TestSuites)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null) return NotFound();

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Id = Guid.NewGuid();
                project.CreatedAt = DateTime.UtcNow;
                _context.Add(project);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo project thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description,CreatedAt")] Project project)
        {
            if (id != project.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    project.UpdatedAt = DateTime.UtcNow;
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                        return NotFound();
                    throw;
                }
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Xóa project thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(Guid id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}