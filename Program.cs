using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using WebsiteQL_Testcase.Data;
using WebsiteQL_Testcase.Models;
using System.Globalization;

namespace WebsiteQL_Testcase
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://0.0.0.0:5000");

            // 1. Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Cấu hình Identity
            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            // --- QUAN TRỌNG: CẤU HÌNH ĐƯỜNG DẪN LOGIN ĐÚNG ---
            // Đoạn này giúp sửa lỗi 404 khi bấm Login
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });
            // -------------------------------------------------

            // Thêm dịch vụ Razor Pages (Cần cho Identity UI)
            builder.Services.AddRazorPages();

            // CONTROLLERS WITH VIEWS + VIEW LOCALIZATION
            builder.Services.AddControllersWithViews()
                .AddViewLocalization();

            var app = builder.Build();

            // 2. Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRequestLocalization();

            app.UseRouting();

            // Middleware xác thực (Identity)
            app.UseAuthentication();
            app.UseAuthorization();

            // Định tuyến cho Controller (MVC)
            app.MapControllerRoute(
              name: "default",
              pattern: "{controller=Dashboard}/{action=Index}/{id?}");

            // Định tuyến cho Razor Pages (Identity UI)
            app.MapRazorPages();

            app.Run();
        }
    }
}