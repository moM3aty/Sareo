using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class CoursesController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;

        public CoursesController(PlatformDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: Admin/Courses
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var coursesQuery = _context.Courses.AsQueryable();

            if (User.IsInRole("Teacher"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var teacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.ApplicationUserId == userId);
                if (teacher != null)
                {
                    coursesQuery = coursesQuery.Where(c => c.TeacherId == teacher.Id);
                }
                else
                {
                    coursesQuery = coursesQuery.Where(c => false); 
                }
            }

            var projectedQuery = coursesQuery
                .Include(c => c.Teacher)
                .Include(c => c.Subject)
                .Include(c => c.Grade)
                .Include(c => c.Lessons)
                .Select(c => new CourseIndexViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    TeacherName = c.Teacher.FullName,
                    SubjectName = c.Subject.Name,
                    GradeName = c.Grade.Name,
                    CoverImageUrl = c.CoverImageUrl,
                    LessonsCount = c.Lessons.Count()
                });

            if (!String.IsNullOrEmpty(searchString))
            {
                projectedQuery = projectedQuery.Where(c => c.Title.Contains(searchString)
                                              || c.TeacherName.Contains(searchString)
                                              || c.SubjectName.Contains(searchString)
                                              || c.GradeName.Contains(searchString));
            }

            var courses = await projectedQuery.ToListAsync();
            return View(courses);
        }

        // GET: Admin/Courses/Create
        public IActionResult Create()
        {
            var viewModel = new CourseFormViewModel
            {
                Course = new Course(),
                Teachers = new SelectList(_context.Teachers, "Id", "FullName"),
                Subjects = new SelectList(_context.Subjects, "Id", "Name"),
                Grades = new SelectList(_context.Grades, "Id", "Name")
            };
            return View(viewModel);
        }

        // POST: Admin/Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseFormViewModel viewModel)
        {
            ModelState.Remove("Subjects");
            ModelState.Remove("Teachers");
            ModelState.Remove("Grades");
            ModelState.Remove("Course.Grade");
            ModelState.Remove("Course.Exam");
            ModelState.Remove("Course.Lessons");
            ModelState.Remove("Course.Subject");
            ModelState.Remove("Course.Certificates");
            ModelState.Remove("Course.Teacher");
            ModelState.Remove("Course.StudentCourses");
            ModelState.Remove("Course.CoverImageUrl");

            if (ModelState.IsValid)
            {
                var course = viewModel.Course;
                if (viewModel.CoverImage != null)
                {
                    course.CoverImageUrl = await _fileService.SaveFileAsync(viewModel.CoverImage, "courses");
                }
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            viewModel.Teachers = new SelectList(_context.Teachers, "Id", "FullName", viewModel.Course.TeacherId);
            viewModel.Subjects = new SelectList(_context.Subjects, "Id", "Name", viewModel.Course.SubjectId);
            viewModel.Grades = new SelectList(_context.Grades, "Id", "Name", viewModel.Course.GradeId);
            return View(viewModel);
        }

        // GET: Admin/Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var viewModel = new CourseFormViewModel
            {
                Course = course,
                Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName", course.TeacherId),
                Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", course.SubjectId),
                Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", course.GradeId)
            };

            return View(viewModel);
        }

        // POST: Admin/Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseFormViewModel viewModel)
        {
            if (id != viewModel.Course.Id) return NotFound();

            ModelState.Remove("CoverImage");
            ModelState.Remove("Subjects");
            ModelState.Remove("Teachers");
            ModelState.Remove("Grades");
            ModelState.Remove("Course.Grade");
            ModelState.Remove("Course.Exam");
            ModelState.Remove("Course.Lessons");
            ModelState.Remove("Course.Subject");
            ModelState.Remove("Course.Certificates");
            ModelState.Remove("Course.Teacher");
            ModelState.Remove("Course.StudentCourses");
            ModelState.Remove("Course.CoverImageUrl");
            if (ModelState.IsValid)
            {
                var courseToUpdate = await _context.Courses.FindAsync(id);
                if (courseToUpdate == null) return NotFound();

                if (viewModel.CoverImage != null)
                {
                    _fileService.DeleteFile(courseToUpdate.CoverImageUrl);
                    courseToUpdate.CoverImageUrl = await _fileService.SaveFileAsync(viewModel.CoverImage, "courses");
                }

                courseToUpdate.Title = viewModel.Course.Title;
                courseToUpdate.Description = viewModel.Course.Description;
                courseToUpdate.TotalHours = viewModel.Course.TotalHours;
                courseToUpdate.TeacherId = viewModel.Course.TeacherId;
                courseToUpdate.SubjectId = viewModel.Course.SubjectId;
                courseToUpdate.GradeId = viewModel.Course.GradeId; // Make sure to update GradeId

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Courses.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            viewModel.Teachers = new SelectList(_context.Teachers, "Id", "FullName", viewModel.Course.TeacherId);
            viewModel.Subjects = new SelectList(_context.Subjects, "Id", "Name", viewModel.Course.SubjectId);
            viewModel.Grades = new SelectList(_context.Grades, "Id", "Name", viewModel.Course.GradeId);
            return View(viewModel);
        }

        // GET: Admin/Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }

        // POST: Admin/Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _fileService.DeleteFile(course.CoverImageUrl);
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
