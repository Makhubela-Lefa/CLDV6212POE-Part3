//Program.cs
using System.Globalization;
using ABCRetailers.Data;
using ABCRetailers.Services;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.EntityFrameworkCore;
namespace ABCRetailers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure HTTP client for Azure Functions API
            //builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();

            builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnection")));

            builder.Services.AddHttpClient("Functions", (sp, client) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var baseUrl = cfg["Functions:BaseUrl"] ?? "http://localhost:7071";
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/api/");
                client.Timeout = TimeSpan.FromSeconds(100);
            });

            // Register the typed client (Functions API)
            builder.Services.AddScoped<ABCRetailers.Services.FunctionsApi.IFunctionsApi, ABCRetailers.Services.FunctionsApi.FunctionsApiClient>();

            // Add logging
            builder.Services.AddLogging();

            var app = builder.Build();

            // Set culture for decimal handling (FIXES PRICE ISSUE)
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseStaticFiles();   
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
