
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GradesController : Controller
    {
        private readonly PlatformDbContext _context;
        public GradesController(PlatformDbContext context) { _context = context; }

        // GET: Admin/Grades
        public async Task<IActionResult> Index()
        {
            var gradesByStage = await _context.Stages
                .Include(s => s.Grades)
                .OrderBy(s => s.Id)
                .Select(s => new GradeGroupedViewModel
                {
                    StageName = s.Name,
                    Grades = s.Grades.OrderBy(g => g.Id).ToList()
                })
                .ToListAsync();

            return View(gradesByStage);
        }

        // NEW ACTION: Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return NotFound();

            var viewModel = new GradeDetailsViewModel
            {
                Grade = grade,
                Courses = await _context.Courses.Where(c => c.GradeId == id).Include(c => c.Teacher).ToListAsync(),
                Books = await _context.Books.Where(b => b.GradeId == id).Include(b => b.Subject).ToListAsync(),
                EducationalMaterials = await _context.EducationalMaterials.Where(em => em.GradeId == id).Include(em => em.Subject).ToListAsync(),
                LiveLessons = await _context.LiveLessons.Where(ll => ll.GradeId == id).Include(ll => ll.Teacher).ToListAsync()
            };

            return View(viewModel);
        }

        #region Other Actions
        public async Task<IActionResult> Create()
        {
            var viewModel = new GradeFormViewModel
            {
                Grade = new Grade(),
                Stages = new SelectList(await _context.Stages.ToListAsync(), "Id", "Name")
            };
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GradeFormViewModel viewModel)
        {
            if (await _context.Grades.AnyAsync(g => g.Name == viewModel.Grade.Name && g.StageId == viewModel.Grade.StageId))
            {
                ModelState.AddModelError("Grade.Name", "هذا الصف موجود بالفعل في نفس المرحلة الدراسية.");
            }

            ModelState.Remove("Stages");
            ModelState.Remove("Grade.EducationalMaterials");
            ModelState.Remove("Grade.Students");
            ModelState.Remove("Grade.Subjects");
            ModelState.Remove("Grade.Stage");
            if (ModelState.IsValid)
            {
                _context.Add(viewModel.Grade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            viewModel.Stages = new SelectList(await _context.Stages.ToListAsync(), "Id", "Name", viewModel.Grade.StageId);
            return View(viewModel);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return NotFound();
            var viewModel = new GradeFormViewModel
            {
                Grade = grade,
                Stages = new SelectList(await _context.Stages.ToListAsync(), "Id", "Name", grade.StageId)
            };
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GradeFormViewModel viewModel)
        {
            if (id != viewModel.Grade.Id) return NotFound();
            if (await _context.Grades.AnyAsync(g => g.Name == viewModel.Grade.Name && g.StageId == viewModel.Grade.StageId && g.Id != id))
            {
                ModelState.AddModelError("Grade.Name", "هذا الصف موجود بالفعل في نفس المرحلة الدراسية.");
            }

            ModelState.Remove("Stages");
            ModelState.Remove("Grade.Students");
            ModelState.Remove("Grade.Subjects");
            ModelState.Remove("Grade.EducationalMaterials");
            ModelState.Remove("Grade.Stage");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewModel.Grade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Grades.Any(e => e.Id == viewModel.Grade.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            viewModel.Stages = new SelectList(await _context.Stages.ToListAsync(), "Id", "Name", viewModel.Grade.StageId);
            return View(viewModel);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var grade = await _context.Grades.Include(g => g.Stage).FirstOrDefaultAsync(m => m.Id == id);
            if (grade == null) return NotFound();
            return View(grade);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade != null)
            {
                try
                {
                    _context.Grades.Remove(grade);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "لا يمكن حذف هذا الصف لأنه مرتبط ببيانات أخرى (مثل المواد التعليمية أو الطلاب). يرجى إزالة الارتباطات أولاً.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
