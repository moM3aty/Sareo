using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Sareoo.Models;
using Sareoo.Trackers;
using Sareoo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class StudentsController : Controller
    {
        private readonly PlatformDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PresenceTracker _presenceTracker;

        public StudentsController(PlatformDbContext context,
                                  UserManager<ApplicationUser> userManager,
                                  RoleManager<IdentityRole> roleManager,
                                  PresenceTracker presenceTracker)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _presenceTracker = presenceTracker;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var onlineUsers = await _presenceTracker.GetOnlineUsers();
            var studentsQuery = _context.Students.Include(s => s.Grade).AsQueryable();

            if (User.IsInRole("Teacher"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var teacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.ApplicationUserId == userId);
                if (teacher != null)
                {
                    var teacherCourseIds = await _context.Courses
                        .Where(c => c.TeacherId == teacher.Id)
                        .Select(c => c.Id)
                        .ToListAsync();

                    var studentIds = await _context.StudentCourses
                        .Where(sc => teacherCourseIds.Contains(sc.CourseId))
                        .Select(sc => sc.StudentId)
                        .Distinct()
                        .ToListAsync();

                    studentsQuery = studentsQuery.Where(s => studentIds.Contains(s.Id));
                }
                else
                {
                    studentsQuery = studentsQuery.Where(s => false); 
                }
            }

            var projectedQuery = studentsQuery.Select(s => new StudentIndexViewModel
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
                GradeName = s.Grade.Name,
                RegistrationDate = s.RegistrationDate,
                IsActive = s.IsActive,
                IsOnline = onlineUsers.Contains(s.ApplicationUserId)
            });

            if (!string.IsNullOrEmpty(searchString))
            {
                projectedQuery = projectedQuery.Where(s => s.FullName.Contains(searchString) || s.Email.Contains(searchString));
            }

            var students = await projectedQuery.ToListAsync();
            return View(students);

            
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.Grade)
                .Include(s => s.GivenRatings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            var user = await _userManager.FindByIdAsync(student.ApplicationUserId);
            if (user != null && user.LastLoginDate.HasValue)
            {
                student.LastAccessDate = user.LastLoginDate.Value;
            }

            var studentCourses = await _context.StudentCourses
                .Include(sc => sc.Course)
                .Where(sc => sc.StudentId == id)
                .ToListAsync();

            var quizAttempts = await _context.LessonQuizAttempts
                          .Where(a => a.StudentId == id)
                          .Include(a => a.LessonQuiz.Lesson.Course)
                          .OrderByDescending(a => a.AttemptDate)
                          .Select(a => new LessonQuizAttemptViewModel
                          {
                              AttemptId = a.Id,
                              CourseTitle = a.LessonQuiz.Lesson.Course.Title,
                              LessonTitle = a.LessonQuiz.Lesson.Title,
                              Score = a.Score,
                              TotalPoints = a.TotalPoints,
                              AttemptDate = a.AttemptDate
                          })
                          .ToListAsync();

            var finalExamAttempts = await _context.ExamAttempts
                           .Where(a => a.StudentId == id)
                           .Include(a => a.Exam.Course)
                           .OrderByDescending(a => a.AttemptDate)
                           .Select(a => new ExamAttemptViewModel
                           {
                               AttemptId = a.Id,
                               CourseTitle = a.Exam.Course.Title,
                               Score = a.Score,
                               AttemptDate = a.AttemptDate
                           })
                           .ToListAsync();

            var coursesProgress = studentCourses.Select(sc =>
            {
                string status;
                if (sc.ProgressPercentage >= 100)
                    status = "مكتمل";
                else if (sc.ProgressPercentage > 0)
                    status = "قيد التقدم";
                else
                    status = "لم يبدأ";

                return new StudentCourseProgressViewModel
                {
                    CourseTitle = sc.Course.Title,
                    ProgressPercentage = sc.ProgressPercentage,
                    Status = status,
                    LastAccessDate = sc.LastAccessDate
                };
            }).ToList();

            var certificates = await _context.Certificates
                .Include(c => c.Course)
                .Where(c => c.StudentId == id)
                .ToListAsync();

            var totalHours = studentCourses.Sum(sc => (sc.ProgressPercentage / 100.0) * sc.Course.TotalHours);
            double averageGrade = certificates.Any() ? certificates.Average(c => c.GradePercentage) : 0;

            var totalRatings = student.GivenRatings != null ? student.GivenRatings.Count : 0;
            var averageRating = totalRatings > 0
                ? Math.Round(student.GivenRatings.Average(r => r.Rating), 2)
                : 0;

            var viewModel = new StudentProfileViewModel
            {
                Student = student,
                QuizAttempts = quizAttempts,
                CoursesProgress = coursesProgress,
                Certificates = certificates.Select(c => new CertificateInfoViewModel
                {
                    CertificateId = c.Id,
                    CourseTitle = c.Course.Title,
                    IssueDate = c.IssueDate
                }).ToList(),
                CertificatesCount = certificates.Count,
                CompletedCourses = studentCourses.Count(sc => sc.ProgressPercentage >= 100),
                TotalLearningHours = Math.Round(totalHours, 1),
                AverageGrade = Math.Round(averageGrade, 2),
                IsActive = student.IsActive,
                LastAccessDate = student.LastAccessDate,
                TotalRatings = totalRatings,
                AverageRating = averageRating,
                FinalExamAttempts = finalExamAttempts
            };

            return View(viewModel);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewExamAttempt(int attemptId)
        {
            var attempt = await _context.ExamAttempts
                .Include(a => a.Exam.Course)
                .Include(a => a.Student)
                .Include(a => a.Answers).ThenInclude(ans => ans.Question)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return NotFound();

            var viewModel = new ExamReviewViewModel
            {
                Attempt = attempt,
                Questions = attempt.Answers.Select(a => a.Question).ToList(),
                StudentAnswers = attempt.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedAnswer),
                StudentName = attempt.Student.FullName
            };

            return View("~/Views/Exam/Review.cshtml", viewModel);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewQuizAttempt(int attemptId)
        {
            var attempt = await _context.LessonQuizAttempts
                .Include(a => a.LessonQuiz).ThenInclude(lq => lq.Lesson)
                .Include(a => a.Student)
                .Include(a => a.Answers).ThenInclude(ans => ans.Question)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return NotFound();

            var viewModel = new QuizResultViewModel
            {
                Attempt = attempt,
                Questions = attempt.Answers.Select(a => a.Question).ToList(),
                StudentAnswers = attempt.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedAnswer),
                StudentName = attempt.Student.FullName
            };

            return View(viewModel);
        }
        public IActionResult Create()
        {
            var viewModel = new StudentFormViewModel
            {
                Student = new Student(),
                Grades = new SelectList(_context.Grades, "Id", "Name")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentFormViewModel viewModel)
        {

            ModelState.Remove("Grades");
            ModelState.Remove("Student.Grade");
            ModelState.Remove("Student.ApplicationUser");
            ModelState.Remove("Student.Certificates");
            ModelState.Remove("Student.StudentCourses");
            ModelState.Remove("Student.GivenRatings");
            ModelState.Remove("Student.ApplicationUserId");
            ModelState.Remove("Student.HomeworkSubmissions");
            ModelState.Remove("Student.LiveLessonReminders");
            ModelState.Remove("Student.LessonQuizAttempts");

            if (!ModelState.IsValid)
            {
                viewModel.Grades = new SelectList(_context.Grades, "Id", "Name", viewModel.Student.GradeId);
                return View(viewModel);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            var user = new ApplicationUser
            {
                UserName = viewModel.Student.Email,
                Email = viewModel.Student.Email,
                FullName = viewModel.Student.FullName,
                PhoneNumber = viewModel.Student.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, viewModel.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await transaction.RollbackAsync();
                viewModel.Grades = new SelectList(_context.Grades, "Id", "Name", viewModel.Student.GradeId);
                return View(viewModel);
            }

            try
            {
                viewModel.Student.ApplicationUserId = user.Id;
                viewModel.Student.RegistrationDate = DateTime.Now;
                viewModel.Student.IsActive = true;

                _context.Students.Add(viewModel.Student);
                await _context.SaveChangesAsync();

                if (!await _roleManager.RoleExistsAsync("Student"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Student"));
                }
                await _userManager.AddToRoleAsync(user, "Student");

                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                await _userManager.DeleteAsync(user);

                ModelState.AddModelError("", "حدث خطأ غير متوقع أثناء إضافة الطالب.");
                viewModel.Grades = new SelectList(_context.Grades, "Id", "Name", viewModel.Student.GradeId);
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            var viewModel = new StudentFormViewModel
            {
                Student = student,
                Grades = new SelectList(_context.Grades, "Id", "Name", student.GradeId)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentFormViewModel viewModel)
        {
            if (id != viewModel.Student.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Grades");
            ModelState.Remove("Student.Grade");
            ModelState.Remove("Student.ApplicationUser");
            ModelState.Remove("Student.Certificates");
            ModelState.Remove("Student.StudentCourses");
            ModelState.Remove("Student.GivenRatings");
            ModelState.Remove("Student.HomeworkSubmissions");
            ModelState.Remove("Student.LiveLessonReminders");
            ModelState.Remove("Password");
            ModelState.Remove("Student.LessonQuizAttempts");

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(viewModel.NewPassword))
                {
                    var user = await _userManager.FindByIdAsync(viewModel.Student.ApplicationUserId);
                    if (user != null)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var result = await _userManager.ResetPasswordAsync(user, token, viewModel.NewPassword);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", "فشل تحديث كلمة المرور: " + error.Description);
                            }
                            viewModel.Grades = new SelectList(_context.Grades, "Id", "Name", viewModel.Student.GradeId);
                            return View(viewModel);
                        }
                    }
                }

                try
                {
                    var studentToUpdate = await _context.Students.FindAsync(id);
                    if (studentToUpdate == null) return NotFound();

                    studentToUpdate.FullName = viewModel.Student.FullName;
                    studentToUpdate.Email = viewModel.Student.Email;
                    studentToUpdate.PhoneNumber = viewModel.Student.PhoneNumber;
                    studentToUpdate.GradeId = viewModel.Student.GradeId;
                    studentToUpdate.City = viewModel.Student.City;
                    studentToUpdate.Country = viewModel.Student.Country;
                    studentToUpdate.IsActive = viewModel.Student.IsActive;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Students.Any(e => e.Id == viewModel.Student.Id))
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

            viewModel.Grades = new SelectList(_context.Grades, "Id", "Name", viewModel.Student.GradeId);
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                var user = await _userManager.FindByIdAsync(student.ApplicationUserId);

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                student.IsActive = !student.IsActive;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
