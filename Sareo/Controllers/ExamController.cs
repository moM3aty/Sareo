using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sareoo.Controllers
{
    [Authorize(Roles = "Student")]
    public class ExamController : Controller
    {
        private readonly PlatformDbContext _context;

        public ExamController(PlatformDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Take(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var exam = await _context.Exams
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.CourseId == id);

            if (exam == null)
            {
                return View("NoExamAvailable", course);
            }

            var questions = await _context.Questions
                .Where(q => q.ExamId == exam.CourseId)
                .ToListAsync();

            var viewModel = new TakeExamViewModel
            {
                CourseId = exam.CourseId,
                CourseTitle = course.Title,
                Questions = questions,
                StudentAnswers = new Dictionary<int, string>()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TakeExamViewModel viewModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (student == null) return Unauthorized();

            var exam = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.CourseId == viewModel.CourseId);

            if (exam == null) return NotFound("لم يتم العثور على الاختبار.");

            int correctAnswersCount = 0;
            var attemptAnswers = new List<ExamAttemptAnswer>();

            foreach (var question in exam.Questions)
            {
                viewModel.StudentAnswers.TryGetValue(question.Id, out var selectedAnswer);
                bool isCorrect = !string.IsNullOrEmpty(selectedAnswer) &&
                                 selectedAnswer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    correctAnswersCount++;
                }

                attemptAnswers.Add(new ExamAttemptAnswer
                {
                    QuestionId = question.Id,
                    SelectedAnswer = selectedAnswer ?? "لم تتم الإجابة",
                    IsCorrect = isCorrect
                });
            }

            int totalQuestions = exam.Questions.Count;
            int score = totalQuestions > 0 ? (int)Math.Round((double)correctAnswersCount / totalQuestions * 100) : 0;

            var attempt = new ExamAttempt
            {
                StudentId = student.Id,
                ExamId = exam.CourseId,
                Score = score,
                AttemptDate = DateTime.UtcNow,
                Answers = attemptAnswers
            };
            _context.ExamAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            bool passed = score >= exam.PassPercentage;
            string newCertificateGuid = null;

            if (passed)
            {
                var existingCert = await _context.Certificates
                    .FirstOrDefaultAsync(c => c.StudentId == student.Id && c.CourseId == exam.CourseId);

                if (existingCert == null)
                {
                    var certificate = new Certificate
                    {
                        StudentId = student.Id,
                        CourseId = exam.CourseId,
                        IssueDate = DateTime.UtcNow,
                        GradePercentage = score,
                        CertificateGuid = Guid.NewGuid().ToString()
                    };
                    _context.Certificates.Add(certificate);
                    await _context.SaveChangesAsync();
                    newCertificateGuid = certificate.CertificateGuid;
                }
                else
                {
                    newCertificateGuid = existingCert.CertificateGuid;
                }
            }

            var resultViewModel = new ExamResultViewModel
            {
                AttemptId = attempt.Id,
                Passed = passed,
                Score = score,
                PassPercentage = exam.PassPercentage,
                CourseTitle = exam.Course.Title,
                CertificateGuid = newCertificateGuid
            };

            return View("Result", resultViewModel);
        }
        public async Task<IActionResult> Review(int attemptId)
        {
            var attempt = await _context.ExamAttempts
                .Include(a => a.Exam.Course)
                .Include(a => a.Answers).ThenInclude(ans => ans.Question)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return NotFound();

            var viewModel = new ExamReviewViewModel
            {
                Attempt = attempt,
                Questions = attempt.Answers.Select(a => a.Question).ToList(),
                StudentAnswers = attempt.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedAnswer)
            };

            return View(viewModel);
        }
    }
}
