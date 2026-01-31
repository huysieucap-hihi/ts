using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.DTOs; 
using WebsiteQL_Testcase.Models;

namespace WebsiteQL_Testcase.Controllers.Api
{
    [ApiController]
    [Route("api/users")]
    public class UsersApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersApiController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // ==========================================
        // 1. LẤY DANH SÁCH USER (GET: api/users)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName
                })
                .ToListAsync();

            return Ok(users);
        }

        // ==========================================
        // 2. TẠO USER MỚI (POST: api/users/register)
        // ==========================================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Tạo đối tượng AppUser từ DTO
            var newUser = new AppUser
            {
                UserName = model.Email, // Identity yêu cầu UserName, thường lấy Email luôn
                Email = model.Email,
                FullName = model.FullName
            };

            // Quan trọng: Hàm này sẽ tự động mã hóa mật khẩu
            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                // Trả về thông tin user vừa tạo (nhưng giấu password đi)
                return CreatedAtAction(nameof(GetAllUsers), new
                {
                    id = newUser.Id,
                    email = newUser.Email,
                    fullName = newUser.FullName
                });
            }

            // Nếu lỗi (vd: mật khẩu yếu, trùng email...)
            return BadRequest(result.Errors);
        }

        // ==========================================
        // 3. XÓA USER (DELETE: api/users/{id})
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User không tồn tại");

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded) return NoContent();

            return BadRequest(result.Errors);
        }
    }
}