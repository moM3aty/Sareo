using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class EducationalMaterial
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان المادة التعليمية مطلوب")]
        [Display(Name = "العنوان")]
        public string Title { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Display(Name = "صورة الغلاف")]
        public string CoverImageUrl { get; set; }

        [Display(Name = "التقييم (من ٥)")]
        [Range(0, 5, ErrorMessage = "التقييم يجب أن يكون بين ٠ و ٥.")]
        public double Rating { get; set; }

        [Display(Name = "عدد الصفحات")]
        public int PageCount { get; set; }

        [Required(ErrorMessage = "يجب اختيار الصف")]
        [Display(Name = "الصف الدراسي")]
        public int GradeId { get; set; }

        [Required(ErrorMessage = "يجب اختيار المادة")]
        [Display(Name = "المادة")]
        public int SubjectId { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }

        public virtual ICollection<SalesOutlet> SalesOutlets { get; set; }
    }
}
