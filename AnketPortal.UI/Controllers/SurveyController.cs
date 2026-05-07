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
        // Sonuçları Grafiklerle Gösterme Sayfası
        public IActionResult Results(int id)
        {
            ViewBag.SurveyId = id; // API'ye istek atarken URL'deki ID'yi kullanmak için ViewBag ile sayfaya yolluyoruz
            return View();
        }// Kullanıcıların anketi çözdüğü sayfa (User Side)
        [HttpGet]
        public IActionResult Solve(int id) // URL'den gelen id (Örn: /Survey/Solve/2)
        {
            if (id == 0) return RedirectToAction("Surveys", "Home");

            ViewBag.SurveyId = id; // BU SATIR ÇOK ÖNEMLİ!
            return View();
        }

    }
}