using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class BooksController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;

        public BooksController(PlatformDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var booksQuery = _context.Books.AsQueryable();

            if (User.IsInRole("Teacher"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var teacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.ApplicationUserId == userId);
                if (teacher != null)
                {
                    booksQuery = booksQuery.Where(b => b.TeacherId == teacher.Id);
                }
                else
                {
                    booksQuery = booksQuery.Where(b => false); 
                }
            }

            var projectedQuery = booksQuery
                .Include(b => b.Grade)
                .Include(b => b.Subject)
                .Include(b => b.Teacher)
                .Select(b => new BookIndexViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    CoverImageUrl = b.CoverImageUrl,
                    GradeName = b.Grade.Name,
                    SubjectName = b.Subject.Name,
                    TeacherName = b.Teacher.FullName,
                    PdfFilePath = b.PdfFilePath
                });

            if (!String.IsNullOrEmpty(searchString))
            {
                projectedQuery = projectedQuery.Where(b => b.Title.Contains(searchString)
                                                || b.GradeName.Contains(searchString)
                                                || b.SubjectName.Contains(searchString)
                                                || b.TeacherName.Contains(searchString));
            }

            var books = await projectedQuery.OrderBy(b => b.Title).ToListAsync();
            return View(books);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new BookFormViewModel
            {
                Book = new Book(),
                Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name"),
                Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name"),
                Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel viewModel)
        {
            ModelState.Remove("Book.CoverImageUrl");
            ModelState.Remove("Book.PdfFilePath");
            ModelState.Remove("Book.Grade");
            ModelState.Remove("Book.Subject");
            ModelState.Remove("Book.Teacher");
            ModelState.Remove("Grades");
            ModelState.Remove("Subjects");
            ModelState.Remove("Teachers");

            if (ModelState.IsValid)
            {
                var book = viewModel.Book;
                book.UploadDate = DateTime.UtcNow;

                if (viewModel.CoverImage != null)
                {
                    book.CoverImageUrl = await _fileService.SaveFileAsync(viewModel.CoverImage, "books/covers");
                }
                if (viewModel.PdfFile != null)
                {
                    book.PdfFilePath = await _fileService.SaveFileAsync(viewModel.PdfFile, "books/pdfs");
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", viewModel.Book.GradeId);
            viewModel.Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", viewModel.Book.SubjectId);
            viewModel.Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName", viewModel.Book.TeacherId);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var viewModel = new BookFormViewModel
            {
                Book = book,
                Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", book.GradeId),
                Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", book.SubjectId),
                Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName", book.TeacherId)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookFormViewModel viewModel)
        {
            if (id != viewModel.Book.Id) return NotFound();

            ModelState.Remove("PdfFile");
            ModelState.Remove("Book.CoverImageUrl");
            ModelState.Remove("Book.PdfFilePath");
            ModelState.Remove("Book.Grade");
            ModelState.Remove("Book.Subject");
            ModelState.Remove("Book.Teacher");
            ModelState.Remove("Grades");
            ModelState.Remove("Subjects");
            ModelState.Remove("Teachers");
            ModelState.Remove("CoverImage");

            if (ModelState.IsValid)
            {
                var bookToUpdate = await _context.Books.FindAsync(id);
                if (bookToUpdate == null) return NotFound();

                if (viewModel.CoverImage != null)
                {
                    _fileService.DeleteFile(bookToUpdate.CoverImageUrl);
                    bookToUpdate.CoverImageUrl = await _fileService.SaveFileAsync(viewModel.CoverImage, "books/covers");
                }
                if (viewModel.PdfFile != null)
                {
                    _fileService.DeleteFile(bookToUpdate.PdfFilePath);
                    bookToUpdate.PdfFilePath = await _fileService.SaveFileAsync(viewModel.PdfFile, "books/pdfs");
                }

                bookToUpdate.Title = viewModel.Book.Title;
                bookToUpdate.Description = viewModel.Book.Description;
                bookToUpdate.GradeId = viewModel.Book.GradeId;
                bookToUpdate.SubjectId = viewModel.Book.SubjectId;
                bookToUpdate.TeacherId = viewModel.Book.TeacherId;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", viewModel.Book.GradeId);
            viewModel.Subjects = new SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", viewModel.Book.SubjectId);
            viewModel.Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName", viewModel.Book.TeacherId);
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.Include(b => b.Grade).FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _fileService.DeleteFile(book.CoverImageUrl);
                _fileService.DeleteFile(book.PdfFilePath);
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
