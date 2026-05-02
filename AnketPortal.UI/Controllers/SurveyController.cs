using Microsoft.AspNetCore.Mvc;

namespace AnketPortal.UI.Controllers
{
    public class SurveyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Detail(int id)
        {
            ViewBag.SurveyId = id; // Anket ID'sini sayfaya taşıyoruz
            return View();
        }
    }
}