using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Models;
using Sareoo.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Sareoo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PlatformDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, PlatformDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var viewModel = new RegisterViewModel
            {
                Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ModelState.Remove("Grades");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    EmailConfirmed = true 
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Student"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Student"));
                    }
                    await _userManager.AddToRoleAsync(user, "Student");

                    var student = new Student
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = model.PhoneNumber,
                        RegistrationDate = DateTime.UtcNow,
                        GradeId = model.GradeId,
                        ApplicationUserId = user.Id,
                        City = model.City,
                        Country = model.Country,
                        IsActive = true 
                    };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            model.Grades = new SelectList(await _context.Grades.ToListAsync(), "Id", "Name", model.GradeId);
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);

                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                        }
                        if (roles.Contains("Teacher"))
                        {
                            return RedirectToAction("Index", "Courses", new { area = "Admin" });
                        }
                        if (roles.Contains("Student"))
                        {
                            return RedirectToAction("Index", "Profile");
                        }
                    }
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "الحساب مقفل مؤقتاً.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "محاولة تسجيل دخول غير صالحة.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
