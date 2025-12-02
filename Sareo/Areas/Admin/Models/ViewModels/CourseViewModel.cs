using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    // ViewModel for listing courses
    public class CourseIndexViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TeacherName { get; set; }
        public string SubjectName { get; set; }
        public string GradeName { get; set; }
        public string CoverImageUrl { get; set; }
        public int LessonsCount { get; set; }
    }

    // ViewModel for the Create/Edit form
    public class CourseFormViewModel
    {
        public Course Course { get; set; }

        [Display(Name = "صورة الغلاف")]
        public IFormFile CoverImage { get; set; }

        [Display(Name = "المعلم")]
        public IEnumerable<SelectListItem> Teachers { get; set; }

        [Display(Name = "الصف الدراسي")]
        public IEnumerable<SelectListItem> Grades { get; set; }

        [Display(Name = "المادة الدراسية")]
        public IEnumerable<SelectListItem> Subjects { get; set; }
    }
}
