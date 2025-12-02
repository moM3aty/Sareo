using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class Exam
    {
        [Key]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "عنوان الاختبار مطلوب")]
        [Display(Name = "عنوان الاختبار")]
        public string Title { get; set; }

        [Display(Name = "نسبة النجاح (%)")]
        [Range(0, 100, ErrorMessage = "النسبة يجب أن تكون بين ٠ و ١٠٠.")]
        public int PassPercentage { get; set; }

        public virtual Course Course { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}
