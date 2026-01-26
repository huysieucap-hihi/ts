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
        // Cập nhật để nhận tham số tìm kiếm
        public async Task<IActionResult> Index(string searchString)
        {
            // 1. Lưu từ khóa tìm kiếm vào ViewData để hiển thị lại trên View
            ViewData["CurrentFilter"] = searchString;

            // 2. Khởi tạo truy vấn cơ bản (chưa thực thi)
            var projectsQuery = _context.Projects
                .Include(p => p.TestSuites) // Eager loading nếu cần
                .AsQueryable();

            // 3. Nếu có từ khóa tìm kiếm, thêm điều kiện lọc
            if (!string.IsNullOrEmpty(searchString))
            {
                // Lọc theo Tên Project hoặc Mô tả (không phân biệt hoa thường)
                projectsQuery = projectsQuery.Where(p =>
                    p.Name.Contains(searchString) ||
                    (p.Description != null && p.Description.Contains(searchString)));
            }

            // 4. Sắp xếp: Mới nhất lên đầu
            projectsQuery = projectsQuery.OrderByDescending(p => p.CreatedAt);

            // 5. Thực thi truy vấn và trả về danh sách
            var projects = await projectsQuery.ToListAsync();

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