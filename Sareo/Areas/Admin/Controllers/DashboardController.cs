using Sareoo.Areas.Admin.Data;
using Sareoo.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Sareoo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly PlatformDbContext _context;

        public DashboardController(PlatformDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalStudents = await _context.Students.CountAsync();
            var totalTeachers = await _context.Teachers.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();

            // Data for New Students Chart (Line Chart)
            var studentData = await _context.Students
                .Where(s => s.RegistrationDate >= DateTime.Now.AddMonths(-6))
                .GroupBy(s => new { s.RegistrationDate.Year, s.RegistrationDate.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var newStudentsChartLabels = new List<string>();
            var newStudentsChartData = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var monthData = studentData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);
                newStudentsChartLabels.Add(new CultureInfo("ar-EG").DateTimeFormat.GetMonthName(date.Month));
                newStudentsChartData.Add(monthData?.Count ?? 0);
            }

            // Data for Students by Grade Chart (Doughnut Chart)
            var studentsByGrade = await _context.Students
                .Include(s => s.Grade)
                .GroupBy(s => s.Grade.Name)
                .Select(g => new { GradeName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var latestStudents = await _context.Students
                .OrderByDescending(s => s.RegistrationDate)
                .Take(5)
                .Select(s => new StudentIndexViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    RegistrationDate = s.RegistrationDate
                })
                .ToListAsync();

            var popularCourses = await _context.Courses
                .OrderByDescending(c => c.StudentCourses.Count())
                .Take(5)
                .Select(c => new CourseIndexViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    LessonsCount = c.StudentCourses.Count()
                })
                .ToListAsync();

            var latestPosts = await _context.BlogPosts
                .OrderByDescending(p => p.PublishDate)
                .Take(5)
                .Select(p => new BlogPostIndexViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    PublishDate = p.PublishDate
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalCourses = totalCourses,
                LatestStudents = latestStudents,
                PopularCourses = popularCourses,
                LatestBlogPosts = latestPosts,
                NewStudentsChartLabels = newStudentsChartLabels,
                NewStudentsChartData = newStudentsChartData,
                StudentsByGradeChartLabels = studentsByGrade.Select(g => g.GradeName).ToList(),
                StudentsByGradeChartData = studentsByGrade.Select(g => g.Count).ToList()
            };

            return View(viewModel);
        }
    }
}
