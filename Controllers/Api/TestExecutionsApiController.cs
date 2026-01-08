using Microsoft.AspNetCore.Mvc;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;
using WebsiteQL_Testcase.Models.Enums;

namespace WebsiteQL_Testcase.Controllers.Api
{
    [ApiController]
    [Route("api/executions")]
    public class TestExecutionsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestExecutionsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/executions
        [HttpPost]
        public async Task<IActionResult> Execute(TestExecution model)
        {
            model.Id = Guid.NewGuid();
            model.ExecutedAt = DateTime.UtcNow;

            _context.TestExecutions.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // GET: api/executions/latest
        [HttpGet("latest")]
        public IActionResult Latest()
        {
            var data = _context.TestExecutions
                .OrderByDescending(e => e.ExecutedAt)
                .Take(5)
                .Select(e => new
                {
                    e.TestCase.Code,
                    e.Result,
                    e.ExecutedAt
                })
                .ToList();

            return Ok(data);
        }
    }
}
