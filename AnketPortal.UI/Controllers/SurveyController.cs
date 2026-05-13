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
            ViewBag.SurveyId = id; 
            return View();
        }

        public IActionResult Results(int id)
        {
            ViewBag.SurveyId = id; 
            return View();
        }
        [HttpGet]
        public IActionResult Solve(int id) 
        {
            if (id == 0) return RedirectToAction("Surveys", "Home");

            ViewBag.SurveyId = id; 
            return View();
        }

    }
}