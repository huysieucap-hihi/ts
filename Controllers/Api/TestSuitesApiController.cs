using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;

namespace WebsiteQL_Testcase.Controllers.Api
{
    [ApiController]
    [Route("api/testsuites")]
    public class TestSuitesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestSuitesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/testsuites?projectId=xxx
        [HttpGet]
        public async Task<IActionResult> GetByProject(Guid projectId)
        {
            var data = await _context.TestSuites
                .Where(s => s.ProjectId == projectId)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    TestCaseCount = s.TestCases.Count
                })
                .ToListAsync();

            return Ok(data);
        }

    }
}
