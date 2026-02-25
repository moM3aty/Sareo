
using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class LessonsController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;
        public LessonsController(PlatformDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index(int? courseId, string searchString)
        {
            if (courseId == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound();
            }

            ViewBag.CourseTitle = course.Title;
            ViewBag.CourseId = course.Id;
            ViewData["CurrentFilter"] = searchString;

            var lessonsQuery = _context.Lessons.Where(l => l.CourseId == courseId);

            if (!string.IsNullOrEmpty(searchString))
            {
                lessonsQuery = lessonsQuery.Where(l => l.Title.Contains(searchString));
            }

            var lessons = await lessonsQuery.OrderBy(l => l.Title).ToListAsync();

            return View(lessons);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Lessons
                .Include(l => l.Attachments)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound();

            var viewModel = new LessonDetailsViewModel
            {
                Lesson = lesson,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create(int? courseId)
        {
            if (courseId == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var viewModel = new LessonViewModel
            {
                Lesson = new Lesson { CourseId = course.Id },
                CourseId = course.Id,
                CourseTitle = course.Title
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonViewModel viewModel)
        {
            ModelState.Remove("CourseTitle");
            ModelState.Remove("Lesson.Course");
            ModelState.Remove("Lesson.Attachments");
            ModelState.Remove("Lesson.HomeworkTitle");
            ModelState.Remove("Lesson.HomeworkDescription");
            ModelState.Remove("Lesson.HomeworkSubmissions");
            ModelState.Remove("Lesson.LessonQuiz");
            if (ModelState.IsValid)
            {
                _context.Add(viewModel.Lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { courseId = viewModel.Lesson.CourseId });
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            var course = await _context.Courses.FindAsync(lesson.CourseId);

            var viewModel = new LessonViewModel
            {
                Lesson = lesson,
                CourseId = course.Id,
                CourseTitle = course.Title
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LessonViewModel viewModel)
        {
            if (id != viewModel.Lesson.Id) return NotFound();
            ModelState.Remove("CourseTitle");
            ModelState.Remove("Lesson.Course");
            ModelState.Remove("Lesson.Attachments");
            ModelState.Remove("Lesson.HomeworkTitle");
            ModelState.Remove("Lesson.HomeworkDescription");
            ModelState.Remove("Lesson.HomeworkSubmissions");
            ModelState.Remove("Lesson.LessonQuiz");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewModel.Lesson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Lessons.Any(e => e.Id == viewModel.Lesson.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index), new { courseId = viewModel.Lesson.CourseId });
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            var courseId = lesson.CourseId;
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { courseId = courseId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAttachment(int lessonId, IFormFile NewAttachment)
        {
            if (NewAttachment != null && NewAttachment.Length > 0)
            {
                var filePath = await _fileService.SaveFileAsync(NewAttachment, "attachments");

                var attachment = new LessonAttachment
                {
                    LessonId = lessonId,
                    FileName = Path.GetFileName(NewAttachment.FileName),
                    FilePath = filePath 
                };

                _context.LessonAttachments.Add(attachment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = lessonId });
        }
    }
}