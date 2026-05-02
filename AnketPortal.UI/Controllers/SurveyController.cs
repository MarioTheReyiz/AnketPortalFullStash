using Microsoft.AspNetCore.Mvc;

namespace AnketPortal.UI.Controllers
{
    public class SurveyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}