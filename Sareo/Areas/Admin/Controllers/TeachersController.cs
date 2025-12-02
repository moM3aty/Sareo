using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Models;
using Sareoo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeachersController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TeachersController(PlatformDbContext context, IFileService fileService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _fileService = fileService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var teachersQuery = _context.Teachers
                .Select(t => new TeacherIndexViewModel
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    JobTitle = t.JobTitle,
                    ExperienceYears = t.ExperienceYears,
                    ProfileImageUrl = t.ProfileImageUrl
                });

            if (!string.IsNullOrEmpty(searchString))
            {
                teachersQuery = teachersQuery.Where(t => t.FullName.Contains(searchString) || t.JobTitle.Contains(searchString));
            }

            var teachers = await teachersQuery.OrderBy(t => t.FullName).ToListAsync();
            return View(teachers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers
                .Include(t => t.Subjects)
                .Include(t => t.Ratings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (teacher == null) return NotFound();

            var user = await _userManager.FindByIdAsync(teacher.ApplicationUserId);

            var viewModel = new TeacherProfileViewModel
            {
                Teacher = teacher,
                Email = user?.Email, 
                Courses = await _context.Courses.Where(c => c.TeacherId == id).Include(c => c.Grade).ToListAsync(),
                Books = await _context.Books.Where(b => b.TeacherId == id).Include(b => b.Grade).ToListAsync(),
                LiveLessons = await _context.LiveLessons.Where(l => l.TeacherId == id).Include(l => l.Grade).ToListAsync(),
                TotalRatings = teacher.Ratings.Count,
                AverageRating = teacher.Ratings.Any() ? teacher.Ratings.Average(r => r.Rating) : 0
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new TeacherFormViewModel
            {
                Teacher = new Teacher(),
                Subjects = await _context.Subjects.Select(s => new AssignedSubjectViewModel
                {
                    SubjectId = s.Id,
                    Name = s.Name,
                    IsAssigned = false
                }).ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherFormViewModel viewModel)
        {
            ModelState.Remove("Teacher.Courses");
            ModelState.Remove("Teacher.Ratings");
            ModelState.Remove("Teacher.Subjects");
            ModelState.Remove("Teacher.LiveLessons");
            ModelState.Remove("Teacher.ProfileImageUrl");
            ModelState.Remove("Teacher.ApplicationUser");
            ModelState.Remove("Teacher.ApplicationUserId");
            ModelState.Remove("Teacher.Books");
            ModelState.Remove("NewPassword");

            if (!ModelState.IsValid)
            {
                var assignedSubjectIds = new HashSet<int>(viewModel.Subjects?.Where(s => s.IsAssigned).Select(s => s.SubjectId) ?? Enumerable.Empty<int>());
                viewModel.Subjects = await _context.Subjects.Select(s => new AssignedSubjectViewModel
                {
                    SubjectId = s.Id,
                    Name = s.Name,
                    IsAssigned = assignedSubjectIds.Contains(s.Id)
                }).ToListAsync();
                return View(viewModel);
            }

            var user = new ApplicationUser
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                FullName = viewModel.Teacher.FullName,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, viewModel.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                var assignedSubjectIds = new HashSet<int>(viewModel.Subjects?.Where(s => s.IsAssigned).Select(s => s.SubjectId) ?? Enumerable.Empty<int>());
                viewModel.Subjects = await _context.Subjects.Select(s => new AssignedSubjectViewModel
                {
                    SubjectId = s.Id,
                    Name = s.Name,
                    IsAssigned = assignedSubjectIds.Contains(s.Id)
                }).ToListAsync();
                return View(viewModel);
            }

            await _userManager.AddToRoleAsync(user, "Teacher");

            var teacher = viewModel.Teacher;
            teacher.ApplicationUserId = user.Id;

            if (viewModel.ProfileImage != null)
            {
                teacher.ProfileImageUrl = await _fileService.SaveFileAsync(viewModel.ProfileImage, "teachers");
            }

            if (viewModel.Subjects != null)
            {
                teacher.Subjects = new List<Subject>();
                foreach (var subjectVM in viewModel.Subjects.Where(s => s.IsAssigned))
                {
                    var subject = await _context.Subjects.FindAsync(subjectVM.SubjectId);
                    if (subject != null) teacher.Subjects.Add(subject);
                }
            }

            _context.Add(teacher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers
                .Include(t => t.Subjects)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null) return NotFound();

            var user = await _userManager.FindByIdAsync(teacher.ApplicationUserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "خطأ: حساب تسجيل الدخول لهذا المعلم غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            var allSubjects = await _context.Subjects.ToListAsync();
            var viewModel = new TeacherFormViewModel
            {
                Teacher = teacher,
                Email = user.Email,
                Subjects = allSubjects.Select(s => new AssignedSubjectViewModel
                {
                    SubjectId = s.Id,
                    Name = s.Name,
                    IsAssigned = teacher.Subjects.Any(ts => ts.Id == s.Id)
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherFormViewModel viewModel)
        {
            if (id != viewModel.Teacher.Id) return NotFound();

            if (ModelState.ContainsKey(nameof(viewModel.Password)))
            {
                ModelState[nameof(viewModel.Password)].Errors.Clear();
            }
            if (viewModel.ProfileImage == null && ModelState.ContainsKey(nameof(viewModel.ProfileImage)))
            {
                ModelState[nameof(viewModel.ProfileImage)].Errors.Clear();
            }

            ModelState.Remove("Teacher.Courses");
            ModelState.Remove("Teacher.Ratings");
            ModelState.Remove("Teacher.Subjects");
            ModelState.Remove("Teacher.LiveLessons");
            ModelState.Remove("Teacher.ProfileImageUrl");
            ModelState.Remove("Teacher.ApplicationUser");
            ModelState.Remove("Teacher.ApplicationUserId");
            ModelState.Remove("Teacher.Books");
            ModelState.Remove("NewPassword");

            if (ModelState.IsValid)
            {
                var teacherToUpdate = await _context.Teachers
                    .Include(t => t.Subjects)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (teacherToUpdate == null) return NotFound();

                var user = await _userManager.FindByIdAsync(teacherToUpdate.ApplicationUserId);
                if (user == null) return NotFound();

                user.Email = viewModel.Email;
                user.UserName = viewModel.Email;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    var allSubjects = await _context.Subjects.ToListAsync();
                    viewModel.Subjects = allSubjects.Select(s => new AssignedSubjectViewModel
                    {
                        SubjectId = s.Id,
                        Name = s.Name,
                        IsAssigned = teacherToUpdate.Subjects.Any(ts => ts.Id == s.Id)
                    }).ToList();
                    return View(viewModel);
                }

                if (!string.IsNullOrEmpty(viewModel.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, viewModel.NewPassword);
                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        var allSubjects = await _context.Subjects.ToListAsync();
                        viewModel.Subjects = allSubjects.Select(s => new AssignedSubjectViewModel
                        {
                            SubjectId = s.Id,
                            Name = s.Name,
                            IsAssigned = teacherToUpdate.Subjects.Any(ts => ts.Id == s.Id)
                        }).ToList();
                        return View(viewModel);
                    }
                }

                if (viewModel.ProfileImage != null)
                {
                    _fileService.DeleteFile(teacherToUpdate.ProfileImageUrl);
                    teacherToUpdate.ProfileImageUrl = await _fileService.SaveFileAsync(viewModel.ProfileImage, "teachers");
                }

                teacherToUpdate.FullName = viewModel.Teacher.FullName;
                teacherToUpdate.JobTitle = viewModel.Teacher.JobTitle;
                teacherToUpdate.ExperienceSummary = viewModel.Teacher.ExperienceSummary;
                teacherToUpdate.ExperienceYears = viewModel.Teacher.ExperienceYears;

                teacherToUpdate.Subjects.Clear();
                if (viewModel.Subjects != null)
                {
                    foreach (var subjectVM in viewModel.Subjects.Where(s => s.IsAssigned))
                    {
                        var subject = await _context.Subjects.FindAsync(subjectVM.SubjectId);
                        if (subject != null) teacherToUpdate.Subjects.Add(subject);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var subjectsList = await _context.Subjects.ToListAsync();
            viewModel.Subjects = subjectsList.Select(s => new AssignedSubjectViewModel
            {
                SubjectId = s.Id,
                Name = s.Name,
                IsAssigned = viewModel.Subjects?.Any(sub => sub.SubjectId == s.Id && sub.IsAssigned) ?? false
            }).ToList();
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                _fileService.DeleteFile(teacher.ProfileImageUrl);
                _context.Teachers.Remove(teacher);

                if (!string.IsNullOrEmpty(teacher.ApplicationUserId))
                {
                    var user = await _userManager.FindByIdAsync(teacher.ApplicationUserId);
                    if (user != null)
                    {
                        await _userManager.DeleteAsync(user);
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
