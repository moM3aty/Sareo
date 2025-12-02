using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class LessonQuiz
    {
        [Key]
        [ForeignKey("Lesson")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "عنوان الاختبار مطلوب")]
        [Display(Name = "عنوان الاختبار")]
        public string Title { get; set; }

        public virtual Lesson Lesson { get; set; }
        public virtual ICollection<LessonQuizQuestion> Questions { get; set; }
    }
}
