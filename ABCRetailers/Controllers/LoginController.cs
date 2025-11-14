using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABCRetailers.Data;
using ABCRetailers.Models;
using ABCRetailers.Models.FunctionsDtos;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ABCRetailers.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthDbContext _db;
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<LoginController> _logger;

        public LoginController(AuthDbContext db, IFunctionsApi functionsApi, ILogger<LoginController> logger)
        {
            _db = db;
            _functionsApi = functionsApi;
            _logger = logger;
        }

        //==============================================
        // GET: /Login
        //==============================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel
            {
                Username = "",
                Password = ""
            });
        }

        //==============================================
        // POST: /Login
        //==============================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // 1. verify user in SQL
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.PasswordHash == model.Password);
                if (user == null)
                {
                    ViewBag.Error = "Invalid username or password.";
                    return View(model);
                }

                // 2. fetch customer from Azure
                var customers = await _functionsApi.GetCustomersAsync();
                var customer = customers.FirstOrDefault(c => c.Username == user.Username);

                if (customer == null)
                {
                    _logger.LogWarning("No matching customer found in Azure for username {Username}", user.Username);
                    ViewBag.Error = "No customer found in the system.";
                    return View(model);
                }

                // 3. build claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("CustomerId", customer.Id ?? "")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // 4. sign in
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                    });

                // 5. session data
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("CustomerId", customer.Id ?? "");

                // 6. redirect based on role
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return user.Role switch
                {
                    "Admin" => RedirectToAction("AdminHome", "Home"),
                    _ => RedirectToAction("CustomerHome", "Home")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected login error for user {Username}", model.Username);
                ViewBag.Error = "Unexpected error occurred during login. Please try again later.";
                return View(model);
            }
        }

        //==============================================
        // GET: /Login/Register
        //==============================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        //==============================================
        // POST: /Login/Register
        //==============================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = await _db.Users.AnyAsync(u => u.Username == model.Username);
            if (exists)
            {
                ViewBag.Error = "Username already exists.";
                return View(model);
            }

            try
            {
                // 1. save local SQL user
                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = model.Password, // (TODO: hash later)
                    Role = model.Role
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                // 2. save customer in Azure
                var customerDto = new CreateCustomerDto
                {
                    Username = model.Username,
                    Name = model.FirstName,
                    Surname = model.LastName,
                    Email = model.Email,
                    ShippingAddress = model.ShippingAddress
                };

                await _functionsApi.CreateCustomerAsync(customerDto);

                TempData["Success"] = "Registration successful! Please log in.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for user {Username}", model.Username);
                ViewBag.Error = "Could not complete registration. Please try again later.";
                return View(model);
            }
        }

        //==============================================
        // LOGOUT
        //==============================================
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //==============================================
        // ACCESS DENIED 
        //==============================================
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
