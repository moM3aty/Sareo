using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Models;
using Sareoo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sareoo.Controllers
{
    [Authorize(Roles = "Student")]
    public class ProfileController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(PlatformDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var student = await _context.Students
                .Include(s => s.Grade)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (student == null) return NotFound("Profile not found.");

            var studentCourses = await _context.StudentCourses
                .Where(sc => sc.StudentId == student.Id && sc.Course != null)
                .Include(sc => sc.Course).ThenInclude(c => c.Teacher)
                .Include(sc => sc.Course).ThenInclude(c => c.Lessons)
                .AsNoTracking()
                .ToListAsync();

            var certificates = await _context.Certificates
                .Where(c => c.StudentId == student.Id && c.Course != null)
                .Include(c => c.Course).ThenInclude(c => c.Teacher)
                .AsNoTracking()
                .ToListAsync();

            var subscribedCourseIds = studentCourses.Select(sc => sc.CourseId).ToList();
            var availableCourses = await _context.Courses
                .Where(c => c.GradeId == student.GradeId)
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .AsNoTracking()
                .Select(c => new AvailableCourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    TeacherName = c.Teacher.FullName,
                    CoverImageUrl = c.CoverImageUrl,
                    LessonsCount = c.Lessons.Count(),
                    TotalHours = c.TotalHours,
                    IsSubscribed = subscribedCourseIds.Contains(c.Id)
                }).ToListAsync();

            var now = DateTime.UtcNow;
            var subjectIdsForGrade = await _context.Subjects
                .Where(s => s.Grades.Any(g => g.Id == student.GradeId))
                .Select(s => s.Id)
                .ToListAsync();
            var allGrades = await _context.Grades.AsNoTracking().ToListAsync();

            var allLiveLessonsForGrade = await _context.LiveLessons
                .Where(l => subjectIdsForGrade.Contains(l.SubjectId))
                .Include(l => l.Teacher)
                .Include(l => l.Subject)
                .AsNoTracking()
                .OrderByDescending(l => l.StartTime)
                .ToListAsync();

            var studentReminders = await _context.LiveLessonReminders
                .Where(r => r.StudentId == student.Id)
                .Select(r => r.LiveLessonId)
                .ToListAsync();

            var liveLessonsViewModel = allLiveLessonsForGrade.Select(l => new UserLiveLessonViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                TeacherName = l.Teacher.FullName,
                SubjectName = l.Subject.Name,
                GradeName = student.Grade.Name,
                StartTime = l.StartTime,
                DurationMinutes = l.DurationMinutes,
                MaxStudents = l.MaxStudents,
                CoverImageUrl = l.CoverImageUrl,
                MeetingUrl = l.MeetingUrl,
                Status = now >= l.StartTime && now < l.StartTime.AddMinutes(l.DurationMinutes) ? LiveLessonStatus.Live :
                         now < l.StartTime ? LiveLessonStatus.Upcoming : LiveLessonStatus.Recorded,
                RemindersCount = l.RemindersCount,
                IsReminderSet = studentReminders.Contains(l.Id)
            }).ToList();

            var availableBooks = await _context.Books
                .Where(b => b.GradeId == student.GradeId)
                .Include(b => b.Teacher)
                .Include(b => b.Subject)
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new UserProfileViewModel
            {
                FullName = student.FullName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                City = student.City,
                Country = student.Country,
                GradeName = student.Grade?.Name,
                RegistrationDate = student.RegistrationDate,
                CompletedCoursesCount = studentCourses.Count(sc => sc.ProgressPercentage >= 100),
                InProgressCoursesCount = studentCourses.Count(sc => sc.ProgressPercentage < 100),
                CertificatesCount = certificates.Count,
                TotalLearningHours = Math.Round(studentCourses.Sum(sc => (sc.ProgressPercentage / 100.0) * (sc.Course?.TotalHours ?? 0)), 1),
                AverageGrade = certificates.Any() ? Math.Round(certificates.Average(c => c.GradePercentage)) : 0,
                OverallProgressPercentage = studentCourses.Any() ? (int)Math.Round(studentCourses.Average(sc => sc.ProgressPercentage)) : 0,

                InProgressCourses = studentCourses
                    .Where(sc => sc.ProgressPercentage < 100)
                    .Select(sc => new UserCourseViewModel
                    {
                        Id = sc.CourseId,
                        Title = sc.Course.Title,
                        TeacherName = sc.Course.Teacher?.FullName ?? "معلم غير محدد",
                        CoverImageUrl = sc.Course.CoverImageUrl,
                        ProgressPercentage = sc.ProgressPercentage,
                        Grade = GetGradeFromPercentage(certificates.FirstOrDefault(c => c.CourseId == sc.CourseId)?.GradePercentage)
                    }).ToList(),

                CompletedCourses = studentCourses
                    .Where(sc => sc.ProgressPercentage >= 100)
                    .Select(sc => new UserCourseViewModel
                    {
                        Id = sc.CourseId,
                        Title = sc.Course.Title,
                        TeacherName = sc.Course.Teacher?.FullName ?? "معلم غير محدد",
                        CoverImageUrl = sc.Course.CoverImageUrl,
                        ProgressPercentage = 100,
                        Grade = GetGradeFromPercentage(certificates.FirstOrDefault(c => c.CourseId == sc.CourseId)?.GradePercentage)
                    }).ToList(),


                Certificates = certificates
                    .Select(c => new UserCertificateViewModel
                    {
                        Id = c.Id,
                        CourseTitle = c.Course.Title,
                        TeacherName = c.Course.Teacher?.FullName ?? "معلم غير محدد",
                        GradePercentage = c.GradePercentage,
                        IssueDate = c.IssueDate,
                        CertificateGuid = c.CertificateGuid
                    }).ToList(),

                AvailableCourses = availableCourses,
                LiveNowLessons = liveLessonsViewModel.Where(l => l.Status == LiveLessonStatus.Live).ToList(),
                UpcomingLessons = liveLessonsViewModel.Where(l => l.Status == LiveLessonStatus.Upcoming && l.StartTime > now).ToList(),
                AvailableBooks = availableBooks,
                EditProfileViewModel = new EditProfileViewModel
                {
                    FullName = student.FullName,
                    PhoneNumber = student.PhoneNumber,
                    City = student.City,
                    Country = student.Country,
                    GradeId = student.GradeId,
                    Grades = new SelectList(allGrades, "Id", "Name", student.GradeId)
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfileViewModel model)
        {
            ModelState.Remove("City");
            ModelState.Remove("Email");
            ModelState.Remove("Country");
            ModelState.Remove("InProgressCourses");
            ModelState.Remove("CompletedCourses");
            ModelState.Remove("UpcomingLessons");
            ModelState.Remove("LiveNowLessons");
            ModelState.Remove("Certificates");
            ModelState.Remove("PhoneNumber");
            ModelState.Remove("GradeName");
            ModelState.Remove("FullName");
            ModelState.Remove("AvailableCourses");
            ModelState.Remove("AvailableBooks");
            ModelState.Remove("EditProfileViewModel.Grades");
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                var user = await _userManager.FindByIdAsync(userId);

                if (student != null && user != null)
                {
                    student.FullName = model.EditProfileViewModel.FullName;
                    student.PhoneNumber = model.EditProfileViewModel.PhoneNumber;
                    student.City = model.EditProfileViewModel.City;
                    student.GradeId = model.EditProfileViewModel.GradeId;
                    student.Country = model.EditProfileViewModel.Country;
                    user.FullName = model.EditProfileViewModel.FullName;
                    user.PhoneNumber = model.EditProfileViewModel.PhoneNumber;

                    await _context.SaveChangesAsync();
                    await _userManager.UpdateAsync(user);

                    TempData["SuccessMessage"] = "تم تحديث ملفك الشخصي بنجاح!";
                    return RedirectToAction("Index");
                }
            }

            TempData["ShowEditMode"] = true;
            return await Index();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Subscribe(int courseId)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

        //    if (student == null) return Unauthorized();

        //    var isAlreadySubscribed = await _context.StudentCourses
        //        .AnyAsync(sc => sc.StudentId == student.Id && sc.CourseId == courseId);

        //    if (!isAlreadySubscribed)
        //    {
        //        var studentCourse = new StudentCourse
        //        {
        //            StudentId = student.Id,
        //            CourseId = courseId,
        //            ProgressPercentage = 0,
        //            LastAccessDate = null
        //        };
        //        _context.StudentCourses.Add(studentCourse);
        //        await _context.SaveChangesAsync();
        //    }

        //    return RedirectToAction("Index");
        //}

        private string GetGradeFromPercentage(int? percentage)
        {
            if (!percentage.HasValue) return "--";
            if (percentage >= 95) return "A+";
            if (percentage >= 90) return "A";
            if (percentage >= 85) return "B+";
            if (percentage >= 80) return "B";
            if (percentage >= 75) return "C+";
            if (percentage >= 70) return "C";
            if (percentage >= 65) return "D+";
            if (percentage >= 60) return "D";
            return "F";
        }
    }
}
