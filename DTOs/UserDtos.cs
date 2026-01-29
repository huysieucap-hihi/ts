namespace WebsiteQL_Testcase.DTOs // Chú ý namespace phải có đuôi .DTOs
{
    // Class dùng để nhận dữ liệu Đăng ký từ App gửi lên
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    // Class dùng để trả dữ liệu User về cho App (đã giấu mật khẩu)
    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}