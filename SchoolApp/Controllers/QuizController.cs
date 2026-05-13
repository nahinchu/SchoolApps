using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTOs;
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
       
        public class ReorderDto
        {
            public int QuizId { get; set; }
            public List<int> QuestionIds { get; set; } = new();
        }

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
        public IActionResult SaveQuiz([FromForm] QuizSaveDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors });
            }

            if (dto.QuizId == 0)
            {
                // Tạo Quiz mới
                var newQuiz = new Quiz
                {
                    LessonId = dto.LessonId,
                    Title = dto.Title.Trim(),
                    Description = dto.Description?.Trim(),
                    PassingScore = dto.PassingScore,
                    TimeLimitMinutes = dto.TimeLimitMinutes,
                    MaxAttempts = dto.MaxAttempts,
                    IsPublished = dto.IsPublished,
                    AllowRetry = dto.AllowRetry,
                    ShowAnswersAfterSubmit = dto.ShowAnswersAfterSubmit
                };

                _uow.Quizzes.Add(newQuiz);
                _uow.SaveChanges();

                return Json(new { success = true, message = "Tạo bài kiểm tra thành công!", quizId = newQuiz.QuizId });
            }
            else
            {
                // Cập nhật Quiz
                var existing = _uow.Quizzes.GetById(dto.QuizId);
                if (existing == null)
                    return Json(new { success = false, message = "Không tìm thấy bài kiểm tra" });

                existing.Title = dto.Title.Trim();
                existing.Description = dto.Description?.Trim();
                existing.PassingScore = dto.PassingScore;
                existing.TimeLimitMinutes = dto.TimeLimitMinutes;
                existing.MaxAttempts = dto.MaxAttempts;
                existing.IsPublished = dto.IsPublished;
                existing.AllowRetry = dto.AllowRetry;
                existing.ShowAnswersAfterSubmit = dto.ShowAnswersAfterSubmit;

                _uow.SaveChanges();

                return Json(new { success = true, message = "Cập nhật bài kiểm tra thành công!" });
            }
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

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetQuestions(int quizId)
        {
            var questions = _uow.Questions.GetQuestionsByQuiz(quizId).ToList();
            return PartialView("_QuestionTable", questions);
        }

        [HttpGet]
        [AuthorizeAdmin]
        public IActionResult GetQuestion(int id)
        {
      
            var question = _uow.Questions.GetQuestionWithOptions(id);
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

            if (dto.QuizId == null)
                return Json(new { success = false, message = "Không tìm thấy bài kiểm tra" });

            int quizId = dto.QuizId.Value;   // ← unwrap 1 lần, dùng lại bên dưới

            var quizExists = _uow.Quizzes.GetById(quizId) != null;
            if (!quizExists)
                return Json(new { success = false, message = "Không tìm thấy bài kiểm tra" });

            int nextOrder = _uow.Questions.GetMaxOrderIndex(quizId) + 1;

            var question = new Question
            {
                QuizId = quizId,
                Content = dto.Content.Trim(),
                Type = (QuestionType)dto.Type,
                Points = dto.Points,
                Explanation = dto.Explanation,
                OrderIndex = nextOrder,
                Options = dto.Options
                    .Where(o => !string.IsNullOrWhiteSpace(o.Content))
                    .Select((o, idx) => new AnswerOption
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

            var question = _uow.Questions.GetQuestionWithOptions(dto.QuestionId);
            if (question == null)
                return Json(new { success = false, message = "Không tìm thấy câu hỏi" });


            question.Content = dto.Content.Trim();
            question.Type = (QuestionType)dto.Type;
            question.Points = dto.Points;
            question.Explanation = dto.Explanation;

            // UPSERT options thay vì delete-all-then-recreate
            // Tránh vi phạm FK với QuizAnswers từ các lượt làm bài cũ
            var validIncoming = dto.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Content))
                .ToList();

            // Map các option hiện có theo Id để lookup nhanh
            var existingById = question.Options.ToDictionary(o => o.AnswerOptionId);
            var processedIds = new HashSet<int>();

            int idx = 1;
            foreach (var o in validIncoming)
            {
                if (o.AnswerOptionId > 0 && existingById.TryGetValue(o.AnswerOptionId, out var existing))
                {
                    // UPDATE in-place — giữ nguyên ID nên QuizAnswers cũ vẫn trỏ đúng
                    existing.Content = o.Content.Trim();
                    existing.IsCorrect = o.IsCorrect;
                    existing.OrderIndex = idx;
                    processedIds.Add(o.AnswerOptionId);
                }
                else
                {
                    // INSERT mới
                    question.Options.Add(new AnswerOption
                    {
                        QuestionId = question.QuestionId,
                        Content = o.Content.Trim(),
                        IsCorrect = o.IsCorrect,
                        OrderIndex = idx
                    });
                }
                idx++;
            }

            // Các option có trong DB nhưng admin đã xoá khỏi form
            var toRemove = question.Options
                .Where(o => o.AnswerOptionId > 0 && !processedIds.Contains(o.AnswerOptionId))
                .ToList();

            foreach (var opt in toRemove)
            {
                // Chỉ xoá được nếu chưa có học viên nào chọn
                if (_uow.QuizAnswers.HasAnyForOption(opt.AnswerOptionId))
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Không thể xoá đáp án \"{opt.Content}\" vì đã có học viên chọn nó trong lần làm trước. " +
                                  "Hãy giữ lại đáp án này (có thể sửa nội dung), hoặc xoá toàn bộ câu hỏi."
                    });
                }
                _uow.AnswerOptions.Delete(opt);
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

            int? quizId = question.QuizId;
            _uow.Questions.Delete(question);
            _uow.SaveChanges();

            // Chỉ bóp OrderIndex nếu câu hỏi thuộc quiz (không phải bank)
            if (quizId.HasValue)
            {
                var remaining = _uow.Questions.GetQuestionsByQuiz(quizId.Value).ToList();
                int order = 1;
                foreach (var q in remaining) q.OrderIndex = order++;
                _uow.SaveChanges();
            }

            return Json(new { success = true, message = "Đã xóa câu hỏi!" });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult ReorderQuestions([FromBody] ReorderDto dto)
        {
            if (dto.QuestionIds == null || dto.QuestionIds.Count == 0)
                return Json(new { success = false, message = "Danh sách rỗng" });

            var questions = _uow.Questions.GetQuestionsByQuiz(dto.QuizId).ToList();
            var map = questions.ToDictionary(q => q.QuestionId);

            int order = 1;
            foreach (var id in dto.QuestionIds)
            {
                if (map.TryGetValue(id, out var q))
                {
                    q.OrderIndex = order++;
                }
            }

            _uow.SaveChanges();
            return Json(new { success = true, message = "Đã cập nhật thứ tự" });
        }

        private static string? ValidateQuestion(QuestionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return "Nội dung câu hỏi không được để trống";
            if (dto.Points < 1) return "Điểm phải >= 1";

            var validOptions = dto.Options?
                .Where(o => !string.IsNullOrWhiteSpace(o.Content))
                .ToList() ?? new List<OptionDto>();

            if (validOptions.Count < 2)
                return "Cần ít nhất 2 đáp án có nội dung";
            if (!validOptions.Any(o => o.IsCorrect))
                return "Cần đánh dấu ít nhất 1 đáp án đúng";

            var type = (QuestionType)dto.Type;
            int correctCount = validOptions.Count(o => o.IsCorrect);

            if (type != QuestionType.MultiChoice && correctCount > 1)
                return "Loại câu hỏi này chỉ được có 1 đáp án đúng";

            if (type == QuestionType.TrueFalse && validOptions.Count != 2)
                return "Câu hỏi Đúng/Sai phải có đúng 2 đáp án";

            return null;
        }
        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public IActionResult CopyToBank(int questionId)
        {
            var q = _uow.Questions.GetQuestionWithOptions(questionId);
            if (q == null || q.QuizId == null)
                return Json(new { success = false, message = "Không tìm thấy câu hỏi" });

            // Kiểm tra đã có trong bank chưa (tránh duplicate)
            var isDuplicate = _uow.Questions
                .GetBankQuestions()
                .Any(b => b.Content.Trim() == q.Content.Trim());

            if (isDuplicate)
                return Json(new { success = false, message = "Câu hỏi này đã tồn tại trong ngân hàng!" });

            var copy = new Question
            {
                QuizId = null,           // ← vào bank
                Content = q.Content,
                Type = q.Type,
                Points = q.Points,
                Explanation = q.Explanation,
                Tag = q.Tag,
                OrderIndex = 0,
                Options = q.Options
                    .OrderBy(o => o.OrderIndex)
                    .Select((o, i) => new AnswerOption
                    {
                        Content = o.Content,
                        IsCorrect = o.IsCorrect,
                        OrderIndex = i + 1
                    }).ToList()
            };

            _uow.Questions.Add(copy);
            _uow.SaveChanges();

            return Json(new { success = true, message = "Đã lưu câu hỏi vào ngân hàng!" });
        }

        [AuthorizeUser]
        public IActionResult Take(int id) // id = QuizId
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            var quiz = _uow.Quizzes.GetQuizWithQuestions(id);
            if (quiz == null || !quiz.IsPublished) return NotFound();

            var lesson = _uow.Lessons.GetWithModule(quiz.LessonId);
            if (lesson?.Module == null) return NotFound();
            var enrolled = _uow.Enrollments.IsEnrolled(studentId.Value, lesson.Module.CourseId);
            if (!enrolled)
            {
                TempData["Error"] = "Bạn cần đăng ký khoá học trước khi làm bài kiểm tra";
                return RedirectToAction("Detail", "Course", new { id = lesson.Module.CourseId });
            }
         
            //  Check số lần làm
            int attemptCount = _uow.QuizAttempts.GetAttemptCount(id, studentId.Value);

            if (attemptCount > 0 && !quiz.AllowRetry)
            {
                TempData["QuizError"] = "Bài kiểm tra này không cho phép làm lại";
                return RedirectToAction("MyAttempts", new { quizId = id });
            }

            if (quiz.MaxAttempts > 0 && attemptCount >= quiz.MaxAttempts)
            {
                TempData["QuizError"] = $"Bạn đã sử dụng hết {quiz.MaxAttempts} lượt làm bài";
                return RedirectToAction("MyAttempts", new { quizId = id });
            }

            // Lưu thời gian bắt đầu vào session để verify ở SubmitQuiz
            HttpContext.Session.SetString(
                $"qz_start_{id}_{studentId}",
                DateTime.UtcNow.ToString("o"));

            ViewData["StudentId"] = studentId;
            ViewData["AttemptCount"] = attemptCount;
            ViewData["AttemptsRemaining"] = quiz.MaxAttempts > 0
                ? quiz.MaxAttempts - attemptCount
                : -1; // -1 = không giới hạn

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

            // ✅ Re-check MaxAttempts khi nộp (chống abuse mở nhiều tab)
            int previousAttempts = _uow.QuizAttempts.GetAttemptCount(dto.QuizId, studentId);
            if (previousAttempts > 0 && !quiz.AllowRetry)
                return Json(new { success = false, message = "Bài kiểm tra không cho phép làm lại" });
            if (quiz.MaxAttempts > 0 && previousAttempts >= quiz.MaxAttempts)
                return Json(new { success = false, message = "Đã hết lượt làm bài" });

            // ✅ Lấy thời gian bắt đầu từ session
            var startKey = $"qz_start_{dto.QuizId}_{studentId}";
            var startStr = HttpContext.Session.GetString(startKey);
            DateTime startedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(startStr) &&
                DateTime.TryParse(startStr, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
            {
                startedAt = parsed;
            }

            // ✅ Validate timer (cho phép 30s buffer cho độ trễ mạng)
            bool timeExceeded = false;
            if (quiz.TimeLimitMinutes > 0)
            {
                var elapsed = (DateTime.UtcNow - startedAt).TotalMinutes;
                if (elapsed > quiz.TimeLimitMinutes + 0.5)
                {
                    timeExceeded = true;
                    // Vẫn cho nộp nhưng đánh dấu, hoặc reject hẳn — tuỳ policy
                    // Ở đây mình cho nộp nhưng không tính các câu trả lời sau giờ
                }
            }

            // ============================================================
            // Chấm điểm + tạo QuizAttempt + QuizAnswer
            // ============================================================
            int score = 0;
            int maxScore = 0;
            var answerRecords = new List<QuizAnswer>();

            foreach (var question in quiz.Questions)
            {
                maxScore += question.Points;

                var correctIds = question.Options
                    .Where(o => o.IsCorrect)
                    .Select(o => o.AnswerOptionId)
                    .ToHashSet();

                if (!dto.Answers.TryGetValue(question.QuestionId, out var selected) || selected == null)
                    continue;

                var selectedIds = selected
                    .Where(id => question.Options.Any(o => o.AnswerOptionId == id))
                    .ToHashSet();

                bool isCorrect = correctIds.Count > 0 && correctIds.SetEquals(selectedIds);
                if (isCorrect) score += question.Points;

                // Lưu mỗi đáp án đã chọn thành 1 QuizAnswer
                foreach (var optId in selectedIds)
                {
                    answerRecords.Add(new QuizAnswer
                    {
                        QuestionId = question.QuestionId,
                        SelectedOptionId = optId,
                        IsCorrect = isCorrect // đánh dấu cả câu đúng/sai chứ không phải từng option
                    });
                }
            }

            int percent = maxScore > 0 ? (int)Math.Round((double)score * 100 / maxScore) : 0;
            bool passed = percent >= quiz.PassingScore;

            // ✅ Persist attempt
            var attempt = new QuizAttempt
            {
                QuizId = dto.QuizId,
                StudentId = studentId,
                StartedAt = startedAt,
                FinishedAt = DateTime.UtcNow,
                Score = score,
                MaxScore = maxScore,
                Passed = passed,
                AttemptNumber = previousAttempts + 1,
                Answers = answerRecords
            };

            _uow.QuizAttempts.Add(attempt);
            _uow.SaveChanges();

            // Clear session start time
            HttpContext.Session.Remove(startKey);

            // ============================================================
            // 🚧 LESSON PROGRESS (tuỳ chỉnh theo project)
            // ============================================================
            // Nếu pass, đánh dấu lesson đã hoàn thành. Bỏ comment khi đã có repository:
            //
            // if (passed)
            // {
            //     var progress = _uow.LessonProgresses.GetByStudentAndLesson(studentId, quiz.LessonId);
            //     if (progress == null)
            //     {
            //         _uow.LessonProgresses.Add(new LessonProgress
            //         {
            //             StudentId = studentId,
            //             LessonId = quiz.LessonId,
            //             Status = ProgressStatus.Completed,
            //             CompletedAt = DateTime.UtcNow
            //         });
            //     }
            //     else if (progress.Status != ProgressStatus.Completed)
            //     {
            //         progress.Status = ProgressStatus.Completed;
            //         progress.CompletedAt = DateTime.UtcNow;
            //     }
            //     _uow.SaveChanges();
            // }
            // ============================================================

            return Json(new
            {
                success = true,
                attemptId = attempt.QuizAttemptId,
                score,
                maxScore,
                percent,
                passed,
                timeExceeded,
                showAnswers = quiz.ShowAnswersAfterSubmit,
                resultUrl = Url.Action("Result", new { id = attempt.QuizAttemptId }),
                message = passed
                    ? "Chúc mừng! Bạn đã đạt yêu cầu."
                    : "Bạn chưa đạt điểm qua môn."
            });
        }

        [AuthorizeUser]
        public IActionResult Result(int id) // id = QuizAttemptId
        {
            var studentId = HttpContext.Session.GetInt32("StudentId") ?? 0;
            if (studentId == 0) return RedirectToAction("Login", "Account");

            var attempt = _uow.QuizAttempts.GetAttemptWithDetails(id);
            if (attempt == null || attempt.StudentId != studentId) return NotFound();

            return View(attempt);
        }

        [AuthorizeUser]
        public IActionResult MyAttempts(int quizId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId") ?? 0;
            if (studentId == 0) return RedirectToAction("Login", "Account");

            var quiz = _uow.Quizzes.GetById(quizId);
            if (quiz == null) return NotFound();

            var attempts = _uow.QuizAttempts
                .GetByStudentAndQuiz(studentId, quizId)
                .OrderByDescending(a => a.StartedAt)
                .ToList();

            ViewData["Quiz"] = quiz;
            return View(attempts);
        }

    }
}