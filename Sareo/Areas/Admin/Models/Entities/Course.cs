using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الدورة مطلوب")]
        [StringLength(200)]
        [Display(Name = "عنوان الدورة")]
        public string Title { get; set; }

        [Display(Name = "وصف الدورة")]
        public string Description { get; set; }

        [Display(Name = "رابط صورة الغلاف")]
        public string CoverImageUrl { get; set; }

        [Display(Name = "إجمالي الساعات")]
        public double TotalHours { get; set; }

        [Required(ErrorMessage = "يجب اختيار المعلم")]
        [Display(Name = "المعلم")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "يجب اختيار المادة")]
        [Display(Name = "المادة")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "يجب اختيار الصف الدراسي")]
        [Display(Name = "الصف الدراسي")]
        public int GradeId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }
        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }

        public virtual ICollection<Lesson> Lessons { get; set; }
        public virtual ICollection<StudentCourse> StudentCourses { get; set; }
        public virtual Exam Exam { get; set; }
        public virtual ICollection<Certificate> Certificates { get; set; }
    }
}
