using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogPostsController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;

        public BlogPostsController(PlatformDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: Admin/BlogPosts
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var postsQuery = _context.BlogPosts
                .Select(p => new BlogPostIndexViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    PublishDate = p.PublishDate,
                    ImageUrl = p.ImageUrl,
                    Keywords = p.Keywords
                });

            if (!String.IsNullOrEmpty(searchString))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(searchString) || p.Keywords.Contains(searchString));
            }

            var posts = await postsQuery.OrderByDescending(p => p.PublishDate).ToListAsync();
            return View(posts);
        }

        // GET: Admin/BlogPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null) return NotFound();
            return View(blogPost);
        }

        // GET: Admin/BlogPosts/Create
        public IActionResult Create()
        {
            var viewModel = new BlogPostFormViewModel
            {
                BlogPost = new BlogPost { PublishDate = DateTime.Now }
            };
            return View(viewModel);
        }

        // POST: Admin/BlogPosts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPostFormViewModel viewModel)
        {
            ModelState.Remove("BlogPost.ImageUrl");
            if (ModelState.IsValid)
            {
                var blogPost = viewModel.BlogPost;
                if (viewModel.PostImage != null)
                {
                    blogPost.ImageUrl = await _fileService.SaveFileAsync(viewModel.PostImage, "blog");
                }
                _context.Add(blogPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Admin/BlogPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null) return NotFound();
            var viewModel = new BlogPostFormViewModel
            {
                BlogPost = blogPost
            };
            return View(viewModel);
        }

        // POST: Admin/BlogPosts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogPostFormViewModel viewModel)
        {
            if (id != viewModel.BlogPost.Id) return NotFound();

            ModelState.Remove("PostImage");
            ModelState.Remove("BlogPost.ImageUrl");

            if (ModelState.IsValid)
            {
                var postToUpdate = await _context.BlogPosts.FindAsync(id);
                if (postToUpdate == null) return NotFound();

                if (viewModel.PostImage != null)
                {
                    _fileService.DeleteFile(postToUpdate.ImageUrl);
                    postToUpdate.ImageUrl = await _fileService.SaveFileAsync(viewModel.PostImage, "blog");
                }

                postToUpdate.Title = viewModel.BlogPost.Title;
                postToUpdate.Content = viewModel.BlogPost.Content;
                postToUpdate.Keywords = viewModel.BlogPost.Keywords;
                postToUpdate.PublishDate = viewModel.BlogPost.PublishDate;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BlogPosts.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Admin/BlogPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null) return NotFound();
            return View(blogPost);
        }

        // POST: Admin/BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                _fileService.DeleteFile(blogPost.ImageUrl);
                _context.BlogPosts.Remove(blogPost);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
