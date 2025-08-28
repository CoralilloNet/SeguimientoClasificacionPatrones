using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SeguimientoTareas.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public IActionResult MyTasks()
        {
            return View();
        }
    }
}