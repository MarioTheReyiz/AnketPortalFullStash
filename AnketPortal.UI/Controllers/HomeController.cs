using AnketPortal.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AnketPortal.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult SystemLogs()
        {
            return View();
        }

        // Adminlerin ve SuperAdminlerin göreceði özel Dashboard sayfasý
        public IActionResult Dashboard()
        {
            return View();
        }
        // Kullanýcýlarýn tüm anketleri görebileceði sayfa
        public IActionResult Surveys()
        {
            return View();
        }
        public IActionResult Users() { return View(); }
    }
}
