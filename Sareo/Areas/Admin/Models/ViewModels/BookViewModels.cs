
using Sareoo.Areas.Admin.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class BookIndexViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string GradeName { get; set; }
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
        public string PdfFilePath { get; set; }
    }

    public class BookFormViewModel
    {
        public Book Book { get; set; }

        [Display(Name = "صورة الغلاف")]
        public IFormFile CoverImage { get; set; }

        [Display(Name = "ملف الكتاب (PDF)")]
        public IFormFile PdfFile { get; set; }

        public IEnumerable<SelectListItem> Grades { get; set; }
        public IEnumerable<SelectListItem> Subjects { get; set; }
        public IEnumerable<SelectListItem> Teachers { get; set; }
    }
}
