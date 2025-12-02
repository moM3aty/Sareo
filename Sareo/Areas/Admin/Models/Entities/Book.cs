using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الكتاب مطلوب")]
        [Display(Name = "العنوان")]
        public string Title { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Display(Name = "صورة الغلاف")]
        public string CoverImageUrl { get; set; }

        [Display(Name = "ملف PDF")]
        [Required(ErrorMessage = "يجب رفع ملف الكتاب")]
        public string PdfFilePath { get; set; }

        [Display(Name = "تاريخ الرفع")]
        public DateTime UploadDate { get; set; }

        [Required(ErrorMessage = "يجب اختيار الصف")]
        [Display(Name = "الصف الدراسي")]
        public int GradeId { get; set; }

        [Required(ErrorMessage = "يجب اختيار المادة")]
        [Display(Name = "المادة")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "يجب اختيار المدرس")]
        [Display(Name = "المدرس")]
        public int TeacherId { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
    }
}
