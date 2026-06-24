using Microsoft.AspNetCore.Mvc;
using SchoolApp.Models.Enums;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    public class GameController : Controller
    {
        private readonly IUnitOfWork _uow;
        public GameController(IUnitOfWork uow) => _uow = uow;

        // Sảnh chọn game: /Game
        public IActionResult Index()
        {
            return View();
        }

        // Cờ vua Unity WebGL: /Game/Chess
        public IActionResult Chess()
        {
            return View();
        }

        // Lật thẻ ghép cặp: /Game/Matching
        // Lấy TOÀN BỘ câu hỏi đủ điều kiện từ NGÂN HÀNG (không chia theo tag) làm
        // một "kho" cặp. Mỗi cặp: term = đáp án đúng, def = nội dung câu hỏi.
        // Chỉ lấy câu "Một đáp án" có đúng 1 đáp án đúng. Trả kho về 1 lần; client
        // tự random 8 cặp (lưới 4x4) mỗi ván / mỗi lần chơi lại.
        public IActionResult Matching()
        {
            var pool = _uow.Questions.GetBankQuestions()
                .Where(q => q.Type == QuestionType.SingleChoice
                            && q.Options.Count(o => o.IsCorrect) == 1)
                .Select(q => new
                {
                    term = q.Options.First(o => o.IsCorrect).Content,
                    def = q.Content
                })
                // tránh 2 thẻ đáp án trùng nội dung
                .GroupBy(p => p.term)
                .Select(g => g.First())
                .ToList();

            ViewBag.PoolJson = System.Text.Json.JsonSerializer.Serialize(pool);
            return View();
        }

        // Quiz Battle: /Game/QuizBattle (sắp ra mắt)
        // public IActionResult QuizBattle() => View();

        // Code Race: /Game/CodeRace (sắp ra mắt)
        // public IActionResult CodeRace() => View();
    }
}
