using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EducationalMaterialsController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;

        public EducationalMaterialsController(PlatformDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: Admin/EducationalMaterials
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var materialsQuery = _context.EducationalMaterials
                .Include(e => e.Grade)
                .Include(e => e.Subject)
                .Select(e => new EducationalMaterialIndexViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    CoverImageUrl = e.CoverImageUrl,
                    GradeName = e.Grade.Name,
                    SubjectName = e.Subject.Name
                });

            if (!String.IsNullOrEmpty(searchString))
            {
                materialsQuery = materialsQuery.Where(m => m.Title.Contains(searchString)
                                                        || m.GradeName.Contains(searchString)
                                                        || m.SubjectName.Contains(searchString));
            }

            var materials = await materialsQuery.ToListAsync();
            return View(materials);
        }

        // GET: Admin/EducationalMaterials/Create
        public async Task<IActionResult> Create()
        {
            var allOutlets = await _context.SalesOutlets.OrderBy(o => o.Governorate).ThenBy(o => o.BookstoreName).ToListAsync();
            var viewModel = new EducationalMaterialFormViewModel
            {
                EducationalMaterial = new EducationalMaterial(),
                Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name"),
                Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name"),
                SalesOutlets = allOutlets.Select(o => new AssignedSalesOutletViewModel
                {
                    OutletId = o.Id,
                    Name = $"{o.BookstoreName} - {o.Governorate}",
                    IsAssigned = false
                }).ToList()
            };
            return View(viewModel);
        }

        // POST: Admin/EducationalMaterials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EducationalMaterialFormViewModel viewModel)
        {
            ModelState.Remove("EducationalMaterial.Grade");
            ModelState.Remove("EducationalMaterial.Subject");
            ModelState.Remove("EducationalMaterial.SalesOutlets");
            ModelState.Remove("EducationalMaterial.CoverImageUrl");
            ModelState.Remove("Grades");
            ModelState.Remove("Subjects");

            if (ModelState.IsValid)
            {
                if (viewModel.CoverImage != null)
                {
                    viewModel.EducationalMaterial.CoverImageUrl = await _fileService.SaveFileAsync(viewModel.CoverImage, "materials_covers");
                }

                viewModel.EducationalMaterial.SalesOutlets = new List<SalesOutlet>();
                if (viewModel.SalesOutlets != null)
                {
                    foreach (var outletVM in viewModel.SalesOutlets.Where(o => o.IsAssigned))
                    {
                        var outlet = await _context.SalesOutlets.FindAsync(outletVM.OutletId);
                        if (outlet != null)
                        {
                            viewModel.EducationalMaterial.SalesOutlets.Add(outlet);
                        }
                    }
                }

                _context.Add(viewModel.EducationalMaterial);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", viewModel.EducationalMaterial.GradeId);
            viewModel.Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", viewModel.EducationalMaterial.SubjectId);
            return View(viewModel);
        }

        // GET: Admin/EducationalMaterials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.EducationalMaterials
                .Include(m => m.SalesOutlets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null) return NotFound();

            var allOutlets = await _context.SalesOutlets.OrderBy(o => o.Governorate).ThenBy(o => o.BookstoreName).ToListAsync();
            var assignedOutletIds = new HashSet<int>(material.SalesOutlets.Select(o => o.Id));

            var viewModel = new EducationalMaterialFormViewModel
            {
                EducationalMaterial = material,
                Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", material.GradeId),
                Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", material.SubjectId),
                SalesOutlets = allOutlets.Select(o => new AssignedSalesOutletViewModel
                {
                    OutletId = o.Id,
                    Name = $"{o.BookstoreName} - {o.Governorate}",
                    IsAssigned = assignedOutletIds.Contains(o.Id)
                }).ToList()
            };
            return View(viewModel);
        }

        // POST: Admin/EducationalMaterials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EducationalMaterialFormViewModel viewModel)
        {
            if (id != viewModel.EducationalMaterial.Id) return NotFound();

            ModelState.Remove("CoverImage");
            ModelState.Remove("EducationalMaterial.Grade");
            ModelState.Remove("EducationalMaterial.Subject");
            ModelState.Remove("EducationalMaterial.SalesOutlets");
            ModelState.Remove("EducationalMaterial.CoverImageUrl");
            ModelState.Remove("Grades");
            ModelState.Remove("Subjects");

            if (ModelState.IsValid)
            {
                var materialToUpdate = await _context.EducationalMaterials
                    .Include(m => m.SalesOutlets)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (materialToUpdate == null) return NotFound();

                if (viewModel.CoverImage != null)
                {
                    _fileService.DeleteFile(materialToUpdate.CoverImageUrl);
                    materialToUpdate.CoverImageUrl = await _fileService.SaveFileAsync(viewModel.CoverImage, "materials_covers");
                }

                materialToUpdate.Title = viewModel.EducationalMaterial.Title;
                materialToUpdate.Description = viewModel.EducationalMaterial.Description;
                materialToUpdate.PageCount = viewModel.EducationalMaterial.PageCount;
                materialToUpdate.Rating = viewModel.EducationalMaterial.Rating;
                materialToUpdate.GradeId = viewModel.EducationalMaterial.GradeId;
                materialToUpdate.SubjectId = viewModel.EducationalMaterial.SubjectId;

                // Update SalesOutlets
                materialToUpdate.SalesOutlets.Clear();
                if (viewModel.SalesOutlets != null)
                {
                    foreach (var outletVM in viewModel.SalesOutlets.Where(o => o.IsAssigned))
                    {
                        var outlet = await _context.SalesOutlets.FindAsync(outletVM.OutletId);
                        if (outlet != null)
                        {
                            materialToUpdate.SalesOutlets.Add(outlet);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", viewModel.EducationalMaterial.GradeId);
            viewModel.Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", viewModel.EducationalMaterial.SubjectId);
            return View(viewModel);
        }

        // GET: Admin/EducationalMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var material = await _context.EducationalMaterials
                .Include(e => e.Grade)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null) return NotFound();
            return View(material);
        }

        // POST: Admin/EducationalMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.EducationalMaterials.FindAsync(id);
            if (material != null)
            {
                _fileService.DeleteFile(material.CoverImageUrl);
                _context.EducationalMaterials.Remove(material);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
