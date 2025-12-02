using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Sareoo.ViewModels
{
    public class PublicCoursesViewModel
    {
        public List<CourseCardViewModel> Courses { get; set; }
        public SelectList Grades { get; set; }
        public SelectList Subjects { get; set; }
        public int? SelectedGradeId { get; set; }
        public int? SelectedSubjectId { get; set; }
        public string SearchTerm { get; set; }
    }

    public class CourseCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public string TeacherName { get; set; }
        public double TotalHours { get; set; }
        public int LessonsCount { get; set; }
        public double AverageRating { get; set; } 
    }
}
