using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;

namespace WebsiteQL_Testcase.Controllers.Api
{
    [ApiController]
    [Route("api/testcases")]
    public class TestCasesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestCasesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: api/testcases?testSuiteId=xxx
        // (DTO - KHÔNG CÒN JSON CYCLE)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? testSuiteId)
        {
            var query = _context.TestCases.AsQueryable();

            if (testSuiteId.HasValue)
                query = query.Where(t => t.TestSuiteId == testSuiteId);

            var data = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Code,
                    t.Title,
                    t.Priority,
                    t.Status,
                    t.CreatedAt,
                    TestSuite = new
                    {
                        t.TestSuite.Id,
                        t.TestSuite.Name
                    }
                })
                .ToListAsync();

            return Ok(data);
        }

        // ==========================================
        // GET: api/testcases/{id}
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var testCase = await _context.TestCases
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Code,
                    t.Title,
                    t.Description,
                    t.Priority,
                    t.Status,
                    t.CreatedAt,
                    TestSuite = new
                    {
                        t.TestSuite.Id,
                        t.TestSuite.Name
                    },
                    Steps = t.Steps
                        .OrderBy(s => s.StepNumber)
                        .Select(s => new
                        {
                            s.StepNumber,
                            s.Action,
                            s.ExpectedResult
                        })
                })
                .FirstOrDefaultAsync();

            if (testCase == null)
                return NotFound();

            return Ok(testCase);
        }

        // ==========================================
        // POST: api/testcases
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Create(TestCase model)
        {
            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            _context.TestCases.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        // ==========================================
        // PUT: api/testcases/{id}
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, TestCase model)
        {
            if (id != model.Id)
                return BadRequest();

            model.UpdatedAt = DateTime.UtcNow;
            _context.Entry(model).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================================
        // DELETE: api/testcases/{id}
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var testCase = await _context.TestCases.FindAsync(id);
            if (testCase == null)
                return NotFound();

            _context.TestCases.Remove(testCase);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
