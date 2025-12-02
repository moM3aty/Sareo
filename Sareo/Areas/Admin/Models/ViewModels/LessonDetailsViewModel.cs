using Sareoo.Areas.Admin.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class LessonDetailsViewModel
    {
        public Lesson Lesson { get; set; }

        [Display(Name = "ملف مرفق جديد")]
        public IFormFile NewAttachment { get; set; }
    }
}
