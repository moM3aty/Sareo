using Sareoo.Areas.Admin.Data;
using Sareoo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;

namespace Sareoo.Controllers
{
    [Authorize(Roles = "Student")]
    public class LessonQuizPlayerController : Controller
    {
        private readonly PlatformDbContext _context;

        public LessonQuizPlayerController(PlatformDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Take(int lessonId)
        {
            var quiz = await _context.LessonQuizzes
                .Include(q => q.Questions)
                .Include(q => q.Lesson)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.LessonId == lessonId);

            if (quiz == null || !quiz.Questions.Any())
            {
                TempData["ErrorMessage"] = "لا يوجد اختبار متاح لهذا الدرس.";
                return RedirectToAction("Index", "Profile");
            }

            var viewModel = new TakeLessonQuizViewModel
            {
                Quiz = quiz,
                Answers = new Dictionary<int, string>()
            };

            return View(viewModel);
        }

        [HttpPost("LessonQuizPlayer/Submit")] // Explicit route definition
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TakeLessonQuizViewModel viewModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (student == null) return Unauthorized();

            var quiz = await _context.LessonQuizzes
                .Include(q => q.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.LessonId == viewModel.Quiz.LessonId);

            if (quiz == null) return NotFound();

            int score = 0;
            var attemptAnswers = new List<LessonQuizAttemptAnswer>();

            foreach (var question in quiz.Questions)
            {
                viewModel.Answers.TryGetValue(question.Id, out var selectedAnswer);
                bool isCorrect = !string.IsNullOrEmpty(selectedAnswer) &&
                                 selectedAnswer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    score += question.Points;
                }

                attemptAnswers.Add(new LessonQuizAttemptAnswer
                {
                    QuestionId = question.Id,
                    SelectedAnswer = selectedAnswer ?? "لم تتم الإجابة",
                    IsCorrect = isCorrect
                });
            }

            var attempt = new LessonQuizAttempt
            {
                LessonQuizId = quiz.LessonId,
                StudentId = student.Id,
                Score = score,
                TotalPoints = quiz.Questions.Sum(q => q.Points),
                AttemptDate = DateTime.UtcNow,
                Answers = attemptAnswers
            };

            _context.LessonQuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return RedirectToAction("Results", new { attemptId = attempt.Id });
        }

        public async Task<IActionResult> Results(int attemptId)
        {
            var attempt = await _context.LessonQuizAttempts
                .Include(a => a.LessonQuiz).ThenInclude(lq => lq.Lesson)
                .Include(a => a.Answers).ThenInclude(ans => ans.Question)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return NotFound();

            var viewModel = new QuizResultViewModel
            {
                Attempt = attempt,
                Questions = attempt.Answers.Select(a => a.Question).ToList(),
                StudentAnswers = attempt.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedAnswer)
            };

            return View(viewModel);
        }
    }
}