using Sareoo.Areas.Admin.Data;
using Sareoo.ViewModels;
using Microsoft.AspNetCore.Authorization; // Required for AllowAnonymous
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Sareoo.Controllers
{
    public class EducationalMaterialsController : Controller
    {
        private readonly PlatformDbContext _context;

        public EducationalMaterialsController(PlatformDbContext context)
        {
            _context = context;
        }

        // GET: /EducationalMaterials
        public async Task<IActionResult> Index()
        {
            var materials = await _context.EducationalMaterials
                .Include(m => m.Grade)
                .Include(m => m.Subject)
                .Select(m => new PublicEducationalMaterialViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description.Length > 100 ? m.Description.Substring(0, 100) + "..." : m.Description,
                    CoverImageUrl = m.CoverImageUrl,
                    PageCount = m.PageCount,
                    GradeName = m.Grade.Name,
                    SubjectName = m.Subject.Name
                }).ToListAsync();

            return View(materials);
        }

        // GET: /EducationalMaterials/GetMaterialDetails/5
        [AllowAnonymous]
        public async Task<IActionResult> GetMaterialDetails(int id)
        {
            var material = await _context.EducationalMaterials
                .Include(m => m.SalesOutlets)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null) return NotFound();

            var viewModel = new
            {
                title = material.Title,
                description = material.Description,
                coverImageUrl = material.CoverImageUrl,
                governorates = material.SalesOutlets.Select(o => o.Governorate).Distinct().ToList(),
                outlets = material.SalesOutlets.Select(o => $"{o.BookstoreName} - {o.Governorate}").ToList()
            };

            return Json(viewModel);
        }
    }
}
