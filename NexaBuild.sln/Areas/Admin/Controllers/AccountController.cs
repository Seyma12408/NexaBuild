using Microsoft.AspNetCore.Mvc;

namespace NexaBuild.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        [HttpGet("admin/login")]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            return RedirectToAction("Login", "Account", new { area = "", returnUrl = "/Admin" });
        }
    }
}
