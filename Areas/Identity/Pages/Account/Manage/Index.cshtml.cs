using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebsiteQL_Testcase.Models; // Đảm bảo dòng này trỏ đúng namespace chứa class AppUser của bạn

namespace WebsiteQL_Testcase.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public IndexModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        // BIẾN MỚI: Dùng để chứa danh sách Role hiển thị ra View
        public IList<string> UserRoles { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            // BIẾN MỚI: Thêm FullName vào Input để cho phép sửa
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }
        }

        private async Task LoadAsync(AppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            // LẤY DANH SÁCH ROLE CỦA USER
            UserRoles = await _userManager.GetRolesAsync(user);

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                // LẤY FULLNAME TỪ DATABASE ĐỔ VÀO Ô INPUT
                FullName = user.FullName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // 1. XỬ LÝ LƯU SỐ ĐIỆN THOẠI (Logic mặc định)
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // 2. XỬ LÝ LƯU FULL NAME (Logic thêm mới)
            // Nếu tên nhập vào khác tên trong DB thì mới lưu
            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Lỗi khi cập nhật Họ tên.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Hồ sơ của bạn đã được cập nhật thành công.";
            return RedirectToPage();
        }
    }
}