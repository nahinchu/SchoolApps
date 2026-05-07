using Microsoft.AspNetCore.Mvc;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Models.Enums;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    public class QuizController : Controller
    {
        private readonly IUnitOfWork _uow;

        public QuizController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region === DTOs ===

        public class OptionDto
        {
            public int AnswerOptionId { get; set; }
            public string Content { get; set; } = "";
            public bool IsCorrect { get; set; }
        }

        public class QuestionDto
        {
            public int QuestionId { get; set; }
            public int QuizId { get; set; }
            public string Content { get; set; } = "";
            public int Type { get; set; }
            public int Points { get; set; } = 1;
            public string? Explanation { get; set; }
            public List<OptionDto> Options { get; set; } = new();
        }

        public class SubmitQuizDto
        {
            public int QuizId { get; set; }
            // Key = QuestionId, Value = List of selected AnswerOptionIds
            public Dictionary<int, List<int>> Answers { get; set; } = new();
        }

        #endregion

        #region === ADMIN: Quiz settings ===

        [AuthorizeAdmin]
        public IActionResult Manage(int lessonId)
        {
            var lesson = _uow.Lessons.GetById(lessonId);
            if (lesson == null) return NotFound();

            var quiz = _uow.Quizzes.GetByLesson(lessonId).FirstOrDefault();

            ViewData["LessonId"] = lessonId;
            ViewData["LessonTitle"] = lesson.Title;
            ViewData["ModuleId"] = lesson.ModuleId;

            return View(quiz ?? new Quiz { LessonId = lessonId, IsPublished = true });
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetQuiz(int id)
        {
            var quiz = _uow.Quizzes.GetById(id);
            if (quiz == null) return NotFound();

            return Json(new
            {
                quizId = quiz.QuizId,
                title = quiz.Title,
                description = quiz.Description,
                passingScore = quiz.PassingScore,
                timeLimitMinutes = quiz.TimeLimitMinutes,
                allowRetry = quiz.AllowRetry,
                maxAttempts = quiz.MaxAttempts,
                showAnswersAfterSubmit = quiz.ShowAnswersAfterSubmit,
                isPublished = quiz.IsPublished,
                lessonId = quiz.LessonId
            });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult SaveQuiz(Quiz quiz)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return Json(new { success = false, errors });
            }

            if (quiz.QuizId == 0)
            {
                _uow.Quizzes.Add(quiz);
                _uow.SaveChanges();
                return Json(new { success = true, message = "Tạo bài kiểm tra thành công!", quizId = quiz.QuizId });
            }

            var existing = _uow.Quizzes.GetById(quiz.QuizId);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy Quiz" });

            existing.Title = quiz.Title;
            existing.Description = quiz.Description;
            existing.PassingScore = quiz.PassingScore;
            existing.TimeLimitMinutes = quiz.TimeLimitMinutes;
            existing.AllowRetry = quiz.AllowRetry;
            existing.MaxAttempts = quiz.MaxAttempts;
            existing.ShowAnswersAfterSubmit = quiz.ShowAnswersAfterSubmit;
            existing.IsPublished = quiz.IsPublished;

            _uow.SaveChanges();
            return Json(new { success = true, message = "Cập nhật bài kiểm tra thành công!", quizId = existing.QuizId });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var quiz = _uow.Quizzes.GetById(id);
            if (quiz == null)
                return Json(new { success = false, message = "Không tìm thấy bài kiểm tra" });

            _uow.Quizzes.Delete(quiz);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Đã xóa bài kiểm tra!" });
        }

        #endregion

        #region === ADMIN: Question CRUD ===

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetQuestions(int quizId)
        {
            var quiz = _uow.Quizzes.GetQuizWithQuestions(quizId);
            var questions = quiz?.Questions
                .OrderBy(q => q.OrderIndex)
                .ToList() ?? new List<Question>();
            return PartialView("_QuestionTable", questions);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetQuestion(int id)
        {
            var question = _uow.Questions.GetById(id);
            if (question == null) return NotFound();

            return Json(new
            {
                questionId = question.QuestionId,
                quizId = question.QuizId,
                content = question.Content,
                type = (int)question.Type,
                points = question.Points,
                explanation = question.Explanation,
                orderIndex = question.OrderIndex,
                options = question.Options
                    .OrderBy(o => o.OrderIndex)
                    .Select(o => new
                    {
                        answerOptionId = o.AnswerOptionId,
                        content = o.Content,
                        isCorrect = o.IsCorrect
                    })
            });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult CreateQuestion([FromBody] QuestionDto dto)
        {
            var error = ValidateQuestion(dto);
            if (error != null) return Json(new { success = false, message = error });

            var quiz = _uow.Quizzes.GetQuizWithQuestions(dto.QuizId);
            if (quiz == null) return Json(new { success = false, message = "Không tìm thấy bài kiểm tra" });

            int nextOrder = (quiz.Questions?.Any() == true)
                ? quiz.Questions.Max(q => q.OrderIndex) + 1
                : 1;

            var question = new Question
            {
                QuizId = dto.QuizId,
                Content = dto.Content.Trim(),
                Type = (QuestionType)dto.Type,
                Points = dto.Points,
                Explanation = dto.Explanation,
                OrderIndex = nextOrder,
                Options = dto.Options.Select((o, idx) => new AnswerOption
                {
                    Content = o.Content.Trim(),
                    IsCorrect = o.IsCorrect,
                    OrderIndex = idx + 1
                }).ToList()
            };

            _uow.Questions.Add(question);
            _uow.SaveChanges();

            return Json(new { success = true, message = "Thêm câu hỏi thành công!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult EditQuestion([FromBody] QuestionDto dto)
        {
            var error = ValidateQuestion(dto);
            if (error != null) return Json(new { success = false, message = error });

            var question = _uow.Questions.GetById(dto.QuestionId);
            if (question == null)
                return Json(new { success = false, message = "Không tìm thấy câu hỏi" });

            question.Content = dto.Content.Trim();
            question.Type = (QuestionType)dto.Type;
            question.Points = dto.Points;
            question.Explanation = dto.Explanation;

            // Replace all options (simplest & safest)
            foreach (var oldOpt in question.Options.ToList())
            {
                _uow.AnswerOptions.Delete(oldOpt);
            }
            question.Options.Clear();

            int idx = 1;
            foreach (var o in dto.Options)
            {
                question.Options.Add(new AnswerOption
                {
                    Content = o.Content.Trim(),
                    IsCorrect = o.IsCorrect,
                    OrderIndex = idx++
                });
            }

            _uow.SaveChanges();
            return Json(new { success = true, message = "Cập nhật câu hỏi thành công!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteQuestion(int id)
        {
            var question = _uow.Questions.GetById(id);
            if (question == null)
                return Json(new { success = false, message = "Không tìm thấy câu hỏi" });

            _uow.Questions.Delete(question);
            _uow.SaveChanges();
            return Json(new { success = true, message = "Đã xóa câu hỏi!" });
        }

        private static string? ValidateQuestion(QuestionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return "Nội dung câu hỏi không được để trống";
            if (dto.Points < 1) return "Điểm phải >= 1";
            if (dto.Options == null || dto.Options.Count(o => !string.IsNullOrWhiteSpace(o.Content)) < 2)
                return "Cần ít nhất 2 đáp án có nội dung";
            if (!dto.Options.Any(o => o.IsCorrect))
                return "Cần đánh dấu ít nhất 1 đáp án đúng";
            // For SingleChoice / TrueFalse only one correct allowed
            if (dto.Type != 1 && dto.Options.Count(o => o.IsCorrect) > 1)
                return "Loại câu hỏi này chỉ được có 1 đáp án đúng";
            return null;
        }

        #endregion

        #region === STUDENT ===

        [AuthorizeUser]
        public IActionResult Take(int id) // id = QuizId
        {
            var quiz = _uow.Quizzes.GetQuizWithQuestions(id);
            if (quiz == null || !quiz.IsPublished) return NotFound();

            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            ViewData["StudentId"] = studentId;
            return View(quiz);
        }

        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitQuiz([FromBody] SubmitQuizDto dto)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId") ?? 0;
            if (studentId == 0)
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var quiz = _uow.Quizzes.GetQuizWithQuestions(dto.QuizId);
            if (quiz == null)
                return Json(new { success = false, message = "Không tìm thấy bài kiểm tra" });

            int score = 0;
            int maxScore = 0;

            foreach (var question in quiz.Questions)
            {
                maxScore += question.Points;

                if (!dto.Answers.TryGetValue(question.QuestionId, out var selected) || selected == null)
                    continue;

                var correctIds = question.Options
                    .Where(o => o.IsCorrect)
                    .Select(o => o.AnswerOptionId)
                    .ToHashSet();
                var selectedIds = selected.ToHashSet();

                // Đúng khi tập đáp án chọn = tập đáp án đúng (áp dụng cho cả Single, Multi, TrueFalse)
                if (correctIds.Count > 0 && correctIds.SetEquals(selectedIds))
                {
                    score += question.Points;
                }
            }

            int percent = maxScore > 0 ? (score * 100 / maxScore) : 0;
            bool passed = percent >= quiz.PassingScore;

            // === Lưu lịch sử làm bài (bỏ comment khi đã có model QuizAttempt + repo) ===
            // var attempt = new QuizAttempt
            // {
            //     QuizId = dto.QuizId,
            //     StudentId = studentId,
            //     Score = score,
            //     MaxScore = maxScore,
            //     Percent = percent,
            //     Passed = passed,
            //     SubmittedAt = DateTime.UtcNow
            // };
            // _uow.QuizAttempts.Add(attempt);
            // _uow.SaveChanges();

            return Json(new
            {
                success = true,
                score,
                maxScore,
                percent,
                passed,
                message = passed ? "Chúc mừng! Bạn đã đạt yêu cầu." : "Bạn chưa đạt điểm qua môn."
            });
        }

        #endregion
    }
}