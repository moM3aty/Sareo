using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Models;
using Sareoo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sareoo.Controllers
{
    public class TeacherProfileController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherProfileController(PlatformDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /TeacherProfile/Index/5
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers
                .Include(t => t.Courses).ThenInclude(c => c.Subject)
                .Include(t => t.Courses).ThenInclude(c => c.Lessons)
                .Include(t => t.Courses).ThenInclude(c => c.StudentCourses)
                .Include(t => t.Ratings).ThenInclude(r => r.Student)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null) return NotFound();

            var viewModel = new TeacherProfileViewModel
            {
                Teacher = teacher,
                Courses = teacher.Courses.Select(c => new CourseInfoViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    CoverImageUrl = c.CoverImageUrl,
                    SubjectName = c.Subject.Name,
                    LessonsCount = c.Lessons.Count,
                    TotalHours = c.TotalHours
                }).ToList(),
                TotalCourses = teacher.Courses.Count,
                TotalStudents = teacher.Courses.SelectMany(c => c.StudentCourses).Select(sc => sc.StudentId).Distinct().Count(),
                AverageRating = teacher.Ratings.Any() ? teacher.Ratings.Average(r => r.Rating) : 0,
                RatingsCount = teacher.Ratings.Count,
                RecentRatings = teacher.Ratings.OrderByDescending(r => r.RatingDate).Take(5).ToList(),
                NewRating = new SubmitRatingViewModel { TeacherId = teacher.Id }
            };

            return View(viewModel);
        }

        // POST: /TeacherProfile/SubmitRating
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")] // Ensure only students can submit
        public async Task<IActionResult> SubmitRating(SubmitRatingViewModel newRating)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

                if (student == null) return Unauthorized();

                var existingRating = await _context.TeacherRatings
                    .FirstOrDefaultAsync(r => r.TeacherId == newRating.TeacherId && r.StudentId == student.Id);

                if (existingRating != null)
                {
                    existingRating.Rating = newRating.Rating;
                    existingRating.Comment = newRating.Comment;
                    existingRating.RatingDate = DateTime.UtcNow;
                }
                else
                {
                    var rating = new TeacherRating
                    {
                        TeacherId = newRating.TeacherId,
                        StudentId = student.Id,
                        Rating = newRating.Rating,
                        Comment = newRating.Comment,
                        RatingDate = DateTime.UtcNow
                    };
                    _context.TeacherRatings.Add(rating);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { id = newRating.TeacherId });
        }
    }
}
