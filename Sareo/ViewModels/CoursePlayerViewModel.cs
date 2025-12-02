using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Sareoo.ViewModels
{
    public class CoursePlayerViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public Lesson CurrentLesson { get; set; }
        public List<Lesson> AllLessons { get; set; }
        public int StudentProgress { get; set; }

        public bool IsCourseCompleted { get; set; }
        public LessonDetailsForStudentViewModel LessonDetails { get; set; }
        public int? NextLessonId { get; set; }
        public int? PreviousLessonId { get; set; }
    }
    public class LessonDetailsForStudentViewModel
    {
        public List<LessonAttachment> Attachments { get; set; }

        public IFormFile SubmittedFile { get; set; }
    }
}
