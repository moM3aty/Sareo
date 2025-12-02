using Sareoo.Areas.Admin.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Sareoo.Controllers
{
    public class CertificateController : Controller
    {
        private readonly PlatformDbContext _context;

        public CertificateController(PlatformDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("الرقم المرجعي للشهادة غير صحيح.");
            }

            var certificate = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                .ThenInclude(co => co.Teacher)
                .FirstOrDefaultAsync(m => m.CertificateGuid == id);

            if (certificate == null)
            {
                return NotFound("لم يتم العثور على الشهادة المطلوبة.");
            }

            // --- هذا هو التعديل: تحديد مسار الـ View بشكل صريح ---
            return View("~/Views/Certificate/Details.cshtml", certificate);
        }
    }
}
