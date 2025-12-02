using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class ExamsController : Controller
    {
        private readonly PlatformDbContext _context;

        public ExamsController(PlatformDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Exams/Index/5 (courseId)
        public async Task<IActionResult> Index(int? courseId, string searchString)
        {
            if (courseId == null) return NotFound();

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            ViewBag.CourseTitle = course.Title;
            ViewBag.CourseId = course.Id;
            ViewData["CurrentFilter"] = searchString;

            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.CourseId == courseId);

            if (exam == null)
            {
                return View("NoExam", course);
            }

            var questionsQuery = _context.Questions.Where(q => q.ExamId == exam.CourseId);

            if (!String.IsNullOrEmpty(searchString))
            {
                questionsQuery = questionsQuery.Where(q => q.QuestionText.Contains(searchString));
            }

            var questions = await questionsQuery.ToListAsync();

            var viewModel = new ExamViewModel
            {
                Exam = exam,
                CourseTitle = course.Title,
                Questions = questions
            };

            return View(viewModel);
        }

        // GET: Admin/Exams/Create?courseId=5
        public async Task<IActionResult> Create(int? courseId)
        {
            if (courseId == null) return NotFound();
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var exam = new Exam
            {
                CourseId = course.Id,
                Title = $"اختبار دورة: {course.Title}"
            };

            ViewBag.CourseTitle = course.Title;
            return View(exam);
        }

        // POST: Admin/Exams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,Title,PassPercentage")] Exam exam)
        {
            ModelState.Remove("Course");
            ModelState.Remove("Questions");
            if (ModelState.IsValid)
            {
                _context.Add(exam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { courseId = exam.CourseId });
            }
            var course = await _context.Courses.FindAsync(exam.CourseId);
            ViewBag.CourseTitle = course.Title;
            return View(exam);
        }

        // GET: Admin/Exams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound();

            var course = await _context.Courses.FindAsync(exam.CourseId);
            ViewBag.CourseTitle = course.Title;
            return View(exam);
        }

        // POST: Admin/Exams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,Title,PassPercentage")] Exam exam)
        {
            if (id != exam.CourseId)
            {
                return NotFound();
            }
            ModelState.Remove("Course");
            ModelState.Remove("Questions");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(exam);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Exams.Any(e => e.CourseId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index), new { courseId = exam.CourseId });
            }
            var course = await _context.Courses.FindAsync(exam.CourseId);
            ViewBag.CourseTitle = course.Title;
            return View(exam);
        }

        // GET: Admin/Exams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var exam = await _context.Exams
                .Include(e => e.Course)
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (exam == null) return NotFound();
            return View(exam);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Courses");
        }
    }
}
