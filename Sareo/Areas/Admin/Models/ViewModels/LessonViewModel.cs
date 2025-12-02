using Sareoo.Areas.Admin.Models.Entities;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class LessonViewModel
    {
        public Lesson Lesson { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
    }
    public class HomeworkSubmissionViewModel
    {
        public string StudentName { get; set; }
        public string LessonTitle { get; set; }
        public string CourseTitle { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}
