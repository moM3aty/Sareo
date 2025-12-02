using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sareoo.Controllers
{
    [Authorize(Roles = "Student")]
    public class LiveLessonsController : Controller
    {
        private readonly PlatformDbContext _context;
        public LiveLessonsController(PlatformDbContext context) { _context = context; }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetReminder(int lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (student == null) return Unauthorized();

            var lesson = await _context.LiveLessons.FindAsync(lessonId);
            if (lesson == null) return NotFound();

            var existingReminder = await _context.LiveLessonReminders
                .AnyAsync(r => r.StudentId == student.Id && r.LiveLessonId == lessonId);

            if (!existingReminder)
            {
                _context.LiveLessonReminders.Add(new LiveLessonReminder { StudentId = student.Id, LiveLessonId = lessonId });
                lesson.RemindersCount++;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true, message = "تم ضبط التذكير بنجاح!" });
        }
    }
}
