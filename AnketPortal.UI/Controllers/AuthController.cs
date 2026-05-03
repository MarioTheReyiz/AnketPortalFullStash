using Microsoft.AspNetCore.Mvc;

namespace AnketPortal.UI.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        // İŞTE EKSİK OLAN KISIM BURASIYDI! BUNU EKLE:
        public IActionResult Register()
        {
            return View();
        }
    }
}