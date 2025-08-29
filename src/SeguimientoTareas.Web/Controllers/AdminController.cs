using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SeguimientoTareas.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        // Users action added for admin user management
        public IActionResult Users()
        {
            return View();
        }

        // Specialists view removed - functionality replaced by Users
        // public IActionResult Specialists()
        // {
        //     return View();
        // }

        public IActionResult Tasks()
        {
            return View();
        }

        public IActionResult Assignments()
        {
            return View();
        }
    }
}