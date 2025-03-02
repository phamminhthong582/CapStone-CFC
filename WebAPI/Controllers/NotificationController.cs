using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

public class NotificationController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}