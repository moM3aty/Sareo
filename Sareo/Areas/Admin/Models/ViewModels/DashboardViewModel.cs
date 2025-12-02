using System.Collections.Generic;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }

        public List<StudentIndexViewModel> LatestStudents { get; set; }
        public List<CourseIndexViewModel> PopularCourses { get; set; }
        public List<BlogPostIndexViewModel> LatestBlogPosts { get; set; }

        public List<string> NewStudentsChartLabels { get; set; }
        public List<int> NewStudentsChartData { get; set; }
        public List<string> StudentsByGradeChartLabels { get; set; }
        public List<int> StudentsByGradeChartData { get; set; }
    }
}
