using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class QuestionsController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;

        public QuestionsController(PlatformDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IActionResult> Create(int? examId)
        {
            if (examId == null) return NotFound();
            var exam = await _context.Exams.Include(e => e.Course).FirstOrDefaultAsync(e => e.CourseId == examId);
            if (exam == null) return NotFound();

            var viewModel = new QuestionFormViewModel
            {
                Question = new Question { ExamId = exam.CourseId, NumberOfOptions = 4 },
                ExamId = exam.CourseId,
                CourseTitle = exam.Course.Title,
            };
            viewModel.GenerateCorrectAnswerOptions();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionFormViewModel viewModel)
        {
            ModelState.Remove("CourseTitle");
            ModelState.Remove("Question.Exam");
            ModelState.Remove("CorrectAnswerOptions");


            if (ModelState.IsValid)
            {
                var question = viewModel.Question;
                string content = viewModel.QuestionContent;

                if (content.StartsWith("data:image/"))
                {
                    question.ImageUrl = await _fileService.SaveBase64FileAsync(content, "exam_images");
                    question.QuestionText = "[سؤال عبارة عن صورة]";
                }
                else
                {
                    question.QuestionText = content;
                }

                _context.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Exams", new { courseId = question.ExamId });
            }

            var exam = await _context.Exams.Include(e => e.Course).FirstOrDefaultAsync(e => e.CourseId == viewModel.Question.ExamId);
            if (exam == null) return NotFound();
            viewModel.CourseTitle = exam.Course.Title;
            viewModel.ExamId = exam.CourseId;
            viewModel.GenerateCorrectAnswerOptions();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var question = await _context.Questions.FindAsync(id);
            if (question == null) return NotFound();
            var exam = await _context.Exams.Include(e => e.Course).FirstOrDefaultAsync(e => e.CourseId == question.ExamId);
            if (exam == null) return NotFound();

            var viewModel = new QuestionFormViewModel
            {
                Question = question,
                ExamId = exam.CourseId,
                CourseTitle = exam.Course.Title,
            };
            viewModel.GenerateCorrectAnswerOptions();
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionFormViewModel viewModel)
        {
            if (id != viewModel.Question.Id) return NotFound();

            ModelState.Remove("CourseTitle");
            ModelState.Remove("Question.Exam");
            ModelState.Remove("CorrectAnswerOptions");
            ModelState.Remove("QuestionContent");

            if (string.IsNullOrEmpty(viewModel.QuestionContent) && string.IsNullOrEmpty(viewModel.Question.ImageUrl))
            {
                ModelState.AddModelError("QuestionContent", "محتوى السؤال مطلوب.");
            }

            if (ModelState.IsValid)
            {
                var questionToUpdate = await _context.Questions.FindAsync(id);
                if (questionToUpdate == null) return NotFound();

                if (!string.IsNullOrEmpty(viewModel.QuestionContent))
                {
                    if (viewModel.QuestionContent.StartsWith("data:image/"))
                    {
                        _fileService.DeleteFile(questionToUpdate.ImageUrl);
                        questionToUpdate.ImageUrl = await _fileService.SaveBase64FileAsync(viewModel.QuestionContent, "exam_images");
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
                return RedirectToAction("Index", "Exams", new { courseId = questionToUpdate.ExamId });
            }

            var exam = await _context.Exams.Include(e => e.Course).FirstOrDefaultAsync(e => e.CourseId == viewModel.Question.ExamId);
            if (exam == null) return NotFound();
            viewModel.CourseTitle = exam.Course.Title;
            viewModel.ExamId = exam.CourseId;
            viewModel.GenerateCorrectAnswerOptions();
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var question = await _context.Questions
                .Include(q => q.Exam)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null) return NotFound();
            return View(question);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _fileService.DeleteFile(question.ImageUrl);
                var examId = question.ExamId;
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Exams", new { courseId = examId });
            }
            return NotFound();
        }
    }
}
