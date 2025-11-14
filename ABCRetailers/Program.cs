// Program.cs
using System.Globalization;
using ABCRetailers.Data;
using ABCRetailers.Services;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

namespace ABCRetailers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================
            // 10 MVC + Accessor
            // ============================================
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            // ============================================
            // 20 EF Core: Identity/Auth Database
            // ============================================
            var authConn = builder.Configuration.GetConnectionString("AuthDbConnection");
            builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(authConn));

            // ============================================
            // 30 Azure Functions API Client
            // ============================================
            builder.Services.AddHttpClient("Functions", (sp, client) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var baseUrl = cfg["Functions:BaseUrl"] ?? "http://localhost:7071";

                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/api/");
                client.Timeout = TimeSpan.FromSeconds(100);
            });

            builder.Services.AddScoped<IFunctionsApi, FunctionsApiClient>();

            // ============================================
            // 40 Cookie Authentication (Unified Scheme)
            // ============================================
            builder.Services.AddAuthentication("ABCAuth")
                .AddCookie("ABCAuth", options =>
                {
                    options.LoginPath = "/Login/Index";
                    options.AccessDeniedPath = "/Login/AccessDenied";
                    options.LogoutPath = "/Login/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                });

            // ============================================
            // 50 Session Setup
            // ============================================
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "ABCSession";
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // ============================================
            // 60 File Upload Limit
            // ============================================
            builder.Services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
            });

            // ============================================
            // 70 Build Application
            // ============================================
            var app = builder.Build();

            // ============================================
            // 80 Culture Settings
            // ============================================
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // ============================================
            // 90 Middleware Pipeline
            // ============================================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            // ============================================
            // 100 Routes
            // ============================================
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
