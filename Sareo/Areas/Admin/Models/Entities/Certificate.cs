using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class Certificate
    {
        public int Id { get; set; }

        [Display(Name = "تاريخ الإصدار")]
        public DateTime IssueDate { get; set; }

        [Display(Name = "الدرجة المئوية")]
        [Range(0, 100, ErrorMessage = "الدرجة يجب أن تكون بين ٠ و ١٠٠.")]
        public int GradePercentage { get; set; }

        public string CertificateGuid { get; set; }

        [Required(ErrorMessage = "يجب اختيار الطالب")]
        [Display(Name = "الطالب")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "يجب اختيار الدورة")]
        [Display(Name = "الدورة")]
        public int CourseId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}
