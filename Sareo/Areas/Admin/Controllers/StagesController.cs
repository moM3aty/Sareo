using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StagesController : Controller
    {
        private readonly PlatformDbContext _context;
        public StagesController(PlatformDbContext context) { _context = context; }

        public IActionResult CreatePartial() => PartialView("_CreatePartial", new Stage());

        // GET: Admin/Stages/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Stage stage)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            // Check for duplicates
            if (await _context.Stages.AnyAsync(s => s.Name == stage.Name))
            {
                ModelState.AddModelError("Name", "هذه المرحلة الدراسية موجودة بالفعل.");
            }
            ModelState.Remove("Grades");

            if (ModelState.IsValid)
            {
                _context.Add(stage);
                await _context.SaveChangesAsync();
                if (isAjax) return Json(new { success = true, id = stage.Id, text = stage.Name });
                return RedirectToAction(nameof(Index));
            }

            if (isAjax)
            {
                var errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault());
                return Json(new { success = false, errors = errors });
            }
            return View(stage);
        }

        // GET: Admin/Stages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var stage = await _context.Stages.FindAsync(id);
            if (stage == null) return NotFound();
            return View(stage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Stage stage)
        {
            if (id != stage.Id) return NotFound();

            // Check for duplicates, excluding the current item
            if (await _context.Stages.AnyAsync(s => s.Name == stage.Name && s.Id != id))
            {
                ModelState.AddModelError("Name", "هذه المرحلة الدراسية موجودة بالفعل.");
            }
            ModelState.Remove("Grades");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Stages.Any(e => e.Id == stage.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(stage);
        }

        #region Other Actions
        public async Task<IActionResult> Index() { var stages = await _context.Stages.Select(s => new StageIndexViewModel { Id = s.Id, Name = s.Name, GradesCount = s.Grades.Count() }).ToListAsync(); return View(stages); }
        public async Task<IActionResult> Details(int? id) { if (id == null) return NotFound(); var stage = await _context.Stages.Include(s => s.Grades).FirstOrDefaultAsync(m => m.Id == id); if (stage == null) return NotFound(); return View(stage); }
        public async Task<IActionResult> Delete(int? id) { if (id == null) return NotFound(); var stage = await _context.Stages.FirstOrDefaultAsync(m => m.Id == id); if (stage == null) return NotFound(); return View(stage); }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stage = await _context.Stages.FindAsync(id);
            if (stage != null)
            {
                try
                {
                    _context.Stages.Remove(stage);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "لا يمكن حذف هذه المرحلة لأنها (أو أحد صفوفها) مرتبطة ببيانات أخرى. يرجى إزالة الارتباطات أولاً.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
