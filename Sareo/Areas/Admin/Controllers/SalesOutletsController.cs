using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SalesOutletsController : Controller
    {
        private readonly PlatformDbContext _context;

        public SalesOutletsController(PlatformDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var outletsQuery = _context.SalesOutlets.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                outletsQuery = outletsQuery.Where(o => o.BookstoreName.Contains(searchString) || o.Governorate.Contains(searchString));
            }

            var groupedOutlets = await outletsQuery
                .GroupBy(o => o.Governorate)
                .Select(g => new SalesOutletGroupedViewModel
                {
                    Governorate = g.Key,
                    Outlets = g.OrderBy(o => o.BookstoreName).ToList()
                })
                .OrderBy(g => g.Governorate)
                .ToListAsync();

            return View(groupedOutlets);
        }

        private async Task<SelectList> GetGovernoratesSelectList()
        {
            var governorates = await _context.SalesOutlets
                .Select(o => o.Governorate)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();
            return new SelectList(governorates);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new SalesOutletFormViewModel
            {
                SalesOutlet = new SalesOutlet(),
                Governorates = await GetGovernoratesSelectList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesOutletFormViewModel viewModel)
        {
            ModelState.Remove("SalesOutlet.AvailableMaterials");
            ModelState.Remove("NewGovernorate");
            ModelState.Remove("AvailableMaterials");
            ModelState.Remove("SalesOutlet.Governorate");
            ModelState.Remove("Governorates");

            if (!string.IsNullOrWhiteSpace(viewModel.NewGovernorate))
            {
                viewModel.SalesOutlet.Governorate = viewModel.NewGovernorate;
            }

            if (ModelState.IsValid)
            {
                _context.Add(viewModel.SalesOutlet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Governorates = await GetGovernoratesSelectList();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var salesOutlet = await _context.SalesOutlets.FindAsync(id);
            if (salesOutlet == null) return NotFound();

            var viewModel = new SalesOutletFormViewModel
            {
                SalesOutlet = salesOutlet,
                Governorates = await GetGovernoratesSelectList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SalesOutletFormViewModel viewModel)
        {
            if (id != viewModel.SalesOutlet.Id) return NotFound();

            ModelState.Remove("SalesOutlet.AvailableMaterials");
            ModelState.Remove("NewGovernorate");
            ModelState.Remove("AvailableMaterials");
            ModelState.Remove("Governorates");

            if (!string.IsNullOrWhiteSpace(viewModel.NewGovernorate))
            {
                viewModel.SalesOutlet.Governorate = viewModel.NewGovernorate;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewModel.SalesOutlet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SalesOutlets.Any(e => e.Id == viewModel.SalesOutlet.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            viewModel.Governorates = await GetGovernoratesSelectList();
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var salesOutlet = await _context.SalesOutlets.FirstOrDefaultAsync(m => m.Id == id);
            if (salesOutlet == null) return NotFound();
            return View(salesOutlet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesOutlet = await _context.SalesOutlets.FindAsync(id);
            _context.SalesOutlets.Remove(salesOutlet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
