using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class SubjectsController : Controller
    {
        private readonly PlatformDbContext _context;
        public SubjectsController(PlatformDbContext context) { _context = context; }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var subjectsQuery = _context.Subjects.Include(s => s.Grades).AsQueryable();

            if (User.IsInRole("Teacher"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var teacher = await _context.Teachers
                    .Include(t => t.Subjects)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.ApplicationUserId == userId);

                if (teacher != null)
                {
                    var teacherSubjectIds = teacher.Subjects.Select(s => s.Id).ToList();
                    subjectsQuery = subjectsQuery.Where(s => teacherSubjectIds.Contains(s.Id));
                }
                else
                {
                    subjectsQuery = subjectsQuery.Where(s => false); 
                }
            }

            var projectedQuery = subjectsQuery.Select(s => new SubjectIndexViewModel
            {
                Id = s.Id,
                Name = s.Name,
                GradeNames = s.Grades.Select(g => g.Name).ToList()
            });

            if (!string.IsNullOrEmpty(searchString))
            {
                projectedQuery = projectedQuery.Where(s => s.Name.Contains(searchString));
            }

            var subjects = await projectedQuery.ToListAsync();
            return View(subjects);
        }

        public async Task<IActionResult> Create()
        {
            var allGrades = await _context.Grades.ToListAsync();
            var viewModel = new SubjectFormViewModel
            {
                Subject = new Subject(),
                Grades = allGrades.Select(g => new AssignedGradeViewModel
                {
                    GradeId = g.Id,
                    Name = g.Name,
                    IsAssigned = false
                }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectFormViewModel viewModel)
        {
            ModelState.Remove("Subject.Grade");
            ModelState.Remove("Subject.Grades");
            ModelState.Remove("Subject.Teachers");
            ModelState.Remove("Subject.Courses");
            ModelState.Remove("Subject.LiveLessons");
            ModelState.Remove("Subject.LessonMaterials");
            ModelState.Remove("Subject.EducationalMaterials");
            ModelState.Remove("CoverImage");
            if (await _context.Subjects.AnyAsync(s => s.Name == viewModel.Subject.Name))
            {
                ModelState.AddModelError("Subject.Name", "هذه المادة موجودة بالفعل.");
            }

            if (ModelState.IsValid)
            {

                var subject = viewModel.Subject;
                subject.Grades = new List<Grade>();
                if (viewModel.Grades != null)
                {
                    foreach (var gradeVM in viewModel.Grades.Where(g => g.IsAssigned))
                    {
                        var grade = await _context.Grades.FindAsync(gradeVM.GradeId);
                        if (grade != null) subject.Grades.Add(grade);
                    }
                }
                _context.Add(subject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var subject = await _context.Subjects.Include(s => s.Grades).FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null) return NotFound();

            var allGrades = await _context.Grades.ToListAsync();
            var viewModel = new SubjectFormViewModel
            {
                Subject = subject,
                Grades = allGrades.Select(g => new AssignedGradeViewModel
                {
                    GradeId = g.Id,
                    Name = g.Name,
                    IsAssigned = subject.Grades.Any(sg => sg.Id == g.Id)
                }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectFormViewModel viewModel)
        {

            if (id != viewModel.Subject.Id) return NotFound();
            ModelState.Remove("Subject.Grade");
            ModelState.Remove("Subject.Grades");
            ModelState.Remove("Subject.Teachers");
            ModelState.Remove("Subject.Courses");
            ModelState.Remove("Subject.LiveLessons");
            ModelState.Remove("Subject.EducationalMaterials");
            ModelState.Remove("Subject.LessonMaterials");
            ModelState.Remove("CoverImage");


            if (ModelState.IsValid)
            {
                var subjectToUpdate = await _context.Subjects.Include(s => s.Grades).FirstOrDefaultAsync(s => s.Id == id);
                if (subjectToUpdate == null) return NotFound();

                subjectToUpdate.Name = viewModel.Subject.Name;
                subjectToUpdate.Grades.Clear();
                if (viewModel.Grades != null)
                {
                    foreach (var gradeVM in viewModel.Grades.Where(g => g.IsAssigned))
                    {
                        var grade = await _context.Grades.FindAsync(gradeVM.GradeId);
                        if (grade != null) subjectToUpdate.Grades.Add(grade);
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }
        
        public async Task<IActionResult> Delete(int? id) { if (id == null) return NotFound(); var subject = await _context.Subjects.FirstOrDefaultAsync(m => m.Id == id); if (subject == null) return NotFound(); return View(subject); }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) { var subject = await _context.Subjects.FindAsync(id); if (subject != null) { _context.Subjects.Remove(subject); await _context.SaveChangesAsync(); } return RedirectToAction(nameof(Index)); }
    }
}
