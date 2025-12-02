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
    public class CertificatesController : Controller
    {
        private readonly PlatformDbContext _context;

        public CertificatesController(PlatformDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var certificatesQuery = _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                .Select(c => new CertificateIndexViewModel
                {
                    Id = c.Id,
                    StudentName = c.Student.FullName,
                    CourseTitle = c.Course.Title,
                    IssueDate = c.IssueDate,
                    GradePercentage = c.GradePercentage
                });

            if (!String.IsNullOrEmpty(searchString))
            {
                certificatesQuery = certificatesQuery.Where(c => c.StudentName.Contains(searchString)
                                                           || c.CourseTitle.Contains(searchString));
            }

            var certificates = await certificatesQuery.OrderByDescending(c => c.IssueDate).ToListAsync();
            return View(certificates);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                .ThenInclude(co => co.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new CertificateFormViewModel
            {
                Certificate = new Certificate { IssueDate = DateTime.Now, CertificateGuid = Guid.NewGuid().ToString() },
                Students = new SelectList(await _context.Students.ToListAsync(), "Id", "FullName"),
                Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CertificateFormViewModel viewModel)
        {
            ModelState.Remove("Certificate.Student");
            ModelState.Remove("Certificate.Course");
            ModelState.Remove("Students");
            ModelState.Remove("Courses");

            if (ModelState.IsValid)
            {
                _context.Add(viewModel.Certificate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            viewModel.Students = new SelectList(await _context.Students.ToListAsync(), "Id", "FullName", viewModel.Certificate.StudentId);
            viewModel.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title", viewModel.Certificate.CourseId);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }
            var viewModel = new CertificateFormViewModel
            {
                Certificate = certificate,
                Students = new SelectList(await _context.Students.ToListAsync(), "Id", "FullName", certificate.StudentId),
                Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title", certificate.CourseId)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CertificateFormViewModel viewModel)
        {
            if (id != viewModel.Certificate.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Certificate.Student");
            ModelState.Remove("Certificate.Course");
            ModelState.Remove("Students");
            ModelState.Remove("Courses");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewModel.Certificate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Certificates.Any(e => e.Id == viewModel.Certificate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            viewModel.Students = new SelectList(await _context.Students.ToListAsync(), "Id", "FullName", viewModel.Certificate.StudentId);
            viewModel.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title", viewModel.Certificate.CourseId);
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate != null)
            {
                _context.Certificates.Remove(certificate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الشهادة بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
