using Microsoft.AspNetCore.Mvc.Rendering;
using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class CertificateIndexViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string CourseTitle { get; set; }
        public DateTime IssueDate { get; set; }
        public int GradePercentage { get; set; }
    }

    public class CertificateFormViewModel
    {
        public Certificate Certificate { get; set; }

        [Display(Name = "الطالب")]
        public IEnumerable<SelectListItem> Students { get; set; }

        [Display(Name = "الدورة")]
        public IEnumerable<SelectListItem> Courses { get; set; }
    }
}
