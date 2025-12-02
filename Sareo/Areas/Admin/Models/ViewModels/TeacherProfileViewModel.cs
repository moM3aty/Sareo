using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class TeacherProfileViewModel
    {
        public Teacher Teacher { get; set; }
        public string Email { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<Book> Books { get; set; }
        public IEnumerable<LiveLesson> LiveLessons { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
    }
}