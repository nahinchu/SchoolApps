using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Models;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _uow;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork uow)
        {
            _logger = logger;
            _uow = uow;
        }

        public IActionResult Index()
        {
            var trending = _uow.Courses.SearchByName(null)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.Enrollments.Count)
                .Take(8)
                .ToList();
            ViewBag.TrendingCourses = trending;
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
    }
}