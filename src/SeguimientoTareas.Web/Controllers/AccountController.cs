using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SeguimientoTareas.Web.Data;
using SeguimientoTareas.Web.Models;
using SeguimientoTareas.Web.Services;
using System.Security.Claims;

namespace SeguimientoTareas.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IPasswordService _passwordService;

        public AccountController(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If user is already authenticated, redirect to home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return Json(new ApiResponse { Success = false, Message = "Email y contraseña son requeridos" });
                }

                // Get user from database
                const string sql = @"
                    SELECT Id, Email, FullName, Password, IsAdmin, IsActive 
                    FROM Users 
                    WHERE Email = @Email AND IsActive = 1";

                var user = await Db.ExecuteReaderSingleAsync(sql, reader => new User
                {
                    Id = reader.GetInt32(0), // Id
                    Email = reader.GetString(1), // Email 
                    FullName = reader.GetString(2), // FullName
                    Password = reader.GetString(3), // Password
                    IsAdmin = reader.GetBoolean(4), // IsAdmin
                    IsActive = reader.GetBoolean(5) // IsActive
                }, Db.CreateParameter("@Email", request.Email));

                if (user == null)
                {
                    return Json(new ApiResponse { Success = false, Message = "Credenciales inválidas" });
                }

                // Verify password
                if (!_passwordService.VerifyPassword(request.Password, user.Password))
                {
                    return Json(new ApiResponse { Success = false, Message = "Credenciales inválidas" });
                }

                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                if (user.IsAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                return Json(new ApiResponse { Success = true, Message = "Login exitoso" });
            }
            catch (Exception)
            {
                return Json(new ApiResponse { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}