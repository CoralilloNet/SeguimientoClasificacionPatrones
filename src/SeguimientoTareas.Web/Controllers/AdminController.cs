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

        public IActionResult Specialists()
        {
            return View();
        }

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