using Microsoft.AspNetCore.Mvc;

namespace SchoolApp.Controllers
{
    public class GameController : Controller
    {
        // Trang nhúng game cờ vua Unity WebGL: /Game/Chess
        public IActionResult Chess()
        {
            return View();
        }
    }
}
