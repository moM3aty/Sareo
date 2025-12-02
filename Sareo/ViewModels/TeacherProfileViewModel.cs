using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sareoo.Areas.Admin.Models.Entities;

namespace Sareoo.ViewModels
{
    public class TeacherProfileViewModel
    {
        public Teacher Teacher { get; set; }
        public List<CourseInfoViewModel> Courses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public double AverageRating { get; set; }
        public int RatingsCount { get; set; }
        public List<TeacherRating> RecentRatings { get; set; }
        public SubmitRatingViewModel NewRating { get; set; }
    }

    public class CourseInfoViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string SubjectName { get; set; }
        public int LessonsCount { get; set; }
        public double TotalHours { get; set; }
    }

    public class SubmitRatingViewModel
    {
        public int TeacherId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "الرجاء اختيار تقييم من 1 إلى 5 نجوم.")]
        public int Rating { get; set; }

        [StringLength(500)]
        public string Comment { get; set; }
    }
}
