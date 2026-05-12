using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTOs;
using SchoolApp.Models;
using SchoolApp.Models.Enums;
using SchoolApp.UnitOfWork;
using SchoolApp.Filters;
public class QuestionBankController : Controller
{
    private readonly IUnitOfWork _uow;
    public QuestionBankController(IUnitOfWork uow) => _uow = uow;

    // LIST
    public IActionResult Index(string? tag)
    {
        var questions = _uow.Questions.GetBankQuestions(tag);
        var allTags = _uow.Questions.GetBankQuestions()
                            .Where(q => !string.IsNullOrWhiteSpace(q.Tag))
                            .Select(q => q.Tag!).Distinct().OrderBy(t => t).ToList();

        ViewData["CurrentTag"] = tag;
        ViewData["AllTags"] = allTags;
        return View(questions);
    }

    // GET (modal sửa) — dùng GetQuestionWithOptions đã có sẵn
    [HttpGet]
    public IActionResult GetQuestion(int id)
    {
        var q = _uow.Questions.GetQuestionWithOptions(id);
        if (q == null || q.QuizId != null) return NotFound(); // chặn lấy câu quiz

        return Json(new
        {
            questionId = q.QuestionId,
            content = q.Content,
            type = (int)q.Type,
            points = q.Points,
            explanation = q.Explanation,
            tag = q.Tag,
            options = q.Options.OrderBy(o => o.OrderIndex).Select(o => new
            {
                answerOptionId = o.AnswerOptionId,
                content = o.Content,
                isCorrect = o.IsCorrect
            })
        });
    }

    
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Save([FromBody] QuestionDto dto)
    {      
        var error = QuestionValidator.Validate(dto);
        if (error != null) return Json(new { success = false, message = error });

        if (dto.QuestionId == 0)
        {
            // CREATE — QuizId = null → nằm trong bank
            var q = new Question
            {
                QuizId = null,                    // ← bank
                Content = dto.Content.Trim(),
                Type = (QuestionType)dto.Type,
                Points = dto.Points,
                Explanation = dto.Explanation?.Trim(),
                Tag = dto.Tag?.Trim(),
                OrderIndex = 0,
                Options = dto.Options
                    .Where(o => !string.IsNullOrWhiteSpace(o.Content))
                    .Select((o, i) => new AnswerOption
                    {
                        Content = o.Content.Trim(),
                        IsCorrect = o.IsCorrect,
                        OrderIndex = i + 1
                    }).ToList()
            };
            _uow.Questions.Add(q);
        }
        else
        {
            var existing = _uow.Questions.GetQuestionWithOptions(dto.QuestionId);
            if (existing == null || existing.QuizId != null)
                return Json(new { success = false, message = "Không tìm thấy câu hỏi trong ngân hàng" });

            existing.Content = dto.Content.Trim();
            existing.Type = (QuestionType)dto.Type;
            existing.Points = dto.Points;
            existing.Explanation = dto.Explanation?.Trim();
            existing.Tag = dto.Tag?.Trim();

            UpsertOptions(existing, dto.Options);
        }

        _uow.SaveChanges();
        return Json(new
        {
            success = true,
            message = dto.QuestionId == 0
            ? "Đã thêm câu hỏi vào ngân hàng!"
            : "Cập nhật câu hỏi thành công!"
        });
    }

    // DELETE
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var q = _uow.Questions.GetById(id);
        if (q == null || q.QuizId != null)
            return Json(new { success = false, message = "Không tìm thấy câu hỏi" });

        _uow.Questions.Delete(q);
        _uow.SaveChanges();
        return Json(new { success = true, message = "Đã xóa khỏi ngân hàng!" });
    }

    // IMPORT → deep copy vào quiz (QuizId = null → QuizId = dto.QuizId)
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ImportToQuiz([FromBody] ImportFromBankDto dto)
    {
        if (dto.BankQuestionIds.Count == 0)
            return Json(new { success = false, message = "Chưa chọn câu hỏi nào" });

        var quiz = _uow.Quizzes.GetById(dto.QuizId);
        if (quiz == null)
            return Json(new { success = false, message = "Không tìm thấy bài kiểm tra" });

        int nextOrder = _uow.Questions.GetMaxOrderIndex(dto.QuizId) + 1;
        int imported = 0;

        foreach (var bankId in dto.BankQuestionIds)
        {
            var bankQ = _uow.Questions.GetQuestionWithOptions(bankId);
            if (bankQ == null || bankQ.QuizId != null) continue;  // chỉ lấy từ bank

            var copy = new Question
            {
                QuizId = dto.QuizId,           // ← gắn vào quiz
                Content = bankQ.Content,
                Type = bankQ.Type,
                Points = bankQ.Points,
                Explanation = bankQ.Explanation,
                Tag = bankQ.Tag,
                OrderIndex = nextOrder++,
                Options = bankQ.Options
                    .OrderBy(o => o.OrderIndex)
                    .Select((o, i) => new AnswerOption
                    {
                        Content = o.Content,
                        IsCorrect = o.IsCorrect,
                        OrderIndex = i + 1
                    }).ToList()
            };

            _uow.Questions.Add(copy);
            imported++;
        }

        _uow.SaveChanges();
        return Json(new { success = true, message = $"Đã import {imported} câu hỏi!" });
    }

    // GET ALL — JSON cho import modal
    [HttpGet]
    public IActionResult GetAll(string? tag)
    {
        var list = _uow.Questions.GetBankQuestions(tag).Select(q => new
        {
            questionId = q.QuestionId,
            content = q.Content,
            type = (int)q.Type,
            points = q.Points,
            tag = q.Tag
        });
        return Json(list);
    }


    private void UpsertOptions(Question question, List<OptionDto> incoming)
    {
        var valid = incoming.Where(o => !string.IsNullOrWhiteSpace(o.Content)).ToList();
        var existingById = question.Options.ToDictionary(o => o.AnswerOptionId);
        var processed = new HashSet<int>();

        int idx = 1;
        foreach (var o in valid)
        {
            if (o.AnswerOptionId > 0 && existingById.TryGetValue(o.AnswerOptionId, out var opt))
            {
                opt.Content = o.Content.Trim();
                opt.IsCorrect = o.IsCorrect;
                opt.OrderIndex = idx;
                processed.Add(o.AnswerOptionId);
            }
            else
            {
                question.Options.Add(new AnswerOption
                {
                    Content = o.Content.Trim(),
                    IsCorrect = o.IsCorrect,
                    OrderIndex = idx
                });
            }
            idx++;
        }

        var toRemove = question.Options
            .Where(o => o.AnswerOptionId > 0 && !processed.Contains(o.AnswerOptionId))
            .ToList();

        foreach (var opt in toRemove)
            _uow.AnswerOptions.Delete(opt);   
    }
}