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

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // ĐĂNG KÝ IDENTITY VỚI APPUSER (CUSTOM USER)
            builder.Services.AddDefaultIdentity<AppUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true; // Có thể đổi false nếu không cần confirm email
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // LOCALIZATION (ĐA NGÔN NGỮ)
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("vi-VN"),
                    new CultureInfo("en-US")
                };

                options.DefaultRequestCulture = new RequestCulture("vi-VN");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                };
            });

            // CONTROLLERS WITH VIEWS + VIEW LOCALIZATION
            builder.Services.AddControllersWithViews()
                .AddViewLocalization(); // Cho phép @inject IViewLocalizer trong View

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRequestLocalization(); // Phải đặt trước UseRouting

            app.UseRouting();

            // THÊM DÒNG NÀY NẾU CHƯA CÓ (QUAN TRỌNG CHO IDENTITY)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
              name: "default",
              pattern: "{controller=Dashboard}/{action=Index}/{id?}");

          

            app.MapRazorPages(); // Cần cho các trang Identity (Login, Register...)

            app.Run();
        }
    }
}