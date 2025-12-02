using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class LessonQuizzesController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;

        public LessonQuizzesController(PlatformDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index(int lessonId, string searchString)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return NotFound();

            var quiz = await _context.LessonQuizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.LessonId == lessonId);

            // Apply search if a search string is provided
            if (quiz != null && !string.IsNullOrEmpty(searchString))
            {
                quiz.Questions = quiz.Questions.Where(q => q.QuestionText.Contains(searchString)).ToList();
            }

            ViewData["CurrentFilter"] = searchString;

            var viewModel = new LessonQuizViewModel
            {
                Lesson = lesson,
                Quiz = quiz
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuiz(int lessonId, string title)
        {
            var quiz = new LessonQuiz { LessonId = lessonId, Title = title };
            _context.LessonQuizzes.Add(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { lessonId });
        }

        public async Task<IActionResult> Create(int lessonQuizId)
        {
            var quiz = await _context.LessonQuizzes
                .Include(q => q.Lesson)
                .FirstOrDefaultAsync(q => q.LessonId == lessonQuizId);

            if (quiz == null) return NotFound();

            var viewModel = new LessonQuizQuestionFormViewModel
            {
                Question = new LessonQuizQuestion { LessonQuizId = lessonQuizId, NumberOfOptions = 4 },
                LessonQuizId = lessonQuizId,
                LessonTitle = quiz.Lesson.Title
            };
            viewModel.GenerateCorrectAnswerOptions();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonQuizQuestionFormViewModel viewModel)
        {
            ModelState.Remove("LessonTitle");
            ModelState.Remove("Question.LessonQuiz");
            ModelState.Remove("CorrectAnswerOptions");


            if (ModelState.IsValid)
            {
                var question = viewModel.Question;
                string content = viewModel.QuestionContent;

                if (content.StartsWith("data:image/"))
                {
                    question.ImageUrl = await _fileService.SaveBase64FileAsync(content, "quiz_images");
                    question.QuestionText = "[سؤال عبارة عن صورة]";
                }
                else
                {
                    question.QuestionText = content;
                }

                _context.LessonQuizQuestions.Add(question);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { lessonId = question.LessonQuizId });
            }

            var quizForTitle = await _context.LessonQuizzes.Include(q => q.Lesson).FirstOrDefaultAsync(q => q.LessonId == viewModel.Question.LessonQuizId);
            if (quizForTitle == null) return NotFound();
            viewModel.LessonTitle = quizForTitle.Lesson.Title;
            viewModel.LessonQuizId = viewModel.Question.LessonQuizId;
            viewModel.GenerateCorrectAnswerOptions();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var question = await _context.LessonQuizQuestions
                .Include(q => q.LessonQuiz)
                .ThenInclude(lq => lq.Lesson)
                .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null) return NotFound();

            var viewModel = new LessonQuizQuestionFormViewModel
            {
                Question = question,
                LessonTitle = question.LessonQuiz.Lesson.Title,
                LessonQuizId = question.LessonQuizId
            };
            viewModel.GenerateCorrectAnswerOptions();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LessonQuizQuestionFormViewModel viewModel)
        {
            if (id != viewModel.Question.Id) return NotFound();

            ModelState.Remove("LessonTitle");
            ModelState.Remove("Question.LessonQuiz");
            ModelState.Remove("CorrectAnswerOptions");


            if (string.IsNullOrEmpty(viewModel.QuestionContent) && string.IsNullOrEmpty(viewModel.Question.ImageUrl))
            {
                ModelState.AddModelError("QuestionContent", "محتوى السؤال مطلوب.");
            }
            ModelState.Remove("QuestionContent");

            if (ModelState.IsValid)
            {
                var questionToUpdate = await _context.LessonQuizQuestions.FindAsync(id);
                if (questionToUpdate == null) return NotFound();

                if (!string.IsNullOrEmpty(viewModel.QuestionContent))
                {
                    if (viewModel.QuestionContent.StartsWith("data:image/"))
                    {
                        _fileService.DeleteFile(questionToUpdate.ImageUrl);
                        questionToUpdate.ImageUrl = await _fileService.SaveBase64FileAsync(viewModel.QuestionContent, "quiz_images");
                        questionToUpdate.QuestionText = "[سؤال عبارة عن صورة]";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(questionToUpdate.ImageUrl))
                        {
                            _fileService.DeleteFile(questionToUpdate.ImageUrl);
                            questionToUpdate.ImageUrl = null;
                        }
                        questionToUpdate.QuestionText = viewModel.QuestionContent;
                    }
                }

                questionToUpdate.NumberOfOptions = viewModel.Question.NumberOfOptions;
                questionToUpdate.OptionA = viewModel.Question.OptionA;
                questionToUpdate.OptionB = viewModel.Question.OptionB;
                questionToUpdate.OptionC = viewModel.Question.NumberOfOptions >= 3 ? viewModel.Question.OptionC : null;
                questionToUpdate.OptionD = viewModel.Question.NumberOfOptions == 4 ? viewModel.Question.OptionD : null;
                questionToUpdate.CorrectAnswer = viewModel.Question.CorrectAnswer;
                questionToUpdate.Points = viewModel.Question.Points;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { lessonId = questionToUpdate.LessonQuizId });
            }

            var quiz = await _context.LessonQuizzes.Include(q => q.Lesson).FirstOrDefaultAsync(q => q.LessonId == viewModel.Question.LessonQuizId);
            if (quiz == null) return NotFound();
            viewModel.LessonTitle = quiz.Lesson.Title;
            viewModel.LessonQuizId = viewModel.Question.LessonQuizId;
            viewModel.GenerateCorrectAnswerOptions();
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var question = await _context.LessonQuizQuestions
                .Include(q => q.LessonQuiz.Lesson)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null) return NotFound();
            return View(question);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.LessonQuizQuestions.FindAsync(id);
            if (question != null)
            {
                _fileService.DeleteFile(question.ImageUrl);
                _context.LessonQuizQuestions.Remove(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { lessonId = question.LessonQuizId });
            }
            return NotFound();
        }
    }
}
