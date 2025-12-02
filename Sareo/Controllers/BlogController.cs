using Sareoo.Areas.Admin.Data;
using Sareoo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sareoo.Controllers
{
    public class BlogController : Controller
    {
        private readonly PlatformDbContext _context;

        public BlogController(PlatformDbContext context)
        {
            _context = context;
        }

        // GET: /Blog
        public async Task<IActionResult> Index()
        {
            char[] separator = new[] { ',' };

            var blogPostsRaw = await _context.BlogPosts
                .OrderByDescending(p => p.PublishDate)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.PublishDate,
                    p.ImageUrl,
                    p.Content,
                    p.Keywords
                })
                .ToListAsync(); 

            var blogPosts = blogPostsRaw
                .Select(p => new BlogPostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    PublishDate = p.PublishDate,
                    ImageUrl = p.ImageUrl,
                    ShortDescription = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                    Keywords = string.IsNullOrEmpty(p.Keywords)
                        ? new List<string>()
                        : p.Keywords.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim()).ToList()
                })
                .ToList(); 

            return View(blogPosts);
        }


        // GET: /Blog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts.FindAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost); 
        }
    }
}