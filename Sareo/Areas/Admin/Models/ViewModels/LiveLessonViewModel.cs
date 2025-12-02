using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    // ViewModel for the main index page, containing all categories of lessons
    public class LiveLessonIndexViewModel
    {
        public IEnumerable<LiveLessonListItemViewModel> UpcomingLessons { get; set; }
        public IEnumerable<LiveLessonListItemViewModel> LiveNowLessons { get; set; }
        public IEnumerable<LiveLessonListItemViewModel> RecordedLessons { get; set; }
    }

    // ViewModel for a single lesson item in the lists
    public class LiveLessonListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TeacherName { get; set; }
        public string SubjectName { get; set; }
        public DateTime StartTime { get; set; }
        public string StartTimeFormatted { get; set; }
        public int DurationMinutes { get; set; } 
        public string CoverImageUrl { get; set; }
        public string MeetingUrl { get; set; }
    }

    // ViewModel for the Create/Edit form
    public class LiveLessonFormViewModel
    {
        public LiveLesson LiveLesson { get; set; }

        [Display(Name = "صورة الغلاف")]
        public IFormFile CoverImage { get; set; }

        [Display(Name = "المعلم")]
        public IEnumerable<SelectListItem> Teachers { get; set; }

        [Display(Name = "المادة الدراسية")]
        public IEnumerable<SelectListItem> Subjects { get; set; }

       
        [Display(Name = "الصف الدراسي")]
        public IEnumerable<SelectListItem> Grades { get; set; }
    }
}
