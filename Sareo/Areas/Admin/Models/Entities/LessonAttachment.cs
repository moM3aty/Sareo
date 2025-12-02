using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class LessonAttachment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الملف مطلوب")]
        [Display(Name = "اسم الملف")]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }
        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; }

    }
}
