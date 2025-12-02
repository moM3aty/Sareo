using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Services;
using Sareoo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sareoo.Controllers
{
    [Authorize(Roles = "Student")]
    [Route("CoursePlayer")] 
    public class CoursePlayerController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;

        public CoursePlayerController(PlatformDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [Route("Index/{courseId}")]
        public async Task<IActionResult> Index(int courseId, int? lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (student == null) return Unauthorized();

            var subscription = await _context.StudentCourses.FirstOrDefaultAsync(sc => sc.StudentId == student.Id && sc.CourseId == courseId);
            if (subscription == null) return RedirectToAction("Index", "Profile");

            var course = await _context.Courses
                // --- FIX APPLIED HERE: Added includes for LessonQuiz ---
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Attachments)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonQuiz)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();

            if (!course.Lessons.Any())
            {
                TempData["ErrorMessage"] = "هذه الدورة لا تحتوي على دروس بعد.";
                return RedirectToAction("Index", "Profile");
            }

            var lessons = course.Lessons.OrderBy(l => l.Id).ToList();
            var currentLesson = lessonId.HasValue ? lessons.FirstOrDefault(l => l.Id == lessonId.Value) : lessons.FirstOrDefault();
            if (currentLesson == null) return NotFound();

            var currentLessonIndex = lessons.FindIndex(l => l.Id == currentLesson.Id);
            int progressPercentage = (int)Math.Round(((double)(currentLessonIndex + 1) / lessons.Count) * 100);

            subscription.ProgressPercentage = progressPercentage;
            subscription.LastAccessDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

           

            var viewModel = new CoursePlayerViewModel
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                AllLessons = lessons,
                CurrentLesson = currentLesson,
                StudentProgress = subscription.ProgressPercentage,
                IsCourseCompleted = (currentLessonIndex == lessons.Count - 1),
                PreviousLessonId = currentLessonIndex > 0 ? lessons[currentLessonIndex - 1].Id : (int?)null,
                NextLessonId = currentLessonIndex < lessons.Count - 1 ? lessons[currentLessonIndex + 1].Id : (int?)null,

                LessonDetails = new LessonDetailsForStudentViewModel
                {
                    Attachments = currentLesson.Attachments.ToList()
                }
            };

            return View(viewModel);
        }
        // POST: /CoursePlayer/SubmitHomework
        [HttpPost("SubmitHomework")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitHomework(int courseId, int lessonId, IFormFile SubmittedFile)
        {
            if (SubmittedFile == null || SubmittedFile.Length == 0)
            {
                TempData["ErrorMessage"] = "الرجاء اختيار ملف لرفعه.";
                return RedirectToAction("Index", new { courseId, lessonId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (student == null) return Unauthorized();

            var filePath = await _fileService.SaveFileAsync(SubmittedFile, "submissions");
        
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تسليم واجبك بنجاح!";
            return RedirectToAction("Index", new { courseId, lessonId });
        }
    }
}
