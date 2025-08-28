using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeguimientoTareas.Web.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace SeguimientoTareas.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        // Redirect based on user role
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Dashboard", "Admin");
        }
        else
        {
            return RedirectToAction("MyTasks", "User");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
