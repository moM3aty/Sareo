using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Helpers;
using Sareoo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class LiveLessonsController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;
        private readonly TimeZoneInfo _localZone;

        public LiveLessonsController(PlatformDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
            try
            {
                _localZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
            }
            catch
            {
                _localZone = TimeZoneInfo.Local;
            }
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var nowUtc = DateTime.UtcNow;

            var allLessonsQuery = _context.LiveLessons.AsQueryable();

            if (User.IsInRole("Teacher"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var teacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.ApplicationUserId == userId);
                if (teacher != null)
                {
                    allLessonsQuery = allLessonsQuery.Where(l => l.TeacherId == teacher.Id);
                }
                else
                {
                    allLessonsQuery = allLessonsQuery.Where(l => false); 
                }
            }

            var allLessons = await allLessonsQuery
                .Include(l => l.Teacher)
                .Include(l => l.Subject)
                .OrderByDescending(l => l.StartTime)
                .ToListAsync();

            var viewModel = new LiveLessonIndexViewModel
            {
                UpcomingLessons = allLessons.Where(l => l.StartTime > nowUtc).Select(MapToListItemViewModel),
                LiveNowLessons = allLessons.Where(l => l.StartTime <= nowUtc && l.StartTime.AddMinutes(l.DurationMinutes) > nowUtc).Select(MapToListItemViewModel),
                RecordedLessons = allLessons.Where(l => l.StartTime.AddMinutes(l.DurationMinutes) <= nowUtc).Select(MapToListItemViewModel)
            };

            return View(viewModel);
        }

        private LiveLessonListItemViewModel MapToListItemViewModel(LiveLesson lesson)
        {
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(lesson.StartTime, _localZone);
            return new LiveLessonListItemViewModel
            {
                Id = lesson.Id,
                Title = lesson.Title,
                TeacherName = lesson.Teacher.FullName,
                SubjectName = lesson.Subject.Name,
                StartTime = localTime,
                StartTimeFormatted = ArabicNumberHelper.ToArabicNumerals(localTime.ToString("g", new CultureInfo("ar-EG"))),
                DurationMinutes = lesson.DurationMinutes,
                CoverImageUrl = lesson.CoverImageUrl,
                MeetingUrl = lesson.MeetingUrl
            };
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new LiveLessonFormViewModel
            {
                LiveLesson = new LiveLesson { StartTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddDays(1), _localZone) },
                Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName"),
                Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name"),
                Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LiveLessonFormViewModel viewModel)
        {
            var lessonStartTimeUtc = TimeZoneInfo.ConvertTimeToUtc(viewModel.LiveLesson.StartTime, _localZone);
            if (lessonStartTimeUtc < DateTime.UtcNow)
            {
                ModelState.AddModelError("LiveLesson.StartTime", "لا يمكن جدولة درس بتاريخ قديم.");
            }

            ModelState.Remove("LiveLesson.Subject");
            ModelState.Remove("LiveLesson.Teacher");
            ModelState.Remove("LiveLesson.Grade");
            ModelState.Remove("LiveLesson.CoverImageUrl");
            ModelState.Remove("Grades");
            ModelState.Remove("Subjects");
            ModelState.Remove("Teachers");
            ModelState.Remove("LiveLesson.LiveLessonReminders");

            if (ModelState.IsValid)
            {
                var liveLesson = viewModel.LiveLesson;
                if (viewModel.CoverImage != null)
                {
                    liveLesson.CoverImageUrl = await _fileService.SaveFileAsync(viewModel.CoverImage, "livelessons");
                }
                liveLesson.StartTime = lessonStartTimeUtc;

                _context.Add(liveLesson);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName", viewModel.LiveLesson.TeacherId);
            viewModel.Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", viewModel.LiveLesson.SubjectId);
            viewModel.Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", viewModel.LiveLesson.GradeId);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var liveLesson = await _context.LiveLessons.FindAsync(id);
            if (liveLesson == null) return NotFound();

            liveLesson.StartTime = TimeZoneInfo.ConvertTimeFromUtc(liveLesson.StartTime, _localZone);

            var viewModel = new LiveLessonFormViewModel
            {
                LiveLesson = liveLesson,
                Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName", liveLesson.TeacherId),
                Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", liveLesson.SubjectId),
                Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", liveLesson.GradeId)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LiveLessonFormViewModel viewModel)
        {
            if (id != viewModel.LiveLesson.Id) return NotFound();

            var lessonStartTimeUtc = TimeZoneInfo.ConvertTimeToUtc(viewModel.LiveLesson.StartTime, _localZone);

            ModelState.Remove("LiveLesson.Subject");
            ModelState.Remove("LiveLesson.Teacher");
            ModelState.Remove("LiveLesson.Grade");
            ModelState.Remove("LiveLesson.CoverImageUrl");
            ModelState.Remove("CoverImage");
            ModelState.Remove("Grades");
            ModelState.Remove("Subjects");
            ModelState.Remove("Teachers");
            ModelState.Remove("LiveLesson.LiveLessonReminders");

            if (ModelState.IsValid)
            {
                var lessonToUpdate = await _context.LiveLessons.FindAsync(id);
                if (lessonToUpdate == null) return NotFound();

                if (viewModel.CoverImage != null)
                {
                    _fileService.DeleteFile(lessonToUpdate.CoverImageUrl);
                    lessonToUpdate.CoverImageUrl = await _fileService.SaveFileAsync(viewModel.CoverImage, "livelessons");
                }

                lessonToUpdate.Title = viewModel.LiveLesson.Title;
                lessonToUpdate.Description = viewModel.LiveLesson.Description;
                lessonToUpdate.StartTime = lessonStartTimeUtc;
                lessonToUpdate.DurationMinutes = viewModel.LiveLesson.DurationMinutes;
                lessonToUpdate.MeetingUrl = viewModel.LiveLesson.MeetingUrl;
                lessonToUpdate.TeacherId = viewModel.LiveLesson.TeacherId;
                lessonToUpdate.SubjectId = viewModel.LiveLesson.SubjectId;
                lessonToUpdate.GradeId = viewModel.LiveLesson.GradeId;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName", viewModel.LiveLesson.TeacherId);
            viewModel.Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", viewModel.LiveLesson.SubjectId);
            viewModel.Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", viewModel.LiveLesson.GradeId);
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var liveLesson = await _context.LiveLessons
                .Include(l => l.Teacher)
                .Include(l => l.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (liveLesson == null) return NotFound();
            return View(liveLesson);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var liveLesson = await _context.LiveLessons
                .Include(l => l.LiveLessonReminders)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (liveLesson != null)
            {
                _context.LiveLessonReminders.RemoveRange(liveLesson.LiveLessonReminders);
                _fileService.DeleteFile(liveLesson.CoverImageUrl);
                _context.LiveLessons.Remove(liveLesson);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
